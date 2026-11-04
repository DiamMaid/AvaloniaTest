using AvaloniaTest.Models;

namespace AvaloniaTest.Services;

/// <summary>
/// Хранилище состояния сессии в памяти процесса.
/// Регистрируется как singleton, чтобы все ViewModel читали и меняли одно и то же состояние.
/// Все изменения происходят в потоке UI, поэтому дополнительная синхронизация не требуется.
/// </summary>
public sealed class SessionStore : ISessionStore
{
    /// <inheritdoc />
    public bool IsAuthenticated => CurrentUser is not null && !string.IsNullOrEmpty(AccessToken);

    /// <inheritdoc />
    public string? AccessToken { get; private set; }

    /// <inheritdoc />
    public string? RefreshToken { get; private set; }

    /// <inheritdoc />
    public User? CurrentUser { get; private set; }

    /// <inheritdoc />
    public event EventHandler? SessionChanged;

    /// <inheritdoc />
    public void SignIn(string accessToken, string refreshToken, User user)
    {
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        CurrentUser = user;

        OnSessionChanged();
    }

    /// <inheritdoc />
    public void UpdateUser(User user)
    {
        CurrentUser = user;

        OnSessionChanged();
    }

    /// <inheritdoc />
    public void SignOut()
    {
        AccessToken = null;
        RefreshToken = null;
        CurrentUser = null;

        OnSessionChanged();
    }

    /// <summary>Оповещает подписчиков об изменении состояния сессии.</summary>
    private void OnSessionChanged()
    {
        var handler = SessionChanged;
        if (handler is not null)
        {
            handler(this, EventArgs.Empty);
        }
    }
}
