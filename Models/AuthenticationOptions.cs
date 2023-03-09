namespace AssetServer.Models;

public class AuthenticationOptions
{
    public required string TokenIssuer { get; set; }

    public required string TokenAudience { get; set; }
}
