namespace AvaloniaTest.Dialogs;

/// <summary>
/// Результат диалога регистрации: данные, необходимые для автоматического входа.
/// Null возвращается, если пользователь закрыл окно без сохранения.
/// </summary>
/// <param name="Email">Email зарегистрированного пользователя.</param>
/// <param name="Password">Пароль, указанный при регистрации.</param>
public sealed record RegisterResult(string Email, string Password);
