using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LangNghe.Infrastructure.Persistence.Configurations;

/// <summary>
/// Class tiện ích chứa các Enum ValueConverters để sử dụng trong Fluent API,
/// trong trường hợp cần thiết nếu database dùng kiểu dữ liệu khác với Enum.
/// 
/// Lưu ý: Hầu hết các Enums trong hệ thống đã được khai báo kế thừa từ `byte`, 
/// nên EF Core sẽ tự động ánh xạ trực tiếp xuống `TINYINT` mà không cần ValueConverter.
/// </summary>
public static class EnumConversion
{
    // Ví dụ ValueConverter nếu có Enum cần chuyển sang chuỗi (NVARCHAR)
    // public static readonly ValueConverter<SomeStringEnum, string> SomeStringEnumConverter = 
    //     new ValueConverter<SomeStringEnum, string>(
    //         v => v.ToString(),
    //         v => (SomeStringEnum)Enum.Parse(typeof(SomeStringEnum), v));
}