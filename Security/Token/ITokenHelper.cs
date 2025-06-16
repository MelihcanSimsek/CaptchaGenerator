using CaptchaGenerator.Models.Entites;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CaptchaGenerator.Security.Token;

public interface ITokenHelper
{
    Task<JwtSecurityToken> CreateCaptchaToken(string captchaText,string ip);
    Task<JwtSecurityToken> CreateAccessToken(User user, IList<string> roles);
    Task<string> GenerateRefreshToken();
    Task<bool> IsCapthcaTokenExpired(string captchaToken);
    Task<ClaimsPrincipal> GetCaptchaTokenPrincipal(string captchaToken);
    Task<ClaimsPrincipal> GetAccessTokenPrincipal(string accessToken);
    Task<ClaimsPrincipal> GetIdTokenPrincipal(string googleIdToken);



}
