using System.ComponentModel.DataAnnotations;

namespace MauiBlazorWeb.Models;

public sealed class RegisterModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), MinLength(6)] public string Password { get; set; } = string.Empty;
}

public sealed class ResetPasswordModel
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string ResetCode { get; set; } = string.Empty;
    [Required, DataType(DataType.Password), MinLength(6)] public string NewPassword { get; set; } = string.Empty;
}

public sealed record AccountInfo(string? Email, bool IsEmailConfirmed, string? PhoneNumber, bool HasPassword,
    bool IsTwoFactorEnabled, bool HasAuthenticator, int RecoveryCodesLeft, int PasskeyCount, string[] ExternalLoginProviders);
public sealed record TwoFactorStatus(bool IsTwoFactorEnabled, bool HasAuthenticator, int RecoveryCodesLeft, bool IsMachineRemembered);
public sealed record TwoFactorSetup(string? SharedKey, string[]? RecoveryCodes, int RecoveryCodesLeft, bool IsTwoFactorEnabled, bool IsMachineRemembered);
public sealed record Passkey(string CredentialId, string? Name, DateTimeOffset CreatedAt, bool IsUserVerified, bool IsBackedUp);
public sealed record PersonalData(string UserId, string? UserName, string? Email, string? PhoneNumber, bool IsEmailConfirmed, bool IsTwoFactorEnabled);
public sealed record ExternalLogin(string Provider, string? DisplayName);
