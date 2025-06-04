
using CaptchaGenerator.Models.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq.Expressions;

namespace CaptchaGenerator.Persistence.Repositories.ReadRepository;

public sealed class ReadRepository<T> : IReadRepository<T> where T:EntityBase,new()
{
    private readonly DbContext dbContext;
    private DbSet<T> Table { get => dbContext.Set<T>(); }

    public ReadRepository(DbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<IList<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Func<IQueryable<T>, IOrderedQueryable<T>>? sort = null, bool enableTracking = false)
    {
        IQueryable<T> queryable = Table;
        if (!enableTracking) queryable.AsNoTracking();
        if (include is not null) queryable = include(queryable);
        if (predicate is not null) queryable = queryable.Where(predicate);
        if (sort is not null) return await sort(queryable).ToListAsync();

        return await queryable.ToListAsync();
    }

    public async Task<IList<T>> GetAllByPagingAsync(Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, Func<IQueryable<T>, IOrderedQueryable<T>>? sort = null, bool enableTracking = false, int currentPage = 1, int pageSize = 5)
    {
        IQueryable<T> queryable = Table;
        if (!enableTracking) queryable.AsNoTracking();
        if (include is not null) queryable = include(queryable);
        if (predicate is not null) queryable = queryable.Where(predicate);
        if (sort is not null) return await sort(queryable).Skip((currentPage - 1)*pageSize).Take(pageSize).ToListAsync();

        return await queryable.Skip((currentPage-1)*pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<T> GetAsync(Expression<Func<T, bool>> predicate, Func<IQueryable<T>, IIncludableQueryable<T, object>>? include = null, bool enableTracking = false)
    {
        IQueryable<T> queryable = Table;
        if (!enableTracking) queryable.AsNoTracking();
        if (include is not null) queryable = include(queryable);

        return await queryable.FirstOrDefaultAsync(predicate);
    }

    public IQueryable<T> Find(Expression<Func<T, bool>> predicate, bool enableTracking = false)
    {
        var query = Table.Where(predicate);
        return enableTracking ? query : query.AsNoTracking();
    }

    public async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        Table.AsNoTracking();
        if (predicate is not null)
            return await Table.Where(predicate).CountAsync();

        return await Table.CountAsync();
    }
}
