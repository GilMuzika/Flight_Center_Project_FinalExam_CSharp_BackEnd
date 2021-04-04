USE [master]
GO

/****** Object:  Database [The_very_important_Flight_Center_Project]    Script Date: 1/27/2020 1:31:37 PM ******/
CREATE DATABASE [The_very_important_Flight_Center_Project]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'The_very_important_Flight_Center_Project', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\The_very_important_Flight_Center_Project.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'The_very_important_Flight_Center_Project_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL14.MSSQLSERVER\MSSQL\DATA\The_very_important_Flight_Center_Project_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO

IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [The_very_important_Flight_Center_Project].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET ANSI_NULL_DEFAULT OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET ANSI_NULLS OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET ANSI_PADDING OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET ANSI_WARNINGS OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET ARITHABORT OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET AUTO_CLOSE OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET AUTO_SHRINK OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET AUTO_UPDATE_STATISTICS ON 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET CURSOR_DEFAULT  GLOBAL 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET CONCAT_NULL_YIELDS_NULL OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET NUMERIC_ROUNDABORT OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET QUOTED_IDENTIFIER OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET RECURSIVE_TRIGGERS OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET  DISABLE_BROKER 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET TRUSTWORTHY OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET PARAMETERIZATION SIMPLE 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET READ_COMMITTED_SNAPSHOT OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET HONOR_BROKER_PRIORITY OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET RECOVERY FULL 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET  MULTI_USER 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET PAGE_VERIFY CHECKSUM  
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET DB_CHAINING OFF 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET DELAYED_DURABILITY = DISABLED 
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET QUERY_STORE = OFF
GO

ALTER DATABASE [The_very_important_Flight_Center_Project] SET  READ_WRITE 
GO

