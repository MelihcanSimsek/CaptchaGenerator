using CaptchaGenerator.Infrastructure.Context;
using CaptchaGenerator.Persistence.Repositories.ReadRepository;
using CaptchaGenerator.Persistence.Repositories.WriteRepository;
using CaptchaGenerator.Persistence.UnitOfWorks;
using Microsoft.EntityFrameworkCore;

namespace CaptchaGenerator.Infrastructure;

public static class PersistenceRegistration
{
    public static IServiceCollection AddPersistenceRegistration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
        services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
