using Avalonia.Media;
using AvaloniaTest.Models;
using AvaloniaTest.Navigation;
using AvaloniaTest.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaTest.ViewModels;

/// <summary>
/// Раздел View_1: профиль пользователя. Без активной сессии показывает просьбу
/// авторизоваться, с активной — id, email, имя, роль и аватар.
/// </summary>
public partial class ProfileViewModel : AsyncViewModelBase, ISectionViewModel
{
    private readonly IAuthService _authService;
    private readonly IAvatarService _avatarService;

    /// <summary>Создаёт раздел профиля и подписывается на изменения сессии.</summary>
    public ProfileViewModel(IAuthService authService, IAvatarService avatarService)
    {
        _authService = authService;
        _avatarService = avatarService;
        _user = authService.Session.CurrentUser;
        _authService.Session.SessionChanged += OnSessionChanged;
    }

    /// <inheritdoc />
    public string Title => "Профиль";

    /// <inheritdoc />
    public string Icon => "\ue77b";

    /// <summary>Данные текущего пользователя; null, когда сессии нет.</summary>
    [ObservableProperty]
    private User? _user;

    /// <summary>Загруженный аватар; null, пока картинка не получена.</summary>
    [ObservableProperty]
    private IImage? _avatar;

    /// <summary>Показывает ли раздел данные пользователя или просьбу авторизоваться.</summary>
    public bool IsAuthenticated => User is not null;

    /// <inheritdoc />
    public Task ActivateAsync()
    {
        var current = _authService.Session.CurrentUser;
        if (!_authService.Session.IsAuthenticated || current is null)
        {
            User = null;
            Avatar = null;
            return Task.CompletedTask;
        }

        User = current;
        return RunAsync(ct => LoadProfileAndAvatarAsync(current, ct));
    }

    /// <summary>Повторно загружает профиль с сервера.</summary>
    [RelayCommand]
    private Task RefreshAsync() => ActivateAsync();

    // Свойство вычисляемое, поэтому о его изменении нужно сообщать явно,
    // иначе привязка в ProfileView не узнает о смене состояния.
    partial void OnUserChanged(User? value) => OnPropertyChanged(nameof(IsAuthenticated));

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        var current = _authService.Session.CurrentUser;
        if (ReferenceEquals(current, User))
        {
            return;
        }

        User = current;
        Avatar = null;
        if (current is not null)
        {
            _ = RunAsync(ct => LoadAvatarAsync(current, ct));
        }
    }

    /// <summary>
    /// Обновляет профиль и догружает аватар одной операцией. UpdateUser внутри
    /// LoadProfileAsync поднимет SessionChanged, но повторной загрузки аватара не будет:
    /// OnSessionChanged в этот момент увидит занятый RunAsync и его вызов игнорируется.
    /// </summary>
    private async Task LoadProfileAndAvatarAsync(User user, CancellationToken ct)
    {
        await _authService.LoadProfileAsync(ct);

        var refreshed = _authService.Session.CurrentUser ?? user;
        Avatar = await _avatarService.LoadAsync(refreshed.AvatarUrl, ct);
    }

    private async Task LoadAvatarAsync(User user, CancellationToken ct)
    {
        Avatar = await _avatarService.LoadAsync(user.AvatarUrl, ct);
    }
}
