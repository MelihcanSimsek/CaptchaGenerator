using CaptchaGenerator.Infrastructure.Context;
using CaptchaGenerator.Models.Abstractions;
using CaptchaGenerator.Persistence.Repositories.ReadRepository;
using CaptchaGenerator.Persistence.Repositories.WriteRepository;

namespace CaptchaGenerator.Persistence.UnitOfWorks;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext appDbContext;

    public UnitOfWork(AppDbContext appDbContext)
    {
        this.appDbContext = appDbContext;
    }

    public async ValueTask DisposeAsync() => await appDbContext.DisposeAsync();

    public IReadRepository<T> GetReadRepository<T>() where T : EntityBase, new() => new ReadRepository<T>(appDbContext);

    public IWriteRepository<T> GetWriteRepository<T>() where T : EntityBase, new() => new WriteRepository<T>(appDbContext);

    public int Save() => appDbContext.SaveChanges();

    public async Task<int> SaveAsync() => await appDbContext.SaveChangesAsync();
}
