using System.Net;
using AvaloniaTest.Models;
using AvaloniaTest.Models.Api;

namespace AvaloniaTest.Services;

/// <summary>
/// Прикладной сервис авторизации. Связывает <see cref="IAuthApiClient"/> и <see cref="ISessionStore"/>,
/// переводит ответы сервера в доменную модель <see cref="User"/> и управляет жизненным циклом сессии.
/// </summary>
public sealed class AuthService : IAuthService
{
    private readonly IAuthApiClient _client;
    private readonly ISessionStore _session;

    /// <summary>Создаёт сервис авторизации на основе API-клиента и хранилища сессии.</summary>
    public AuthService(IAuthApiClient client, ISessionStore session)
    {
        _client = client;
        _session = session;
    }

    /// <inheritdoc />
    public ISessionStore Session => _session;

    /// <inheritdoc />
    public async Task<User> LoginAsync(string email, string password, CancellationToken ct = default)
    {
        var response = await _client.LoginAsync(new LoginRequest { Email = email, Password = password }, ct);

        if (string.IsNullOrEmpty(response.AccessToken) || response.User is null)
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "Сервер не вернул учётные данные.");
        }

        User user;
        try
        {
            var profile = await _client.GetProfileAsync(response.AccessToken, ct);
            user = ToUser(profile);
        }
        catch (ApiException)
        {
            // Профиль догружается по принципу «лучшего усилия»: это единственный источник аватара,
            // но его недоступность не должна отменять уже успешный вход.
            user = ToUser(response.User);
        }

        _session.SignIn(response.AccessToken, response.RefreshToken, user);

        return user;
    }

    /// <inheritdoc />
    public async Task<User> RegisterAsync(string email, string password, string name, CancellationToken ct = default)
    {
        var response = await _client.RegisterAsync(
            new RegisterRequest { Email = email, Password = password, Name = name },
            ct);

        // Регистрация не возвращает токенов, поэтому сессия здесь не создаётся:
        // автоматический вход выполняет вызывающий код.
        return ToUser(response);
    }

    /// <inheritdoc />
    public async Task<User> LoadProfileAsync(CancellationToken ct = default)
    {
        var accessToken = _session.AccessToken;
        if (string.IsNullOrEmpty(accessToken))
        {
            throw new ApiException(HttpStatusCode.Unauthorized, "Требуется авторизация.");
        }

        UserProfileResponse profile;
        try
        {
            profile = await _client.GetProfileAsync(accessToken, ct);
        }
        catch (ApiException ex) when (ex.IsUnauthorized)
        {
            // Токен протух (expires_in = 900 с, обновления нет): сессию гасим сразу,
            // иначе UI продолжит показывать авторизованное состояние при живом 401.
            _session.SignOut();
            throw;
        }

        var user = ToUser(profile);

        _session.UpdateUser(user);

        return user;
    }

    /// <inheritdoc />
    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var accessToken = _session.AccessToken;
        var refreshToken = _session.RefreshToken;

        if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(refreshToken))
        {
            try
            {
                await _client.LogoutAsync(accessToken, refreshToken, ct);
            }
            catch (ApiException)
            {
                // Ошибка сервера не должна блокировать выход: локальная сессия
                // очищается в любом случае, иначе пользователь останется «залогиненным» навсегда.
            }
        }

        _session.SignOut();
    }

    /// <summary>Преобразует ответ GET /api/auth/me в доменного пользователя вместе с аватаром.</summary>
    private static User ToUser(UserProfileResponse profile) => new()
    {
        Id = profile.Id,
        Email = profile.Email,
        Name = profile.Name,
        Role = profile.Role,
        AvatarUrl = profile.Avatar,
    };

    /// <summary>Преобразует краткие данные из ответа login; аватара в них нет.</summary>
    private static User ToUser(LoginUser loginUser) => new()
    {
        Id = loginUser.Id,
        Email = loginUser.Email,
        Name = loginUser.Name,
        Role = loginUser.Role,
        AvatarUrl = null,
    };

    /// <summary>Преобразует ответ POST /api/auth/register; аватара в нём нет.</summary>
    private static User ToUser(RegisterResponse register) => new()
    {
        Id = register.Id,
        Email = register.Email,
        Name = register.Name,
        Role = register.Role,
        AvatarUrl = null,
    };
}
