namespace AvaloniaTest.Models.Api;

/// <summary>
/// Тело ответа API при ошибке.
/// Форма неоднородна: login возвращает только <c>error</c>,
/// а me — и <c>error</c>, и <c>message</c>. Поэтому оба поля необязательны.
/// </summary>
public sealed record ApiErrorResponse
{
    /// <summary>Код ошибки, например "unauthorized".</summary>
    public string? Error { get; init; }

    /// <summary>Человекочитаемое описание, присутствует не во всех ответах.</summary>
    public string? Message { get; init; }
}
