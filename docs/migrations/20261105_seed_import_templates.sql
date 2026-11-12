/* =====================================================================
   SEED DỮ LIỆU Import_TemplateConfig CHO V1_FULL (47 CỘT) VÀ MONTHLY
   ---------------------------------------------------------------------
   Mục đích : Bổ sung cấu hình template còn thiếu trong DB.
              DB hiện chỉ có V1_MINIMAL (11 cột).
   Bảng     : dbo.Import_TemplateConfig
   Cột map  : TemplateCode, FieldName, ExcelColumnHeader, FieldOrder,
              IsRequired, DefaultValue, DataType, MaxLength,
              ValidationRegex, GroupName, Description, HelpText,
              IsActive, CreatedAt
   Ràng buộc: UNIQUE (TemplateCode, FieldName)  -> script idempotent,
              chạy lại nhiều lần KHÔNG gây lỗi trùng.
   Lưu ý     : DataType dùng đúng các giá trị mà ExcelTemplateService
              đang xử lý: string | int | long | decimal | bool | datetime
   ===================================================================== */

SET NOCOUNT ON;
GO

/* ---------------------------------------------------------------------
   1) V1_FULL — 47 CỘT (nhập đầy đủ dữ liệu làng nghề)
   --------------------------------------------------------------------- */
