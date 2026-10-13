namespace LangNghe.Domain.Constants;

/// <summary>
/// Các hằng số mặc định dùng chung trong toàn bộ Domain.
/// </summary>
public static class DomainConstants
{
    public static class MaxLengths
    {
        public const int Code = 20;
        public const int Name = 255;
        public const int NameLong = 500;
        public const int Description = 1000;
        public const int Email = 255;
        public const int Phone = 20;
        public const int Username = 100;
        public const int Note = 1000;
    }

    public static class Formats
    {
        /// <summary>
        /// Định dạng kỳ báo cáo (Ví dụ: 2026, 2026-Q1, 2026-H1).
        /// </summary>
        public const string PeriodFormat = "yyyy";
    }

    public static class Rules
    {
        /// <summary>
        /// Mã làng nghề phải gồm đúng 5 chữ số.
        /// </summary>
        public const string CraftVillageCodeRegex = @"^\d{5}$";

        /// <summary>
        /// Mức cho phép lỗi tối đa khi Import Excel là 10%.
        /// </summary>
        public const decimal MaxImportErrorRate = 0.10m;
        
        /// <summary>
        /// Số lần sai mật khẩu tối đa trước khi bị lock.
        /// </summary>
        public const int MaxFailedLoginAttempts = 5;

        /// <summary>
        /// Thời gian khóa tài khoản sau khi nhập sai mật khẩu quá số lần quy định (phút).
        /// </summary>
        public const int LockoutDurationMinutes = 15;
    }

    public static class Roles
    {
        public const string AdminTW = "ADMIN_TW";
        public const string SoNN = "SO_NN";
        public const string XaCB = "XA_CB";
        public const string ViewerTW = "VIEWER_TW";
        public const string ViewerTinh = "VIEWER_TINH";
    }
}