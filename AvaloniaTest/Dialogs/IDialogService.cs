namespace AvaloniaTest.Dialogs;

/// <summary>
/// Порт показа модальных окон. ViewModel не имеет права создавать Window —
/// иначе нарушается MVVM и класс перестаёт быть тестируемым,
/// поэтому доступ к UI спрятан за этим интерфейсом.
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Открывает диалог регистрации и ждёт его закрытия.
    /// Возвращает null, если пользователь отменил ввод.
    /// </summary>
    Task<RegisterResult?> ShowRegisterAsync();
}
