USE [master]
GO

/****** DROP DATABASE: [SenseNetContentRepository] ******/
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'SenseNetContentRepository')
BEGIN
	/****** Restricts access to this database to only one user at a time  ******/
	ALTER DATABASE [SenseNetContentRepository] SET SINGLE_USER WITH ROLLBACK IMMEDIATE 
	ALTER DATABASE [SenseNetContentRepository] SET MULTI_USER WITH ROLLBACK IMMEDIATE
	DROP DATABASE [SenseNetContentRepository]
END 
go
/****** CREATE DATABASE: [SenseNetContentRepository] ******/
CREATE DATABASE [SenseNetContentRepository]
GO
EXEC dbo.sp_dbcmptlevel @dbname=N'SenseNetContentRepository' --, @new_cmptlevel=100
GO
--IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
--begin
--EXEC [SenseNetContentRepository].[dbo].[sp_fulltext_database] @action = 'disable'
--end
--GO
--EXEC sp_fulltext_database enable
--GO
ALTER DATABASE [SenseNetContentRepository] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET ARITHABORT OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET AUTO_CREATE_STATISTICS ON 
GO
ALTER DATABASE [SenseNetContentRepository] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SenseNetContentRepository] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SenseNetContentRepository] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET  ENABLE_BROKER 
GO
ALTER DATABASE [SenseNetContentRepository] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SenseNetContentRepository] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SenseNetContentRepository] SET  READ_WRITE 
GO
ALTER DATABASE [SenseNetContentRepository] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SenseNetContentRepository] SET  MULTI_USER 
GO
ALTER DATABASE [SenseNetContentRepository] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SenseNetContentRepository] SET DB_CHAINING OFF
GO