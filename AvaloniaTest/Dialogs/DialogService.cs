using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using AvaloniaTest.ViewModels;
using AvaloniaTest.Views;

namespace AvaloniaTest.Dialogs;

/// <summary>
/// Адаптер <see cref="IDialogService"/>: единственное место приложения,
/// которому разрешено создавать окна. ViewModel через него получает только результат.
/// </summary>
public sealed class DialogService : IDialogService
{
    private readonly Func<RegisterViewModel> _registerViewModelFactory;

    /// <summary>Создаёт сервис диалогов с фабрикой модели регистрации.</summary>
    public DialogService(Func<RegisterViewModel> registerViewModelFactory)
    {
        _registerViewModelFactory = registerViewModelFactory;
    }

    /// <inheritdoc />
    public async Task<RegisterResult?> ShowRegisterAsync()
    {
        var owner = GetActiveWindow();
        if (owner is null)
        {
            return null;
        }

        var viewModel = _registerViewModelFactory();
        var window = new RegisterWindow { DataContext = viewModel };

        RegisterResult? result = null;
        viewModel.CloseRequested += (_, value) =>
        {
            result = value;
            window.Close();
        };

        await window.ShowDialog(owner);

        // Если окно закрыли системной кнопкой, CloseRequested не сработал — это отмена.
        return result;
    }

    private static Window? GetActiveWindow()
    {
        if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return null;
        }

        return desktop.Windows.FirstOrDefault(window => window.IsActive) ?? desktop.MainWindow;
    }
}
