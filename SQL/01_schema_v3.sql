/* ==============================================================================
   CSDL LÀNG NGHỀ — SCRIPT V3.0 (FINAL)
   Tương thích: SQL Server 2022 + .NET 10
   Tự động: Bỏ qua Full-Text nếu chưa cài. Fix hết 4 lỗi của V1.
   Cấu trúc: 29 bảng | 3 Agg tables | Partition by year | Columnstore
   ============================================================================== */

USE master;
GO

-- ⚠️ BỎ COMMENT 5 DÒNG DƯỚI NẾU MUỐN XÓA DB CŨ VÀ TẠO LẠI TỪ ĐẦU
-- IF DB_ID('CSDL_LangNghe') IS NOT NULL
-- BEGIN
--     ALTER DATABASE CSDL_LangNghe SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
--     DROP DATABASE CSDL_LangNghe;
-- END
-- GO

IF DB_ID('CSDL_LangNghe') IS NULL
    CREATE DATABASE CSDL_LangNghe COLLATE Vietnamese_CI_AS;
GO

USE CSDL_LangNghe;
GO

ALTER DATABASE CSDL_LangNghe SET RECOVERY SIMPLE;
GO
ALTER DATABASE CSDL_LangNghe SET READ_COMMITTED_SNAPSHOT ON WITH ROLLBACK IMMEDIATE;
GO
ALTER DATABASE CSDL_LangNghe SET ALLOW_SNAPSHOT_ISOLATION ON;
GO
ALTER DATABASE CSDL_LangNghe SET QUERY_STORE = ON;
GO
ALTER DATABASE CSDL_LangNghe SET AUTO_CREATE_STATISTICS ON;
GO
ALTER DATABASE CSDL_LangNghe SET AUTO_UPDATE_STATISTICS_ASYNC ON;
GO

-- Full-Text Catalog (chỉ tạo nếu có cài feature)
IF SERVERPROPERTY('IsFullTextInstalled') = 1
BEGIN
    IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'ftLangNghe')
        CREATE FULLTEXT CATALOG ftLangNghe AS DEFAULT;
    PRINT N'✅ Full-Text Catalog đã sẵn sàng';
END
ELSE
    PRINT N'⚠️ Full-Text chưa cài — bỏ qua (search sẽ dùng LIKE)';
GO

/* =================== SECTION 1: PARTITION BY YEAR =================== */
IF NOT EXISTS (SELECT 1 FROM sys.partition_functions WHERE name = 'PF_ReportPeriod')
    CREATE PARTITION FUNCTION PF_ReportPeriod (VARCHAR(20))
    AS RANGE RIGHT FOR VALUES ('2021','2022','2023','2024','2025','2026','2027','2028','2029','2030');
GO
IF NOT EXISTS (SELECT 1 FROM sys.partition_schemes WHERE name = 'PS_ReportPeriod')
    CREATE PARTITION SCHEME PS_ReportPeriod
    AS PARTITION PF_ReportPeriod ALL TO ([PRIMARY]);
GO

/* =================== SECTION 2: DIMENSIONS =================== */

