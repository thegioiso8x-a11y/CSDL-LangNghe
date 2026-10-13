using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Infrastructure.Services;

public class VillageOwnershipService : IVillageOwnershipService
{
    private readonly ICurrentUserService _currentUser;
    private readonly ILangNgheDbContext _context;

    public VillageOwnershipService(ICurrentUserService currentUser, ILangNgheDbContext context)
    {
        _currentUser = currentUser;
        _context = context;
    }

    public async Task ValidateCreateAsync(int regionId, string villageCode, CancellationToken ct)
    {
        var dataScope = _currentUser.DataScope;
        var userRegionId = _currentUser.RegionId;
        
        // TW: không check
        if (dataScope == 1 || dataScope == null) return;
        
        // Lấy region đích
        var region = await _context.Regions.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == regionId, ct)
            ?? throw new BusinessException("Xã/phường không tồn tại");
        
        if (region.LevelType != 2)
            throw new BusinessException("Làng nghề phải thuộc cấp Xã/Phường");
        
        if (string.IsNullOrEmpty(villageCode) || villageCode.Length < 2)
            throw new BusinessException("Mã làng nghề không hợp lệ");
            
        // 2 số đầu VillageCode = mã tỉnh
        var provinceCodeInVillage = villageCode.Substring(0, 2);
        
        if (dataScope == 2)  // Tỉnh
        {
            if (userRegionId != region.ParentId)
                throw new BusinessException("Bạn chỉ được tạo làng nghề thuộc tỉnh mình quản lý");
            
            var province = await _context.Regions.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == userRegionId, ct)
                ?? throw new BusinessException("Tỉnh không tồn tại");
                
            if (province.RegionCode != provinceCodeInVillage)
                throw new BusinessException($"2 số đầu mã LN phải khớp mã tỉnh ({province.RegionCode})");
        }
        else if (dataScope == 3)  // Xã
        {
            if (userRegionId != regionId)
                throw new BusinessException("Bạn chỉ được tạo làng nghề thuộc xã mình quản lý");
            
            var myCommune = await _context.Regions.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == userRegionId, ct)
                ?? throw new BusinessException("Xã không tồn tại");
                
            var myProvince = await _context.Regions.AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == myCommune.ParentId, ct)
                ?? throw new BusinessException("Tỉnh không tồn tại");
                
            if (myProvince.RegionCode != provinceCodeInVillage)
                throw new BusinessException($"2 số đầu mã LN phải khớp mã tỉnh ({myProvince.RegionCode})");
        }
    }

    public async Task ValidateVillageAccessAsync(int craftVillageId, CancellationToken ct)
    {
        var dataScope = _currentUser.DataScope;
        var userRegionId = _currentUser.RegionId;

        if (dataScope == null || dataScope == 1) return;

        var village = await _context.CraftVillages
            .AsNoTracking()
            .Include(v => v.Region)
            .ThenInclude(r => r.Parent)
            .FirstOrDefaultAsync(v => v.Id == craftVillageId, ct)
            ?? throw new NotFoundException("Làng nghề không tồn tại", craftVillageId);

        if (dataScope == 2) // Tỉnh
        {
            if (village.Region?.ParentId != userRegionId)
                throw new BusinessException("Bạn không có quyền truy cập làng nghề này");
        }
        else if (dataScope == 3) // Xã
        {
            if (village.RegionId != userRegionId)
                throw new BusinessException("Bạn không có quyền truy cập làng nghề này");
        }
    }

    public async Task ValidateMetricAccessAsync(int craftVillageId, CancellationToken ct)
    {
        await ValidateVillageAccessAsync(craftVillageId, ct);
    }
}
