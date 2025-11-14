namespace MauiBlazorWeb.Components.Account;

/// <summary>
/// Provides the APIs for user sign in.
/// </summary>
public interface ISignInManager
{
    /// <summary>
    /// Attempts to sign in the specified <paramref name="userName"/> and <paramref name="password"/> combination
    /// as an asynchronous operation.
    /// </summary>
    /// <param name="userName">The user name to sign in.</param>
    /// <param name="password">The password to attempt to sign in with.</param>
    /// <param name="isPersistent">Flag indicating whether the sign-in cookie should persist after the browser is closed.</param>
    /// <returns>The task object representing the asynchronous operation containing the <see cref="SignInResult"/>
    /// for the sign-in attempt.</returns>
    Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent);

    /// <summary>
    /// Signs the current user out of the application.
    /// </summary>
    Task SignOutAsync();
}
