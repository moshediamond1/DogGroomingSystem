-- Initial Database Setup Script
-- Run this script to create the database and tables

USE master;
GO

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'DogGroomingDB')
BEGIN
    CREATE DATABASE DogGroomingDB;
END
GO

USE DogGroomingDB;
GO

-- Create Users table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Username NVARCHAR(50) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(MAX) NOT NULL,
        FirstName NVARCHAR(100) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
END
GO

-- Create Appointments table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Appointments')
BEGIN
    CREATE TABLE Appointments (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        AppointmentTime DATETIME2 NOT NULL,
        DogSize INT NOT NULL,
        DurationMinutes INT NOT NULL,
        Price DECIMAL(18,2) NOT NULL,
        FinalPrice DECIMAL(18,2) NOT NULL,
        DiscountApplied BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_Appointments_Users FOREIGN KEY (UserId) 
            REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_Appointments_UserId ON Appointments(UserId);
    CREATE INDEX IX_Appointments_AppointmentTime ON Appointments(AppointmentTime);
END
GO

PRINT 'Database tables created successfully!';
