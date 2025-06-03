using CaptchaGenerator.Infrastructure.Context;
using CaptchaGenerator.Models.Entites;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CaptchaGenerator.Infrastructure;

public static class InfrastructureRegistration
{
    public static IServiceCollection AddInfrastructureRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


        services.AddIdentity<User, Role>(opt =>
        {
            opt.Password.RequiredLength = 8;
            opt.Password.RequireLowercase = false;
            opt.Password.RequireUppercase = false;
            opt.Password.RequireDigit = false;
            opt.Password.RequireNonAlphanumeric = false;
            opt.SignIn.RequireConfirmedEmail = false;
        })
     .AddEntityFrameworkStores<AppDbContext>()
     .AddDefaultTokenProviders();

        return services;
    }
}
