namespace AvaloniaTest.Models.Api;

/// <summary>
/// Краткое представление пользователя внутри ответа <c>login</c>.
/// Поля <c>avatar</c> здесь нет — оно приходит только из GET /api/auth/me.
/// </summary>
public sealed record LoginUser
{
    /// <summary>Идентификатор пользователя.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Электронная почта пользователя.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Отображаемое имя пользователя.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Роль пользователя.</summary>
    public string Role { get; init; } = string.Empty;
}
