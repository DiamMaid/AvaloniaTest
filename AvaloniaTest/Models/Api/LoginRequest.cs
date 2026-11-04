namespace AvaloniaTest.Models.Api;

/// <summary>Тело запроса POST /api/auth/login.</summary>
public sealed record LoginRequest
{
    /// <summary>Электронная почта пользователя.</summary>
    public required string Email { get; init; }

    /// <summary>Пароль пользователя.</summary>
    public required string Password { get; init; }
}
