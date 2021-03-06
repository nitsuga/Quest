USE [master]
GO
/****** Object:  Database [Quest]    Script Date: 04/07/2017 08:59:17 ******/
CREATE DATABASE [Quest]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'Quest', FILENAME = N'/tmp/data1/Quest.mdf' , SIZE = 1719936KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
 LOG ON 
( NAME = N'Quest_log', FILENAME = N'/tmp/data1/Quest_log.ldf' , SIZE = 4096KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
GO
ALTER DATABASE [Quest] SET COMPATIBILITY_LEVEL = 120
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [Quest].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [Quest] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [Quest] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [Quest] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [Quest] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [Quest] SET ARITHABORT OFF 
GO
ALTER DATABASE [Quest] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [Quest] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [Quest] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [Quest] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [Quest] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [Quest] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [Quest] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [Quest] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [Quest] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [Quest] SET  DISABLE_BROKER 
GO
ALTER DATABASE [Quest] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [Quest] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [Quest] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [Quest] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [Quest] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [Quest] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [Quest] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [Quest] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [Quest] SET  MULTI_USER 
GO
ALTER DATABASE [Quest] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [Quest] SET DB_CHAINING OFF 
GO
ALTER DATABASE [Quest] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [Quest] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [Quest] SET DELAYED_DURABILITY = DISABLED 
GO
EXEC sys.sp_db_vardecimal_storage_format N'Quest', N'ON'
GO
ALTER DATABASE [Quest] SET QUERY_STORE = OFF
GO
USE [Quest]
GO
ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET LEGACY_CARDINALITY_ESTIMATION = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET MAXDOP = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET PARAMETER_SNIFFING = PRIMARY;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION FOR SECONDARY SET QUERY_OPTIMIZER_HOTFIXES = PRIMARY;
GO
USE [Quest]
GO
/****** Object:  User [questuser]    Script Date: 04/07/2017 08:59:17 ******/
CREATE USER [questuser] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo]
GO
/****** Object:  User [Quest]    Script Date: 04/07/2017 08:59:17 ******/
CREATE USER [Quest] WITHOUT LOGIN WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [questuser]
GO
ALTER ROLE [db_datareader] ADD MEMBER [questuser]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [questuser]
GO
ALTER ROLE [db_datareader] ADD MEMBER [Quest]
GO
ALTER ROLE [db_datawriter] ADD MEMBER [Quest]
GO
/****** Object:  Schema [NLPG]    Script Date: 04/07/2017 08:59:17 ******/
CREATE SCHEMA [NLPG]
GO
USE [Quest]
GO
/****** Object:  Sequence [dbo].[RevisionSequence]    Script Date: 04/07/2017 08:59:17 ******/
CREATE SEQUENCE [dbo].[RevisionSequence] 
 AS [bigint]
 START WITH 1
 INCREMENT BY 1
 MINVALUE -9223372036854775808
 MAXVALUE 9223372036854775807
 CACHE 
GO
/****** Object:  UserDefinedFunction [dbo].[GetNearestRoad]    Script Date: 04/07/2017 08:59:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetNearestRoad]
(
	-- Add the parameters for the function here
	@Position geometry
)
RETURNS int
AS
BEGIN
	-- Declare the return variable here
	DECLARE @ResultVar int

	-- Add the T-SQL statements to compute the return value here
    SELECT TOP 1
	 @ResultVar = RoadLinkID -- RoadTypeID
    FROM RoadLink  
	Where Shape.STDistance(@Position) is not null -- and @Position.STDistance(Shape)< 30 
    ORDER BY Shape.STDistance(@Position) ASC
	OPTION (TABLE HINT(RoadLink, INDEX ([idx-position])))

	-- Return the result of the function
	RETURN @ResultVar

END
GO
/****** Object:  Table [dbo].[RoadSpeedMatrixDoW]    Script Date: 04/07/2017 08:59:17 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoadSpeedMatrixDoW](
	[RoadSpeedMatrixDoWId] [int] IDENTITY(1,1) NOT NULL,
	[DayOfWeek] [int] NOT NULL,
	[AvgSpeed] [real] NOT NULL,
	[VehicleId] [int] NOT NULL,
	[GridX] [int] NOT NULL,
	[GridY] [int] NOT NULL,
	[RoadTypeId] [int] NOT NULL,
 CONSTRAINT [PK_RoadSpeedMatrixDoW] PRIMARY KEY CLUSTERED 
(
	[RoadSpeedMatrixDoWId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[RoadSpeedMatrixDoWSummary]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



    Create View [dbo].[RoadSpeedMatrixDoWSummary] as 
	Select 
		ISNULL(Row_Number() OVER (ORDER BY (SELECT 1)), - 1) AS Id,
		count( *  ) as RecordCount, 
		max(RoadTypeid) as MaxRoadType,
		max(VehicleId) as MaxVehicleType,
		count( distinct [DayOfWeek]) as DayCount,
		MIN([GridX]) as MinX, 
		MAX(GridX) as MaxX,
		MIN([GridY]) as MinY, 
		MAX(GridY) as MaxY ,
		(MAX([GridX]) - MIN(GridX))/100 as XCount, 
		(MAX([GridY]) - MIN(GridY))/100 as YCount 
	from [RoadSpeedMatrixDow];



GO
/****** Object:  Table [dbo].[RoadSpeedMatrixHoW]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoadSpeedMatrixHoW](
	[RoadSpeedMatrixId] [int] IDENTITY(1,1) NOT NULL,
	[HourOfWeek] [int] NOT NULL,
	[AvgSpeed] [real] NOT NULL,
	[VehicleId] [int] NOT NULL,
	[GridX] [int] NOT NULL,
	[GridY] [int] NOT NULL,
	[RoadTypeId] [int] NOT NULL,
 CONSTRAINT [PK_RoadSpeedMatrix] PRIMARY KEY CLUSTERED 
(
	[RoadSpeedMatrixId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[RoadSpeedMatrixHoWSummary]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



    CREATE View [dbo].[RoadSpeedMatrixHoWSummary] as 
	Select 
		ISNULL(Row_Number() OVER (ORDER BY (SELECT 1)), - 1) AS Id,
		count( *  ) as RecordCount, 
		max(RoadTypeid) as MaxRoadType,
		max(VehicleId) as MaxVehicleType,
		count( distinct [HourOfWeek]) as HourCount,
		MIN([GridX]) as MinX, 
		MAX(GridX) as MaxX,
		MIN([GridY]) as MinY, 
		MAX(GridY) as MaxY ,
		(MAX([GridX]) - MIN(GridX))/100 as XCount, 
		(MAX([GridY]) - MIN(GridY))/100 as YCount 
	from [RoadSpeedMatrixHow];



GO
/****** Object:  Table [dbo].[Incident]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Incident](
	[IncidentID] [int] IDENTITY(1,1) NOT NULL,
	[Revision] [bigint] NULL,
	[LastUpdated] [datetime] NULL,
	[Status] [varchar](50) NULL,
	[Serial] [varchar](50) NULL,
	[SerialNumber] [bigint] NULL,
	[IncidentType] [varchar](50) NULL,
	[Complaint] [varchar](50) NULL,
	[Determinant] [varchar](50) NULL,
	[DeterminantDescription] [varchar](max) NULL,
	[Location] [varchar](250) NULL,
	[Priority] [varchar](50) NULL,
	[Sector] [varchar](50) NULL,
	[AZ] [varchar](50) NULL,
	[Created] [datetime] NULL,
	[LocationComment] [varchar](max) NULL,
	[PatientSex] [varchar](50) NULL,
	[PatientAge] [varchar](50) NULL,
	[ProblemDescription] [varchar](max) NULL,
	[DisconnectTime] [datetime] NULL,
	[AssignedResources] [int] NULL,
	[IsClosed] [bit] NULL,
	[CallerTelephone] [varchar](50) NULL,
	[Latitude] [real] NULL,
	[Longitude] [real] NULL,
 CONSTRAINT [PK_Incident] PRIMARY KEY CLUSTERED 
(
	[IncidentID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[IncidentDensityView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

/****** Script for SelectTopNRows command from SSMS  ******/
CREATE VIEW [dbo].[IncidentDensityView]
AS
SELECT     TOP (100) PERCENT COUNT(*) AS Quantity, CONVERT( int, (Incident.Geometry.STX-500000) / 100) as CellX, convert(int, (Incident.Geometry.STY-150000) / 100) as CellY
FROM         dbo.Incident
WHERE     (LastUpdated > DATEADD(HOUR, - 2, GETDATE())) OR
                      (LastUpdated > DATEADD(HOUR, - 169, GETDATE())) AND (LastUpdated < DATEADD(HOUR, - 167, GETDATE())) OR
                      (LastUpdated > DATEADD(HOUR, - 337, GETDATE())) AND (LastUpdated < DATEADD(HOUR, - 335, GETDATE())) OR
                      (LastUpdated > DATEADD(HOUR, - 505, GETDATE())) AND (LastUpdated < DATEADD(HOUR, - 503, GETDATE())) OR
                      (LastUpdated > DATEADD(HOUR, - 673, GETDATE())) AND (LastUpdated < DATEADD(HOUR, - 671, GETDATE())) OR
                      (LastUpdated > DATEADD(HOUR, - 841, GETDATE())) AND (LastUpdated < DATEADD(HOUR, - 839, GETDATE())) OR
                      (LastUpdated > DATEADD(HOUR, - 1004, GETDATE())) AND (LastUpdated < DATEADD(HOUR, - 1002, GETDATE())) OR
                      (LastUpdated > DATEADD(HOUR, - 1167, GETDATE())) AND (LastUpdated < DATEADD(HOUR, - 1165, GETDATE())) OR
                      (LastUpdated > DATEADD(HOUR, - 1330, GETDATE())) AND (LastUpdated < DATEADD(HOUR, - 1328, GETDATE()))
