using BCrypt.Net;

// Password khởi tạo — user BẮT BUỘC đổi khi login lần đầu (MustChangePassword = 1)
const string PASSWORD = "***REDACTED_ADMIN_PWD***";
const int WORK_FACTOR = 12;   // Bảo mật cấp 2 — TT 03/2017/TT-BTTTT

var hash = BCrypt.Net.BCrypt.HashPassword(PASSWORD, workFactor: WORK_FACTOR);

Console.WriteLine("=== COPY DÒNG DƯỚI ĐÂY ===");
Console.WriteLine(hash);
Console.WriteLine();
Console.WriteLine("=== VERIFY ===");
Console.WriteLine($"Length : {hash.Length} ký tự (bcrypt luôn = 60)");
Console.WriteLine($"Prefix : {hash.Substring(0, 7)}");  // $2a$12$
Console.WriteLine($"Valid  : {BCrypt.Net.BCrypt.Verify(PASSWORD, hash)}");
