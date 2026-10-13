namespace LangNghe.Domain.Constants;

/// <summary>
/// Mã định danh cho các quyền (Permissions).
/// Khớp với dữ liệu đã seed trong cột Code của bảng Sys_Permissions (Module.Action).
/// </summary>
public static class PermissionCodes
{
    // Làng nghề
    public const string VillageRead = "VILLAGE.READ";
    public const string VillageCreate = "VILLAGE.CREATE";
    public const string VillageUpdate = "VILLAGE.UPDATE";
    public const string VillageDelete = "VILLAGE.DELETE";
    public const string VillageApprove = "VILLAGE.APPROVE";
    public const string VillagePublish = "VILLAGE.PUBLISH";

    // Import Excel
    public const string ImportUpload = "IMPORT.UPLOAD";
    public const string ImportRollback = "IMPORT.ROLLBACK";

    // Báo cáo & Dashboard
    public const string ReportView = "REPORT.VIEW";
    public const string ReportExport = "REPORT.EXPORT";
    
    // Hệ thống
    public const string UserManage = "USER.MANAGE";
    public const string DimManage = "DIM.MANAGE";
    public const string AuditView = "AUDIT.VIEW";
    public const string ConfigManage = "CONFIG.MANAGE";
}