using CaptchaGenerator.Models.Abstractions;

namespace CaptchaGenerator.Persistence.Repositories.WriteRepository;

public interface IWriteRepository<T> where T: EntityBase,new()
{
    Task AddAsync(T entity);
    Task AddRangeAsync(ICollection<T> entities);
    Task<T> UpdateAsync(T entity);
    Task UpdateRangeAsync(ICollection<T> entities);
    Task DeleteAsync(T entity);
    Task DeleteRangeAsync(ICollection<T> entities);
}
