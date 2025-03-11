using System.Text.Json.Serialization;

namespace MauiBlazorWeb.Models
{
    /// <summary>
    /// This class represents the information related to an access token.
    /// </summary>
    public class AccessTokenInfo
    {
        public required string Email { get; set; }
        public required LoginResponse LoginResponse { get; set; }
        public required DateTime AccessTokenExpiration { get; set; }
    }
}
