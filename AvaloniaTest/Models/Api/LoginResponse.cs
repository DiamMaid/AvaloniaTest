namespace AvaloniaTest.Models.Api;

/// <summary>Ответ POST /api/auth/login: токены сессии и краткие данные пользователя.</summary>
public sealed record LoginResponse
{
    /// <summary>JWT-токен доступа для заголовка Authorization: Bearer.</summary>
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>Токен обновления; обязателен для корректного logout.</summary>
    public string RefreshToken { get; init; } = string.Empty;

    /// <summary>Схема токена, фактически всегда "Bearer".</summary>
    public string TokenType { get; init; } = string.Empty;

    /// <summary>Время жизни <see cref="AccessToken"/> в секундах (фактически 900).</summary>
    public int ExpiresIn { get; init; }

    /// <summary>Краткие данные пользователя.</summary>
    public LoginUser? User { get; init; }
}
