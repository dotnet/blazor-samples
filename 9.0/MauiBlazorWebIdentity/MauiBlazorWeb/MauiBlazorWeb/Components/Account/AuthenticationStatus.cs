namespace MauiBlazorWeb.Components.Account;

public record AuthenticationStatus
{
    public static AuthenticationStatus None { get; } = new AuthenticationStatus();

    public static AuthenticationStatus Success { get; } = new SuccessAuthenticationStatus();

    public static AuthenticationStatus Failed(string errorMessage) => new FailedAuthenticationStatus(errorMessage);

    public bool IsSuccess => this is SuccessAuthenticationStatus;

    public bool IsFailed => this is FailedAuthenticationStatus;

    public string? GetErrorMessage() =>
        this is FailedAuthenticationStatus failed ? failed.ErrorMessage : null;

    private record SuccessAuthenticationStatus : AuthenticationStatus;

    private record FailedAuthenticationStatus(string ErrorMessage) : AuthenticationStatus;
}
