using AvaloniaTest.Models;

namespace AvaloniaTest.Services;

/// <summary>
/// Порт прикладного уровня авторизации. ViewModel работают только с ним:
/// он скрывает и HTTP-клиент, и хранилище сессии, и особенность API,
/// из-за которой профиль приходится догружать отдельным запросом.
/// </summary>
public interface IAuthService
{
    /// <summary>Хранилище состояния сессии.</summary>
    ISessionStore Session { get; }

    /// <summary>
    /// Вход. После получения токенов догружает профиль через GET /me,
    /// потому что ответ login не содержит аватара.
    /// </summary>
    Task<User> LoginAsync(string email, string password, CancellationToken ct = default);

    /// <summary>
    /// Регистрация. API не выдаёт токенов, поэтому метод только создаёт учётную запись
    /// и возвращает её данные; авторизацию выполняет вызывающий код.
    /// </summary>
    Task<User> RegisterAsync(string email, string password, string name, CancellationToken ct = default);

    /// <summary>Загружает профиль текущего пользователя и обновляет сессию.</summary>
    Task<User> LoadProfileAsync(CancellationToken ct = default);

    /// <summary>Завершает сессию на сервере и очищает локальное состояние.</summary>
    Task LogoutAsync(CancellationToken ct = default);
}
