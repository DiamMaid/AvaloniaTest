# AvaloniaTest

Тестовое задание WCL на позицию `.NET frontend developer (AvaloniaUI)`: десктопное приложение на Avalonia UI и .NET 10 с авторизацией и регистрацией через REST API. Архитектура — MVVM с CommunityToolkit.Mvvm, внедрением зависимостей через Microsoft.Extensions.DependencyInjection и разделением View / ViewModel / Model.

Основной инженерный интерес в этом задании представляет не форма логина, а поведение внешнего mock-API: его реальный контракт расходится с официальной страницей `/docs`, и это продиктовало несколько неочевидных решений, описанных ниже.

## Возможности

- Авторизация по email и паролю через `POST /api/auth/login`.
- Регистрация в отдельном модальном окне (`RegisterWindow`) с автоматическим входом после успешного создания аккаунта.
- Выход из сессии через `POST /api/auth/logout` с отзывом токена на сервере.
- Блок `AuthView` в главном окне: при отсутствии сессии показывает форму входа и кнопку регистрации, при активной сессии — только кнопку выхода.
- Раздел `View_1` (профиль): без сессии — просьба войти или зарегистрироваться; с сессией — id, email, имя, аватар и роль пользователя.
- Раздел `View_2` (об авторе): статическая информация о разработчике.
- Переключение разделов через меню в `MainViewModel` без обращения к UI из code-behind; набор разделов расширяется без изменения `MainViewModel`.
- Все сетевые вызовы асинхронные, с обработкой ошибок API и отображением сообщения пользователю.
- Базовое оформление вынесено в общий словарь стилей `Styles/AppStyles.axaml`.

## Стек и обоснование NuGet-пакетов

| Пакет | Версия | Зачем нужен |
| --- | --- | --- |
| `Avalonia` | 12.1.2 | Сам UI-фреймворк: базовые типы, привязки, система стилей. Требуется заданием. |
| `Avalonia.Desktop` | 12.1.2 | Интеграция с десктопными платформами; даёт `ClassicDesktopStyleApplicationLifetime`, который используется в `Program.cs`. |
| `Avalonia.Themes.Fluent` | 12.1.2 | Тема Fluent — стили всех стандартных контролов. Без неё приложение рисуется без оформления. |
| `Avalonia.Fonts.Inter` | 12.1.2 | Встраивает шрифт Inter, чтобы текст выглядел одинаково на машинах, где этот шрифт не установлен. |
| `AvaloniaUI.DiagnosticsSupport` | 2.2.3 | Отладочная диагностика (DevTools). Подключена только для конфигурации Debug — в `.csproj` для остальных конфигураций заданы `IncludeAssets=None` и `PrivateAssets=All`. |
| `CommunityToolkit.Mvvm` | 8.4.2 | Прямо требуется заданием. Даёт `ObservableObject` и генераторы `[ObservableProperty]` / `[RelayCommand]`, что убирает boilerplate-реализацию `INotifyPropertyChanged` и ручные обёртки команд. |
| `Microsoft.Extensions.DependencyInjection` | 10.0.14 | DI-контейнер, предложенный заданием. На нём построен корень композиции в `App.axaml.cs`. |

### Что сознательно НЕ добавлялось

- `Microsoft.Extensions.Http` (фабрика `HttpClient`) — в приложении один `HttpClient`-адаптер с единственным базовым адресом; фабрика не решает здесь ни одной задачи, но добавляет зависимость.
- `Newtonsoft.Json` — достаточно встроенного `System.Text.Json`, который к тому же поддерживает `JsonNamingPolicy.SnakeCaseLower`.
- `AsyncImageLoader.Avalonia` — загрузка удалённого аватара реализована вручную на `HttpClient` + `Avalonia.Media.Imaging.Bitmap`.
- Навигационные фреймворки (`ReactiveUI`, Prism и т. п.) — переключение разделов решается привязкой `ContentControl` к `CurrentSection`, чего достаточно для двух разделов и не противоречит MVVM.

Итог: минимальная поверхность зависимостей — только то, что требует задание, и ничего, что дублируется встроенными API .NET.

## Структура проекта

```
AvaloniaTest/
  Models/               — User.cs, доменная модель пользователя
  Models/Api/           — DTO: LoginRequest, LoginResponse, LoginUser, RegisterRequest,
                          RegisterResponse, UserProfileResponse, LogoutRequest, ApiErrorResponse
  Services/             — порты: IAuthApiClient, ISessionStore, IAuthService, IAvatarService;
                          адаптеры: AuthApiClient, SessionStore, AuthService, AvatarService;
                          ApiJson, ApiException, ServiceCollectionExtensions
  Navigation/           — ISectionViewModel
  Dialogs/              — RegisterResult, IDialogService, DialogService
  ViewModels/           — ViewModelBase, AsyncViewModelBase, MainViewModel, AuthViewModel,
                          RegisterViewModel, ProfileViewModel, AboutViewModel
  Views/                — MainWindow, AuthView, RegisterWindow, ProfileView, AboutView
  Styles/               — AppStyles.axaml
  App.axaml, App.axaml.cs, Program.cs, ViewLocator.cs
```

Проект построен по схеме «порты и адаптеры»:

- **Порты** — интерфейсы `IAuthApiClient`, `ISessionStore`, `IAuthService`, `IAvatarService`, `IDialogService`. ViewModel знают только их.
- **Адаптеры** — реализации поверх `HttpClient` и локального состояния (`AuthApiClient`, `SessionStore`, `AuthService`, `AvatarService`, `DialogService`).
- **Корень композиции** — `App.axaml.cs`, где регистрируются все сервисы и создаётся `MainWindow` вместе с `MainViewModel`.