GROUP BY CONVERT( int, (Incident.Geometry.STX-500000) / 100) , convert(int, (Incident.Geometry.STY-150000) / 100) 
ORDER BY CONVERT( int, (Incident.Geometry.STX-500000) / 100) , convert(int, (Incident.Geometry.STY-150000) / 100) 

GO
/****** Object:  Table [dbo].[RoadLinkEdge]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoadLinkEdge](
	[RoadLinkEdgeId] [int] NOT NULL,
	[RoadLinkId] [int] NOT NULL,
	[RoadName] [varchar](150) NOT NULL,
	[RoadTypeId] [int] NOT NULL,
	[SourceGrade] [int] NOT NULL,
	[TargetGrade] [int] NOT NULL,
	[Length] [int] NOT NULL,
	[WKT] [varchar](max) NULL,
	[X] [int] NULL,
	[Y] [int] NULL,
 CONSTRAINT [PK_RoadLinkEdge] PRIMARY KEY CLUSTERED 
(
	[RoadLinkEdgeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[RoadLinkEdgeGeom]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Script for SelectTopNRows command from SSMS  ******/
Create view [dbo].[RoadLinkEdgeGeom]
as
	SELECT [RoadLinkEdgeId]
      ,[RoadLinkId]
      ,[RoadName]
      ,[RoadTypeId]
      ,[SourceGrade]
      ,[TargetGrade]
      ,[Length]
      ,[WKT]
      ,[X]
      ,[Y]
	  ,(geometry::STGeomFromText(WKT,27700)) as geom
  FROM [Quest].[dbo].[RoadLinkEdge]
