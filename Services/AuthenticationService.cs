using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using AssetServer.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AssetServer.Services;

public class AuthenticationService
{
    private readonly ILogger<AuthenticationService> logger;
    private readonly IOptionsMonitor<AuthenticationOptions> options;
    private readonly SymmetricSecurityKey jwtKey;
    private readonly JwtSecurityTokenHandler tokenHandler = new();

    private const string KeyPath = "key_secret";
    private const int KeySize = 4096;

    public AuthenticationService(
        ILogger<AuthenticationService> logger,
        IOptionsMonitor<AuthenticationOptions> options
    )
    {
        this.logger = logger;
        this.options = options;
        if (!Path.Exists("key_secret"))
        {
            this.logger.LogInformation("Could not find secret file. Generating new one...");

            byte[] key = new byte[KeySize];
            RandomNumberGenerator.Create().GetBytes(key);

            File.WriteAllBytes(KeyPath, key);
        }

        jwtKey = new(File.ReadAllBytes(KeyPath));
    }

    public Task<AuthenticationState> HandleAuthenticateAsync(Shared.Login.LoginModel loginModel)
    {
        ClaimsPrincipal result = this.tokenHandler.ValidateToken(
            loginModel.Jwt,
            new()
            {
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidateAudience = true,
                ValidIssuer = this.options.CurrentValue.TokenIssuer,
                ValidAudience = this.options.CurrentValue.TokenAudience,
                IssuerSigningKey = this.jwtKey
            },
            out SecurityToken _
        );

        return Task.FromResult(new AuthenticationState(result));
    }

    public void IssueToken()
    {
        SecurityTokenDescriptor descriptor =
            new()
            {
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = options.CurrentValue.TokenIssuer,
                Audience = options.CurrentValue.TokenAudience,
                SigningCredentials = new(this.jwtKey, SecurityAlgorithms.HmacSha256Signature)
            };

        JwtSecurityToken token = this.tokenHandler.CreateJwtSecurityToken(descriptor);

        this.logger.LogInformation(
            "Generated new token. Please copy and paste the below into the login page."
        );
        this.logger.LogInformation("{token}", this.tokenHandler.WriteToken(token));
    }
}
