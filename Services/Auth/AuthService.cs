
using CaptchaGenerator.Constants;
using CaptchaGenerator.Models.DTOs.Requests.Auth;
using CaptchaGenerator.Models.DTOs.Responses.Auth;
using CaptchaGenerator.Models.Entites;
using CaptchaGenerator.Persistence.UnitOfWorks;
using CaptchaGenerator.Security.Password;
using CaptchaGenerator.Security.Token;
using CaptchaGenerator.Services.Captcha;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;

namespace CaptchaGenerator.Services.Auth;

public class AuthService : IAuthService
{
    private readonly ITokenHelper tokenHelper;
    private readonly ICaptchaService captchaService;
    private readonly IConfiguration configuration;
    private readonly IUnitOfWork unitOfWork;
    public AuthService(ICaptchaService captchaService, ITokenHelper tokenHelper, IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        this.captchaService = captchaService;
        this.tokenHelper = tokenHelper;
        this.configuration = configuration;
        this.unitOfWork = unitOfWork;
    }

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
        User? user = await unitOfWork.GetReadRepository<User>().GetAsync(p => p.Email == loginDto.Email);
        bool isPasswordCorrect = PasswordHelper.VerifyPassword(loginDto.Password, user.PasswordHash, user.PasswordSalt);
        if (user is null || !isPasswordCorrect)
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
        if (!await IsUserAlreadyExists(userForChecking))
            return new(false, AuthConstant.Messages.UserAlreadyExists);

        //create user
        PasswordHelper.CreatePassword(registerDto.Password, out byte[] passwordHash, out byte[] passwordSalt);

        string userRoleTag = "user";
        User newUser = new()
        {
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt
        };

        await unitOfWork.GetWriteRepository<User>().AddAsync(newUser);
        await unitOfWork.SaveAsync();

        Role? role = await unitOfWork.GetReadRepository<Role>().GetAsync(p => p.Name == userRoleTag);
        
        if(role is null)
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
        return user is null;
    }

}
