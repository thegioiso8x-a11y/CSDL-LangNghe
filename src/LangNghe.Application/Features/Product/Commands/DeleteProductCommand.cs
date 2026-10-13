using System.Threading;
using System.Threading.Tasks;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Product.Commands;

public record DeleteProductCommand(int Id) : IRequest<bool>;

public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly ILangNgheDbContext _context;

    public DeleteProductCommandHandler(ILangNgheDbContext context)
    {
        _context = context;
    }

    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.Products
            .AsTracking()
            .Include(x => x.ProductOcopHistories)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (entity == null)
            throw new NotFoundException("Không tìm thấy sản phẩm", request.Id);

        var historyCount = entity.ProductOcopHistories.Count;
        if (historyCount > 0)
            throw new BusinessException($"Không thể xóa: có {historyCount} lịch sử OCOP");

        _context.Products.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
