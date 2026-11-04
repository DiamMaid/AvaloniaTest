namespace AvaloniaTest.Models.Api;

/// <summary>Тело запроса POST /api/auth/register.</summary>
public sealed record RegisterRequest
{
    /// <summary>Электронная почта пользователя.</summary>
    public required string Email { get; init; }

    /// <summary>Пароль пользователя.</summary>
    public required string Password { get; init; }

    /// <summary>Отображаемое имя пользователя.</summary>
    public required string Name { get; init; }
}