GO
/****** Object:  Table [dbo].[RoadSpeedMatrixHoD]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoadSpeedMatrixHoD](
	[RoadSpeedMatrixId] [int] IDENTITY(1,1) NOT NULL,
	[HourOfDay] [int] NOT NULL,
	[AvgSpeed] [real] NOT NULL,
	[VehicleId] [int] NOT NULL,
	[GridX] [int] NOT NULL,
	[GridY] [int] NOT NULL,
	[RoadTypeId] [int] NOT NULL,
 CONSTRAINT [PK_RoadSpeedMatrix2] PRIMARY KEY CLUSTERED 
(
	[RoadSpeedMatrixId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[RoadSpeedMatrixHoDSummary]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





    CREATE View [dbo].[RoadSpeedMatrixHoDSummary] as 
	Select 
		ISNULL(Row_Number() OVER (ORDER BY (SELECT 1)), - 1) AS Id,
		count( *  ) as RecordCount, 
		max(RoadTypeid) as MaxRoadType,
		max(VehicleId) as MaxVehicleType,
		count( distinct [HourOfDay]) as HourCount,
		MIN([GridX]) as MinX, 
		MAX(GridX) as MaxX,
		MIN([GridY]) as MinY, 
		MAX(GridY) as MaxY ,
		(MAX([GridX]) - MIN(GridX))/100 as XCount, 
		(MAX([GridY]) - MIN(GridY))/100 as YCount 
	from [RoadSpeedMatrixHoD];





GO
/****** Object:  Table [dbo].[Resource]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Resource](
	[ResourceID] [int] IDENTITY(1,1) NOT NULL,
	[Revision] [bigint] NULL,
	[CallsignID] [int] NULL,
	[ResourceStatusID] [int] NULL,
	[ResourceStatusPrevID] [int] NULL,
	[Speed] [int] NULL,
	[Direction] [int] NULL,
	[Skill] [varchar](50) NULL,
	[LastUpdated] [datetime] NULL,
	[FleetNo] [int] NULL,
	[Sector] [varchar](50) NULL,
	[Serial] [varchar](50) NULL,
	[Emergency] [varchar](50) NULL,
	[Destination] [varchar](max) NULL,
	[Agency] [varchar](50) NULL,
	[Class] [varchar](50) NULL,
	[EventType] [varchar](50) NULL,
	[ETA] [datetime] NULL,
	[Comment] [varchar](max) NULL,
	[Road] [varchar](150) NULL,
	[ResourceTypeId] [int] NULL,
	[Latitude] [real] NULL,
	[Longitude] [real] NULL,
	[DestLatitude] [real] NULL,
	[DestLongitude] [real] NULL,
 CONSTRAINT [PK_Resource] PRIMARY KEY CLUSTERED 
(
	[ResourceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResourceStatus]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceStatus](
	[ResourceStatusID] [int] IDENTITY(1,1) NOT NULL,
	[Status] [nvarchar](50) NOT NULL,
	[Available] [bit] NULL,
	[Busy] [bit] NULL,
	[Rest] [bit] NULL,
	[Offroad] [bit] NULL,
	[NoSignal] [bit] NULL,
	[BusyEnroute] [bit] NULL,
 CONSTRAINT [PK_ResourceStatus] PRIMARY KEY CLUSTERED 
(
	[ResourceStatusID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Callsign]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Callsign](
	[CallsignID] [int] IDENTITY(1,1) NOT NULL,
	[Callsign] [varchar](50) NULL,
 CONSTRAINT [PK_Callsign] PRIMARY KEY CLUSTERED 
(
	[CallsignID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Devices]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Devices](
	[DeviceID] [int] IDENTITY(1,1) NOT NULL,
	[Revision] [bigint] NULL,
	[OwnerID] [varchar](50) NULL,
	[ResourceID] [int] NULL,
	[AuthToken] [varchar](max) NULL,
	[DeviceIdentity] [varchar](max) NULL,
	[NotificationTypeID] [int] NULL,
	[NotificationID] [varchar](1024) NULL,
	[LastUpdate] [datetime] NULL,
	[LastStatusUpdate] [datetime] NULL,
	[PositionAccuracy] [real] NULL,
	[isEnabled] [bit] NULL,
	[SendNearby] [bit] NULL,
	[NearbyDistance] [real] NULL,
	[LoggedOnTime] [datetime] NULL,
	[LoggedOffTime] [datetime] NULL,
	[DeviceRoleID] [int] NULL,
	[OSVersion] [varchar](50) NULL,
	[DeviceMake] [varchar](50) NULL,
	[DeviceModel] [varchar](50) NULL,
	[UseExternalStatus] [bit] NULL,
	[ResourceStatusId] [int] NULL,
	[DeviceCallsign] [varchar](50) NULL,
	[PrevStatus] [varchar](50) NULL,
	[Destination] [varchar](150) NULL,
	[Road] [varchar](150) NULL,
	[Skill] [varchar](150) NULL,
	[Speed] [int] NULL,
	[Direction] [int] NULL,
	[Event] [varchar](150) NULL,
	[Latitude] [real] NULL,
	[Longitude] [real] NULL,
 CONSTRAINT [PK_Devices] PRIMARY KEY CLUSTERED 
(
	[DeviceID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[DeviceView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[DeviceView]
AS
SELECT        dbo.Callsign.Callsign, dbo.ResourceStatus.ResourceStatusID, dbo.ResourceStatus.Status, dbo.ResourceStatus.Available, dbo.ResourceStatus.Busy, dbo.ResourceStatus.Rest, dbo.ResourceStatus.Offroad, 
                         dbo.ResourceStatus.NoSignal, dbo.ResourceStatus.BusyEnroute, dbo.Callsign.CallsignID, dbo.Devices.DeviceID, dbo.Devices.UseExternalStatus, dbo.Devices.DeviceModel, dbo.Devices.DeviceMake, 
                         dbo.Devices.OSVersion, dbo.Devices.DeviceRoleID, dbo.Devices.LoggedOffTime, dbo.Devices.LoggedOnTime, dbo.Devices.NearbyDistance, dbo.Devices.SendNearby, dbo.Devices.isEnabled, 
                         dbo.Devices.PositionAccuracy, dbo.Devices.LastStatusUpdate, dbo.Devices.LastUpdate, dbo.Devices.NotificationID, dbo.Devices.NotificationTypeID, dbo.Devices.DeviceIdentity, dbo.Devices.AuthToken, 
                         dbo.Devices.ResourceID, dbo.Devices.OwnerID, dbo.Devices.Revision, dbo.Devices.DeviceCallsign, dbo.Devices.PrevStatus, dbo.Devices.Destination, dbo.Devices.Road, dbo.Devices.Skill, dbo.Devices.Speed, 
                         dbo.Devices.Direction, dbo.Devices.Event, dbo.Devices.Latitude, dbo.Devices.Longitude
FROM            dbo.Callsign INNER JOIN
                         dbo.Resource ON dbo.Callsign.CallsignID = dbo.Resource.CallsignID RIGHT OUTER JOIN
                         dbo.Devices ON dbo.Resource.ResourceID = dbo.Devices.ResourceID LEFT OUTER JOIN
                         dbo.ResourceStatus ON dbo.Devices.ResourceStatusId = dbo.ResourceStatus.ResourceStatusID
GO
/****** Object:  View [dbo].[IncidentView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[IncidentView]
AS
SELECT        IncidentID, CallerTelephone, IsClosed, AssignedResources, DisconnectTime, ProblemDescription, PatientAge, PatientSex, LocationComment, Created, AZ, Sector, Priority, Location, DeterminantDescription, 
                         Determinant, Complaint, IncidentType, SerialNumber, Serial, Status, LastUpdated, Revision, Latitude, Longitude
FROM            dbo.Incident
GO
/****** Object:  View [dbo].[ResourceCallsignView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ResourceCallsignView]
AS
SELECT        dbo.Resource.ResourceID, dbo.Callsign.Callsign + ' (' + CONVERT(varchar, dbo.Resource.FleetNo) + ')' AS CallsignFleet
FROM            dbo.Resource INNER JOIN
                         dbo.Callsign ON dbo.Resource.CallsignID = dbo.Callsign.CallsignID
GO
/****** Object:  Table [dbo].[Destinations]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Destinations](
	[DestinationID] [int] IDENTITY(1,1) NOT NULL,
	[Destination] [varchar](255) NULL,
	[Shortcode] [varchar](50) NULL,
	[IsHospital] [bit] NULL,
	[IsStandby] [bit] NULL,
	[IsStation] [bit] NULL,
	[IsRoad] [bit] NULL,
	[IsPolice] [bit] NULL,
	[IsAandE] [bit] NULL,
	[IsOld] [bit] NULL,
	[Position] [geometry] NULL,
	[CoverageTier] [int] NULL,
	[Status] [varchar](50) NULL,
	[Timestamp] [datetime] NULL,
 CONSTRAINT [PK_Destinations] PRIMARY KEY CLUSTERED 
(
	[DestinationID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[DestinationView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[DestinationView]
AS
SELECT        DestinationID, Destination, Timestamp, Status, CoverageTier, IsOld, IsAandE, IsPolice, IsRoad, IsStation, IsStandby, IsHospital, Shortcode, Position.STX AS X, Position.STY AS Y
FROM            dbo.Destinations
GO
/****** Object:  Table [dbo].[MapOverlayItem]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MapOverlayItem](
	[MapOverlayItemID] [int] NOT NULL,
	[MapOverlayID] [int] NOT NULL,
	[geom] [geometry] NOT NULL,
	[Description] [nvarchar](250) NOT NULL,
	[FillColour] [nvarchar](50) NOT NULL,
	[Flash] [bit] NOT NULL,
	[IsClosed] [bit] NOT NULL,
	[Visible] [bit] NOT NULL,
	[AmberLimit] [real] NULL,
	[RedLimit] [real] NULL,
	[FlashLimit] [real] NULL,
 CONSTRAINT [PK_MapOverlayItem] PRIMARY KEY CLUSTERED 
(
	[MapOverlayItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  View [dbo].[MapOverlayItemView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[MapOverlayItemView]
AS
SELECT        MapOverlayItemID, FlashLimit, RedLimit, AmberLimit, CoverageMap, Visible, IsClosed, Flash, FillColour, Description, MapOverlayID, geom.ToString() AS WKT
FROM            dbo.MapOverlayItem
GO
/****** Object:  Table [dbo].[ResourceType]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceType](
	[ResourceTypeId] [int] IDENTITY(1,1) NOT NULL,
	[ResourceType] [varchar](50) NULL,
	[ResourceTypeGroup] [varchar](50) NULL,
 CONSTRAINT [PK_ResourceType] PRIMARY KEY CLUSTERED 
(
	[ResourceTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  View [dbo].[ResourceView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE VIEW [dbo].[ResourceView]
AS
SELECT        dbo.ResourceStatus.Status, dbo.ResourceStatus.Available, dbo.ResourceStatus.Busy, dbo.ResourceStatus.BusyEnroute, dbo.ResourceStatus.NoSignal, dbo.ResourceStatus.Offroad, dbo.ResourceStatus.Rest, 
                         dbo.Resource.ResourceID, dbo.Callsign.Callsign, dbo.Resource.Speed, dbo.Resource.Direction, dbo.Resource.Skill, dbo.Resource.LastUpdated, dbo.Resource.FleetNo, dbo.Resource.Sector, dbo.Resource.Serial,
                          dbo.Resource.Emergency, dbo.Resource.Destination, dbo.Resource.Agency, dbo.Resource.Class, dbo.Resource.EventType, dbo.Resource.ETA, dbo.Resource.Comment, dbo.Resource.Road, 
                         dbo.Resource.Revision, dbo.Resource.ResourceStatusPrevID, ResourceStatus_1.Status AS PrevStatus, dbo.ResourceType.ResourceTypeGroup, dbo.ResourceType.ResourceType, dbo.Resource.DestLatitude, 
                         dbo.Resource.Longitude, dbo.Resource.ResourceTypeId, dbo.Resource.Latitude, dbo.Resource.DestLongitude
FROM            dbo.Resource INNER JOIN
                         dbo.ResourceStatus ON dbo.Resource.ResourceStatusID = dbo.ResourceStatus.ResourceStatusID INNER JOIN
                         dbo.Callsign ON dbo.Resource.CallsignID = dbo.Callsign.CallsignID INNER JOIN
                         dbo.ResourceStatus AS ResourceStatus_1 ON dbo.Resource.ResourceStatusID = ResourceStatus_1.ResourceStatusID INNER JOIN
                         dbo.ResourceType ON dbo.Resource.ResourceTypeId = dbo.ResourceType.ResourceTypeId
GO
/****** Object:  View [dbo].[JobView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[JobView]
AS
SELECT        dbo.JobInfo.JobInfoId, dbo.JobInfo.Taskname, dbo.JobInfo.Description, dbo.JobInfo.[Key], dbo.JobInfo.Parameters, dbo.JobInfo.Scheduled, dbo.JobInfo.JobStatusId, dbo.JobInfo.Message, dbo.JobInfo.Started, 
                         dbo.JobInfo.Stopped, dbo.JobInfo.Success, dbo.JobInfo.Created, dbo.JobInfo.NotifyAddresses, dbo.JobInfo.NotifyLevel, dbo.JobInfo.NotifyReplyTo, dbo.JobStatus.JobStatus
FROM            dbo.JobInfo INNER JOIN
                         dbo.JobStatus ON dbo.JobInfo.JobStatusId = dbo.JobStatus.JobStatusId
GO
/****** Object:  View [dbo].[RevisionView]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create view [dbo].[RevisionView]
as
select current_value from sys.sequences where sys.sequences.name='RevisionSequence'
GO
/****** Object:  Table [dbo].[__MigrationHistory]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__MigrationHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ContextKey] [nvarchar](300) NOT NULL,
	[Model] [varbinary](max) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK_dbo.__MigrationHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC,
	[ContextKey] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetRoles]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetRoles](
	[Id] [nvarchar](128) NOT NULL,
	[Name] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetRoles] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserClaims]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserClaims](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
	[ClaimType] [nvarchar](max) NULL,
	[ClaimValue] [nvarchar](max) NULL,
 CONSTRAINT [PK_dbo.AspNetUserClaims] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserLogins]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserLogins](
	[LoginProvider] [nvarchar](128) NOT NULL,
	[ProviderKey] [nvarchar](128) NOT NULL,
	[UserId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserLogins] PRIMARY KEY CLUSTERED 
(
	[LoginProvider] ASC,
	[ProviderKey] ASC,
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUserRoles]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUserRoles](
	[UserId] [nvarchar](128) NOT NULL,
	[RoleId] [nvarchar](128) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUserRoles] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC,
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AspNetUsers]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AspNetUsers](
	[Id] [nvarchar](128) NOT NULL,
	[Fullname] [nvarchar](max) NULL,
	[Email] [nvarchar](256) NULL,
	[EmailConfirmed] [bit] NOT NULL,
	[PasswordHash] [nvarchar](max) NULL,
	[SecurityStamp] [nvarchar](max) NULL,
	[PhoneNumber] [nvarchar](max) NULL,
	[PhoneNumberConfirmed] [bit] NOT NULL,
	[TwoFactorEnabled] [bit] NOT NULL,
	[LockoutEndDateUtc] [datetime] NULL,
	[LockoutEnabled] [bit] NOT NULL,
	[AccessFailedCount] [int] NOT NULL,
	[UserName] [nvarchar](256) NOT NULL,
 CONSTRAINT [PK_dbo.AspNetUsers] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Call]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Call](
	[CallId] [int] IDENTITY(1,1) NOT NULL,
	[Address1] [varchar](128) NULL,
	[Address2] [varchar](128) NULL,
	[Address3] [varchar](128) NULL,
	[Address4] [varchar](128) NULL,
	[Address5] [varchar](128) NULL,
	[Address6] [varchar](128) NULL,
	[Easting] [int] NULL,
	[Northing] [int] NULL,
	[SemiMajor] [int] NULL,
	[SemiMinor] [int] NULL,
	[Name] [varchar](128) NULL,
	[Confidence] [int] NULL,
	[Angle] [int] NULL,
	[Altitude] [int] NULL,
	[Direction] [int] NULL,
	[Speed] [int] NULL,
	[Updated] [datetime] NULL,
	[SwitchId] [bigint] NULL,
	[Status] [varchar](50) NULL,
	[Requery] [int] NULL,
	[Extension] [varchar](50) NULL,
	[Event] [varchar](50) NULL,
	[IsMobile] [bit] NULL,
	[IsClosed] [bit] NULL,
	[TimeConnected] [datetime] NULL,
	[TimeAnswered] [datetime] NULL,
	[TimeClosed] [datetime] NULL,
 CONSTRAINT [PK_Call] PRIMARY KEY CLUSTERED 
(
	[CallId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CoverageMapDefinition]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoverageMapDefinition](
	[CoverageMapDefinitionId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[VehicleCodes] [nvarchar](250) NULL,
	[MinuteLimit] [int] NULL,
	[StyleCode] [nvarchar](50) NULL,
	[IsEnabled] [bit] NULL,
	[RoutingResource] [nvarchar](50) NULL,
 CONSTRAINT [PK_CoverageMapDefinition] PRIMARY KEY CLUSTERED 
(
	[CoverageMapDefinitionId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CoverageMapStore]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CoverageMapStore](
	[CoverageMapStoreId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Data] [varbinary](max) NULL,
	[OffsetX] [int] NOT NULL,
	[OffsetY] [int] NOT NULL,
	[Blocksize] [int] NOT NULL,
	[Rows] [int] NOT NULL,
	[Columns] [int] NOT NULL,
	[tstamp] [datetime] NOT NULL,
	[Percent] [float] NULL,
 CONSTRAINT [PK_CoverageMap_1] PRIMARY KEY CLUSTERED 
(
	[CoverageMapStoreId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[DeviceRole]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DeviceRole](
	[DeviceRoleID] [int] NOT NULL,
	[DeviceRoleName] [nvarchar](50) NULL,
 CONSTRAINT [PK_DeviceRole] PRIMARY KEY CLUSTERED 
(
	[DeviceRoleID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JobTemplate]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JobTemplate](
	[JobTemplateId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Group] [nvarchar](50) NULL,
	[Order] [int] NULL,
	[Task] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
	[Parameters] [nvarchar](max) NULL,
	[Key] [nvarchar](max) NULL,
	[NotifyAddresses] [nvarchar](max) NULL,
	[NotifyLevel] [int] NULL,
	[NotifyReplyTo] [nvarchar](max) NULL,
	[Classname] [nvarchar](max) NULL,
 CONSTRAINT [PK_JobTemplate] PRIMARY KEY CLUSTERED 
(
	[JobTemplateId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[MapOverlay]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MapOverlay](
	[MapOverlayID] [int] NOT NULL,
	[OverlayName] [nvarchar](50) NOT NULL,
	[Stroke] [nvarchar](50) NOT NULL,
	[StrokeThickness] [int] NOT NULL,
	[FromZoom] [int] NOT NULL,
	[ToZoom] [int] NOT NULL,
	[GeomUpdateFrequency] [int] NOT NULL,
	[AttrUpdateFrequency] [int] NOT NULL,
	[CalculateCoverage] [int] NULL,
 CONSTRAINT [PK_MapOverlay] PRIMARY KEY CLUSTERED 
(
	[MapOverlayID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Profile]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Profile](
	[ProfileId] [int] NOT NULL,
	[ProfileName] [nvarchar](150) NULL,
 CONSTRAINT [PK_Profile] PRIMARY KEY CLUSTERED 
(
	[ProfileId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfileParameter]    Script Date: 04/07/2017 08:59:18 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfileParameter](
	[ProfileParameterId] [int] NOT NULL,
	[ProfileId] [int] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](max) NOT NULL,
	[ProfileParameterTypeId] [int] NULL,
 CONSTRAINT [PK_ProfileParameter] PRIMARY KEY CLUSTERED 
(
	[ProfileParameterId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ProfileParameterType]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ProfileParameterType](
	[ProfileParameterTypeId] [int] NOT NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_ProfileParameterType] PRIMARY KEY CLUSTERED 
(
	[ProfileParameterTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResourceArea]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceArea](
	[AreaId] [int] NOT NULL,
	[Area] [nvarchar](50) NULL,
	[Latitude] [real] NULL,
	[Longitude] [real] NULL,
	[Zoom] [real] NULL,
	[Shape] [geometry] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ResourceStatusHistory]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ResourceStatusHistory](
	[ResourceStatusHistoryID] [int] IDENTITY(1,1) NOT NULL,
	[ResourceID] [int] NULL,
	[ResourceStatusID] [int] NULL,
	[Revision] [bigint] NULL,
 CONSTRAINT [PK_ResourceStatusHistory] PRIMARY KEY CLUSTERED 
(
	[ResourceStatusHistoryID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoadLinkEdgeLink]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoadLinkEdgeLink](
	[RoadLinkEdgeLinkId] [int] IDENTITY(1,1) NOT NULL,
	[SourceRoadLinkEdge] [int] NOT NULL,
	[TargetRoadLinkEdge] [int] NOT NULL,
 CONSTRAINT [PK_RoadLinkEdgeLink_1] PRIMARY KEY CLUSTERED 
(
	[RoadLinkEdgeLinkId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoadSpeed]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoadSpeed](
	[RoadSpeedId] [int] IDENTITY(1,1) NOT NULL,
	[SpeedAvg] [float] NOT NULL,
	[SpeedStDev] [float] NULL,
	[SpeedCount] [int] NOT NULL,
	[HourOfWeek] [int] NOT NULL,
	[VehicleId] [int] NOT NULL,
	[RoadLinkEdgeId] [int] NULL,
 CONSTRAINT [PK_RoadSpeed] PRIMARY KEY CLUSTERED 
(
	[RoadSpeedId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RoadTypes]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RoadTypes](
	[RoadTypeId] [int] NOT NULL,
	[RoadType] [nvarchar](100) NULL,
 CONSTRAINT [PK_RoadType] PRIMARY KEY CLUSTERED 
(
	[RoadTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SecuredItemLinks]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SecuredItemLinks](
	[SecuredItemLinkId] [int] IDENTITY(1,1) NOT NULL,
	[SecuredItemIDParent] [int] NULL,
	[SecuredItemIDChild] [int] NULL,
 CONSTRAINT [PK_SecuredItemLinks] PRIMARY KEY CLUSTERED 
(
	[SecuredItemLinkId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SecuredItems]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SecuredItems](
	[SecuredItemID] [int] IDENTITY(1,1) NOT NULL,
	[SecuredItemName] [varchar](50) NOT NULL,
	[SecuredValue] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Priority] [int] NULL,
 CONSTRAINT [PK_SecuredItems] PRIMARY KEY CLUSTERED 
(
	[SecuredItemID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[StationCatchment]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StationCatchment](
	[StationCatchmentId] [int] NOT NULL,
	[FID] [varchar](50) NULL,
	[code] [varchar](50) NULL,
	[StationName] [varchar](50) NULL,
	[Complex] [varchar](50) NULL,
	[Area] [varchar](50) NULL,
	[Shape] [geometry] NULL,
	[ComplexId] [int] NULL,
	[Enabled] [bit] NULL,
 CONSTRAINT [PK_StationCatchment] PRIMARY KEY CLUSTERED 
(
	[StationCatchmentId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Variable]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Variable](
	[VariableID] [int] IDENTITY(1,1) NOT NULL,
	[Variable] [nvarchar](50) NULL,
	[Value] [nvarchar](max) NULL,
	[Description] [nvarchar](250) NULL,
	[Type] [nvarchar](50) NULL,
 CONSTRAINT [PK_Variable] PRIMARY KEY CLUSTERED 
(
	[VariableID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Vehicle]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vehicle](
	[VehicleId] [int] IDENTITY(1,1) NOT NULL,
	[Vehicle] [nvarchar](50) NULL,
 CONSTRAINT [PK_Vehicle] PRIMARY KEY CLUSTERED 
(
	[VehicleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [RoleNameIndex]    Script Date: 04/07/2017 08:59:19 ******/
