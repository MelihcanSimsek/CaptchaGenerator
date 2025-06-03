using CaptchaGenerator.Model.Security;
using CaptchaGenerator.Models.Security;
using CaptchaGenerator.Security.Hash;
using CaptchaGenerator.Security.Token;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CaptchaGenerator.Security;

public static class HelperRegistration
{
    public static IServiceCollection AddHelperRegistration(this IServiceCollection services,IConfiguration configuration)
    {
        services.AddScoped<ITokenHelper, TokenHelper>();
        services.AddScoped<IHashHelper, HashHelper>();

        services.Configure<TokenOptions>(configuration.GetSection("TokenOptions"));
        services.Configure<HashOptions>(configuration.GetSection("HashOptions"));


        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, opt =>
        {
            opt.SaveToken = true;
            opt.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["TokenOptions:AccessSecret"])),
                ValidateLifetime = true,
                ValidIssuer = configuration["TokenOptions:Issuer"],
                ValidAudience = configuration["TokenOptions:Audience"],
                ClockSkew = TimeSpan.Zero
            };
        });



        return services;
    }
}
