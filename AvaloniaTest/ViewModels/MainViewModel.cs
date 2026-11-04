using AvaloniaTest.Navigation;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AvaloniaTest.ViewModels;

/// <summary>
/// Корневая ViewModel: держит список разделов, активный раздел и блок авторизации.
/// Разделы приходят из контейнера как <see cref="IEnumerable{T}"/>, поэтому добавление
/// нового раздела не требует правки этого класса.
/// </summary>
public partial class MainViewModel : ViewModelBase
{
    /// <summary>Создаёт главную ViewModel из набора разделов и модели авторизации.</summary>
    public MainViewModel(IEnumerable<ISectionViewModel> sections, AuthViewModel authViewModel)
    {
        Sections = sections.ToList();
        Auth = authViewModel;

        // Присваивается свойство, а не поле: стартовый раздел тоже должен пройти
        // через OnCurrentSectionChanged и выполнить свою ленивую загрузку.
        CurrentSection = Sections.FirstOrDefault();
    }

    /// <summary>Разделы приложения в порядке регистрации в контейнере.</summary>
    public IReadOnlyList<ISectionViewModel> Sections { get; }

    /// <summary>Модель блока авторизации, общая для всех разделов.</summary>
    public AuthViewModel Auth { get; }

    /// <summary>Активный раздел; его содержимое показывает центральная область окна.</summary>
    [ObservableProperty]
    private ISectionViewModel? _currentSection;

    /// <summary>Выбор раздела из меню. Вся навигация проходит через это свойство, без кода в View.</summary>
    [RelayCommand]
    private void Navigate(ISectionViewModel? section)
    {
        if (section is not null)
        {
            CurrentSection = section;
        }
    }

    partial void OnCurrentSectionChanged(ISectionViewModel? value)
    {
        // ActivateAsync по контракту не бросает: раздел сам складывает ошибку в ErrorMessage,
        // поэтому ожидание здесь не требуется и не блокирует поток UI.
        if (value is not null)
        {
            _ = value.ActivateAsync();
        }
    }
}