;WITH V1Full AS (
    SELECT * FROM (VALUES
        -- FieldOrder, FieldName, ExcelColumnHeader, DataType, IsRequired, MaxLength, GroupName, Description
        ( 1, N'VillageCode',          N'Ma lang nghe',        'string',   1, 50,  N'Thông tin chung', N'Mã làng nghề (duy nhất)'),
        ( 2, N'Name',                 N'Ten lang nghe',       'string',   1, 255, N'Thông tin chung', N'Tên làng nghề'),
        ( 3, N'RegionId',             N'Ma xa',               'int',      1, NULL,N'Thông tin chung', N'ID nội bộ xã/phường (số nguyên)'),
        ( 4, N'Address',              N'Dia chi',             'string',   0, 500, N'Thông tin chung', N'Địa chỉ chi tiết'),
        ( 5, N'RecognitionDate',      N'Ngay cong nhan',      'datetime', 1, NULL,N'Thông tin chung', N'Ngày công nhận (YYYY-MM-DD)'),
        ( 6, N'RecognitionDecision',  N'Quyet dinh',          'string',   0, 200, N'Thông tin chung', N'Số quyết định công nhận'),
        ( 7, N'ClassificationId',     N'Phan loai',           'int',      1, NULL,N'Thông tin chung', N'ID phân loại (số nguyên)'),
        ( 8, N'VillageTypeId',        N'Loai hinh',           'int',      1, NULL,N'Thông tin chung', N'ID loại hình (số nguyên)'),
        ( 9, N'IndustryGroupId',      N'Nhom nganh',          'int',      1, NULL,N'Thông tin chung', N'ID nhóm ngành (số nguyên)'),
        (10, N'NeedRestoration',      N'Can phuc hoi',        'bool',     0, NULL,N'Thông tin chung', N'Cần phục hồi (TRUE/FALSE)'),
        (11, N'Status',               N'Trang thai',          'int',      0, NULL,N'Thông tin chung', N'Trạng thái (0-9)'),
        (12, N'Latitude',             N'Vi do',               'decimal',  0, NULL,N'GIS',             N'Vĩ độ (WGS84)'),
        (13, N'Longitude',            N'Kinh do',             'decimal',  0, NULL,N'GIS',             N'Kinh độ (WGS84)'),
        (14, N'ReportPeriod',         N'Ky bao cao',          'string',   1, 10,  N'Chỉ tiêu định kỳ',N'Kỳ báo cáo (VD: 2024, 2024-Q1)'),
        (15, N'PeriodType',           N'Loai ky',             'int',      1, NULL,N'Chỉ tiêu định kỳ',N'Loại kỳ (1=Năm,2=Quý,3=6 tháng,4=Tháng)'),
        (16, N'BrandProtection',      N'Bao ho ten thuong hieu','bool',   0, NULL,N'Chỉ tiêu định kỳ',N'Có bảo hộ thương hiệu'),
        (17, N'HasOCOP3Star',         N'Co OCOP 3 sao',       'bool',     0, NULL,N'Chỉ tiêu định kỳ',N'Có sản phẩm OCOP 3 sao'),
        (18, N'ProductionProcess',    N'Quy trinh san xuat',  'string',   0, 500, N'Chỉ tiêu định kỳ',N'Truyền thống/Hiện đại/Kết hợp'),
        (19, N'ExportRate',           N'Ty le xuat khau',     'decimal',  0, NULL,N'Chỉ tiêu định kỳ',N'Tỷ lệ xuất khẩu (%) 0-100'),
        (20, N'ExportCategoryId',     N'Nhom mat hang xuat',  'int',      0, NULL,N'Chỉ tiêu định kỳ',N'ID nhóm mặt hàng xuất khẩu'),
        (21, N'ImportMaterialRate',   N'Ty le nguyen lieu nhap','decimal',0, NULL,N'Chỉ tiêu định kỳ',N'Tỷ lệ nguyên liệu nhập (%) 0-100'),
        (22, N'HouseholdCount',       N'So ho',               'int',      1, NULL,N'Chỉ tiêu định kỳ',N'Số hộ làm nghề'),
        (23, N'EnterpriseCount',      N'So doanh nghiep',     'int',      1, NULL,N'Chỉ tiêu định kỳ',N'Số doanh nghiệp'),
        (24, N'ProductionArea',       N'Dien tich san xuat',  'decimal',  0, NULL,N'Chỉ tiêu định kỳ',N'Diện tích sản xuất (m2)'),
        (25, N'ActiveMonths',         N'So thang hoat dong',  'int',      0, NULL,N'Chỉ tiêu định kỳ',N'Số tháng hoạt động (0-12)'),
        (26, N'Output',               N'San luong',           'decimal',  0, NULL,N'Chỉ tiêu định kỳ',N'Sản lượng'),
        (27, N'OutputUnitId',         N'Don vi tinh',         'int',      0, NULL,N'Chỉ tiêu định kỳ',N'ID đơn vị tính'),
        (28, N'Revenue',              N'Doanh thu',           'decimal',  1, NULL,N'Chỉ tiêu định kỳ',N'Doanh thu (VND)'),
        (29, N'GrowthRate',           N'Ty le tang truong',   'decimal',  0, NULL,N'Chỉ tiêu định kỳ',N'Tỷ lệ tăng trưởng (%) -100..100'),
        (30, N'TotalLabor',           N'Tong lao dong',       'int',      1, NULL,N'Chỉ tiêu định kỳ',N'Tổng số lao động'),
        (31, N'TrainedLaborRate',     N'Ty le lao dong qua dao tao','decimal',0,NULL,N'Chỉ tiêu định kỳ',N'Tỷ lệ lao động qua đào tạo (%)'),
        (32, N'ArtisanCount',         N'So nghe nhan',        'int',      1, NULL,N'Chỉ tiêu định kỳ',N'Số nghệ nhân'),
        (33, N'TrainingNeed',         N'Nhu cau dao tao',     'string',   0, 1000,N'Chỉ tiêu định kỳ',N'Nhu cầu đào tạo'),
        (34, N'AvgIncome',            N'Thu nhap binh quan',  'decimal',  0, NULL,N'Chỉ tiêu định kỳ',N'Thu nhập bình quân (VND/người)'),
        (35, N'DigitalLevel',         N'Muc do so hoa',       'decimal',  0, NULL,N'Chỉ tiêu định kỳ',N'Mức độ số hóa'),
        (36, N'HasWebsite',           N'Co website',          'bool',     0, NULL,N'Chỉ tiêu định kỳ',N'Có website'),
        (37, N'HasMuseum',            N'Co nha trung bay',    'bool',     0, NULL,N'Chỉ tiêu định kỳ',N'Có nhà trưng bày'),
        (38, N'HasDocArchive',        N'Co luu tru tai lieu', 'bool',     0, NULL,N'Chỉ tiêu định kỳ',N'Có lưu trữ tài liệu'),
        (39, N'HasWasteCollection',   N'Co thu gom rac',      'bool',     0, NULL,N'Môi trường',       N'Có thu gom rác thải'),
        (40, N'HasWasteWaterSystem',  N'Co he thong nuoc thai','bool',    0, NULL,N'Môi trường',       N'Có hệ thống xử lý nước thải'),
        (41, N'HasSolidWasteSystem',  N'Co xu ly chat thai ran','bool',   0, NULL,N'Môi trường',       N'Có xử lý chất thải rắn'),
        (42, N'HasEmissionTreatment', N'Co xu ly khi thai',   'bool',     0, NULL,N'Môi trường',       N'Có xử lý khí thải'),
        (43, N'HasDedicatedZone',     N'Co khu san xuat rieng','bool',    0, NULL,N'Môi trường',       N'Có khu sản xuất tập trung'),
        (44, N'HasSupportPolicy',     N'Co chinh sach ho tro','bool',     0, NULL,N'Chính sách',       N'Có chính sách hỗ trợ'),
        (45, N'SupportFunding',       N'Kinh phi ho tro',     'decimal',  0, NULL,N'Chính sách',       N'Kinh phí hỗ trợ (VND)'),
        (46, N'HasTourismActivity',   N'Co hoat dong du lich','bool',     0, NULL,N'Du lịch',          N'Có hoạt động du lịch'),
        (47, N'HasTourLinkage',       N'Co lien ket du lich', 'bool',     0, NULL,N'Du lịch',          N'Có liên kết du lịch')
    ) AS v(FieldOrder, FieldName, ExcelColumnHeader, DataType, IsRequired, MaxLength, GroupName, Description)
)
INSERT INTO dbo.Import_TemplateConfig
    (TemplateCode, FieldName, ExcelColumnHeader, FieldOrder, IsRequired,
     DefaultValue, DataType, MaxLength, ValidationRegex, GroupName,
     Description, HelpText, IsActive, CreatedAt)
