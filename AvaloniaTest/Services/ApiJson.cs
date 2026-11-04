using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvaloniaTest.Services;

/// <summary>
/// Общие параметры сериализации для всего сетевого слоя.
/// API отдаёт имена полей в snake_case (<c>access_token</c>, <c>created_at</c>),
/// а C#-свойства — в PascalCase. Одна политика именования решает это глобально,
/// поэтому атрибуты <c>[JsonPropertyName]</c> на каждом DTO не нужны.
/// </summary>
public static class ApiJson
{
    /// <summary>Настройки сериализатора JSON для запросов к API.</summary>
    public static JsonSerializerOptions Options { get; } = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.Never,
    };
}
