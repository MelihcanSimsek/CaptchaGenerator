using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CaptchaGenerator.Security.Token;

public interface ITokenHelper
{
    Task<JwtSecurityToken> CreateToken(string captchaText,string ip);
    Task<bool> IsTokenExpired(string token);
    Task<ClaimsPrincipal> GetPrincipal(string token);

}