SELECT
    'V1_FULL', v.FieldName, v.ExcelColumnHeader, v.FieldOrder, v.IsRequired,
    NULL, v.DataType, v.MaxLength, NULL, v.GroupName,
    v.Description, NULL, 1, SYSUTCDATETIME()
FROM V1Full v
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Import_TemplateConfig t
    WHERE t.TemplateCode = 'V1_FULL' AND t.FieldName = v.FieldName
);
GO

/* ---------------------------------------------------------------------
   2) MONTHLY — 7 CỘT (cập nhật số liệu hàng tháng)
   --------------------------------------------------------------------- */
;WITH Monthly AS (
    SELECT * FROM (VALUES
        (1, N'VillageCode',   N'Ma lang nghe',   'string', 1, 50,  N'Định danh', N'Mã làng nghề cần cập nhật'),
        (2, N'ReportPeriod',  N'Ky bao cao',     'string', 1, 10,  N'Định danh', N'Kỳ báo cáo tháng (VD: 2024-01)'),
        (3, N'Revenue',       N'Doanh thu',      'decimal',1, NULL,N'Số liệu',   N'Doanh thu tháng (VND)'),
        (4, N'TotalLabor',    N'Tong lao dong',  'int',    1, NULL,N'Số liệu',   N'Tổng số lao động'),
        (5, N'HouseholdCount',N'So ho',          'int',    1, NULL,N'Số liệu',   N'Số hộ làm nghề'),
        (6, N'Output',        N'San luong',      'decimal',0, NULL,N'Số liệu',   N'Sản lượng tháng'),
        (7, N'GrowthRate',    N'Ty le tang truong','decimal',0,NULL,N'Số liệu',  N'Tỷ lệ tăng trưởng (%) -100..100')
    ) AS v(FieldOrder, FieldName, ExcelColumnHeader, DataType, IsRequired, MaxLength, GroupName, Description)
)
INSERT INTO dbo.Import_TemplateConfig
    (TemplateCode, FieldName, ExcelColumnHeader, FieldOrder, IsRequired,
     DefaultValue, DataType, MaxLength, ValidationRegex, GroupName,
     Description, HelpText, IsActive, CreatedAt)
SELECT
    'MONTHLY', v.FieldName, v.ExcelColumnHeader, v.FieldOrder, v.IsRequired,
    NULL, v.DataType, v.MaxLength, NULL, v.GroupName,
    v.Description, NULL, 1, SYSUTCDATETIME()
FROM Monthly v
WHERE NOT EXISTS (
    SELECT 1 FROM dbo.Import_TemplateConfig t
    WHERE t.TemplateCode = 'MONTHLY' AND t.FieldName = v.FieldName
);
GO

/* ---------------------------------------------------------------------
   3) KIỂM TRA KẾT QUẢ
   --------------------------------------------------------------------- */
SELECT TemplateCode, COUNT(*) AS SoCot
FROM dbo.Import_TemplateConfig
WHERE IsActive = 1
GROUP BY TemplateCode
ORDER BY TemplateCode;
GO
