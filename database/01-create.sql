/*
  Idempotent: creates the DeviceManagement database if it does not exist.
  Full schema (ASP.NET Identity + Devices) is applied by either:
    - dotnet ef database update --project src/DeviceManagement.Api
    - or executing database/ef-migration-baseline.sql against this database after creation.
*/
IF DB_ID(N'DeviceManagement') IS NULL
BEGIN
    CREATE DATABASE [DeviceManagement];
END
GO
