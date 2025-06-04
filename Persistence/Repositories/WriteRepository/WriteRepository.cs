using CaptchaGenerator.Models.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace CaptchaGenerator.Persistence.Repositories.WriteRepository;

public sealed class WriteRepository<T> : IWriteRepository<T> where T : EntityBase, new()
{
    private readonly DbContext dbContext;

    private DbSet<T> Table { get => dbContext.Set<T>(); }

    public WriteRepository(DbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task AddAsync(T entity)
    {
        await Table.AddAsync(entity);
    }

    public async Task AddRangeAsync(ICollection<T> entities)
    {
        await Table.AddRangeAsync(entities);
    }

    public async Task<T> UpdateAsync(T entity)
    {
        await Task.Run(() => Table.Update(entity));
        return entity;
    }

    public async Task UpdateRangeAsync(ICollection<T> entities)
    {
        await Task.Run(() => Table.UpdateRange(entities));
    }

    public async Task DeleteAsync(T entity)
    {
       await Task.Run(() => Table.Remove(entity));
    }

    public async Task DeleteRangeAsync(ICollection<T> entities)
    {
        await Task.Run(() => Table.RemoveRange(entities));
    }
}
