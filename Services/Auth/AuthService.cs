
using CaptchaGenerator.Constants;
using CaptchaGenerator.Models.DTOs.Requests.Auth;
using CaptchaGenerator.Models.DTOs.Responses.Auth;
using CaptchaGenerator.Models.Entites;
using CaptchaGenerator.Security.Token;
using CaptchaGenerator.Services.Captcha;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace CaptchaGenerator.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenHelper tokenHelper;
    private readonly ICaptchaService captchaService;
    private readonly UserManager<User> userManager;
    private readonly RoleManager<Role> roleManager;
    private readonly IConfiguration configuration;

    public AuthService(ICaptchaService captchaService, ITokenHelper tokenHelper, UserManager<User> userManager, RoleManager<Role> roleManager, IConfiguration configuration)
    {
        this.captchaService = captchaService;
        this.tokenHelper = tokenHelper;
        this.userManager = userManager;
        this.roleManager = roleManager;
        this.configuration = configuration;
    }

    public async Task<LoginResponse> Login(LoginRequestDto loginDto,string ip)
    {
        var captchaResponse = await captchaService.CheckCaptcha(new(loginDto.Answer, loginDto.Token), ip);
        if (!captchaResponse.IsSuccess) 
            return new(captchaResponse.IsSuccess, captchaResponse.Message, string.Empty, string.Empty, null);

        if(!await IsEmailCorrect(loginDto.Email))
            return new(false, AuthConstant.Messages.EmailIsNotValid, string.Empty, string.Empty, null);

        User? user = await userManager.FindByEmailAsync(loginDto.Email);
        bool isPasswordCorrect = await userManager.CheckPasswordAsync(user, loginDto.Password);
        if (user is null || !isPasswordCorrect)
            return new(false, AuthConstant.Messages.EmailOrPasswordIsNotCorrect, string.Empty, string.Empty, null);

        IList<string> roles = await userManager.GetRolesAsync(user);

        JwtSecurityToken _token = await tokenHelper.CreateAccessToken(user, roles);
        string refreshToken = await tokenHelper.GenerateRefreshToken();

        _ = int.TryParse(configuration["TokenOptions:RefreshTokenValidityInDays"], out int refreshTokenValidityInDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryDate = DateTime.Now.AddDays(refreshTokenValidityInDays);

        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        string token = new JwtSecurityTokenHandler().WriteToken(_token);
        string tokenName = "AccessToken";
        string loginProvider = "Default";

        await userManager.SetAuthenticationTokenAsync(user, loginProvider, tokenName, token);

        return new(true,AuthConstant.Messages.LoginSuccessfully, token, refreshToken, _token.ValidTo);
    }

    public async Task<RegisterResponse> Register(RegisterRequestDto registerDto,string ip)
    {
        var captchaResponse = await captchaService.CheckCaptcha(new(registerDto.Answer, registerDto.Token), ip);
        if (!captchaResponse.IsSuccess) return new(captchaResponse.IsSuccess, captchaResponse.Message);

        if (!await IsEmailCorrect(registerDto.Email))
            return new(false, AuthConstant.Messages.EmailIsNotValid);

        if (!await IsPasswordValid(registerDto.Password))
            return new(false, AuthConstant.Messages.PasswordIsNotValid);

        if (!await ArePasswordsMatch(registerDto.Password, registerDto.ConfirmPassword))
            return new(false, AuthConstant.Messages.PasswordsAreNotMatch);

        User? userForChecking = await userManager.FindByEmailAsync(registerDto.Email);
        if (!await IsUserAlreadyExists(userForChecking))
            return new(false, AuthConstant.Messages.UserAlreadyExists);

        string userRoleTag = "user";
        User newUser = new()
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            SecurityStamp = Guid.NewGuid().ToString()
        };

        IdentityResult result = await userManager.CreateAsync(newUser, registerDto.Password);

        if (result.Succeeded)
            if (!await roleManager.RoleExistsAsync(userRoleTag))
                await roleManager.CreateAsync(new Role()
                {
                    ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Id = Guid.NewGuid(),
                    Name = userRoleTag,
                    NormalizedName = userRoleTag.ToUpper()
                });
        await userManager.AddToRoleAsync(newUser, userRoleTag);

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
        return user is null;
    }

}
