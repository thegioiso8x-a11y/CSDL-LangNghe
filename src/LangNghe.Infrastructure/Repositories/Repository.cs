using System.Linq.Expressions;
using LangNghe.Domain.Interfaces;
using LangNghe.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Infrastructure.Repositories;

public class Repository<T> : IRepository<T> where T : class
{
    protected readonly LangNgheDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public Repository(LangNgheDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public IQueryable<T> AsQueryable() => _dbSet.AsQueryable();

    public async Task<T?> FindAsync(object[] keyValues, CancellationToken cancellationToken = default)
        => await _dbSet.FindAsync(keyValues, cancellationToken);

    public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        => await _dbSet.FirstOrDefaultAsync(predicate, cancellationToken);

    public async Task AddAsync(T entity, CancellationToken cancellationToken = default)
        => await _dbSet.AddAsync(entity, cancellationToken);

    public void Update(T entity) => _dbSet.Update(entity);

    public void Remove(T entity) => _dbSet.Remove(entity);

    public void RemoveRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
}