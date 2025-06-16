
using CaptchaGenerator.Constants;
using CaptchaGenerator.Models.DTOs.Requests.Auth;
using CaptchaGenerator.Models.DTOs.Responses.Auth;
using CaptchaGenerator.Models.Entites;
using CaptchaGenerator.Models.Enums;
using CaptchaGenerator.Persistence.UnitOfWorks;
using CaptchaGenerator.Security.Password;
using CaptchaGenerator.Security.Token;
using CaptchaGenerator.Services.Captcha;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CaptchaGenerator.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenHelper tokenHelper;
    private readonly ICaptchaService captchaService;
    private readonly IConfiguration configuration;
    private readonly IUnitOfWork unitOfWork;
    private readonly HttpClient httpClient;
    public AuthService(ICaptchaService captchaService, ITokenHelper tokenHelper, IConfiguration configuration, IUnitOfWork unitOfWork, HttpClient httpClient)
    {
        this.captchaService = captchaService;
        this.tokenHelper = tokenHelper;
        this.configuration = configuration;
        this.unitOfWork = unitOfWork;
        this.httpClient = httpClient;
    }

    //Get google authentication url
    public async Task<GoogleAuthenticationResponse> GetGoogleAuthentication()
    {
        StringBuilder urlBuilder = new();
        string scopes = "openid email profile";
        urlBuilder.Append("https://accounts.google.com/o/oauth2/v2/auth?");
        urlBuilder.Append($"redirect_uri={Uri.EscapeDataString(configuration["GoogleAuthentication:RedirectUrl"])}&");
        urlBuilder.Append($"prompt=consent&");
        urlBuilder.Append($"response_type=code&");
        urlBuilder.Append($"client_id={configuration["GoogleAuthentication:ClientId"]}&");
        urlBuilder.Append($"scope={Uri.EscapeDataString(scopes)}&");
        urlBuilder.Append($"access_type=offline");

        return new(urlBuilder.ToString());
    }

    //Get google tokens with authorization code
    public async Task<TokenExchangeResponse> ExchangeGoogleTokensWithCode(GoogleCallbackRequest request)
    {
        string exchangeEndpointUrl = "https://oauth2.googleapis.com/token";
        Dictionary<string, string> parameters = new()
            {
                {"code", request.Code },
                {"redirect_uri", configuration["GoogleAuthentication:RedirectUrl"] },
                {"client_id", configuration["GoogleAuthentication:ClientId"] },
                {"client_secret", configuration["GoogleAuthentication:ClientSecret"] },
                {"grant_type", "authorization_code" }
            };

        FormUrlEncodedContent content = new(parameters);
        HttpResponseMessage response = await httpClient.PostAsync(exchangeEndpointUrl, content);
        string responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return new(false, null);

        return new(true, JsonSerializer.Deserialize<GoogleTokenResponse>(responseContent));
    }

    //Create or update user for login google
    public async Task<LoginResponse> LoginWithGoogle(GoogleTokenResponse googleTokens)
    {
        ClaimsPrincipal tokenPrincipal = await tokenHelper.GetIdTokenPrincipal(googleTokens.id_token);
        string userEmail = tokenPrincipal.FindFirstValue(claimType: ClaimTypes.Email);
        User? user = await unitOfWork.GetReadRepository<User>().GetAsync(p => p.Email == userEmail);

        //if no exists register user
        if(!await IsUserAlreadyExists(user))
        {
            string userName = tokenPrincipal.FindFirstValue(claimType: ClaimTypes.GivenName);
            string userRoleTag = "user";

            user = new()
            {
                Email = userEmail,
                FullName = userName,
                AuthenticationType = AuthenticationType.Google
            };

            await unitOfWork.GetWriteRepository<User>().AddAsync(user);
            await unitOfWork.SaveAsync();

            Role? role = await unitOfWork.GetReadRepository<Role>().GetAsync(p => p.Name == userRoleTag);

            //Add a 'user' role if no exists
            if (role is null)
            {
                role = new Role()
                {
                    NormalizedName = userRoleTag.ToUpper(),
                    Name = userRoleTag
                };

                await unitOfWork.GetWriteRepository<Role>().AddAsync(role);
            }

            await unitOfWork.GetWriteRepository<UserRole>().AddAsync(new() { UserId = user.Id, RoleId = role.Id });
            await unitOfWork.SaveAsync();
        }

        IList<string> roles = await unitOfWork.GetReadRepository<UserRole>()
               .Find(p => p.UserId == user.Id)
               .Include(m => m.Role).Select(c => c.Role.Name).ToListAsync();

        JwtSecurityToken _token = await tokenHelper.CreateAccessToken(user, roles);
        string refreshToken = await tokenHelper.GenerateRefreshToken();
        _ = int.TryParse(configuration["TokenOptions:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = DateTime.Now.AddDays(refreshTokenValidityInDays);

        await unitOfWork.GetWriteRepository<User>().UpdateAsync(user);
        await unitOfWork.SaveAsync();

        string token = new JwtSecurityTokenHandler().WriteToken(_token);
        return new(true, AuthConstant.Messages.LoginSuccessfully, token, refreshToken, _token.ValidTo);
    }

    //Default login
    public async Task<LoginResponse> Login(LoginRequestDto loginDto, string ip)
    {
        //validate captcha
        var captchaResponse = await captchaService.CheckCaptcha(new(loginDto.Answer, loginDto.Token), ip);
        if (!captchaResponse.IsSuccess)
            return new(captchaResponse.IsSuccess, captchaResponse.Message, string.Empty, string.Empty, null);

        //validate email
        if (!await IsEmailCorrect(loginDto.Email))
            return new(false, AuthConstant.Messages.EmailIsNotValid, string.Empty, string.Empty, null);

        //validate password
        User? user = await unitOfWork.GetReadRepository<User>().GetAsync(p => p.Email == loginDto.Email && p.AuthenticationType == AuthenticationType.Normal);
        if (user is null || !PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt))
            return new(false, AuthConstant.Messages.EmailOrPasswordIsNotCorrect, string.Empty, string.Empty, null);

        //create token
        IList<string> roles = await unitOfWork.GetReadRepository<UserRole>()
            .Find(p => p.UserId == user.Id)
            .Include(m => m.Role).Select(c => c.Role.Name).ToListAsync();

        JwtSecurityToken _token = await tokenHelper.CreateAccessToken(user, roles);
        string refreshToken = await tokenHelper.GenerateRefreshToken();

        _ = int.TryParse(configuration["TokenOptions:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = DateTime.Now.AddDays(refreshTokenValidityInDays);

        await unitOfWork.GetWriteRepository<User>().UpdateAsync(user);
        await unitOfWork.SaveAsync();

        string token = new JwtSecurityTokenHandler().WriteToken(_token);
        return new(true, AuthConstant.Messages.LoginSuccessfully, token, refreshToken, _token.ValidTo);
    }

    //Default register
    public async Task<RegisterResponse> Register(RegisterRequestDto registerDto, string ip)
    {
        //validate captcha
        var captchaResponse = await captchaService.CheckCaptcha(new(registerDto.Answer, registerDto.Token), ip);
        if (!captchaResponse.IsSuccess) return new(captchaResponse.IsSuccess, captchaResponse.Message);

        //validate email
        if (!await IsEmailCorrect(registerDto.Email))
            return new(false, AuthConstant.Messages.EmailIsNotValid);

        //validate password 
        if (!await IsPasswordValid(registerDto.Password))
            return new(false, AuthConstant.Messages.PasswordIsNotValid);

        //validate passwords match 
        if (!await ArePasswordsMatch(registerDto.Password, registerDto.ConfirmPassword))
            return new(false, AuthConstant.Messages.PasswordsAreNotMatch);

        //validate user not exists
        User? userForChecking = await unitOfWork.GetReadRepository<User>().GetAsync(p => p.Email == registerDto.Email);
        if (await IsUserAlreadyExists(userForChecking))
            return new(false, AuthConstant.Messages.UserAlreadyExists);

        //create user
        PasswordHelper.CreatePassword(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

        string userRoleTag = "user";
        User newUser = new()
        {
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            AuthenticationType = AuthenticationType.Normal
        };

        await unitOfWork.GetWriteRepository<User>().AddAsync(newUser);
        await unitOfWork.SaveAsync();

        Role? role = await unitOfWork.GetReadRepository<Role>().GetAsync(p => p.Name == userRoleTag);

        if (role is null)
        {
            role = new Role()
            {
                NormalizedName = userRoleTag.ToUpper(),
                Name = userRoleTag
            };

            await unitOfWork.GetWriteRepository<Role>().AddAsync(role);
        }

        await unitOfWork.GetWriteRepository<UserRole>().AddAsync(new() { UserId = newUser.Id, RoleId = role.Id });
        await unitOfWork.SaveAsync();

        return new(true, AuthConstant.Messages.UserRegisteredSuccessfully);
    }

    private async Task<bool> IsEmailCorrect(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;

        var pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|" +
                  @"([-a-zA-Z0-9!#\$%&'\*\+/=\?\^_`\{\}\|~\w]+(?:\.[-a-zA-Z0-9!#\$%&'\*\+/=\?\^_`\{\}\|~\w]+)*))@" +
                  @"((([a-zA-Z0-9][a-zA-Z0-9\-]*[a-zA-Z0-9]*\.)+[a-zA-Z]{2,})|" +
                  @"(\[(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                  @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                  @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\." +
                  @"(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)]))$";

        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    private async Task<bool> IsPasswordValid(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;
        return true;
    }

    private async Task<bool> ArePasswordsMatch(string password, string confirmPassword)
    {
        return password == confirmPassword;
    }

    private async Task<bool> IsUserAlreadyExists(User? user)
    {
        return user is not null;
    }


}
