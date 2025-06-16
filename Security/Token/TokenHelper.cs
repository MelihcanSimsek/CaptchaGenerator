using CaptchaGenerator.Models.Entites;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CaptchaGenerator.Security.Token;

public sealed class TokenHelper : ITokenHelper
{
    private readonly Model.Security.TokenOptions tokenOptions;
    private readonly IConfiguration configuration;
    public TokenHelper(IOptions<Model.Security.TokenOptions> options, IConfiguration configuration)
    {
        tokenOptions = options.Value;
        this.configuration = configuration;
    }
    public async Task<JwtSecurityToken> CreateCaptchaToken(string hashedCaptchaText, string ip)
    {
        IList<Claim> claims = await GetCaptchaClaims(hashedCaptchaText, ip);

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(tokenOptions.CaptchaSecret));

        var token = new JwtSecurityToken(
            audience: tokenOptions.Audience,
            issuer: tokenOptions.Issuer,
            expires: DateTime.Now.AddMinutes(tokenOptions.CaptchaTokenExpiredTime),
            claims: claims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return token;
    }

    public async Task<JwtSecurityToken> CreateAccessToken(User user, IList<string> roles)
    {
        IList<Claim> claims = await GetUserClaims(user, roles);

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.AccessSecret));
        var token = new JwtSecurityToken(
            issuer: tokenOptions.Issuer,
            audience: tokenOptions.Audience,
            expires: DateTime.Now.AddMinutes(tokenOptions.AccessTokenValidityInMinutes),
            claims: claims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha512)
            );

        return token;
    }

    public async Task<string> GenerateRefreshToken()
    {
        var randomNumber = new Byte[64];
        using var randomNumberGenerator = RandomNumberGenerator.Create();
        randomNumberGenerator.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<bool> IsCapthcaTokenExpired(string token)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.CaptchaSecret)),
            ValidateLifetime = true,
        };

        try
        {

            JwtSecurityTokenHandler handler = new();
            handler.ValidateToken(token, parameters, out SecurityToken securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken
            || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.CurrentCultureIgnoreCase))
                throw new Exception();
        }
        catch (Exception)
        {
            return true;
        }

        return false;
    }

    //Get captcha token principal
    public async Task<ClaimsPrincipal> GetCaptchaTokenPrincipal(string token)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.CaptchaSecret)),
            ValidateLifetime = false,
        };

        JwtSecurityTokenHandler handler = new();
        var principal = handler.ValidateToken(token, parameters, out SecurityToken securityToken);

        return principal;
    }

    //Get our token principal
    public async Task<ClaimsPrincipal> GetAccessTokenPrincipal(string token)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.AccessSecret)),
            ValidateLifetime = false,
        };

        JwtSecurityTokenHandler handler = new();
        var principal = handler.ValidateToken(token, parameters, out SecurityToken securityToken);

        return principal;
    }

    //Get google token principal
    public async Task<ClaimsPrincipal> GetIdTokenPrincipal(string googleIdToken)
    {
        var googleIssuer = "https://accounts.google.com";
        var metadataAddress = "https://accounts.google.com/.well-known/openid-configuration";

        var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            metadataAddress,
            new OpenIdConnectConfigurationRetriever());

        var config = await configManager.GetConfigurationAsync();

        TokenValidationParameters validationParameters = new TokenValidationParameters
        {
            ValidIssuer = googleIssuer,
            ValidAudience = configuration["GoogleAuthentication:ClientId"],
            IssuerSigningKeys = config.SigningKeys,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(googleIdToken, validationParameters, out _);
        return principal;
    }

    private async Task<IList<Claim>> GetUserClaims(User user, IList<string> roles)
    {
        List<Claim> claims = new();

        claims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()));
        claims.Add(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
        claims.Add(new Claim(ClaimTypes.Email, user.Email.ToString()));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        return claims;
    }

    private async Task<IList<Claim>> GetCaptchaClaims(string hashedCaptchaText, string ip)
    {
        List<Claim> claims = new();
        claims.Add(new Claim(ClaimTypes.Name, hashedCaptchaText));
        claims.Add(new Claim(ClaimTypes.NameIdentifier, ip));
        return claims;
    }

  
}
