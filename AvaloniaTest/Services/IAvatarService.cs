using Avalonia.Media;

namespace AvaloniaTest.Services;

/// <summary>
/// Порт загрузки аватара. Avalonia не умеет получать картинку по URL сама:
/// <c>Image.Source</c> принимает <see cref="IImage"/>, а не строку,
/// поэтому байты приходится скачивать и оборачивать в Bitmap вручную.
/// </summary>
public interface IAvatarService
{
    /// <summary>
    /// Скачивает изображение по URL. Возвращает null при пустом URL
    /// или любой ошибке загрузки — аватар не должен ронять экран профиля.
    /// </summary>
    Task<IImage?> LoadAsync(string? url, CancellationToken ct = default);
}
