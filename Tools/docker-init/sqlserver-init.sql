-- Run by the sqlserver-init service in Tools/docker-compose.yml.
-- Idempotent: safe to run again against a container that is already set up.
-- The login and password match "ConnString" in src\Test.Net.Sf.Dbdeploy\App.config
-- and are for the local test container only.

IF DB_ID(N'DBDEPLOY') IS NULL
    CREATE DATABASE [DBDEPLOY];
GO

IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'DBDeployUser')
    CREATE LOGIN [DBDeployUser] WITH PASSWORD = N'Password01', CHECK_POLICY = OFF, DEFAULT_DATABASE = [DBDEPLOY];
GO

USE [DBDEPLOY];
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'DBDeployUser')
    CREATE USER [DBDeployUser] FOR LOGIN [DBDeployUser];
GO

ALTER ROLE [db_owner] ADD MEMBER [DBDeployUser];
GO
