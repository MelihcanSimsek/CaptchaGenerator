using CaptchaGenerator.Models.DTOs.Requests.Auth;
using CaptchaGenerator.Models.DTOs.Responses.Auth;

namespace CaptchaGenerator.Services.Auth;

public interface IAuthService
{
    Task<RegisterResponse> Register(RegisterRequestDto registerDto,string ip);
    Task<LoginResponse> Login(LoginRequestDto loginDto,string ip);
    Task<GoogleAuthenticationResponse> GetGoogleAuthentication();
    Task<TokenExchangeResponse> ExchangeGoogleTokensWithCode(GoogleCallbackRequest request);
    Task<LoginResponse> LoginWithGoogle(GoogleTokenResponse googleTokens);
}
