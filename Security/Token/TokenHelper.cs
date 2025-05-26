using CaptchaGenerator.Model.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace CaptchaGenerator.Security.Token;

public sealed class TokenHelper : ITokenHelper
{
    private readonly TokenOptions tokenOptions;
    public TokenHelper(IOptions<TokenOptions> options)
    {
        tokenOptions = options.Value;
    }
    public async Task<JwtSecurityToken> CreateToken(string hashedCaptchaText,string ip)
    {
        IList<Claim> claims =  await GetClaims(hashedCaptchaText, ip);

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(tokenOptions.Secret));

        var token = new JwtSecurityToken(
            audience: tokenOptions.Audience,
            issuer: tokenOptions.Issuer,
            expires: DateTime.Now.AddMinutes(tokenOptions.TokenExpiredTime),
            claims: claims,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return token;
    }

    public async Task<bool> IsTokenExpired(string token)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.Secret)),
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

    public async Task<ClaimsPrincipal> GetPrincipal(string token)
    {
        TokenValidationParameters parameters = new()
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.Secret)),
            ValidateLifetime = false,
        };

        JwtSecurityTokenHandler handler = new();
        var principal = handler.ValidateToken(token, parameters, out SecurityToken securityToken);

        return principal;
    }

    private async Task<IList<Claim>> GetClaims(string hashedCaptchaText,string ip)
    {
        List<Claim> claims = new();
        claims.Add(new Claim(ClaimTypes.Name, hashedCaptchaText));
        claims.Add(new Claim(ClaimTypes.NameIdentifier, ip));
        return claims;
    }

  
}
