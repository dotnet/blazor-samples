using System.Net.Http.Headers;
using System.Net.Http.Json;
using MauiBlazorWeb.Models;

namespace MauiBlazorWeb.Services;

/// <summary>Typed REST client for the server's Identity endpoints. It never persists or logs request secrets.</summary>
public sealed class AccountClient(MauiAuthenticationStateProvider authentication)
{
    public Task<ApiResult> RegisterAsync(RegisterModel model) =>
        SendAsync(HttpMethod.Post, "identity/register", new { model.Email, model.Password });

    public Task<ApiResult> ResendConfirmationAsync(string email) =>
        SendAsync(HttpMethod.Post, "identity/resendConfirmationEmail", new { email });

    public Task<ApiResult> ForgotPasswordAsync(string email) =>
        SendAsync(HttpMethod.Post, "identity/forgotPassword", new { email });

    public Task<ApiResult> ResetPasswordAsync(ResetPasswordModel model) =>
        SendAsync(HttpMethod.Post, "identity/resetPassword", new { model.Email, model.ResetCode, model.NewPassword });

    public Task<ApiResult<AccountInfo>> GetInfoAsync() => SendAuthorizedAsync<AccountInfo>(HttpMethod.Get, "identity-overrides/manage/info");
    public Task<ApiResult<TwoFactorStatus>> GetTwoFactorAsync() => SendAuthorizedAsync<TwoFactorStatus>(HttpMethod.Get, "identity-overrides/manage/2fa");
    public Task<ApiResult<Passkey[]>> GetPasskeysAsync() => SendAuthorizedAsync<Passkey[]>(HttpMethod.Get, "identity/manage/passkeys");
    public Task<ApiResult<PersonalData>> GetPersonalDataAsync() => SendAuthorizedAsync<PersonalData>(HttpMethod.Get, "identity/manage/personal-data");
    public Task<ApiResult<ExternalLogin[]>> GetExternalLoginsAsync() => SendAuthorizedAsync<ExternalLogin[]>(HttpMethod.Get, "identity/manage/external-logins");
    public Task<ApiResult<string>> BeginPasskeyRegistrationAsync() => SendAuthorizedTextAsync(HttpMethod.Post, "identity/passkeys/register/begin");

    public Task<ApiResult> UpdateEmailAsync(string email) => SendAuthorizedAsync(HttpMethod.Post, "identity-overrides/manage/info", new { newEmail = email });
    public Task<ApiResult> UpdatePhoneAsync(string phone) => SendAuthorizedAsync(HttpMethod.Post, "identity-overrides/manage/info", new { phoneNumber = phone });
    public Task<ApiResult> UpdatePasswordAsync(string oldPassword, string newPassword) =>
        SendAuthorizedAsync(HttpMethod.Post, "identity-overrides/manage/info", new { oldPassword, newPassword });
    public Task<ApiResult<TwoFactorSetup>> BeginAuthenticatorSetupAsync() =>
        SendAuthorizedAsync<TwoFactorSetup>(HttpMethod.Post, "identity/manage/2fa", new { });
    public Task<ApiResult<TwoFactorSetup>> ConfigureTwoFactorAsync(bool enable, bool resetSharedKey, bool resetRecoveryCodes, bool forgetMachine, string? twoFactorCode) =>
        SendAuthorizedAsync<TwoFactorSetup>(HttpMethod.Post, "identity/manage/2fa", new { enable, resetSharedKey, resetRecoveryCodes, forgetMachine, twoFactorCode });
    public Task<ApiResult> RenamePasskeyAsync(string credentialId, string name) =>
        SendAuthorizedAsync(HttpMethod.Patch, $"identity/manage/passkeys/{Uri.EscapeDataString(credentialId)}", new { name });
    public Task<ApiResult> DeletePasskeyAsync(string credentialId) =>
        SendAuthorizedAsync(HttpMethod.Delete, $"identity/manage/passkeys/{Uri.EscapeDataString(credentialId)}");
    public Task<ApiResult> DeleteExternalLoginAsync(string provider) =>
        SendAuthorizedAsync(HttpMethod.Delete, $"identity/manage/external-logins/{Uri.EscapeDataString(provider)}");
    public Task<ApiResult> LogoutAllAsync() => SendAuthorizedAsync(HttpMethod.Post, "identity/manage/logout-all");
    public Task<ApiResult> DeleteAccountAsync(string currentPassword) =>
        SendAuthorizedAsync(HttpMethod.Delete, "identity/manage/account", new { currentPassword });
    private async Task<ApiResult> SendAsync(HttpMethod method, string route, object? body = null) =>
        (await SendCoreAsync<object>(method, route, body, false)).WithoutValue();
    private async Task<ApiResult> SendAuthorizedAsync(HttpMethod method, string route, object? body = null) =>
        (await SendCoreAsync<object>(method, route, body, true)).WithoutValue();
    private Task<ApiResult<T>> SendAuthorizedAsync<T>(HttpMethod method, string route, object? body = null) =>
        SendCoreAsync<T>(method, route, body, true);
    private Task<ApiResult<string>> SendAuthorizedTextAsync(HttpMethod method, string route) =>
        SendTextCoreAsync(method, route, true);

