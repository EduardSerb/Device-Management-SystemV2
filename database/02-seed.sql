/*
  Idempotent seed for dbo.Devices using MERGE on (Name, Manufacturer).
  Prerequisites: schema exists (see README). AssignedUserId is left NULL here;
  demo users/devices with assignments are created when you run the API (DatabaseSeeder).
*/
SET NOCOUNT ON;

IF DB_ID(N'DeviceManagement') IS NULL
BEGIN
    RAISERROR('Create the database first (01-create.sql).', 16, 1);
    RETURN;
END

USE [DeviceManagement];
GO

IF OBJECT_ID(N'dbo.Devices', N'U') IS NULL
BEGIN
    RAISERROR('Devices table is missing. Apply EF migrations or ef-migration-baseline.sql first.', 16, 1);
    RETURN;
END
GO

SET NOCOUNT ON;
USE [DeviceManagement];

MERGE dbo.Devices AS t
USING (VALUES
    (N'iPhone 17 Pro', N'Apple', 0, N'iOS', N'26.0', N'A19 Pro', N'12GB',
     N'A high-performance Apple smartphone running iOS, suitable for daily business use.'),
    (N'Pixel 10', N'Google', 0, N'Android', N'15', N'Tensor G4', N'12GB',
     N'Google flagship phone for testing and demos.'),
    (N'Galaxy Tab S10', N'Samsung', 1, N'Android', N'15', N'Snapdragon 8 Gen 3', N'12GB',
     N'Large screen tablet for field teams.')
) AS s ([Name], [Manufacturer], [Type], [OS], [OSVersion], [Processor], [RamAmount], [Description])
ON t.[Name] = s.[Name] AND t.[Manufacturer] = s.[Manufacturer]
WHEN NOT MATCHED THEN
    INSERT ([Name], [Manufacturer], [Type], [OS], [OSVersion], [Processor], [RamAmount], [Description], [AssignedUserId])
    VALUES (s.[Name], s.[Manufacturer], s.[Type], s.[OS], s.[OSVersion], s.[Processor], s.[RamAmount], s.[Description], NULL);
GO
