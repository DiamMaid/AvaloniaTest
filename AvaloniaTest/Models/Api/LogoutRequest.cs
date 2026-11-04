namespace AvaloniaTest.Models.Api;

/// <summary>
/// Тело запроса POST /api/auth/logout.
/// API требует <c>refresh_token</c> в теле: без него отвечает 500 internal_server_error.
/// </summary>
public sealed record LogoutRequest
{
    /// <summary>Токен обновления, полученный при входе.</summary>
    public required string RefreshToken { get; init; }
}
