namespace AvaloniaTest.Models;

/// <summary>
/// Пользователь приложения — доменная модель представления.
/// Намеренно отделена от DTO (<c>LoginUser</c>, <c>UserProfileResponse</c>),
/// чтобы форма ответа API могла меняться, не затрагивая ViewModel.
/// </summary>
public sealed record User
{
    /// <summary>Идентификатор пользователя.</summary>
    public required string Id { get; init; }

    /// <summary>Электронная почта пользователя.</summary>
    public required string Email { get; init; }

    /// <summary>Отображаемое имя пользователя.</summary>
    public required string Name { get; init; }

    /// <summary>Роль пользователя.</summary>
    public required string Role { get; init; }

    /// <summary>URL аватара; заполняется только после GET /api/auth/me.</summary>
    public string? AvatarUrl { get; init; }
}
