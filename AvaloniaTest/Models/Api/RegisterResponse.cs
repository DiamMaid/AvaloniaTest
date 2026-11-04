namespace AvaloniaTest.Models.Api;

/// <summary>
/// Ответ POST /api/auth/register (HTTP 201).
/// Токенов не содержит — регистрация не авторизует пользователя,
/// поэтому после неё требуется отдельный вызов login.
/// </summary>
public sealed record RegisterResponse
{
    /// <summary>Идентификатор созданного пользователя.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Электронная почта пользователя.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Отображаемое имя пользователя.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Роль пользователя.</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>Статус учётной записи, например "pending_verification".</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Момент создания учётной записи.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
