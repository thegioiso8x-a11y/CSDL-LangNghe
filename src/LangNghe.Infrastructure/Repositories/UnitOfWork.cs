using LangNghe.Domain.Interfaces;
using LangNghe.Infrastructure.Persistence;

namespace LangNghe.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly LangNgheDbContext _context;

    public UnitOfWork(LangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}