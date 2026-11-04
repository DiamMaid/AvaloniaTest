using AvaloniaTest.Dialogs;
using AvaloniaTest.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaTest.ViewModels;

/// <summary>
/// Блок AuthView: форма входа, кнопка регистрации и кнопка выхода.
/// Одно и то же состояние используется и неавторизованным, и авторизованным видом —
/// переключение выполняет привязка к <see cref="IsAuthenticated"/>.
/// </summary>
public partial class AuthViewModel : AsyncViewModelBase
{
    private readonly IAuthService _authService;
    private readonly IDialogService _dialogService;

    /// <summary>Создаёт модель авторизации и подписывается на изменения сессии.</summary>
    public AuthViewModel(IAuthService authService, IDialogService dialogService)
    {
        _authService = authService;
        _dialogService = dialogService;
        _isAuthenticated = authService.Session.IsAuthenticated;
        _currentUserEmail = authService.Session.CurrentUser?.Email;
        _authService.Session.SessionChanged += OnSessionChanged;
    }

    /// <summary>Введённый email.</summary>
    [ObservableProperty]
    private string _email = string.Empty;

    /// <summary>Введённый пароль. Очищается после успешного входа.</summary>
    [ObservableProperty]
    private string _password = string.Empty;

    /// <summary>Авторизован ли пользователь. Управляет видимостью формы входа и кнопки выхода.</summary>
    [ObservableProperty]
    private bool _isAuthenticated;

    /// <summary>Email текущего пользователя для подписи кнопки выхода.</summary>
    [ObservableProperty]
    private string? _currentUserEmail;

    /// <summary>Вход по email и паролю.</summary>
    [RelayCommand]
    private Task LoginAsync() => RunAsync(async ct =>
    {
        if (!IsFormFilled())
        {
            return;
        }

        await _authService.LoginAsync(Email.Trim(), Password, ct);
        Password = string.Empty;
    });

    /// <summary>
    /// Открывает диалог регистрации и выполняет автоматический вход.
    /// API не выдаёт токенов при регистрации, поэтому вход — отдельный второй запрос.
    /// </summary>
    [RelayCommand]
    private async Task RegisterAsync()
    {
        var result = await _dialogService.ShowRegisterAsync();
        if (result is null)
        {
            return;
        }

        Email = result.Email;
        await RunAsync(async ct =>
        {
            await _authService.LoginAsync(result.Email, result.Password, ct);
            Password = string.Empty;
        });
    }

    /// <summary>Выход: завершает сессию на сервере и очищает локальное состояние.</summary>
    [RelayCommand]
    private Task LogoutAsync() => RunAsync(ct => _authService.LogoutAsync(ct));

    private bool IsFormFilled()
    {
        if (!string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(Password))
        {
            return true;
        }

        ErrorMessage = "Введите email и пароль.";
        return false;
    }

    private void OnSessionChanged(object? sender, EventArgs e)
    {
        IsAuthenticated = _authService.Session.IsAuthenticated;
        CurrentUserEmail = _authService.Session.CurrentUser?.Email;
    }
}
