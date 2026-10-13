using AutoMapper;

namespace LangNghe.Application.Mapping;

/// <summary>
/// Interface đánh dấu một class có thể map tự động từ kiểu T.
/// </summary>
/// <typeparam name="T">Kiểu dữ liệu gốc</typeparam>
public interface IMapFrom<T>
{
    /// <summary>
    /// Định nghĩa map cơ bản từ T sang class thực thi interface này.
    /// Có thể override để tuỳ chỉnh mapping logic.
    /// </summary>
    void Mapping(Profile profile) => profile.CreateMap(typeof(T), GetType());
}
