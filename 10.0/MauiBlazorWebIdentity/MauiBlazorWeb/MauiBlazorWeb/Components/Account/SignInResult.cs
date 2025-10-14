namespace MauiBlazorWeb.Components.Account;

/// <summary>
/// Represents the result of a sign-in operation.
/// </summary>
public class SignInResult
{
    private static readonly SignInResult _success = new SignInResult { Succeeded = true };
    private static readonly SignInResult _failed = new SignInResult();

    /// <summary>
    /// Returns a flag indication whether the sign-in was successful.
    /// </summary>
    /// <value>True if the sign-in was successful, otherwise false.</value>
    public bool Succeeded { get; protected set; }

    /// <summary>
    /// Returns a <see cref="SignInResult"/> that represents a successful sign-in.
    /// </summary>
    /// <returns>A <see cref="SignInResult"/> that represents a successful sign-in.</returns>
    public static SignInResult Success => _success;

    /// <summary>
    /// Returns a <see cref="SignInResult"/> that represents a failed sign-in.
    /// </summary>
    /// <returns>A <see cref="SignInResult"/> that represents a failed sign-in.</returns>
    public static SignInResult Failed => _failed;

    /// <summary>
    /// Converts the value of the current <see cref="SignInResult"/> object to its equivalent string representation.
    /// </summary>
    /// <returns>A string representation of value of the current <see cref="SignInResult"/> object.</returns>
    public override string ToString()
    {
        return Succeeded ? "Succeeded" : "Failed";
    }
}
