/* ==============================================================================
   SEED 34 TỈNH/THÀNH (sau sáp nhập 2025)
   File: 02_seed_provinces.sql
   ============================================================================== */

USE CSDL_LangNghe;
GO

SET NOCOUNT ON;
PRINT N'=== BẮT ĐẦU SEED 34 TỈNH ===';

-- Xóa tỉnh cũ nếu có (chỉ xóa được nếu chưa có xã con trỏ vào)
IF EXISTS (SELECT 1 FROM Dim_Regions WHERE LevelType = 1)
BEGIN
    -- Chỉ xóa nếu không có xã con
    IF NOT EXISTS (SELECT 1 FROM Dim_Regions WHERE LevelType = 2)
    BEGIN
        DELETE FROM Dim_Regions WHERE LevelType = 1;
        PRINT N'🗑️ Đã xóa tỉnh cũ';
    END
    ELSE
    BEGIN
        PRINT N'⚠️ Đã có xã con — bỏ qua bước xóa. Nếu muốn làm lại, DROP DB và chạy lại.';
        RETURN;
    END
END

-- Seed 34 tỉnh
INSERT INTO Dim_Regions 
    (RegionCode, Name, ShortName, LevelType, RegionType, ParentId, SortOrder, IsActive)
VALUES
    ('01', N'Thành phố Hà Nội',       N'Hà Nội',      1, N'Thành phố', NULL, 1,  1),
    ('04', N'Tỉnh Cao Bằng',          N'Cao Bằng',    1, N'Tỉnh',      NULL, 2,  1),
    ('08', N'Tỉnh Tuyên Quang',       N'Tuyên Quang', 1, N'Tỉnh',      NULL, 3,  1),
    ('11', N'Tỉnh Điện Biên',         N'Điện Biên',   1, N'Tỉnh',      NULL, 4,  1),
    ('12', N'Tỉnh Lai Châu',          N'Lai Châu',    1, N'Tỉnh',      NULL, 5,  1),
    ('14', N'Tỉnh Sơn La',            N'Sơn La',      1, N'Tỉnh',      NULL, 6,  1),
    ('15', N'Tỉnh Lào Cai',           N'Lào Cai',     1, N'Tỉnh',      NULL, 7,  1),
    ('19', N'Tỉnh Thái Nguyên',       N'Thái Nguyên', 1, N'Tỉnh',      NULL, 8,  1),
    ('20', N'Tỉnh Lạng Sơn',          N'Lạng Sơn',    1, N'Tỉnh',      NULL, 9,  1),
    ('22', N'Tỉnh Quảng Ninh',        N'Quảng Ninh',  1, N'Tỉnh',      NULL, 10, 1),
    ('24', N'Tỉnh Bắc Ninh',          N'Bắc Ninh',    1, N'Tỉnh',      NULL, 11, 1),
    ('25', N'Tỉnh Phú Thọ',           N'Phú Thọ',     1, N'Tỉnh',      NULL, 12, 1),
    ('31', N'Thành phố Hải Phòng',    N'Hải Phòng',   1, N'Thành phố', NULL, 13, 1),
    ('33', N'Tỉnh Hưng Yên',          N'Hưng Yên',    1, N'Tỉnh',      NULL, 14, 1),
    ('37', N'Tỉnh Ninh Bình',         N'Ninh Bình',   1, N'Tỉnh',      NULL, 15, 1),
    ('38', N'Tỉnh Thanh Hóa',         N'Thanh Hóa',   1, N'Tỉnh',      NULL, 16, 1),
    ('40', N'Tỉnh Nghệ An',           N'Nghệ An',     1, N'Tỉnh',      NULL, 17, 1),
    ('42', N'Tỉnh Hà Tĩnh',           N'Hà Tĩnh',     1, N'Tỉnh',      NULL, 18, 1),
    ('44', N'Tỉnh Quảng Trị',         N'Quảng Trị',   1, N'Tỉnh',      NULL, 19, 1),
    ('46', N'Thành phố Huế',          N'Huế',         1, N'Thành phố', NULL, 20, 1),
    ('48', N'Thành phố Đà Nẵng',      N'Đà Nẵng',     1, N'Thành phố', NULL, 21, 1),
    ('51', N'Tỉnh Quảng Ngãi',        N'Quảng Ngãi',  1, N'Tỉnh',      NULL, 22, 1),
    ('52', N'Tỉnh Gia Lai',           N'Gia Lai',     1, N'Tỉnh',      NULL, 23, 1),
    ('56', N'Tỉnh Khánh Hòa',         N'Khánh Hòa',   1, N'Tỉnh',      NULL, 24, 1),
    ('66', N'Tỉnh Đắk Lắk',           N'Đắk Lắk',     1, N'Tỉnh',      NULL, 25, 1),
    ('68', N'Tỉnh Lâm Đồng',          N'Lâm Đồng',    1, N'Tỉnh',      NULL, 26, 1),
    ('75', N'Tỉnh Đồng Nai',          N'Đồng Nai',    1, N'Tỉnh',      NULL, 27, 1),
    ('79', N'Thành phố Hồ Chí Minh',  N'TP.HCM',      1, N'Thành phố', NULL, 28, 1),
    ('80', N'Tỉnh Tây Ninh',          N'Tây Ninh',    1, N'Tỉnh',      NULL, 29, 1),
    ('82', N'Tỉnh Đồng Tháp',         N'Đồng Tháp',   1, N'Tỉnh',      NULL, 30, 1),
    ('86', N'Tỉnh Vĩnh Long',         N'Vĩnh Long',   1, N'Tỉnh',      NULL, 31, 1),
    ('91', N'Tỉnh An Giang',          N'An Giang',    1, N'Tỉnh',      NULL, 32, 1),
    ('92', N'Thành phố Cần Thơ',      N'Cần Thơ',     1, N'Thành phố', NULL, 33, 1),
    ('96', N'Tỉnh Cà Mau',            N'Cà Mau',      1, N'Tỉnh',      NULL, 34, 1);
GO

PRINT N'✅ Đã seed 34 tỉnh/thành';
GO

-- Verify
SELECT 
    COUNT(*) AS TotalProvinces,
    SUM(CASE WHEN RegionType = N'Thành phố' THEN 1 ELSE 0 END) AS Cities,
    SUM(CASE WHEN RegionType = N'Tỉnh' THEN 1 ELSE 0 END) AS Provinces
FROM Dim_Regions 
WHERE LevelType = 1;
-- Kỳ vọng: 34 | 6 | 28
GO