IF OBJECT_ID('dbo.Dim_Regions','U') IS NULL
CREATE TABLE Dim_Regions (
    Id              INT IDENTITY(1,1) NOT NULL,
    RegionCode      VARCHAR(20)   NOT NULL,
    Name            NVARCHAR(255) NOT NULL,
    ShortName       NVARCHAR(100) NULL,
    LevelType       TINYINT       NOT NULL,
    RegionType      NVARCHAR(50)  NULL,
    ParentId        INT           NULL,
    MergedFromCode  VARCHAR(20)   NULL,
    SortOrder       INT           NULL,
    IsActive        BIT           NOT NULL CONSTRAINT DF_Regions_IsActive DEFAULT 1,
    CreatedAt       DATETIME      NOT NULL CONSTRAINT DF_Regions_CreatedAt DEFAULT GETDATE(),
    UpdatedAt       DATETIME      NULL,
    CONSTRAINT PK_Dim_Regions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Dim_Regions_Code UNIQUE (RegionCode),
    CONSTRAINT CK_Dim_Regions_Level CHECK (LevelType IN (1,2)),
    CONSTRAINT FK_Dim_Regions_Parent FOREIGN KEY (ParentId) REFERENCES Dim_Regions(Id)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Regions_Parent')
    CREATE NONCLUSTERED INDEX IX_Regions_Parent ON Dim_Regions(ParentId, LevelType, IsActive) INCLUDE (RegionCode, Name);
GO

IF OBJECT_ID('dbo.Dim_RegionMapping','U') IS NULL
CREATE TABLE Dim_RegionMapping (
    Id              INT IDENTITY(1,1) NOT NULL,
    OldCode         VARCHAR(20)   NOT NULL,
    NewCode         VARCHAR(20)   NOT NULL,
    OldName         NVARCHAR(255) NULL,
    NewName         NVARCHAR(255) NULL,
    LevelType       TINYINT       NOT NULL,
    EffectiveDate   DATE          NOT NULL,
    DecreeNumber    NVARCHAR(200) NULL,
    Notes           NVARCHAR(500) NULL,
    CreatedAt       DATETIME      NOT NULL CONSTRAINT DF_RegionMapping_CreatedAt DEFAULT GETDATE(),
    CONSTRAINT PK_Dim_RegionMapping PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_RegionMapping UNIQUE (OldCode, NewCode, EffectiveDate)
);
GO

IF OBJECT_ID('dbo.Dim_VillageTypes','U') IS NULL
CREATE TABLE Dim_VillageTypes (
    Id              INT IDENTITY(1,1) NOT NULL,
    Code            VARCHAR(20)   NOT NULL,
    Name            NVARCHAR(255) NOT NULL,
    IsTraditional   BIT           NOT NULL DEFAULT 0,
    IsRestored      BIT           NOT NULL DEFAULT 0,
    IsNew           BIT           NOT NULL DEFAULT 0,
    SortOrder       INT           NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CONSTRAINT PK_Dim_VillageTypes PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Dim_VillageTypes_Code UNIQUE (Code)
);
GO

IF OBJECT_ID('dbo.Dim_IndustryGroups','U') IS NULL
CREATE TABLE Dim_IndustryGroups (
    Id              INT IDENTITY(1,1) NOT NULL,
    Code            VARCHAR(20)   NOT NULL,
    Name            NVARCHAR(500) NOT NULL,
    Description     NVARCHAR(1000) NULL,
    SortOrder       INT           NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CONSTRAINT PK_Dim_IndustryGroups PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Dim_IndustryGroups_Code UNIQUE (Code)
);
GO

IF OBJECT_ID('dbo.Dim_OutputUnits','U') IS NULL
CREATE TABLE Dim_OutputUnits (
    Id              INT IDENTITY(1,1) NOT NULL,
    Code            VARCHAR(20)   NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    SortOrder       INT           NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CONSTRAINT PK_Dim_OutputUnits PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Dim_OutputUnits_Code UNIQUE (Code)
);
GO

IF OBJECT_ID('dbo.Dim_ExportCategories','U') IS NULL
CREATE TABLE Dim_ExportCategories (
    Id              INT IDENTITY(1,1) NOT NULL,
    Code            VARCHAR(20)   NOT NULL,
    Name            NVARCHAR(255) NOT NULL,
    SortOrder       INT           NULL,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CONSTRAINT PK_Dim_ExportCategories PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Dim_ExportCategories_Code UNIQUE (Code)
);
GO

IF OBJECT_ID('dbo.Dim_Classifications','U') IS NULL
CREATE TABLE Dim_Classifications (
    Id              INT IDENTITY(1,1) NOT NULL,
    Code            VARCHAR(20)   NOT NULL,
    Name            NVARCHAR(100) NOT NULL,
    SortOrder       INT           NULL,
    CONSTRAINT PK_Dim_Classifications PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Dim_Classifications_Code UNIQUE (Code)
);
GO

/* =================== SECTION 3: MASTER DATA =================== */

IF OBJECT_ID('dbo.Dim_CraftVillages','U') IS NULL
CREATE TABLE Dim_CraftVillages (
    Id                  INT IDENTITY(1,1) NOT NULL,
    VillageCode         VARCHAR(20)   NOT NULL,
    Name                NVARCHAR(500) NOT NULL,
    RegionId            INT           NOT NULL,
    Address             NVARCHAR(500) NULL,
    RecognitionDecision NVARCHAR(500) NULL,
    RecognitionDate     DATE          NULL,
    ClassificationId    INT           NULL,
    VillageTypeId       INT           NULL,
    IndustryGroupId     INT           NULL,
    NeedRestoration     BIT           NOT NULL CONSTRAINT DF_Villages_NeedRestoration DEFAULT 0,
    Status              TINYINT       NOT NULL CONSTRAINT DF_Villages_Status DEFAULT 1,
    IsPublished         BIT           NOT NULL DEFAULT 0,
    PublishedAt         DATETIME      NULL,
    RowVersion          ROWVERSION    NOT NULL,
    CreatedAt           DATETIME      NOT NULL CONSTRAINT DF_Villages_CreatedAt DEFAULT GETDATE(),
    CreatedBy           INT           NULL,
    UpdatedAt           DATETIME      NULL,
    UpdatedBy           INT           NULL,
    CONSTRAINT PK_Dim_CraftVillages PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Dim_CraftVillages_Code UNIQUE (VillageCode),
    CONSTRAINT FK_Villages_Region FOREIGN KEY (RegionId) REFERENCES Dim_Regions(Id),
    CONSTRAINT FK_Villages_Class FOREIGN KEY (ClassificationId) REFERENCES Dim_Classifications(Id),
    CONSTRAINT FK_Villages_Type FOREIGN KEY (VillageTypeId) REFERENCES Dim_VillageTypes(Id),
    CONSTRAINT FK_Villages_Industry FOREIGN KEY (IndustryGroupId) REFERENCES Dim_IndustryGroups(Id),
    CONSTRAINT CK_Villages_Code CHECK (VillageCode LIKE '[0-9][0-9][0-9][0-9][0-9]'),
    CONSTRAINT CK_Villages_Status CHECK (Status IN (0,1,2))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Villages_Region')
    CREATE NONCLUSTERED INDEX IX_Villages_Region ON Dim_CraftVillages(RegionId, Status) INCLUDE (VillageCode, Name, VillageTypeId, IndustryGroupId);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Villages_Type')
    CREATE NONCLUSTERED INDEX IX_Villages_Type ON Dim_CraftVillages(VillageTypeId, Status) INCLUDE (Name, RegionId);
GO

-- Full-Text Index (chỉ nếu có cài)
IF SERVERPROPERTY('IsFullTextInstalled') = 1
   AND NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('dbo.Dim_CraftVillages'))
BEGIN
    CREATE FULLTEXT INDEX ON Dim_CraftVillages(Name LANGUAGE 1066) 
        KEY INDEX UQ_Dim_CraftVillages_Code ON ftLangNghe 
        WITH CHANGE_TRACKING AUTO;
    PRINT N'✅ Full-Text Index đã tạo trên Dim_CraftVillages';
END
GO

IF OBJECT_ID('dbo.Dim_Products','U') IS NULL
CREATE TABLE Dim_Products (
    Id                  INT IDENTITY(1,1) NOT NULL,
    CraftVillageId      INT           NOT NULL,
    ProductCode         VARCHAR(50)   NULL,
    Name                NVARCHAR(500) NOT NULL,
    Description         NVARCHAR(MAX) NULL,
    Materials           NVARCHAR(1000) NULL,
    ImageUrl            NVARCHAR(500) NULL,
    HasOCOP             BIT           NOT NULL DEFAULT 0,
    OcopStar            TINYINT       NULL,
    OcopEvaluatedYear   INT           NULL,
    MainMarket          NVARCHAR(500) NULL,
    SortOrder           INT           NULL,
    IsActive            BIT           NOT NULL DEFAULT 1,
    CreatedAt           DATETIME      NOT NULL CONSTRAINT DF_Products_CreatedAt DEFAULT GETDATE(),
    UpdatedAt           DATETIME      NULL,
    CONSTRAINT PK_Dim_Products PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Products_Village FOREIGN KEY (CraftVillageId) REFERENCES Dim_CraftVillages(Id),
    CONSTRAINT CK_Products_OcopStar CHECK (OcopStar IS NULL OR OcopStar IN (3,4,5))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Products_Village')
    CREATE NONCLUSTERED INDEX IX_Products_Village ON Dim_Products(CraftVillageId, IsActive) INCLUDE (Name, OcopStar);
GO

IF OBJECT_ID('dbo.Dim_Artisans','U') IS NULL
CREATE TABLE Dim_Artisans (
    Id                  INT IDENTITY(1,1) NOT NULL,
    CraftVillageId      INT           NOT NULL,
    FullName            NVARCHAR(255) NOT NULL,
    BirthYear           INT           NULL,
    Gender              NVARCHAR(10)  NULL,
    ArtisanLevel        NVARCHAR(100) NULL,
    RecognitionYear     INT           NULL,
    Specialty           NVARCHAR(500) NULL,
    Phone               VARCHAR(20)   NULL,
    IsActive            BIT           NOT NULL DEFAULT 1,
    CreatedAt           DATETIME      NOT NULL CONSTRAINT DF_Artisans_CreatedAt DEFAULT GETDATE(),
    UpdatedAt           DATETIME      NULL,
    CONSTRAINT PK_Dim_Artisans PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Artisans_Village FOREIGN KEY (CraftVillageId) REFERENCES Dim_CraftVillages(Id)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Artisans_Village')
    CREATE NONCLUSTERED INDEX IX_Artisans_Village ON Dim_Artisans(CraftVillageId, IsActive) INCLUDE (FullName, ArtisanLevel);
GO

/* =================== SECTION 4: FACTS =================== */

IF OBJECT_ID('dbo.Fact_Village_PeriodicMetrics','U') IS NULL
CREATE TABLE Fact_Village_PeriodicMetrics (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    CraftVillageId          INT             NOT NULL,
    CommuneId               INT             NOT NULL,
    ProvinceId              INT             NOT NULL,
    ReportPeriod            VARCHAR(20)     NOT NULL,
    PeriodType              TINYINT         NOT NULL,
    ImportBatchId           UNIQUEIDENTIFIER NULL,
    BrandProtection         BIT             NOT NULL DEFAULT 0,
    HasOCOP3Star            BIT             NOT NULL DEFAULT 0,
    ProductionProcess       NVARCHAR(50)    NULL,
    ExportRate              DECIMAL(5,2)    NOT NULL DEFAULT 0,
    ExportCategoryId        INT             NULL,
    ImportMaterialRate      DECIMAL(5,2)    NOT NULL DEFAULT 0,
    HouseholdCount          INT             NOT NULL DEFAULT 0,
    EnterpriseCount         INT             NOT NULL DEFAULT 0,
    ProductionArea          DECIMAL(18,2)   NULL,
    ActiveMonths            TINYINT         NULL,
    Output                  DECIMAL(18,2)   NULL,
    OutputUnitId            INT             NULL,
    Revenue                 DECIMAL(18,2)   NOT NULL DEFAULT 0,
    GrowthRate              DECIMAL(5,2)    NULL,
    TotalLabor              INT             NOT NULL DEFAULT 0,
    TrainedLaborRate        DECIMAL(5,2)    NULL,
    ArtisanCount            INT             NOT NULL DEFAULT 0,
    TrainingNeed            NVARCHAR(500)   NULL,
    AvgIncome               DECIMAL(18,2)   NULL,
    DigitalLevel            DECIMAL(5,2)    NULL,
    HasWebsite              BIT             NOT NULL DEFAULT 0,
    HasMuseum               BIT             NOT NULL DEFAULT 0,
    HasDocArchive           BIT             NOT NULL DEFAULT 0,
    HasWasteCollection      BIT             NOT NULL DEFAULT 0,
    HasWasteWaterSystem     BIT             NOT NULL DEFAULT 0,
    HasSolidWasteSystem     BIT             NOT NULL DEFAULT 0,
    HasEmissionTreatment    BIT             NOT NULL DEFAULT 0,
    HasDedicatedZone        BIT             NOT NULL DEFAULT 0,
    HasSupportPolicy        BIT             NOT NULL DEFAULT 0,
    SupportFunding          DECIMAL(18,2)   NULL,
    HasTourismActivity      BIT             NOT NULL DEFAULT 0,
    HasTourLinkage          BIT             NOT NULL DEFAULT 0,
    AnnualVisitors          BIGINT          NULL,
    ExportValueVND          AS (CAST(Revenue * ExportRate / 100.0 AS DECIMAL(18,2))) PERSISTED,
    IsEffective             AS (CASE WHEN ActiveMonths >= 9 AND Revenue > 0
                                        AND HasWasteCollection = 1 AND HasWasteWaterSystem = 1
                                        AND HasSolidWasteSystem = 1 AND HasEmissionTreatment = 1
                                     THEN 1 ELSE 0 END) PERSISTED,
    ImportedAt              DATETIME        NOT NULL CONSTRAINT DF_FactVillage_ImportedAt DEFAULT GETDATE(),
    DataSource              NVARCHAR(100)   NULL,
    CONSTRAINT PK_Fact_Village_PeriodicMetrics PRIMARY KEY CLUSTERED (ReportPeriod, Id) ON PS_ReportPeriod(ReportPeriod),
    CONSTRAINT FK_FactVillage_Village FOREIGN KEY (CraftVillageId) REFERENCES Dim_CraftVillages(Id),
    CONSTRAINT FK_FactVillage_Commune FOREIGN KEY (CommuneId) REFERENCES Dim_Regions(Id),
    CONSTRAINT FK_FactVillage_Province FOREIGN KEY (ProvinceId) REFERENCES Dim_Regions(Id),
    CONSTRAINT FK_FactVillage_ExportCat FOREIGN KEY (ExportCategoryId) REFERENCES Dim_ExportCategories(Id),
    CONSTRAINT FK_FactVillage_OutputUnit FOREIGN KEY (OutputUnitId) REFERENCES Dim_OutputUnits(Id),
    CONSTRAINT UQ_FactVillage_Period UNIQUE (CraftVillageId, ReportPeriod, PeriodType),
    CONSTRAINT CK_FactVillage_PeriodType CHECK (PeriodType IN (1,2,3,4)),
    CONSTRAINT CK_FactVillage_ExportRate CHECK (ExportRate BETWEEN 0 AND 100),
    CONSTRAINT CK_FactVillage_ImportRate CHECK (ImportMaterialRate BETWEEN 0 AND 100),
    CONSTRAINT CK_FactVillage_ExportImport CHECK (ExportRate + ImportMaterialRate <= 100),
    CONSTRAINT CK_FactVillage_ActiveMonths CHECK (ActiveMonths IS NULL OR ActiveMonths BETWEEN 1 AND 12),
    CONSTRAINT CK_FactVillage_TrainedRate CHECK (TrainedLaborRate IS NULL OR TrainedLaborRate BETWEEN 0 AND 100),
    CONSTRAINT CK_FactVillage_DigitalLevel CHECK (DigitalLevel IS NULL OR DigitalLevel BETWEEN 0 AND 100),
    CONSTRAINT CK_FactVillage_GrowthRate CHECK (GrowthRate IS NULL OR GrowthRate BETWEEN -100 AND 100),
    CONSTRAINT CK_FactVillage_ArtisanLabor CHECK (ArtisanCount <= TotalLabor),
    CONSTRAINT CK_FactVillage_Process CHECK (ProductionProcess IS NULL OR ProductionProcess IN (N'Truyền thống', N'Hiện đại', N'Kết hợp'))
) ON PS_ReportPeriod(ReportPeriod);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Fact_Province_Period')
    CREATE NONCLUSTERED INDEX IX_Fact_Province_Period ON Fact_Village_PeriodicMetrics(ProvinceId, ReportPeriod, PeriodType)
        INCLUDE (Revenue, TotalLabor, HouseholdCount, EnterpriseCount, ExportRate, IsEffective) ON PS_ReportPeriod(ReportPeriod);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Fact_Commune_Period')
    CREATE NONCLUSTERED INDEX IX_Fact_Commune_Period ON Fact_Village_PeriodicMetrics(CommuneId, ReportPeriod, PeriodType)
        INCLUDE (Revenue, TotalLabor, HouseholdCount) ON PS_ReportPeriod(ReportPeriod);
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Fact_Village')
    CREATE NONCLUSTERED INDEX IX_Fact_Village ON Fact_Village_PeriodicMetrics(CraftVillageId, ReportPeriod DESC)
        INCLUDE (Revenue, TotalLabor, ActiveMonths) ON PS_ReportPeriod(ReportPeriod);
GO

-- Columnstore: BỎ 2 computed columns (ExportValueVND, IsEffective)
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'CCI_Fact_Village')
CREATE NONCLUSTERED COLUMNSTORE INDEX CCI_Fact_Village ON Fact_Village_PeriodicMetrics
    (ProvinceId, CommuneId, CraftVillageId, ReportPeriod, PeriodType, Revenue, TotalLabor,
     HouseholdCount, EnterpriseCount, ExportRate, TrainedLaborRate, DigitalLevel, AvgIncome)
    ON PS_ReportPeriod(ReportPeriod);
GO

IF OBJECT_ID('dbo.Fact_Product_OCOP_History','U') IS NULL
CREATE TABLE Fact_Product_OCOP_History (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    ProductId           INT           NOT NULL,
    CraftVillageId      INT           NOT NULL,
    EvaluatedYear       INT           NOT NULL,
    OcopStar            TINYINT       NOT NULL,
    YieldCapacity       NVARCHAR(255) NULL,
    MainMarket          NVARCHAR(500) NULL,
    EvaluatedBy         NVARCHAR(255) NULL,
    Notes               NVARCHAR(1000) NULL,
    ImportedAt          DATETIME      NOT NULL CONSTRAINT DF_OCOP_ImportedAt DEFAULT GETDATE(),
    CONSTRAINT PK_Fact_OCOP PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_OCOP_Product FOREIGN KEY (ProductId) REFERENCES Dim_Products(Id),
    CONSTRAINT FK_OCOP_Village FOREIGN KEY (CraftVillageId) REFERENCES Dim_CraftVillages(Id),
    CONSTRAINT UQ_OCOP_Product_Year UNIQUE (ProductId, EvaluatedYear),
    CONSTRAINT CK_OCOP_Star CHECK (OcopStar IN (3,4,5))
);
GO

IF OBJECT_ID('dbo.Fact_Village_Environment','U') IS NULL
CREATE TABLE Fact_Village_Environment (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    CraftVillageId          INT             NOT NULL,
    SurveyPeriod            VARCHAR(20)     NOT NULL,
    PollutionLevel          TINYINT         NULL,
    WasteWaterSystemStatus  TINYINT         NULL,
    SolidWasteSystemStatus  TINYINT         NULL,
    EmissionSystemStatus    TINYINT         NULL,
    Notes                   NVARCHAR(MAX)   NULL,
    ImportedAt              DATETIME        NOT NULL CONSTRAINT DF_Env_ImportedAt DEFAULT GETDATE(),
    CONSTRAINT PK_Fact_Env PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Env_Village FOREIGN KEY (CraftVillageId) REFERENCES Dim_CraftVillages(Id),
    CONSTRAINT UQ_Env_Village_Period UNIQUE (CraftVillageId, SurveyPeriod)
);
GO

/* =================== SECTION 5: AGGREGATED =================== */

IF OBJECT_ID('dbo.Agg_Village_TimeSeries','U') IS NULL
CREATE TABLE Agg_Village_TimeSeries (
    Id                      BIGINT IDENTITY(1,1) NOT NULL,
    RegionId                INT             NOT NULL,
    RegionLevel             TINYINT         NOT NULL,
    ParentRegionId          INT             NULL,
    Period                  VARCHAR(20)     NOT NULL,
    PeriodType              TINYINT         NOT NULL,
    TotalVillages           INT             NOT NULL DEFAULT 0,
    TraditionalVillages     INT             NOT NULL DEFAULT 0,
    NewVillages             INT             NOT NULL DEFAULT 0,
    RestoredVillages        INT             NOT NULL DEFAULT 0,
    OcopVillages            INT             NOT NULL DEFAULT 0,
    EffectiveVillages       INT             NOT NULL DEFAULT 0,
    TotalHouseholds         INT             NOT NULL DEFAULT 0,
    TotalEnterprises        INT             NOT NULL DEFAULT 0,
    TotalLabor              BIGINT          NOT NULL DEFAULT 0,
    TrainedLabor            BIGINT          NOT NULL DEFAULT 0,
    TotalArtisans           INT             NOT NULL DEFAULT 0,
    TotalArea               DECIMAL(18,2)   NULL,
    TotalRevenue            DECIMAL(18,2)   NOT NULL DEFAULT 0,
    TotalExportValue        DECIMAL(18,2)   NOT NULL DEFAULT 0,
    AvgRevenuePerVillage    DECIMAL(18,2)   NULL,
    AvgIncome               DECIMAL(18,2)   NULL,
    AvgGrowthRate           DECIMAL(5,2)    NULL,
    DigitalizedVillages     INT             NOT NULL DEFAULT 0,
    EnvCompliantVillages    INT             NOT NULL DEFAULT 0,
    TourismVillages         INT             NOT NULL DEFAULT 0,
    TotalVisitors           BIGINT          NOT NULL DEFAULT 0,
    LastUpdated             DATETIME        NOT NULL CONSTRAINT DF_Agg_LastUpdated DEFAULT GETDATE(),
    CalculatedBy            NVARCHAR(100)   NULL,
    CONSTRAINT PK_Agg_Village_TimeSeries PRIMARY KEY CLUSTERED (RegionId, Period, PeriodType),
    CONSTRAINT FK_Agg_Region FOREIGN KEY (RegionId) REFERENCES Dim_Regions(Id),
    CONSTRAINT CK_Agg_RegionLevel CHECK (RegionLevel IN (1,2)),
    CONSTRAINT CK_Agg_PeriodType CHECK (PeriodType IN (1,2,3,4))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Agg_Period_Level')
    CREATE NONCLUSTERED INDEX IX_Agg_Period_Level ON Agg_Village_TimeSeries(Period, RegionLevel, RegionId) INCLUDE (TotalVillages, TotalRevenue, TotalLabor);
GO

-- Agg_By_IndustryGroup — RegionId/RegionLevel NOT NULL (0 = Toàn quốc)
IF OBJECT_ID('dbo.Agg_By_IndustryGroup','U') IS NULL
CREATE TABLE Agg_By_IndustryGroup (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    IndustryGroupId     INT           NOT NULL,
    RegionId            INT           NOT NULL,
    RegionLevel         TINYINT       NOT NULL,
    Period              VARCHAR(20)   NOT NULL,
    PeriodType          TINYINT       NOT NULL,
    TotalVillages       INT           NOT NULL DEFAULT 0,
    TotalRevenue        DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalLabor          BIGINT        NOT NULL DEFAULT 0,
    TotalExportValue    DECIMAL(18,2) NOT NULL DEFAULT 0,
    LastUpdated         DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Agg_Industry PRIMARY KEY CLUSTERED (IndustryGroupId, RegionId, Period, PeriodType),
    CONSTRAINT FK_Agg_Industry_Group FOREIGN KEY (IndustryGroupId) REFERENCES Dim_IndustryGroups(Id),
    CONSTRAINT CK_Agg_Industry_RegionLevel CHECK (RegionLevel IN (0,1,2))
);
GO

-- Agg_By_VillageType — RegionId/RegionLevel NOT NULL (0 = Toàn quốc)
IF OBJECT_ID('dbo.Agg_By_VillageType','U') IS NULL
CREATE TABLE Agg_By_VillageType (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    VillageTypeId       INT           NOT NULL,
    RegionId            INT           NOT NULL,
    RegionLevel         TINYINT       NOT NULL,
    Period              VARCHAR(20)   NOT NULL,
    PeriodType          TINYINT       NOT NULL,
    TotalVillages       INT           NOT NULL DEFAULT 0,
    TotalRevenue        DECIMAL(18,2) NOT NULL DEFAULT 0,
    TotalLabor          BIGINT        NOT NULL DEFAULT 0,
    LastUpdated         DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Agg_VillageType PRIMARY KEY CLUSTERED (VillageTypeId, RegionId, Period, PeriodType),
    CONSTRAINT FK_Agg_Type FOREIGN KEY (VillageTypeId) REFERENCES Dim_VillageTypes(Id),
    CONSTRAINT CK_Agg_VillageType_RegionLevel CHECK (RegionLevel IN (0,1,2))
);
GO

/* =================== SECTION 6: SYSTEM =================== */

IF OBJECT_ID('dbo.Sys_Users','U') IS NULL
CREATE TABLE Sys_Users (
    Id                  INT IDENTITY(1,1) NOT NULL,
    Username            VARCHAR(100)    NOT NULL,
    PasswordHash        VARCHAR(500)    NOT NULL,
    FullName            NVARCHAR(255)   NULL,
    Email               VARCHAR(255)    NULL,
    Phone               VARCHAR(20)     NULL,
    RegionId            INT             NULL,
    DataScope           TINYINT         NOT NULL,
    LevelType           TINYINT         NULL,
    IsActive            BIT             NOT NULL DEFAULT 1,
    LockedUntil         DATETIME        NULL,
    FailedLoginCount    INT             NOT NULL DEFAULT 0,
    MustChangePassword  BIT             NOT NULL DEFAULT 0,
    LastLoginAt         DATETIME        NULL,
    LastLoginIp         VARCHAR(50)     NULL,
    CreatedAt           DATETIME        NOT NULL DEFAULT GETDATE(),
    CreatedBy           INT             NULL,
    UpdatedAt           DATETIME        NULL,
    UpdatedBy           INT             NULL,
    CONSTRAINT PK_Sys_Users PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Sys_Users_Username UNIQUE (Username),
    CONSTRAINT FK_Sys_Users_Region FOREIGN KEY (RegionId) REFERENCES Dim_Regions(Id),
    CONSTRAINT CK_Sys_Users_DataScope CHECK (DataScope IN (1,2,3))
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Users_Region')
    CREATE NONCLUSTERED INDEX IX_Users_Region ON Sys_Users(RegionId, DataScope, IsActive) INCLUDE (Username, FullName, Email);
GO

IF OBJECT_ID('dbo.Sys_Roles','U') IS NULL
CREATE TABLE Sys_Roles (
    Id              INT IDENTITY(1,1) NOT NULL,
    Code            VARCHAR(50)   NOT NULL,
    Name            NVARCHAR(255) NOT NULL,
    Level           TINYINT       NOT NULL,
    Description     NVARCHAR(500) NULL,
    IsSystem        BIT           NOT NULL DEFAULT 0,
    IsActive        BIT           NOT NULL DEFAULT 1,
    CreatedAt       DATETIME      NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Sys_Roles PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Sys_Roles_Code UNIQUE (Code),
    CONSTRAINT CK_Sys_Roles_Level CHECK (Level IN (1,2,3))
);
GO

IF OBJECT_ID('dbo.Sys_Permissions','U') IS NULL
CREATE TABLE Sys_Permissions (
    Id              INT IDENTITY(1,1) NOT NULL,
    Code            VARCHAR(100)  NOT NULL,
    Module          NVARCHAR(50)  NOT NULL,
    Action          NVARCHAR(50)  NOT NULL,
    Description     NVARCHAR(500) NULL,
    CONSTRAINT PK_Sys_Permissions PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Sys_Permissions_Code UNIQUE (Code)
);
GO

IF OBJECT_ID('dbo.Sys_RolePermissions','U') IS NULL
CREATE TABLE Sys_RolePermissions (
    RoleId          INT NOT NULL,
    PermissionId    INT NOT NULL,
    GrantedAt       DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Sys_RolePermissions PRIMARY KEY (RoleId, PermissionId),
    CONSTRAINT FK_RolePerm_Role FOREIGN KEY (RoleId) REFERENCES Sys_Roles(Id) ON DELETE CASCADE,
    CONSTRAINT FK_RolePerm_Perm FOREIGN KEY (PermissionId) REFERENCES Sys_Permissions(Id) ON DELETE CASCADE
);
GO

IF OBJECT_ID('dbo.Sys_UserRoles','U') IS NULL
CREATE TABLE Sys_UserRoles (
    UserId          INT NOT NULL,
    RoleId          INT NOT NULL,
    AssignedAt      DATETIME NOT NULL DEFAULT GETDATE(),
    AssignedBy      INT NULL,
    CONSTRAINT PK_Sys_UserRoles PRIMARY KEY (UserId, RoleId),
    CONSTRAINT FK_UserRole_User FOREIGN KEY (UserId) REFERENCES Sys_Users(Id) ON DELETE CASCADE,
    CONSTRAINT FK_UserRole_Role FOREIGN KEY (RoleId) REFERENCES Sys_Roles(Id)
);
GO

IF OBJECT_ID('dbo.Sys_AuditLog','U') IS NULL
CREATE TABLE Sys_AuditLog (
    Id              BIGINT IDENTITY(1,1) NOT NULL,
    UserId          INT             NULL,
    Username        VARCHAR(100)    NULL,
    Action          VARCHAR(50)     NOT NULL,
    EntityName      VARCHAR(100)    NOT NULL,
    EntityId        VARCHAR(50)     NULL,
    OldValues       NVARCHAR(MAX)   NULL,
    NewValues       NVARCHAR(MAX)   NULL,
    IpAddress       VARCHAR(50)     NULL,
    UserAgent       NVARCHAR(500)   NULL,
    CreatedAt       DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Sys_AuditLog PRIMARY KEY CLUSTERED (Id)
);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Audit_User_Time')
    CREATE NONCLUSTERED INDEX IX_Audit_User_Time ON Sys_AuditLog(UserId, CreatedAt DESC) INCLUDE (Action, EntityName, EntityId);
GO

IF OBJECT_ID('dbo.Fact_SubmissionWorkflow','U') IS NULL
CREATE TABLE Fact_SubmissionWorkflow (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    CraftVillageId      INT           NOT NULL,
    ReportPeriod        VARCHAR(20)   NOT NULL,
    CurrentStatus       TINYINT       NOT NULL,
    CreatedByXa         INT           NOT NULL,
    CreatedAt           DATETIME      NOT NULL DEFAULT GETDATE(),
    SentToProvinceAt    DATETIME      NULL,
    ApprovedByTinh      INT           NULL,
    ApprovedByTinhAt    DATETIME      NULL,
    PublishedByTW       INT           NULL,
    PublishedAt         DATETIME      NULL,
    RejectedBy          INT           NULL,
    RejectedAt          DATETIME      NULL,
    RejectionReason     NVARCHAR(1000) NULL,
    Version             INT           NOT NULL DEFAULT 1,
    Notes               NVARCHAR(1000) NULL,
    CONSTRAINT PK_Fact_Workflow PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Workflow_Village FOREIGN KEY (CraftVillageId) REFERENCES Dim_CraftVillages(Id),
    CONSTRAINT FK_Workflow_Xa FOREIGN KEY (CreatedByXa) REFERENCES Sys_Users(Id),
    CONSTRAINT FK_Workflow_Tinh FOREIGN KEY (ApprovedByTinh) REFERENCES Sys_Users(Id),
    CONSTRAINT FK_Workflow_TW FOREIGN KEY (PublishedByTW) REFERENCES Sys_Users(Id),
    CONSTRAINT FK_Workflow_Reject FOREIGN KEY (RejectedBy) REFERENCES Sys_Users(Id),
    CONSTRAINT UQ_Workflow_Version UNIQUE (CraftVillageId, ReportPeriod, Version),
    CONSTRAINT CK_Workflow_Status CHECK (CurrentStatus IN (0,1,2,3,9))
);
GO

/* =================== SECTION 7: IMPORT / STAGING =================== */

IF OBJECT_ID('dbo.Import_History','U') IS NULL
CREATE TABLE Import_History (
    Id                      UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    FileName                NVARCHAR(500)   NOT NULL,
    SheetName               NVARCHAR(100)   NULL,
    UploadedBy              INT             NOT NULL,
    UploadedByRegionId      INT             NULL,
    FileSizeBytes           BIGINT          NULL,
    FileHash                VARCHAR(64)     NULL,
    BlobPath                NVARCHAR(500)   NULL,
    TemplateCode            VARCHAR(50)     NULL,
    ImportMode              VARCHAR(20)     NULL,
    ProvidedFieldsJson      NVARCHAR(MAX)   NULL,
    MissingRequiredJson     NVARCHAR(MAX)   NULL,
    TotalRows               INT             NOT NULL DEFAULT 0,
    SuccessRows             INT             NOT NULL DEFAULT 0,
    ErrorRows               INT             NOT NULL DEFAULT 0,
    WarningRows             INT             NOT NULL DEFAULT 0,
    Status                  TINYINT         NOT NULL DEFAULT 1,
    ErrorSummary            NVARCHAR(MAX)   NULL,
    StartedAt               DATETIME        NOT NULL DEFAULT GETDATE(),
    CompletedAt             DATETIME        NULL,
    CONSTRAINT PK_Import_History PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Import_User FOREIGN KEY (UploadedBy) REFERENCES Sys_Users(Id),
    CONSTRAINT FK_Import_Region FOREIGN KEY (UploadedByRegionId) REFERENCES Dim_Regions(Id),
    CONSTRAINT CK_Import_Status CHECK (Status IN (1,2,3,4,5))
);
GO

IF OBJECT_ID('dbo.Staging_RawImport','U') IS NULL
CREATE TABLE Staging_RawImport (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    ImportBatchId       UNIQUEIDENTIFIER NOT NULL,
    RowNumber           INT             NOT NULL,
    SheetName           NVARCHAR(100)   NULL,
    RawData             NVARCHAR(MAX)   NOT NULL,
    ParsedVillageCode   VARCHAR(20)     NULL,
    ParsedRegionId      INT             NULL,
    ValidationStatus    TINYINT         NOT NULL DEFAULT 0,
    ValidationErrors    NVARCHAR(MAX)   NULL,
    PersistedEntityId   INT             NULL,
    PersistedAt         DATETIME        NULL,
    CreatedAt           DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Staging_RawImport PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Staging_Import FOREIGN KEY (ImportBatchId) REFERENCES Import_History(Id),
    CONSTRAINT CK_Staging_Status CHECK (ValidationStatus IN (0,1,2,3))
);
GO

IF OBJECT_ID('dbo.Import_Error_Log','U') IS NULL
CREATE TABLE Import_Error_Log (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    ImportBatchId       UNIQUEIDENTIFIER NOT NULL,
    RowNumber           INT             NOT NULL,
    ColumnName          NVARCHAR(100)   NULL,
    ErrorCode           VARCHAR(50)     NULL,
    ErrorMessage        NVARCHAR(1000)  NOT NULL,
    RawValue            NVARCHAR(500)   NULL,
    Severity            TINYINT         NOT NULL DEFAULT 1,
    CreatedAt           DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Import_Error_Log PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_ImportError_Import FOREIGN KEY (ImportBatchId) REFERENCES Import_History(Id)
);
GO

IF OBJECT_ID('dbo.Import_TemplateConfig','U') IS NULL
CREATE TABLE Import_TemplateConfig (
    Id                  INT IDENTITY(1,1) NOT NULL,
    TemplateCode        VARCHAR(50)     NOT NULL,
    FieldName           NVARCHAR(100)   NOT NULL,
    ExcelColumnHeader   NVARCHAR(500)   NOT NULL,
    FieldOrder          INT             NOT NULL,
    IsRequired          BIT             NOT NULL DEFAULT 0,
    DefaultValue        NVARCHAR(500)   NULL,
    DataType            VARCHAR(20)     NOT NULL,
    MaxLength           INT             NULL,
    ValidationRegex     VARCHAR(500)    NULL,
    GroupName           NVARCHAR(100)   NULL,
    Description         NVARCHAR(500)   NULL,
    HelpText            NVARCHAR(1000)  NULL,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CreatedAt           DATETIME        NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_Import_TemplateConfig PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_Template_Field UNIQUE (TemplateCode, FieldName)
);
GO

IF OBJECT_ID('dbo.Sys_FormFieldConfig','U') IS NULL
CREATE TABLE Sys_FormFieldConfig (
    Id                  INT IDENTITY(1,1) NOT NULL,
    FieldName           NVARCHAR(100)   NOT NULL,
    DisplayName         NVARCHAR(255)   NOT NULL,
    TabName             NVARCHAR(100)   NOT NULL,
    FieldOrder          INT             NOT NULL,
    ControlType         VARCHAR(50)     NOT NULL,
    DropdownSource      NVARCHAR(200)   NULL,
    IsRequired          BIT             NOT NULL DEFAULT 0,
    DefaultValue        NVARCHAR(500)   NULL,
    Placeholder         NVARCHAR(500)   NULL,
    HelpText            NVARCHAR(1000)  NULL,
    MaxLength           INT             NULL,
    MinValue            DECIMAL(18,2)   NULL,
    MaxValue            DECIMAL(18,2)   NULL,
    VisibleForScopes    VARCHAR(50)     NULL,
    EditableForScopes   VARCHAR(50)     NULL,
    IsActive            BIT             NOT NULL DEFAULT 1,
    CONSTRAINT PK_Sys_FormFieldConfig PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT UQ_FormField_FieldName UNIQUE (FieldName)
);
GO

IF OBJECT_ID('dbo.Fact_Village_Draft','U') IS NULL
CREATE TABLE Fact_Village_Draft (
    Id                  BIGINT IDENTITY(1,1) NOT NULL,
    CraftVillageId      INT             NULL,
    DraftVillageCode    VARCHAR(20)     NULL,
    DraftData           NVARCHAR(MAX)   NOT NULL,
    CreatedByUserId     INT             NOT NULL,
    CreatedAt           DATETIME        NOT NULL DEFAULT GETDATE(),
    UpdatedAt           DATETIME        NOT NULL DEFAULT GETDATE(),
    AutoSavedAt         DATETIME        NULL,
    DraftStatus         TINYINT         NOT NULL DEFAULT 0,
    SubmittedAt         DATETIME        NULL,
    CONSTRAINT PK_Fact_Village_Draft PRIMARY KEY CLUSTERED (Id),
    CONSTRAINT FK_Draft_User FOREIGN KEY (CreatedByUserId) REFERENCES Sys_Users(Id)
);
GO

/* =================== SECTION 8: SEED DATA =================== */

IF NOT EXISTS (SELECT 1 FROM Dim_VillageTypes)
INSERT INTO Dim_VillageTypes (Code, Name, IsTraditional, IsRestored, IsNew, SortOrder) VALUES
    ('LNTT', N'Làng nghề truyền thống', 1, 0, 0, 1),
    ('LNM',  N'Làng nghề mới', 0, 0, 1, 2),
    ('LNKP', N'Làng nghề được khôi phục, bảo tồn', 0, 1, 0, 3),
    ('NTT',  N'Nghề truyền thống', 1, 0, 0, 4),
    ('NNM',  N'Nghề mới', 0, 0, 1, 5),
    ('NKP',  N'Nghề được khôi phục, bảo tồn', 0, 1, 0, 6);
GO

IF NOT EXISTS (SELECT 1 FROM Dim_IndustryGroups)
INSERT INTO Dim_IndustryGroups (Code, Name, SortOrder) VALUES
    ('CB',    N'Chế biến, bảo quản nông, lâm, thủy sản', 1),
    ('TCMN',  N'Sản xuất hàng thủ công mỹ nghệ', 2),
    ('XLCB',  N'Xử lý, chế biến nguyên vật liệu phục vụ sản xuất ngành nghề nông thôn', 3),
    ('GOMAY', N'Sản xuất đồ gỗ, mây tre đan, gốm sứ, thủy tinh, dệt may, sợi, thêu ren, đan lát, cơ khí nhỏ', 4),
    ('SVC',   N'Sản xuất và kinh doanh sinh vật cảnh', 5),
    ('MUOI',  N'Sản xuất muối', 6),
    ('DV',    N'Các dịch vụ phục vụ sản xuất, đời sống dân cư nông thôn', 7);
GO

IF NOT EXISTS (SELECT 1 FROM Dim_OutputUnits)
INSERT INTO Dim_OutputUnits (Code, Name, SortOrder) VALUES
    ('TAN', N'tấn', 1), ('KG', N'kg', 2), ('SP', N'sản phẩm', 3),
    ('BO', N'bộ', 4), ('CHIEC', N'chiếc', 5), ('NGHIN_BO', N'nghìn bộ', 6),
    ('M3', N'm3', 7), ('LIT', N'lít', 8), ('NGHIN_LIT', N'nghìn lít', 9),
    ('CON', N'con', 10), ('NGHIN_CON', N'nghìn con', 11), ('NGHIN_CHIEC', N'nghìn chiếc', 12),
    ('NGHIN_BANH', N'nghìn bánh', 13), ('TRIEU_BANH', N'triệu bánh', 14), ('KHAC', N'khác', 99);
GO

IF NOT EXISTS (SELECT 1 FROM Dim_ExportCategories)
INSERT INTO Dim_ExportCategories (Code, Name, SortOrder) VALUES
    ('GO', N'Gỗ và sản phẩm gỗ', 1),
    ('GOM', N'Sản phẩm gốm, sứ', 2),
    ('THUYTINH', N'Thủy tinh và các sản phẩm thủy tinh', 3),
    ('MAYTRE', N'Sản phẩm mây, tre, cói, thảm', 4),
    ('KHAC', N'Khác', 99);
GO

IF NOT EXISTS (SELECT 1 FROM Dim_Classifications)
INSERT INTO Dim_Classifications (Code, Name, SortOrder) VALUES
    ('LN', N'Làng nghề', 1),
    ('NGHE', N'Nghề', 2);
GO

IF NOT EXISTS (SELECT 1 FROM Sys_Roles)
INSERT INTO Sys_Roles (Code, Name, Level, Description, IsSystem) VALUES
    ('ADMIN_TW',    N'Quản trị TW',   1, N'Bộ NN&PTNT - toàn quyền', 1),
    ('SO_NN',       N'Sở NN&PTNT',    2, N'Cán bộ Sở tỉnh', 1),
    ('XA_CB',       N'Cán bộ Xã',     3, N'Cán bộ NN xã/phường', 1),
    ('VIEWER_TW',   N'Xem TW',        1, N'Chỉ xem toàn quốc', 1),
    ('VIEWER_TINH', N'Xem Tỉnh',      2, N'Chỉ xem cấp tỉnh', 1);
GO

IF NOT EXISTS (SELECT 1 FROM Sys_Permissions)
INSERT INTO Sys_Permissions (Code, Module, Action, Description) VALUES
    ('VILLAGE.READ',    'Village',  'Read',    N'Xem làng nghề'),
    ('VILLAGE.CREATE',  'Village',  'Create',  N'Tạo làng nghề'),
    ('VILLAGE.UPDATE',  'Village',  'Update',  N'Sửa làng nghề'),
    ('VILLAGE.DELETE',  'Village',  'Delete',  N'Xóa làng nghề'),
    ('VILLAGE.APPROVE', 'Village',  'Approve', N'Duyệt dữ liệu'),
    ('VILLAGE.PUBLISH', 'Village',  'Publish', N'Công bố chính thức'),
    ('IMPORT.UPLOAD',   'Import',   'Upload',  N'Upload file Excel'),
    ('IMPORT.ROLLBACK', 'Import',   'Rollback',N'Rollback import'),
    ('REPORT.VIEW',     'Report',   'View',    N'Xem báo cáo'),
    ('REPORT.EXPORT',   'Report',   'Export',  N'Xuất Excel'),
    ('USER.MANAGE',     'User',     'Manage',  N'Quản lý user'),
    ('DIM.MANAGE',      'Dimension','Manage',  N'Quản lý danh mục'),
    ('AUDIT.VIEW',      'Audit',    'View',    N'Xem audit log'),
    ('CONFIG.MANAGE',   'Config',   'Manage',  N'Cấu hình hệ thống');
GO

IF NOT EXISTS (SELECT 1 FROM Sys_RolePermissions)
BEGIN
    INSERT INTO Sys_RolePermissions (RoleId, PermissionId)
    SELECT r.Id, p.Id FROM Sys_Roles r CROSS JOIN Sys_Permissions p WHERE r.Code = 'ADMIN_TW';
    
    INSERT INTO Sys_RolePermissions (RoleId, PermissionId)
    SELECT r.Id, p.Id FROM Sys_Roles r CROSS JOIN Sys_Permissions p
    WHERE r.Code = 'SO_NN' AND p.Code IN ('VILLAGE.READ','VILLAGE.UPDATE','VILLAGE.APPROVE','IMPORT.UPLOAD','REPORT.VIEW','REPORT.EXPORT','AUDIT.VIEW');
    
    INSERT INTO Sys_RolePermissions (RoleId, PermissionId)
    SELECT r.Id, p.Id FROM Sys_Roles r CROSS JOIN Sys_Permissions p
    WHERE r.Code = 'XA_CB' AND p.Code IN ('VILLAGE.READ','VILLAGE.CREATE','VILLAGE.UPDATE','IMPORT.UPLOAD','REPORT.VIEW','REPORT.EXPORT');
END
GO

IF NOT EXISTS (SELECT 1 FROM Sys_Users WHERE Username = 'admin')
BEGIN
    INSERT INTO Sys_Users (Username, PasswordHash, FullName, Email, DataScope, LevelType, MustChangePassword)
    VALUES ('admin', '$2a$11$REPLACE_WITH_BCRYPT_HASH', N'Quản trị viên', 'admin@langnghe.gov.vn', 1, 1, 1);
    
    INSERT INTO Sys_UserRoles (UserId, RoleId)
    SELECT u.Id, r.Id FROM Sys_Users u, Sys_Roles r
    WHERE u.Username = 'admin' AND r.Code = 'ADMIN_TW';
END
GO

/* =================== SECTION 9: VERIFY =================== */
PRINT N'';
PRINT N'========================================';
PRINT N'✅ VERIFY CSDL LÀNG NGHỀ V3.0';
PRINT N'========================================';

SELECT 
    SERVERPROPERTY('ProductVersion') AS SQLVersion,
    SERVERPROPERTY('IsFullTextInstalled') AS FullText;

SELECT COUNT(*) AS TotalTables 
FROM sys.tables WHERE schema_id = SCHEMA_ID('dbo');
-- Kỳ vọng: 29

SELECT name AS AggTables FROM sys.tables WHERE name LIKE 'Agg_%' ORDER BY name;
-- Kỳ vọng: 3 dòng

SELECT i.name, i.type_desc FROM sys.indexes i
WHERE i.type_desc = 'NONCLUSTERED COLUMNSTORE';
-- Kỳ vọng: 1 dòng CCI_Fact_Village

SELECT name, collation_name FROM sys.databases WHERE name = 'CSDL_LangNghe';
-- Kỳ vọng: Vietnamese_CI_AS

PRINT N'🎉 HOÀN TẤT!';
GO