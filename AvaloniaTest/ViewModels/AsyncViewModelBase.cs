using AvaloniaTest.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AvaloniaTest.ViewModels;

/// <summary>
/// База для ViewModel, выполняющих асинхронные запросы.
/// Инкапсулирует две вещи, которые иначе дублировались бы в каждой модели:
/// флаг занятости и текст ошибки для пользователя.
/// </summary>
public abstract partial class AsyncViewModelBase : ViewModelBase
{
    /// <summary>Идёт ли сейчас запрос. Используется для блокировки кнопок и показа индикатора.</summary>
    [ObservableProperty]
    private bool _isBusy;

    /// <summary>Текст ошибки для показа пользователю; null, если ошибок нет.</summary>
    [ObservableProperty]
    private string? _errorMessage;

    /// <summary>
    /// Выполняет операцию в режиме «занято/ошибка/свободно».
    /// Повторный вызов во время выполнения игнорируется, чтобы два запроса не
    /// соревновались за одно состояние, а любая ошибка превращается в текст
    /// для пользователя вместо необработанного исключения в UI-потоке.
    /// </summary>
    protected async Task RunAsync(Func<CancellationToken, Task> action)
    {
        if (IsBusy)
        {
            return;
        }

        using var cts = new CancellationTokenSource();

        ErrorMessage = null;
        IsBusy = true;
        try
        {
            await action(cts.Token);
        }
        catch (OperationCanceledException)
        {
            // Отмена — штатный сценарий, а не ошибка, поэтому сообщение не выставляется.
        }
        catch (ApiException ex)
        {
            ErrorMessage = ex.Message;
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "Сервер недоступен. Проверьте подключение к сети.";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Непредвиденная ошибка: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
