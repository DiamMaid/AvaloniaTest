using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AvaloniaTest.Services;

/// <summary>
/// Загружает аватар по HTTP и оборачивает его в <see cref="Bitmap"/> —
/// единственное представление картинки, которое принимает <c>Image.Source</c>.
/// </summary>
public sealed class AvatarService : IAvatarService
{
    private readonly HttpClient _http;

    /// <summary>Создаёт сервис загрузки аватаров.</summary>
    public AvatarService(HttpClient http)
    {
        _http = http;
    }

    /// <inheritdoc />
    public async Task<IImage?> LoadAsync(string? url, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return null;
        }

        try
        {
            // Хост аватаров (https://avatar.vercel.sh/...) отличается от BaseAddress API,
            // но абсолютный URI перекрывает BaseAddress — запрос уходит на нужный хост.
            // Байты читаются целиком: декодеру изображения нужен поток с произвольным
            // доступом, а сетевой поток GetStreamAsync его не гарантирует.
            var bytes = await _http.GetByteArrayAsync(url, ct);
            using var stream = new MemoryStream(bytes);
            return new Bitmap(stream);
        }
        catch (Exception)
        {
            // Аватар не должен ронять экран профиля: любая ошибка загрузки означает «без картинки».
            return null;
        }
    }
}
