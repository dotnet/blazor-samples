using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;
using MauiBlazorWeb.Web.Data;

namespace MauiBlazorWeb.Web.Components.Account;

/// <summary>
/// Development-only email sender that retains a bounded, in-memory set of
/// identity actions for local testing. It must never be registered in production.
/// </summary>
internal sealed class DevelopmentEmailSender : IEmailSender<ApplicationUser>
{
    private const int MaximumNotifications = 20;
    private readonly object _gate = new();
    private readonly Queue<DevelopmentNotification> _notifications = new();

    public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink)
    {
        Add("Confirm email", email, confirmationLink, null);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink)
    {
        Add("Reset password", email, resetLink, null);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode)
    {
        Add("Reset password", email, null, resetCode);
        return Task.CompletedTask;
    }

    public IReadOnlyList<DevelopmentNotification> GetNotifications()
    {
        lock (_gate)
        {
            return _notifications.Reverse().ToArray();
        }
    }

    private void Add(string kind, string email, string? actionLink, string? code)
    {
        lock (_gate)
        {
            _notifications.Enqueue(new DevelopmentNotification(kind, email, actionLink, code, DateTimeOffset.UtcNow));
            while (_notifications.Count > MaximumNotifications)
            {
                _notifications.Dequeue();
            }
        }
    }
}

internal sealed record DevelopmentNotification(
    string Kind,
    string Email,
    string? ActionLink,
    string? Code,
    DateTimeOffset CreatedAt);

internal static class DevelopmentNotificationPage
{
    public static string Render(DevelopmentEmailSender sender)
    {
        var body = new StringBuilder("""
            <!doctype html>
            <html lang="en"><head><meta charset="utf-8"><title>Development Identity notifications</title></head>
            <body><h1>Development Identity notifications</h1>
            <p>This page exists only in Development. Do not use it as an email service in production.</p>
            <ul>
            """);

        foreach (var notification in sender.GetNotifications())
        {
            body.Append("<li><strong>")
                .Append(WebUtility.HtmlEncode(notification.Kind))
                .Append("</strong> for ")
                .Append(WebUtility.HtmlEncode(notification.Email))
                .Append(" at ")
                .Append(WebUtility.HtmlEncode(notification.CreatedAt.ToString("O")));

            if (notification.ActionLink is not null)
            {
                body.Append(": <a href=\"")
                    // LinkGenerator returns a URL formatted for HTML. Decode it before
                    // encoding the containing attribute so each query separator is encoded once.
                    .Append(WebUtility.HtmlEncode(WebUtility.HtmlDecode(notification.ActionLink)))
                    .Append("\">complete action</a>");
            }
            else if (notification.Code is not null)
            {
                body.Append(": <code>")
                    .Append(WebUtility.HtmlEncode(notification.Code))
                    .Append("</code>");
            }

            body.Append("</li>");
        }

        return body.Append("</ul></body></html>").ToString();
    }
}
