namespace AvaloniaTest.Navigation;

/// <summary>
/// Контракт раздела приложения. MainViewModel получает из контейнера
/// <c>IEnumerable&lt;ISectionViewModel&gt;</c>, поэтому добавление нового раздела
/// не требует правки MainViewModel: достаточно нового ViewModel, новой View
/// и одной строки регистрации в DI (принцип открытости/закрытости).
/// </summary>
public interface ISectionViewModel
{
    /// <summary>Название раздела в меню.</summary>
    string Title { get; }

    /// <summary>Код глифа из шрифта Segoe Fluent Icons для меню.</summary>
    string Icon { get; }

    /// <summary>Вызывается при выборе раздела; здесь выполняется ленивая загрузка данных.</summary>
    Task ActivateAsync();
}
