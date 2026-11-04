using System.Net;

namespace AvaloniaTest.Services;

/// <summary>
/// Ошибка, полученная от API. Хранит HTTP-статус и сообщение,
/// пригодное для показа пользователю.
/// </summary>
public sealed class ApiException : Exception
{
    /// <summary>Создаёт ошибку API с указанным статусом и сообщением.</summary>
    public ApiException(HttpStatusCode statusCode, string message)
        : base(message)
    {
        StatusCode = statusCode;
    }

    /// <summary>HTTP-статус ответа.</summary>
    public HttpStatusCode StatusCode { get; }

    /// <summary>Признак того, что ошибка вызвана отсутствием авторизации.</summary>
    public bool IsUnauthorized => StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden;
}
