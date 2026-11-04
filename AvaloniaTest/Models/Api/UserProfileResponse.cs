namespace AvaloniaTest.Models.Api;

/// <summary>
/// Ответ GET /api/auth/me — полный профиль текущего пользователя.
/// Единственный метод, возвращающий <c>avatar</c>.
/// Вложенные блоки <c>preferences</c> и <c>security</c> намеренно не описаны:
/// приложению они не нужны, а сериализатор просто пропускает неизвестные поля.
/// </summary>
public sealed record UserProfileResponse
{
    /// <summary>Идентификатор пользователя.</summary>
    public string Id { get; init; } = string.Empty;

    /// <summary>Электронная почта пользователя.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>Отображаемое имя пользователя.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Роль пользователя.</summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>Абсолютный URL аватара (PNG).</summary>
    public string? Avatar { get; init; }

    /// <summary>Момент создания учётной записи.</summary>
    public DateTimeOffset CreatedAt { get; init; }
}
