using CaptchaGenerator.Services.Auth;
using CaptchaGenerator.Services.Captcha;

namespace CaptchaGenerator.Services;

public static class ServiceRegistration
{
    public static IServiceCollection AddServiceRegistration(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICaptchaService, CaptchaService>();
        services.AddScoped<Random>();

        return services;
    }
}
