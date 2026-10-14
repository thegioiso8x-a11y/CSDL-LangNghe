-- Thêm cột ImageUrl cho Dim_CraftVillages
IF COL_LENGTH('dbo.Dim_CraftVillages', 'ImageUrl') IS NULL
    ALTER TABLE Dim_CraftVillages ADD ImageUrl NVARCHAR(500) NULL;
GO

-- Thêm cột ImageUrl cho Dim_Artisans
IF COL_LENGTH('dbo.Dim_Artisans', 'ImageUrl') IS NULL
    ALTER TABLE Dim_Artisans ADD ImageUrl NVARCHAR(500) NULL;
GO