    private async Task<ApiResult<T>> SendCoreAsync<T>(HttpMethod method, string route, object? body, bool authorized)
    {
        using var request = new HttpRequestMessage(method, new Uri(new Uri(HttpClientHelper.BaseUrl), route));
        if (body is not null) request.Content = JsonContent.Create(body);
        if (!await AddAuthorizationAsync(request, authorized)) return ApiResult<T>.Failure("Your session has expired. Please sign in again.");
        try
        {
            using var client = HttpClientHelper.GetHttpClient();
            using var response = await client.SendAsync(request);
            if (!response.IsSuccessStatusCode) return ApiResult<T>.Failure(await ErrorAsync(response));
            if (typeof(T) == typeof(object)) return ApiResult<T>.Success(default);
            return ApiResult<T>.Success(await response.Content.ReadFromJsonAsync<T>());
        }
        catch (HttpRequestException) { return ApiResult<T>.Failure("The Identity server could not be reached."); }
        catch (System.Text.Json.JsonException) { return ApiResult<T>.Failure("The Identity server returned an invalid response."); }
    }

    private async Task<ApiResult<string>> SendTextCoreAsync(HttpMethod method, string route, bool authorized)
    {
        using var request = new HttpRequestMessage(method, new Uri(new Uri(HttpClientHelper.BaseUrl), route));
        if (!await AddAuthorizationAsync(request, authorized)) return ApiResult<string>.Failure("Your session has expired. Please sign in again.");
        try {
            using var client = HttpClientHelper.GetHttpClient(); using var response = await client.SendAsync(request);
            return response.IsSuccessStatusCode ? ApiResult<string>.Success(await response.Content.ReadAsStringAsync()) : ApiResult<string>.Failure(await ErrorAsync(response));
        } catch (HttpRequestException) { return ApiResult<string>.Failure("The Identity server could not be reached."); }
    }

    private async Task<bool> AddAuthorizationAsync(HttpRequestMessage request, bool required)
    {
        if (!required) return true;
        var firstPartyOrigin = new Uri(HttpClientHelper.BaseUrl);
        if (request.RequestUri is null ||
            Uri.Compare(firstPartyOrigin, request.RequestUri, UriComponents.SchemeAndServer, UriFormat.Unescaped, StringComparison.Ordinal) != 0)
        {
            return false;
        }

        var token = await authentication.GetAccessTokenInfoAsync();
        if (token is null) return false;
        request.Headers.Authorization = new AuthenticationHeaderValue(token.LoginResponse.TokenType, token.LoginResponse.AccessToken);
        return true;
    }

    private static async Task<string> ErrorAsync(HttpResponseMessage response)
    {
        var errors = await response.Content.ReadFromJsonAsync<ValidationErrors>();
        return errors?.Errors?.SelectMany(pair => pair.Value).FirstOrDefault() ?? "The request could not be completed.";
    }

    private sealed record ValidationErrors(Dictionary<string, string[]>? Errors);
}

public sealed record ApiResult<T>(bool Succeeded, T? Value, string? Error)
{
    public static ApiResult<T> Success(T? value) => new(true, value, null);
    public static ApiResult<T> Failure(string error) => new(false, default, error);
    public ApiResult WithoutValue() => new(Succeeded, Error);
}
public sealed record ApiResult(bool Succeeded, string? Error)
{
    public static implicit operator ApiResult(ApiResult<object> result) => new(result.Succeeded, result.Error);
}
