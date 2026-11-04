using AvaloniaTest.Dialogs;
using AvaloniaTest.Navigation;
using AvaloniaTest.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace AvaloniaTest.Services;

/// <summary>
/// Регистрация зависимостей: единственное место, где порты связываются с адаптерами.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string ApiBaseUrl = "https://fakeapifordevs.vercel.app/";

    private static readonly TimeSpan HttpTimeout = TimeSpan.FromSeconds(20);

    /// <summary>Добавляет в контейнер сервисы, разделы и ViewModel приложения.</summary>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // HttpClient регистрируется как singleton: сокет не исчерпывается при одном
        // долгоживущем клиенте, а аватар запрашивается по абсолютному URL, который
        // переопределяет BaseAddress.
        services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(ApiBaseUrl),
            Timeout = HttpTimeout,
        });

        services.AddSingleton<IAuthApiClient, AuthApiClient>();
        services.AddSingleton<ISessionStore, SessionStore>();
        services.AddSingleton<IAuthService, AuthService>();
        services.AddSingleton<IAvatarService, AvatarService>();
        services.AddSingleton<IDialogService, DialogService>();

        services.AddTransient<RegisterViewModel>();
        services.AddSingleton<Func<RegisterViewModel>>(
            provider => () => provider.GetRequiredService<RegisterViewModel>());

        services.AddSingleton<AuthViewModel>();

        // Порядок регистрации задаёт порядок разделов в меню: View_1, затем View_2.
        services.AddSingleton<ISectionViewModel, ProfileViewModel>();
        services.AddSingleton<ISectionViewModel, AboutViewModel>();

        services.AddSingleton<MainViewModel>();

        return services;
    }
}
