namespace LangNghe.Application.Features.Import.Constants;

/// <summary>
/// Các hằng số cho mã template import
/// </summary>
public static class ImportTemplateCode
{
    public const string V1_FULL = "V1_FULL";
    public const string V1_MINIMAL = "V1_MINIMAL";
    public const string MONTHLY = "MONTHLY";
}

/// <summary>
/// Các chế độ import dữ liệu
/// </summary>
public static class ImportMode
{
    public const string MERGE = "MERGE";
    public const string REPLACE_FULL = "REPLACE_FULL";
    public const string INSERT_ONLY = "INSERT_ONLY";
    public const string UPDATE_ONLY = "UPDATE_ONLY";
}

/// <summary>
/// Trạng thái của một tiến trình import
/// </summary>
public static class ImportStatus
{
    public const byte InProgress = 0;
    public const byte Completed = 1;
    public const byte Failed = 2;
    public const byte RolledBack = 3;
}

/// <summary>
/// Trạng thái xác thực (validate) dữ liệu của một dòng import
/// </summary>
public static class ValidationStatus
{
    public const byte Pending = 0;
    public const byte Valid = 1;
    public const byte Invalid = 2;
    public const byte Persisted = 3;
}

/// <summary>
/// Mức độ nghiêm trọng của lỗi import
/// </summary>
public static class ErrorSeverity
{
    public const byte Warning = 0;
    public const byte Error = 1;
}
