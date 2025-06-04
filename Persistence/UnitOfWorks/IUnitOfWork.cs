using CaptchaGenerator.Models.Abstractions;
using CaptchaGenerator.Persistence.Repositories.ReadRepository;
using CaptchaGenerator.Persistence.Repositories.WriteRepository;

namespace CaptchaGenerator.Persistence.UnitOfWorks;

public interface IUnitOfWork : IAsyncDisposable
{
    IWriteRepository<T> GetWriteRepository<T>() where T : EntityBase, new();
    IReadRepository<T> GetReadRepository<T>() where T : EntityBase, new();
    Task<int> SaveAsync();
    int Save();
}
