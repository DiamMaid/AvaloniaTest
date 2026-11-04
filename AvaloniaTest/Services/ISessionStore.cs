using AvaloniaTest.Models;

namespace AvaloniaTest.Services;

/// <summary>
/// Порт состояния сессии — единственный источник правды о том,
/// авторизован ли пользователь и какими токенами он владеет.
/// Регистрируется как singleton, чтобы все ViewModel видели одно состояние.
/// </summary>
public interface ISessionStore
{
    /// <summary>Признак активной сессии.</summary>
    bool IsAuthenticated { get; }

    /// <summary>JWT-токен доступа или null, если сессии нет.</summary>
    string? AccessToken { get; }

    /// <summary>Токен обновления; обязателен для logout.</summary>
    string? RefreshToken { get; }

    /// <summary>Текущий пользователь или null, если сессии нет.</summary>
    User? CurrentUser { get; }

    /// <summary>Поднимается после SignIn/SignOut/UpdateUser в потоке, выполнившем изменение сессии.</summary>
    event EventHandler? SessionChanged;

    /// <summary>Сохраняет токены и пользователя, переводит сессию в активное состояние.</summary>
    void SignIn(string accessToken, string refreshToken, User user);

    /// <summary>Заменяет пользователя, не трогая токены (используется после GET /me).</summary>
    void UpdateUser(User user);

    /// <summary>Очищает токены и пользователя.</summary>
    void SignOut();
}
