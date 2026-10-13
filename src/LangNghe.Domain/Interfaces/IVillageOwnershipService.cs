namespace LangNghe.Domain.Interfaces;

public interface IVillageOwnershipService
{
    /// <summary>
    /// Validate quyền tạo LN. Trả về true nếu OK, throw BusinessException nếu không.
    /// </summary>
    Task ValidateCreateAsync(int regionId, string villageCode, CancellationToken ct);
    
    /// <summary>
    /// Validate quyền thao tác trên làng nghề cụ thể.
    /// TW: OK hết. Tỉnh: phải thuộc tỉnh mình. Xã: phải thuộc xã mình.
    /// </summary>
    Task ValidateVillageAccessAsync(int craftVillageId, CancellationToken ct);
    
    /// <summary>
    /// Kiểm tra user hiện tại có quyền nhập kỳ báo cáo cho LN này không.
    /// Dùng cho CreateMetric/UpdateMetric/DeleteMetric.
    /// </summary>
    Task ValidateMetricAccessAsync(int craftVillageId, CancellationToken ct);
}
