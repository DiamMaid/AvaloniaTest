using AvaloniaTest.Models.Api;

namespace AvaloniaTest.Services;

/// <summary>
/// Порт доступа к REST API авторизации.
/// ViewModel зависят от этого интерфейса, а не от <see cref="HttpClient"/>,
/// что позволяет подменить реализацию в тестах.
/// </summary>
public interface IAuthApiClient
{
    /// <summary>Вход по email и паролю; возвращает токены и краткие данные пользователя.</summary>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);

    /// <summary>Регистрация нового пользователя; токенов не возвращает.</summary>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default);

    /// <summary>Полный профиль текущего пользователя, включая аватар.</summary>
    Task<UserProfileResponse> GetProfileAsync(string accessToken, CancellationToken ct = default);

    /// <summary>Завершение сессии; API требует refresh_token в теле запроса.</summary>
    Task LogoutAsync(string accessToken, string refreshToken, CancellationToken ct = default);
}
