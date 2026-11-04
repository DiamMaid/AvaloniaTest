using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using AvaloniaTest.Models.Api;

namespace AvaloniaTest.Services;

/// <summary>Адаптер <see cref="IAuthApiClient"/> поверх <see cref="HttpClient"/>.</summary>
public sealed class AuthApiClient : IAuthApiClient
{
    private const string LoginPath = "api/auth/login";
    private const string RegisterPath = "api/auth/register";
    private const string ProfilePath = "api/auth/me";
    private const string LogoutPath = "api/auth/logout";

    private readonly HttpClient _http;

    /// <summary>Создаёт клиент API с указанным <see cref="HttpClient"/>.</summary>
    public AuthApiClient(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc />
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        return await SendAsync<LoginRequest, LoginResponse>(LoginPath, request, null, ct);
    }

    /// <inheritdoc />
    public async Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        return await SendAsync<RegisterRequest, RegisterResponse>(RegisterPath, request, null, ct);
    }

    /// <inheritdoc />
    public async Task<UserProfileResponse> GetProfileAsync(string accessToken, CancellationToken ct = default)
    {
        return await SendAsync<object, UserProfileResponse>(ProfilePath, null, accessToken, ct);
    }

    /// <inheritdoc />
    public async Task LogoutAsync(string accessToken, string refreshToken, CancellationToken ct = default)
    {
        var body = new LogoutRequest { RefreshToken = refreshToken };
        await SendAsync(LogoutPath, body, accessToken, ct);
    }

    /// <summary>Отправляет запрос и возвращает успешный ответ; тело ответа не читается.</summary>
    private async Task SendAsync<TRequest>(
        string path,
        TRequest? body,
        string? accessToken,
        CancellationToken ct)
    {
        using var response = await SendCoreAsync(path, body, accessToken, ct);
    }

    /// <summary>Отправляет запрос и десериализует тело успешного ответа.</summary>
    private async Task<TResponse> SendAsync<TRequest, TResponse>(
        string path,
        TRequest? body,
        string? accessToken,
        CancellationToken ct)
    {
        using var response = await SendCoreAsync(path, body, accessToken, ct);

        var payload = await response.Content.ReadFromJsonAsync<TResponse>(ApiJson.Options, ct);
        if (payload is null)
        {
            throw new ApiException(response.StatusCode, "Сервер вернул пустой ответ.");
        }

        return payload;
    }

    /// <summary>
    /// Единственная точка выхода в сеть: формирует запрос, добавляет токен
    /// и превращает любой неуспешный статус или сетевой сбой в <see cref="ApiException"/>.
    /// Ответ обязан освободить вызывающий код.
    /// </summary>
    private async Task<HttpResponseMessage> SendCoreAsync<TRequest>(
        string path,
        TRequest? body,
        string? accessToken,
        CancellationToken ct)
    {
        using var request = new HttpRequestMessage(body is null ? HttpMethod.Get : HttpMethod.Post, path);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: ApiJson.Options);
        }

        if (!string.IsNullOrEmpty(accessToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            throw new ApiException(HttpStatusCode.ServiceUnavailable, "Сервер недоступен. Проверьте подключение к сети.");
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new ApiException(HttpStatusCode.RequestTimeout, "Сервер не ответил вовремя. Повторите попытку.");
        }

        if (response.IsSuccessStatusCode)
        {
            return response;
        }

        using (response)
        {
            throw new ApiException(response.StatusCode, await ReadErrorAsync(response, ct));
        }
    }

    /// <summary>
    /// Достаёт текст ошибки из тела ответа. API отвечает неоднородно:
    /// login присылает только <c>error</c>, а me — ещё и <c>message</c>.
    /// </summary>
    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var fallback = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Неверный email или пароль.",
            HttpStatusCode.Forbidden => "Доступ запрещён.",
            HttpStatusCode.NotFound => "Запрошенный ресурс не найден.",
            HttpStatusCode.TooManyRequests => "Слишком много запросов. Повторите позже.",
            >= HttpStatusCode.InternalServerError => "Ошибка сервера. Повторите попытку позже.",
            _ => $"Неожиданный ответ сервера ({(int)response.StatusCode}).",
        };

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(ApiJson.Options, ct);
            return error?.Message ?? error?.Error ?? fallback;
        }
        catch (JsonException)
        {
            return fallback;
        }
    }
}
