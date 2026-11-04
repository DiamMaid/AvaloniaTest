using AvaloniaTest.Navigation;

namespace AvaloniaTest.ViewModels;

/// <summary>
/// Раздел View_2: сведения о разработчике приложения.
/// Данные статичны, поэтому раздел не выполняет запросов и доступен без авторизации.
/// </summary>
public sealed class AboutViewModel : ViewModelBase, ISectionViewModel
{
    /// <inheritdoc />
    public string Title => "О разработчике";

    /// <inheritdoc />
    public string Icon => "\ue946";

    /// <summary>Имя разработчика.</summary>
    public string DeveloperName { get; } = "Dmitry Khoraskin";

    /// <summary>Специализация.</summary>
    public string Specialization { get; } = ".NET Developer";

    /// <summary>Профиль на GitHub.</summary>
    public string GitHub { get; } = "https://github.com/DiamMaid";

    /// <summary>Контактный email.</summary>
    public string Email { get; } = "dima.khoraskin@gmail.com";

    /// <summary>Технологии, применённые в этом приложении.</summary>
    public string Technologies { get; } =
        "C# 14, .NET 10, Avalonia UI 12, MVVM (CommunityToolkit.Mvvm), " +
        "Microsoft.Extensions.DependencyInjection, System.Net.Http, System.Text.Json";

    /// <summary>Короткое описание подхода к реализации.</summary>
    public string Summary { get; } =
        "Приложение построено на разделении View / ViewModel / Model и портах-адаптерах: " +
        "ViewModel зависят от интерфейсов сервисов, а конкретные реализации на HttpClient " +
        "регистрируются в контейнере в одной точке — композиционном корне App.axaml.cs.";

    /// <inheritdoc />
    public Task ActivateAsync() => Task.CompletedTask;
}