CREATE UNIQUE NONCLUSTERED INDEX [RoleNameIndex] ON [dbo].[AspNetRoles]
(
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserId]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserClaims]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserId]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserLogins]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_RoleId]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [IX_RoleId] ON [dbo].[AspNetUserRoles]
(
	[RoleId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_UserId]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [IX_UserId] ON [dbo].[AspNetUserRoles]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [UserNameIndex]    Script Date: 04/07/2017 08:59:19 ******/
CREATE UNIQUE NONCLUSTERED INDEX [UserNameIndex] ON [dbo].[AspNetUsers]
(
	[UserName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [_dta_index_Callsign_11_661577395__K2_1]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [_dta_index_Callsign_11_661577395__K2_1] ON [dbo].[Callsign]
(
	[Callsign] ASC
)
INCLUDE ( 	[CallsignID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [NonClusteredIndex-20160719-103259]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20160719-103259] ON [dbo].[Incident]
(
	[Serial] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [NonClusteredIndex-20160719-125115]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20160719-125115] ON [dbo].[Resource]
(
	[CallsignID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [NonClusteredIndex-20141215-164957]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20141215-164957] ON [dbo].[ResourceStatus]
(
	[Available] ASC,
	[Busy] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [NonClusteredIndex-20141229-203340]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20141229-203340] ON [dbo].[ResourceStatus]
(
	[Status] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [NonClusteredIndex-20161222-131845]    Script Date: 04/07/2017 08:59:19 ******/
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20161222-131845] ON [dbo].[RoadSpeed]
(
	[RoadLinkEdgeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AspNetUserClaims]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserClaims] CHECK CONSTRAINT [FK_dbo.AspNetUserClaims_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserLogins]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserLogins] CHECK CONSTRAINT [FK_dbo.AspNetUserLogins_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId] FOREIGN KEY([RoleId])
REFERENCES [dbo].[AspNetRoles] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetRoles_RoleId]
GO
ALTER TABLE [dbo].[AspNetUserRoles]  WITH CHECK ADD  CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[AspNetUsers] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AspNetUserRoles] CHECK CONSTRAINT [FK_dbo.AspNetUserRoles_dbo.AspNetUsers_UserId]
GO
ALTER TABLE [dbo].[Devices]  WITH CHECK ADD  CONSTRAINT [FK_Devices_DeviceRole] FOREIGN KEY([DeviceRoleID])
REFERENCES [dbo].[DeviceRole] ([DeviceRoleID])
GO
ALTER TABLE [dbo].[Devices] CHECK CONSTRAINT [FK_Devices_DeviceRole]
GO
ALTER TABLE [dbo].[Devices]  WITH CHECK ADD  CONSTRAINT [FK_Devices_Resource] FOREIGN KEY([ResourceID])
REFERENCES [dbo].[Resource] ([ResourceID])
ON DELETE SET NULL
GO
ALTER TABLE [dbo].[Devices] CHECK CONSTRAINT [FK_Devices_Resource]
GO
ALTER TABLE [dbo].[Devices]  WITH CHECK ADD  CONSTRAINT [FK_Devices_ResourceStatus] FOREIGN KEY([ResourceStatusId])
REFERENCES [dbo].[ResourceStatus] ([ResourceStatusID])
GO
ALTER TABLE [dbo].[Devices] CHECK CONSTRAINT [FK_Devices_ResourceStatus]
GO
ALTER TABLE [dbo].[MapOverlayItem]  WITH CHECK ADD  CONSTRAINT [FK_MapOverlayItem_MapOverlay] FOREIGN KEY([MapOverlayID])
REFERENCES [dbo].[MapOverlay] ([MapOverlayID])
GO
ALTER TABLE [dbo].[MapOverlayItem] CHECK CONSTRAINT [FK_MapOverlayItem_MapOverlay]
GO
ALTER TABLE [dbo].[ProfileParameter]  WITH CHECK ADD  CONSTRAINT [FK_ProfileParameter_Profile] FOREIGN KEY([ProfileId])
REFERENCES [dbo].[Profile] ([ProfileId])
GO
ALTER TABLE [dbo].[ProfileParameter] CHECK CONSTRAINT [FK_ProfileParameter_Profile]
GO
ALTER TABLE [dbo].[ProfileParameter]  WITH CHECK ADD  CONSTRAINT [FK_ProfileParameter_ProfileParameterType] FOREIGN KEY([ProfileParameterTypeId])
REFERENCES [dbo].[ProfileParameterType] ([ProfileParameterTypeId])
GO
ALTER TABLE [dbo].[ProfileParameter] CHECK CONSTRAINT [FK_ProfileParameter_ProfileParameterType]
GO
ALTER TABLE [dbo].[Resource]  WITH CHECK ADD  CONSTRAINT [FK_Resource_Callsign] FOREIGN KEY([CallsignID])
REFERENCES [dbo].[Callsign] ([CallsignID])
GO
ALTER TABLE [dbo].[Resource] CHECK CONSTRAINT [FK_Resource_Callsign]
GO
ALTER TABLE [dbo].[Resource]  WITH CHECK ADD  CONSTRAINT [FK_Resource_ResourceStatus] FOREIGN KEY([ResourceStatusID])
REFERENCES [dbo].[ResourceStatus] ([ResourceStatusID])
GO
ALTER TABLE [dbo].[Resource] CHECK CONSTRAINT [FK_Resource_ResourceStatus]
GO
ALTER TABLE [dbo].[ResourceStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_ResourceStatusHistory_Resource] FOREIGN KEY([ResourceID])
REFERENCES [dbo].[Resource] ([ResourceID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ResourceStatusHistory] CHECK CONSTRAINT [FK_ResourceStatusHistory_Resource]
GO
ALTER TABLE [dbo].[ResourceStatusHistory]  WITH CHECK ADD  CONSTRAINT [FK_ResourceStatusHistory_ResourceStatus] FOREIGN KEY([ResourceStatusID])
REFERENCES [dbo].[ResourceStatus] ([ResourceStatusID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[ResourceStatusHistory] CHECK CONSTRAINT [FK_ResourceStatusHistory_ResourceStatus]
GO
ALTER TABLE [dbo].[SecuredItemLinks]  WITH CHECK ADD  CONSTRAINT [FK_SecuredItemLinks_SecuredItems] FOREIGN KEY([SecuredItemIDParent])
REFERENCES [dbo].[SecuredItems] ([SecuredItemID])
GO
ALTER TABLE [dbo].[SecuredItemLinks] CHECK CONSTRAINT [FK_SecuredItemLinks_SecuredItems]
GO
ALTER TABLE [dbo].[SecuredItemLinks]  WITH CHECK ADD  CONSTRAINT [FK_SecuredItemLinks_SecuredItems1] FOREIGN KEY([SecuredItemIDChild])
REFERENCES [dbo].[SecuredItems] ([SecuredItemID])
GO
ALTER TABLE [dbo].[SecuredItemLinks] CHECK CONSTRAINT [FK_SecuredItemLinks_SecuredItems1]
GO
ALTER TABLE [dbo].[Resource]  WITH CHECK ADD  CONSTRAINT [CK_Resource] CHECK  (([CallsignID] IS NOT NULL))
GO
ALTER TABLE [dbo].[Resource] CHECK CONSTRAINT [CK_Resource]
GO
/****** Object:  StoredProcedure [dbo].[Clean]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Clean]
AS
BEGIN
	truncate table DeviceHistory;
	delete from Resource;
	delete from Callsign;
	truncate table ResourceStatusHistory;
	delete from Incident;
	
END
GO
/****** Object:  StoredProcedure [dbo].[CleanCoverage]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[CleanCoverage]
AS
BEGIN
	declare @maxid int;

	select @maxid = max([CoverageMapStoreId]) from CoverageMapStore;

	delete from CoverageMapStore where [CoverageMapStoreId]< (@maxid-30);

END
GO
/****** Object:  StoredProcedure [dbo].[GetClaims]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetClaims]	
		@ClaimType NVarchar(50),
		@ClaimValue NVarchar(50)
AS
BEGIN

	WITH ga (SecuredItemID, SecuredItemName, SecuredValue, Priority, Level)
	AS
	(
		-- Anchor member definition
		SELECT SecuredItemID, 
				SecuredItemName,
				SecuredValue,
				SecuredItems.Priority,
				0 as Level 
			from SecuredItems 
			where	SecuredItemName=@ClaimType
			and		SecuredValue=@ClaimValue
		UNION ALL
		-- Recursive member definition
				SELECT SecuredItems.SecuredItemID, 
				SecuredItems.SecuredItemName,
				SecuredItems.SecuredValue,
				SecuredItems.Priority,
				Level + 1
			from ga 
			inner join SecuredItemLinks 
				on SecuredItemLinks.SecuredItemIDParent = ga.SecuredItemID
			inner join SecuredItems
				on SecuredItemLinks.SecuredItemIDChild = SecuredItems.SecuredItemID
			where  level<10 
				)
	Select SecuredItemName, SecuredValue from ga order by Priority;				
END
GO
/****** Object:  StoredProcedure [dbo].[GetIncidentDensity]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetIncidentDensity]
AS
BEGIN
	SET NOCOUNT ON;

	-- This is the REAL call.. but for demo purposes we use the select below
	--SELECT * from IncidentDensityView;


	SELECT        TOP (100) PERCENT COUNT(*) AS Quantity, CONVERT(int, (Geometry.STX - 501000) / 500) AS CellX, CONVERT(int, (Geometry.STY - 153000) / 500) AS CellY
	FROM            dbo.Incident
	--WHERE Priority='R1' or Priority='R2'  
	GROUP BY CONVERT(int, (Geometry.STX - 501000) / 500), CONVERT(int, (Geometry.STY - 153000) / 500)
	ORDER BY CellX, CellY

END

GO
/****** Object:  StoredProcedure [dbo].[GetOperationalArea]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Marcus Poulton
-- Create date: 7 Apr 2016
-- Description:	Get entire operational area
-- =============================================
CREATE PROCEDURE [dbo].[GetOperationalArea]
  (
	@Buffer int
  )
  AS
BEGIN
	SET NOCOUNT ON;
	declare @Shape Geometry;
	declare @ShapeBigger Geometry;
	SET @Shape = GEOMETRY::STGeomFromText('GEOMETRYCOLLECTION EMPTY', 27700 );
	SELECT @Shape = @Shape.STUnion(Shape) FROM StationCatchment;
	SELECT @ShapeBigger = @Shape.STBuffer(@Buffer);
	SELECT @ShapeBigger.ToString();
END
GO
/****** Object:  StoredProcedure [dbo].[GetOverlayAt]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Script for SelectTopNRows command from SSMS  ******/
create procedure [dbo].[GetOverlayAt]
(
	@OverlayName varchar(50),
	@Easting int,
	@Northing int,
	@result nvarchar(50) out
)
as
begin
	set nocount on;
	declare @p as geometry;
	declare @layer as int;
	
	set @p =geometry::STPointFromText('POINT ( ' + convert(nvarchar, @easting ) + ' ' + convert( nvarchar, @northing ) + ' )', 27700) ;
	select @layer = MapOverlayid from MapOverlay where OverlayName=@OverlayName;
	
	SELECT @result = Description from MapOverlayItem where geom.STContains(@p)=1 and @layer = MapOverlayid			
					--OPTION (TABLE HINT(MapOverlayItem, INDEX ([ix-spatial])));

end;
GO
/****** Object:  StoredProcedure [dbo].[GetRevision]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE Procedure [dbo].[GetRevision]
as
begin
   declare @rev as bigint
   select @rev = convert( bigint, current_value ) from sys.sequences where sys.sequences.name='RevisionSequence'
   select @rev;
   return @rev
end
GO
/****** Object:  StoredProcedure [dbo].[GetVehicleCoverage]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
  CREATE procedure [dbo].[GetVehicleCoverage]
  (
	@vehtype int
  )

  AS
  
  Select 
		d.[CoverageMapDefinitionId] as TypeId, 	s.Name, Data, OffsetX,OffsetY,[Blocksize],[Rows],Columns,[Percent],	cast(tstamp as smalldatetime) as tstamp
  from [CoverageMapStore] s
  inner join [CoverageMapDefinition] d on s.Name=d.Name  
  where CoverageMapStoreId in
  (
	select max(coveragemapstoreid) from coveragemapstore	
	group by name  
  )
  and
  d.CoverageMapDefinitionId=@vehtype

GO
/****** Object:  StoredProcedure [dbo].[PrimeDatabase]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE Procedure [dbo].[PrimeDatabase]
as
begin

	delete from devices
	delete from [Resource];
	delete from Callsign;

	Insert into Callsign ( Callsign ) select callsign from Geotracker45.dbo.Callsign;

	INSERT INTO [dbo].[Resource]
	([CallsignID],[ResourceType],[ResourceStatusID],[Geometry],[Speed],[Direction],[Skill],[LastUpdated],[FleetNo]
	,[Sector],[Serial],[Emergency],[Destination],[Agency],[Class],[EventType],[ETA],[Comment],[Road])

	SELECT				Callsign.CallsignID, 
						Geotracker45.dbo.ResourceView.ResourceType, 
						ResourceStatus.ResourceStatusID, 
						Geotracker45.dbo.Resource.Position, 
						Geotracker45.dbo.ResourceView.Speed, 
						Geotracker45.dbo.ResourceView.Direction, 
						Geotracker45.dbo.ResourceView.Skill, 
						Geotracker45.dbo.Resource.TimeStamp,
						Geotracker45.dbo.ResourceView.FleetNo, 
						'',
						Geotracker45.dbo.ResourceView.Serial,
						'',
						'',
						'',
						'',
						'',
						Geotracker45.dbo.ResourceView.ETA, 
						Geotracker45.dbo.ResourceView.Comment, 
						Geotracker45.dbo.ResourceView.Road
	FROM            Geotracker45.dbo.ResourceView 
					LEFT JOIN	Callsign ON Geotracker45.dbo.ResourceView.Callsign = Callsign.Callsign 
					LEFT JOIN	ResourceStatus ON Geotracker45.dbo.ResourceView.Status = ResourceStatus.Status
					LEFT JOIN	Geotracker45.dbo.Resource  ON Geotracker45.dbo.Resource.ResourceId = Geotracker45.dbo.ResourceView.ResourceId

	WHERE Geotracker45.dbo.ResourceView.FleetNo<>0 and Geotracker45.dbo.ResourceView.FleetNo<>Geotracker45.dbo.ResourceView.Callsign

	Delete [dbo].[Incident]

	INSERT INTO [dbo].[Incident]
           ([LastUpdated]
           ,[Status]
           ,[Serial]
           ,[IncidentType]
           ,[Complaint]
           ,[Geometry]
           ,[Determinant]
		   ,[DeterminantDescription]
           ,[Location]
           ,[Priority]
           ,[Sector]
           ,[AZ]
           ,[Created]
           --,[LocationComment]
           --,[PatientSex]
           --,[PatientAge]
           --,[ProblemDescription]
           --,[DisconnectTime]
           ,[AssignedResources]
		   ,[IsClosed]
		   )
select		
			IV.[LastUpdated]
           ,IV.[Status]
           ,IV.[Serial]
           ,IV.[IncidentType]
           ,IV.[Complaint]
           ,Incident.Position as [Geometry]
           ,IV.[IncidentDeterminant]
		   ,IV.[Description]
           ,IV.[Location]
           ,IV.[IncidentPriority]
           ,IV.[Sector]
           ,IV.[AZ]
           ,IV.[Created]
           --,IV.[LocationComment]
           --,IV.[PatientSex]
           --,IV.[PatientAge]
           --,IV.[ProblemDescription]
           --,IV.[DisconnectTime]
           ,0 as [AssignedResources]  
		   ,IV.IsClosed
		   from geotrackerCP_0.dbo.IncidentView IV
		   inner join geotrackerCP_0.dbo.Incident on Incident.IncidentID = IV.Incidentid
		   where IV.isClosed=0;




end;
GO
/****** Object:  StoredProcedure [dbo].[ResourceAtRevision]    Script Date: 04/07/2017 08:59:19 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/****** Script for SelectTopNRows command from SSMS  ******/
CREATE Procedure [dbo].[ResourceAtRevision](
	@revision as bigint
)
as
begin
select ResourceID, Available, Rest, Busy, Offroad, NoSignal, BusyEnroute
from (
	select resourceid,
			revision,
			row_number() over(partition by resourceid order by revision desc) as roworder,
			ResourceStatus.Available, ResourceStatus.Rest, ResourceStatus.Busy, ResourceStatus.Offroad, ResourceStatus.NoSignal, ResourceStatus.BusyEnroute
	from ResourceStatusHistory
INNER JOIN
                         ResourceStatus ON ResourceStatusHistory.ResourceStatusID = ResourceStatus.ResourceStatusID
						 	where revision< @revision
) temp
where roworder = 1
end

GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'flag to indicate whether we use External system for status changes. i.e. device requests status change rather than forcing it.' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'Devices', @level2type=N'COLUMN',@level2name=N'UseExternalStatus'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Destinations"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 317
               Right = 208
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 15
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'DestinationView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'DestinationView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Callsign"
            Begin Extent = 
               Top = 2
               Left = 572
               Bottom = 98
               Right = 742
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Devices"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 302
               Right = 227
            End
            DisplayFlags = 280
            TopColumn = 19
         End
         Begin Table = "ResourceStatus"
            Begin Extent = 
               Top = 86
               Left = 630
               Bottom = 333
               Right = 810
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Resource"
            Begin Extent = 
               Top = 6
               Left = 265
               Bottom = 136
               Right = 468
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 11
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'DeviceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'DeviceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'DeviceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Incident"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 121
               Right = 243
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 18
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
         Or = 1350
         Or = 1350
         Or = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'IncidentDensityView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'IncidentDensityView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Incident"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 324
               Right = 253
            End
            DisplayFlags = 280
            TopColumn = 10
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'IncidentView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'IncidentView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "MapOverlayItem"
            Begin Extent = 
               Top = 6
               Left = 290
               Bottom = 288
               Right = 478
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MapOverlayItemView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'MapOverlayItemView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[41] 4[20] 2[10] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "Resource"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 335
               Right = 218
            End
            DisplayFlags = 280
            TopColumn = 5
         End
         Begin Table = "Callsign"
            Begin Extent = 
               Top = 6
               Left = 256
               Bottom = 102
               Right = 426
            End
            DisplayFlags = 280
            TopColumn = 0
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
      Begin ColumnWidths = 9
         Width = 284
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
         Width = 1500
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 1440
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or = 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ResourceCallsignView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=1 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ResourceCallsignView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane1', @value=N'[0E232FF0-B466-11cf-A24F-00AA00A3EFFF, 1.00]
Begin DesignProperties = 
   Begin PaneConfigurations = 
      Begin PaneConfiguration = 0
         NumPanes = 4
         Configuration = "(H (1[40] 4[20] 2[20] 3) )"
      End
      Begin PaneConfiguration = 1
         NumPanes = 3
         Configuration = "(H (1 [50] 4 [25] 3))"
      End
      Begin PaneConfiguration = 2
         NumPanes = 3
         Configuration = "(H (1 [50] 2 [25] 3))"
      End
      Begin PaneConfiguration = 3
         NumPanes = 3
         Configuration = "(H (4 [30] 2 [40] 3))"
      End
      Begin PaneConfiguration = 4
         NumPanes = 2
         Configuration = "(H (1 [56] 3))"
      End
      Begin PaneConfiguration = 5
         NumPanes = 2
         Configuration = "(H (2 [66] 3))"
      End
      Begin PaneConfiguration = 6
         NumPanes = 2
         Configuration = "(H (4 [50] 3))"
      End
      Begin PaneConfiguration = 7
         NumPanes = 1
         Configuration = "(V (3))"
      End
      Begin PaneConfiguration = 8
         NumPanes = 3
         Configuration = "(H (1[56] 4[18] 2) )"
      End
      Begin PaneConfiguration = 9
         NumPanes = 2
         Configuration = "(H (1 [75] 4))"
      End
      Begin PaneConfiguration = 10
         NumPanes = 2
         Configuration = "(H (1[66] 2) )"
      End
      Begin PaneConfiguration = 11
         NumPanes = 2
         Configuration = "(H (4 [60] 2))"
      End
      Begin PaneConfiguration = 12
         NumPanes = 1
         Configuration = "(H (1) )"
      End
      Begin PaneConfiguration = 13
         NumPanes = 1
         Configuration = "(V (4))"
      End
      Begin PaneConfiguration = 14
         NumPanes = 1
         Configuration = "(V (2))"
      End
      ActivePaneConfig = 0
   End
   Begin DiagramPane = 
      Begin Origin = 
         Top = 0
         Left = 0
      End
      Begin Tables = 
         Begin Table = "ResourceStatus"
            Begin Extent = 
               Top = 132
               Left = 331
               Bottom = 262
               Right = 511
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Callsign"
            Begin Extent = 
               Top = 131
               Left = 630
               Bottom = 227
               Right = 800
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ResourceStatus_1"
            Begin Extent = 
               Top = 6
               Left = 838
               Bottom = 219
               Right = 1018
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "ResourceType"
            Begin Extent = 
               Top = 294
               Left = 38
               Bottom = 407
               Right = 234
            End
            DisplayFlags = 280
            TopColumn = 0
         End
         Begin Table = "Resource"
            Begin Extent = 
               Top = 6
               Left = 38
               Bottom = 294
               Right = 239
            End
            DisplayFlags = 280
            TopColumn = 12
         End
      End
   End
   Begin SQLPane = 
   End
   Begin DataPane = 
      Begin ParameterDefaults = ""
      End
   End
   Begin CriteriaPane = 
      Begin ColumnWidths = 11
         Column = 2355
         Alias = 900
         Table = 1170
         Output = 720
         Append = 1400
         NewValue = 1170
         SortType = 1350
         SortOrder = 1410
         GroupBy = 1350
         Filter = 1350
         Or = 1350
         Or ' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ResourceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPane2', @value=N'= 1350
         Or = 1350
      End
   End
End
' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ResourceView'
GO
EXEC sys.sp_addextendedproperty @name=N'MS_DiagramPaneCount', @value=2 , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'VIEW',@level1name=N'ResourceView'
GO
USE [master]
GO
ALTER DATABASE [Quest] SET  READ_WRITE 
GO