Такое разделение позволяет подменить любой сервис при тестировании и не тащит знания о HTTP в слой представления.

### Ключевые решения

1. **Автоматический вход после регистрации — двумя вызовами.** `register` не возвращает токенов, поэтому диалог регистрации возвращает введённые учётные данные (`RegisterResult`), а вызывающая сторона выполняет `login`.
2. **Профиль требует дополнительного `GET /api/auth/me`.** Поле `avatar` есть только в ответе `/me`, в объекте `user` из `login` его нет.
3. **`refresh_token` хранится в сессии**, хотя само обновление токенов в задание не входит: без тела `{"refresh_token": "..."}` сервер отвечает `500` на logout.
4. **Аватар грузится вручную.** `Image.Source` принимает `IImage`, а не строку, поэтому байты скачиваются через `HttpClient` и оборачиваются в `Avalonia.Media.Imaging.Bitmap`.
5. **Единая сериализация.** API отвечает в `snake_case`, C# использует `PascalCase`. В `ApiJson` задан один общий `JsonSerializerOptions` с `JsonNamingPolicy.SnakeCaseLower`, поэтому ни одному DTO не нужны атрибуты `[JsonPropertyName]`.
6. **Единственный источник правды о сессии — `ISessionStore`.** Все ViewModel подписаны на событие `SessionChanged`, благодаря чему состояние авторизации синхронно расходится в `AuthView`, `View_1` и `View_2`.
7. **Разделы находятся через DI как `IEnumerable<ISectionViewModel>`.** Новый раздел — это новая ViewModel, зарегистрированная в контейнере; `MainViewModel` менять не нужно (принцип открытости/закрытости).

## Контракт API и его особенности

Базовый адрес: `https://fakeapifordevs.vercel.app`. Фактический контракт установлен живыми запросами 2026-11-04 и **отличается от страницы `/docs`**, которая описывает generic-песочницу и не документирует эти формы ответов.

| Запрос | Тело / заголовки | Успешный ответ |
| --- | --- | --- |
| `POST /api/auth/register` | `{email, password, name}` | `201` `{id, email, name, role, status, created_at, onboarding_checklist}` — **токенов нет** |
| `POST /api/auth/login` | `{email, password}` | `200` `{access_token (JWT), refresh_token, token_type: "Bearer", expires_in: 900, user: {id, email, name, role}}` |
| `GET /api/auth/me` | `Authorization: Bearer <access_token>` | `200` `{id, email, name, role, avatar, created_at, preferences, security}`; без заголовка — `401` `{"error": "unauthorized", "message": "Authentication required."}` |
| `POST /api/auth/logout` | `Authorization: Bearer <access_token>` **и** тело `{"refresh_token": "..."}` | `{"success": true, "message": "Session revoked."}`; без тела — `500 internal_server_error` |

Следствия для клиентского кода:

- **Формы ошибок неоднородны.** `login` возвращает только `{"error": ...}`, а `me` — `{"error": ..., "message": ...}`. Поэтому `ApiErrorResponse.Message` объявлен как nullable, и вывод сообщения учитывает оба случая.
- **Проверки пароля фактически нет — войти может любой.** Сервер не сверяет ни существование учётной записи, ни содержимое пароля, ни формат email (`a@b.c` проходит). Единственное, что он вообще проверяет, — длину пароля: замерено перебором, что 1–5 символов дают `401 {"error": "Invalid email or password"}`, 6 и больше — `200`, пустой пароль — `400 {"error": "Email and password are required"}`. Повторная регистрация того же email возвращает `201` с новым id — проверки на дубликат нет. Вывод для кода: ветку `401` обработать необходимо и она достижима коротким паролем, но сценарий «неверный пароль у существующего пользователя» сервером не воспроизводится в принципе.
- **При успешном входе сервер всегда возвращает одно и то же демо-лицо:** `usr_120000`, `hayden.zaman.414@fakeapifordevs.com`, `Hayden Zaman`, роль `admin` — независимо от того, какой аккаунт был зарегистрирован. `GET /me` отдаёт ту же личность. Экран профиля поэтому всегда показывает Hayden Zaman: **это поведение API, а не ошибка приложения.**
- **Параметр `?delay=N` из документации не работает** (замер 0.35 с против 0.40 с), поэтому смоделировать сетевую задержку на стороне сервера нельзя.
- **`avatar` — абсолютный URL** вида `https://avatar.vercel.sh/...`, отдающий `200 image/png`.

## Как запустить

Требуется установленный .NET 10 SDK.

```bash
dotnet run --project AvaloniaTest
```

Команда выполняется из корня репозитория. Приложение работает с живым публичным mock-API, поэтому **нужно подключение к интернету**; без сети авторизация и загрузка профиля завершатся ошибкой, которая будет показана в интерфейсе.

## Об авторе

| | |
| --- | --- |
| Имя | Dmitry Khoraskin |
| Специализация | .NET Developer |
| GitHub | [DiamMaid](https://github.com/DiamMaid) |
| Email | dima.khoraskin@gmail.com |
| Стек | C# 14, .NET 10, Avalonia UI 12, MVVM (CommunityToolkit.Mvvm), Microsoft.Extensions.DependencyInjection, System.Net.Http, System.Text.Json |

Раздел `View_2` в приложении показывает эти же данные; источник — `ViewModels/AboutViewModel.cs`, править нужно там.
