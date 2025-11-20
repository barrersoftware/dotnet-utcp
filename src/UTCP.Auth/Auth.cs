namespace UTCP.Auth;

/// <summary>
/// Authentication type enumeration
/// </summary>
public enum AuthType
{
    ApiKey,
    Basic,
    OAuth2
}

/// <summary>
/// Base interface for all authentication methods
/// </summary>
public interface IAuth
{
    AuthType Type { get; }
    void Validate();
}

/// <summary>
/// API Key authentication
/// </summary>
public class ApiKeyAuth : IAuth
{
    public AuthType Type => AuthType.ApiKey;
    public string ApiKey { get; set; } = string.Empty;
    public string VarName { get; set; } = "X-Api-Key";
    public string Location { get; set; } = "header"; // header, query, or cookie

    public ApiKeyAuth() { }

    public ApiKeyAuth(string apiKey, string varName = "X-Api-Key", string location = "header")
    {
        ApiKey = apiKey;
        VarName = varName;
        Location = location;
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(ApiKey))
        {
            throw new ArgumentException("API key must be provided");
        }

        if (Location != "header" && Location != "query" && Location != "cookie")
        {
            throw new ArgumentException("Location must be 'header', 'query', or 'cookie'");
        }
    }
}

/// <summary>
/// HTTP Basic authentication
/// </summary>
public class BasicAuth : IAuth
{
    public AuthType Type => AuthType.Basic;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public BasicAuth() { }

    public BasicAuth(string username, string password)
    {
        Username = username;
        Password = password;
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(Username))
        {
            throw new ArgumentException("Username must be provided");
        }

        if (string.IsNullOrEmpty(Password))
        {
            throw new ArgumentException("Password must be provided");
        }
    }
}

/// <summary>
/// OAuth2 authentication
/// </summary>
public class OAuth2Auth : IAuth
{
    public AuthType Type => AuthType.OAuth2;
    public string TokenUrl { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? Scope { get; set; }

    public OAuth2Auth() { }

    public OAuth2Auth(string tokenUrl, string clientId, string clientSecret, string? scope = null)
    {
        TokenUrl = tokenUrl;
        ClientId = clientId;
        ClientSecret = clientSecret;
        Scope = scope;
    }

    public void Validate()
    {
        if (string.IsNullOrEmpty(TokenUrl))
        {
            throw new ArgumentException("Token URL must be provided");
        }

        if (string.IsNullOrEmpty(ClientId))
        {
            throw new ArgumentException("Client ID must be provided");
        }

        if (string.IsNullOrEmpty(ClientSecret))
        {
            throw new ArgumentException("Client secret must be provided");
        }
    }
}
