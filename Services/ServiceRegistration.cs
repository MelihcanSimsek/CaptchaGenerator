using CaptchaGenerator.Services.Auth;
using CaptchaGenerator.Services.Captcha;
using System.Net.Security;

namespace CaptchaGenerator.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddServiceRegistration(this IServiceCollection services)
    {
        services.AddHttpClient();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<Random>();

        return services;
    }
}
