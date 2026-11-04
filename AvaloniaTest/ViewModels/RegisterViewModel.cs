using AvaloniaTest.Dialogs;
using AvaloniaTest.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaTest.ViewModels;

/// <summary>
/// Модель диалога регистрации. Не создаёт окно сама: сообщает о результате
/// через <see cref="CloseRequested"/>, а закрывает окно DialogService.
/// </summary>
public partial class RegisterViewModel : AsyncViewModelBase
{
    private readonly IAuthService _authService;

    /// <summary>Создаёт модель регистрации.</summary>
    public RegisterViewModel(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Введённый email.</summary>
    [ObservableProperty]
    private string _email = string.Empty;

    /// <summary>Введённое имя.</summary>
    [ObservableProperty]
    private string _name = string.Empty;

    /// <summary>Введённый пароль.</summary>
    [ObservableProperty]
    private string _password = string.Empty;

    /// <summary>
    /// Поднимается, когда диалог пора закрыть.
    /// Значение null означает отмену, иначе — данные для автоматического входа.
    /// </summary>
    public event EventHandler<RegisterResult?>? CloseRequested;

    /// <summary>Регистрирует пользователя и запрашивает закрытие диалога с учётными данными.</summary>
    [RelayCommand]
    private Task SubmitAsync() => RunAsync(async ct =>
    {
        if (!IsValid())
        {
            return;
        }

        var email = Email.Trim();
        var name = Name.Trim();
        await _authService.RegisterAsync(email, Password, name, ct);
        CloseRequested?.Invoke(this, new RegisterResult(email, Password));
    });

    /// <summary>Закрывает диалог без регистрации.</summary>
    [RelayCommand]
    private void Cancel() => CloseRequested?.Invoke(this, null);

    /// <summary>Очищает поля формы, оставляя окно открытым.</summary>
    [RelayCommand]
    private void Reset()
    {
        Email = string.Empty;
        Name = string.Empty;
        Password = string.Empty;
        ErrorMessage = null;
    }

    private bool IsValid()
    {
        if (string.IsNullOrWhiteSpace(Email) || !Email.Contains('@', StringComparison.Ordinal))
        {
            ErrorMessage = "Введите корректный email.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(Name))
        {
            ErrorMessage = "Введите имя.";
            return false;
        }

        if (string.IsNullOrEmpty(Password) || Password.Length < 6)
        {
            ErrorMessage = "Пароль должен содержать не менее 6 символов.";
            return false;
        }

        return true;
    }
}
