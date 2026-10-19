-- Thêm cột tọa độ cho Dim_CraftVillages
IF COL_LENGTH('dbo.Dim_CraftVillages', 'Latitude') IS NULL
    ALTER TABLE Dim_CraftVillages ADD Latitude DECIMAL(10,7) NULL;
GO
IF COL_LENGTH('dbo.Dim_CraftVillages', 'Longitude') IS NULL
    ALTER TABLE Dim_CraftVillages ADD Longitude DECIMAL(10,7) NULL;
GO
IF COL_LENGTH('dbo.Dim_CraftVillages', 'GeocodedAt') IS NULL
    ALTER TABLE Dim_CraftVillages ADD GeocodedAt DATETIME NULL;
GO
IF COL_LENGTH('dbo.Dim_CraftVillages', 'GeocodeSource') IS NULL
    ALTER TABLE Dim_CraftVillages ADD GeocodeSource VARCHAR(50) NULL;
GO
