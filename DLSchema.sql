USE [master]
GO
/****** Object:  Database [DataLaundry]    Script Date: 13-02-2020 18:14:37 ******/
CREATE DATABASE [DataLaundry]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'DataLaundry', FILENAME = N'D:\RDSDBDATA\DATA\DataLaundry.mdf' , SIZE = 7675904KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'DataLaundry_log', FILENAME = N'D:\RDSDBDATA\DATA\DataLaundry_log.ldf' , SIZE = 19079168KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
GO
ALTER DATABASE [DataLaundry] SET COMPATIBILITY_LEVEL = 130
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [DataLaundry].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [DataLaundry] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [DataLaundry] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [DataLaundry] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [DataLaundry] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [DataLaundry] SET ARITHABORT OFF 
GO
ALTER DATABASE [DataLaundry] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [DataLaundry] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [DataLaundry] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [DataLaundry] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [DataLaundry] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [DataLaundry] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [DataLaundry] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [DataLaundry] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [DataLaundry] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [DataLaundry] SET  DISABLE_BROKER 
GO
ALTER DATABASE [DataLaundry] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [DataLaundry] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [DataLaundry] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [DataLaundry] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [DataLaundry] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [DataLaundry] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [DataLaundry] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [DataLaundry] SET RECOVERY FULL 
GO
ALTER DATABASE [DataLaundry] SET  MULTI_USER 
GO
ALTER DATABASE [DataLaundry] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [DataLaundry] SET DB_CHAINING OFF 
GO
ALTER DATABASE [DataLaundry] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [DataLaundry] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
ALTER DATABASE [DataLaundry] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [DataLaundry] SET QUERY_STORE = OFF
GO
USE [DataLaundry]
GO
ALTER DATABASE SCOPED CONFIGURATION SET LEGACY_CARDINALITY_ESTIMATION = OFF;
GO
ALTER DATABASE SCOPED CONFIGURATION SET MAXDOP = 0;
GO
ALTER DATABASE SCOPED CONFIGURATION SET PARAMETER_SNIFFING = ON;
GO
ALTER DATABASE SCOPED CONFIGURATION SET QUERY_OPTIMIZER_HOTFIXES = OFF;
GO
USE [DataLaundry]
GO
/****** Object:  User [datalaundry_sa]    Script Date: 13-02-2020 18:14:39 ******/
CREATE USER [datalaundry_sa] FOR LOGIN [datalaundry_sa] WITH DEFAULT_SCHEMA=[dbo]
GO
ALTER ROLE [db_owner] ADD MEMBER [datalaundry_sa]
GO
/****** Object:  UserDefinedTableType [dbo].[TTFilterCriteria]    Script Date: 13-02-2020 18:14:39 ******/
CREATE TYPE [dbo].[TTFilterCriteria] AS TABLE(
	[FilterCriteriaId] [int] NULL,
	[RuleId] [int] NULL,
	[FieldMappingId] [int] NULL,
	[FieldId] [nvarchar](250) NULL,
	[OperatorId] [int] NULL,
	[Value] [nvarchar](max) NULL,
	[OperationId] [int] NULL,
	[ParentId] [int] NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TTOperation]    Script Date: 13-02-2020 18:14:39 ******/
CREATE TYPE [dbo].[TTOperation] AS TABLE(
	[OperationId] [int] NULL,
	[FieldId] [int] NULL,
	[Value] [nvarchar](max) NULL,
	[CurrentWord] [nvarchar](max) NULL,
	[NewWord] [nvarchar](max) NULL,
	[Sentance] [nvarchar](max) NULL,
	[FirstFieldId] [int] NULL,
	[SecondFieldId] [int] NULL,
	[OperationTypeId] [int] NULL,
	[OperationType] [nvarchar](255) NULL
)
GO
/****** Object:  UserDefinedTableType [dbo].[TTPlace]    Script Date: 13-02-2020 18:14:39 ******/
CREATE TYPE [dbo].[TTPlace] AS TABLE(
	[Id] [bigint] NULL,
	[PostalCode] [nvarchar](50) NULL,
	[Lat] [nvarchar](50) NULL,
	[Long] [nvarchar](50) NULL,
	[FeedProviderId] [int] NULL,
	[EventId] [bigint] NULL
)
GO
/****** Object:  UserDefinedFunction [dbo].[AgeRangeForGivenEvent]    Script Date: 13-02-2020 18:14:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT [dbo].[AgeRangeForGivenEvent] (107060)
CREATE FUNCTION [dbo].[AgeRangeForGivenEvent] 
(
	@eventId BIGINT
)
RETURNS NVARCHAR(50)
AS
BEGIN	
	DECLARE @resultedAgeRange NVARCHAR(MAX) = NULL;
	DECLARE @Agerange NVARCHAR(MAX);
	DECLARE @minAgeFromTable BIGINT;
	DECLARE @maxAgeFromTable BIGINT;
	
	SELECT @Agerange = Agerange FROM Event WITH (NOLOCK) WHERE id = @EventId; 

	-- check whether agerange contains object
	IF CHARINDEX('{', @Agerange) > 0 AND CHARINDEX('}', @Agerange) > 0
	BEGIN
		SELECT TOP 1 
			@minAgeFromTable = CAST(ROUND(RIGHT(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))), CHARINDEX(':', REVERSE(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))))) - 1),0,1) AS INT)
		FROM dbo.SplitString(@Agerange, ',') 
		where item like '%minvalue%'

		SELECT TOP 1 
			@maxAgeFromTable = CAST(ROUND(RIGHT(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))), CHARINDEX(':', REVERSE(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))))) - 1),0,1) AS INT)
		FROM dbo.SplitString(@Agerange, ',') 
		where item like '%maxvalue%'

		IF @minAgeFromTable IS NOT NULL
		BEGIN
			SET @resultedAgeRange = @minAgeFromTable
		END

		IF @maxAgeFromTable IS NOT NULL
		BEGIN
			SET @resultedAgeRange = CONVERT(NVARCHAR,@resultedAgeRange)+ '-' + CONVERT(NVARCHAR,@maxAgeFromTable)
		END	
	END
	-- check whether agerange contains string with range
	ELSE
		SET @resultedAgeRange = REPLACE(@Agerange, ' ', '')
	
	RETURN @resultedAgeRange;
END

GO
/****** Object:  UserDefinedFunction [dbo].[CheckStringExistanceFromDelimeterSeperatedString]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.CheckStringExistanceFromDelimeterSeperatedString('identifier,Identifier123,a,b', 'Identifier12', ',') 
CREATE FUNCTION [dbo].[CheckStringExistanceFromDelimeterSeperatedString]
(    
      @Input			NVARCHAR(MAX),
	  @ToBeCheckedInput NVARCHAR(MAX),
      @Character		CHAR(1)
)
RETURNS BIT
AS
BEGIN
		DECLARE @StartIndex INT, @EndIndex INT, @text NVARCHAR(100), @MatchFound BIT = 0
		 
		SET @StartIndex = 1
		IF SUBSTRING(@Input, LEN(@Input) - 1, LEN(@Input)) <> @Character
		BEGIN
			SET @Input = @Input + @Character
		END
 
		WHILE CHARINDEX(@Character, @Input) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(@Character, @Input)
            SET @text = SUBSTRING(@Input, @StartIndex, @EndIndex - 1)			
			
			IF LTRIM(RTRIM(@text)) = LTRIM(RTRIM(@ToBeCheckedInput)) COLLATE Latin1_General_CS_AS 
			BEGIN
				SET @MatchFound = 1
				BREAK;
			END 		
           
			SET @Input = SUBSTRING(@Input, @EndIndex + 1, LEN(@Input))
		END
		
		RETURN @MatchFound
END

GO
/****** Object:  UserDefinedFunction [dbo].[DateRangeByFrequencyAndDay]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('yearly',NULL,'2018-09-01','2020-10-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('monthly',NULL,'2018-09-01','2018-10-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('daily',NULL,'2018-09-01','2018-09-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('weekly',NULL,'2018-09-01','2018-09-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('weekly',1,'2018-09-01','2018-09-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('weekly',1,'2018-09-01','2018-09-30','23:15','01:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('weekly',NULL,'2018-09-01','2018-09-30','23:15','01:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('weekly',2,'2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('weekly',3,'2018-03-12','2018-03-12','12:00:00.0000000','13:00:00.0000000')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] ('monthly',5,'2018-09-01','2018-10-30','18:15','19:15')
CREATE FUNCTION [dbo].[DateRangeByFrequencyAndDay]
(     
	@Frequency	 NVARCHAR(50)
	,@ByDay		 INT			 
	,@StartDate  DATETIME	 
	,@EndDate    DATETIME	 
	,@StartTime  TIME        
	,@EndTime    TIME 
)
RETURNS  
	@SelectedRange TABLE 
	(StartDate DATETIME, EndDate DATETIME)
AS 
BEGIN
	DECLARE @TempEndDate DATETIME;

	SET @StartDate = IIF(@StartTime IS NOT NULL, CAST(CONVERT(DATE, @StartDate) AS DATETIME) + CAST(@StartTime AS DATETIME), @StartDate) 

	IF(@ByDay IS NOT NULL AND DATEPART(DW,@StartDate) <> @ByDay AND (@Frequency = 'weekly' OR @Frequency = 'monthly') AND @EndDate > @StartDate)
		SET @StartDate = dbo.NextWeekDayDate(@StartDate,@ByDay)
	
	SET @TempEndDate = (CAST(CONVERT(DATE, @StartDate) AS DATETIME) + IIF(@EndTime IS NOT NULL, CAST(@EndTime AS DATETIME), CAST(CONVERT(TIME, @EndDate) AS DATETIME)))
	IF CONVERT(TIME, @TempEndDate) < CONVERT(TIME, @StartDate)
		SET @TempEndDate = DATEADD(dd, 1, @TempEndDate)	

	;WITH cteRange(StartDate, EndDate) AS 
	(
		SELECT 
			@StartDate 
			,@TempEndDate 
		UNION ALL
		SELECT 
			CASE
				WHEN @Frequency = 'daily'   THEN DATEADD(dd, 1, StartDate)
				WHEN @Frequency = 'weekly'  THEN DATEADD(ww, 1, StartDate)
				WHEN @Frequency = 'monthly' THEN DATEADD(mm, 1, StartDate)
				WHEN @Frequency = 'yearly'  THEN DATEADD(YY, 1, StartDate)
			END,
			CASE
				WHEN @Frequency = 'daily'   THEN DATEADD(dd, 1, EndDate)
				WHEN @Frequency = 'weekly'  THEN DATEADD(ww, 1, EndDate)
				WHEN @Frequency = 'monthly' THEN DATEADD(mm, 1, EndDate)
				WHEN @Frequency = 'yearly'  THEN DATEADD(YY, 1, EndDate)
			END 	
		FROM cteRange
		WHERE CONVERT(DATE,StartDate) <= 
				CASE
					WHEN @Frequency = 'daily'   THEN DATEADD(dd, -1, @EndDate)
					WHEN @Frequency = 'weekly'  THEN DATEADD(ww, -1, @EndDate)
					WHEN @Frequency = 'monthly' THEN DATEADD(mm, -1, @EndDate)
					WHEN @Frequency = 'yearly'  THEN DATEADD(YY, -1, @EndDate)
				END
	)
          
	INSERT INTO @SelectedRange (StartDate,EndDate)
	SELECT * FROM cteRange	
	OPTION (MAXRECURSION 3660);
	RETURN
END

GO
/****** Object:  UserDefinedFunction [dbo].[DateRangeByFrequencyAndMultipleDay]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('yearly',NULL,'2018-09-01','2020-10-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('monthly',NULL,'2018-09-01','2018-10-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('daily',NULL,'2018-09-01','2018-09-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('weekly',NULL,'2018-09-01','2018-09-30','18:15','19:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('weekly',NULL,'2018-09-01','2018-09-30','23:15','01:15')
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('weekly',NULL,'2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('weekly','http://schema.org/Monday','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('weekly','1','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('weekly','http://schema.org/Monday,http://schema.org/Tuesday','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL) ORDER BY 1
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndMultipleDay] ('monthly','Sunday','2018-09-02','2018-09-30','08:00','16:00') ORDER BY 1

CREATE FUNCTION [dbo].[DateRangeByFrequencyAndMultipleDay]
(     
	@Frequency	 NVARCHAR(50)
	,@ByDays     NVARCHAR(500)   			 
	,@StartDate  DATETIME	 
	,@EndDate    DATETIME	 
	,@StartTime  TIME        
	,@EndTime    TIME 
)
RETURNS  
	@SelectedRange TABLE 
	(StartDate DATETIME, EndDate DATETIME)
AS 
BEGIN
	DECLARE @ByDay INT, @StartIndex INT, @EndIndex INT, @Day NVARCHAR(50);		
	
    SET @StartIndex = 1
    IF SUBSTRING(@ByDays, LEN(@ByDays) - 1, LEN(@ByDays)) <> ','
		SET @ByDays = @ByDays + ','

	IF(@ByDays IS NOT NULL AND @Frequency = 'weekly')
	BEGIN
		WHILE CHARINDEX(',', @ByDays) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByDays)		
			
			SET @Day = SUBSTRING(@ByDays, @StartIndex, @EndIndex - 1)						 
			SET @ByDay = IIF(ISNUMERIC(@Day) = 1,@Day,[dbo].[WeekDay_v1](REVERSE(LEFT(REVERSE(@Day), CHARINDEX('/', REVERSE(@Day)) - 1)),1))

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] (@Frequency,@ByDay,@StartDate,@EndDate,@StartTime,@EndTime)
			ORDER BY StartDate ASC
						
			SET @ByDays = SUBSTRING(@ByDays, @EndIndex + 1, LEN(@ByDays))
		END
	END
	ELSE
	BEGIN	
		IF(CHARINDEX('/', @ByDays) > 0)
			SET @ByDay = [dbo].[WeekDay_v1](REVERSE(LEFT(REVERSE(@ByDays), CHARINDEX('/', REVERSE(@ByDays)) - 1)),1)
		ELSE
			SET @ByDay = IIF(ISNUMERIC(@ByDays) = 1, CONVERT(INT,@ByDays), [dbo].[WeekDay_v1](@ByDays,1))
		
		INSERT INTO @SelectedRange (StartDate,EndDate)
		SELECT * FROM [dbo].[DateRangeByFrequencyAndDay] (@Frequency,@ByDay,@StartDate,@EndDate,@StartTime,@EndTime)
		ORDER BY StartDate ASC
	END
	RETURN
END

GO
/****** Object:  UserDefinedFunction [dbo].[DateRangeByFrequencyAndOtherCriteria]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('daily','2018-09-01','2018-09-30','18:15','19:15',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('weekly','2018-09-01','2018-09-30','18:15','19:15',1,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('monthly','2018-09-01','2018-09-30','18:15','19:15',NULL,1,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('monthly','2018-09-02','2018-10-30','18:15','19:15',1,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('yearly','2018-09-01','2020-10-30','18:15','19:15',NULL,NULL,1,DEFAULT)

-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('daily','2018-09-01',NULL,'18:15','19:15',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('weekly','2018-09-01',NULL,'18:15','19:15',1,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('monthly','2018-09-01',NULL,'18:15','19:15',1,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('yearly','2018-09-01',NULL,'18:15','19:15',NULL,NULL,1,DEFAULT)

-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] ('hourly','2018-09-01','2018-09-5','18:15','19:15',NULL,NULL,NULL,NULL,NULL,DEFAULT)
CREATE FUNCTION [dbo].[DateRangeByFrequencyAndOtherCriteria]
(     
	@Frequency			NVARCHAR(50)			 
	,@StartDate			DATETIME	 
	,@EndDate			DATETIME	 
	,@StartTime			TIME        
	,@EndTime			TIME
	,@ByDay				INT
	,@ByMonthDay		INT	
	,@ByMonth			INT
	,@RepeatCount		INT
	,@RepeatFrequency	NVARCHAR(50)
	,@OccCount			INT = 12
)
RETURNS  
	@SelectedRange TABLE 
	(StartDate DATETIME, EndDate DATETIME)
AS 
BEGIN
	DECLARE @TempEndDate DATETIME;

	SET @StartDate = IIF(@StartTime IS NOT NULL, CAST(CONVERT(DATE, @StartDate) AS DATETIME) + CAST(@StartTime AS DATETIME), @StartDate) 

	IF(@ByDay IS NOT NULL AND (@Frequency = 'weekly' OR @Frequency = 'monthly') AND DATEPART(DW,@StartDate) <> @ByDay)--  AND (@EndDate IS NULL OR @EndDate > @StartDate))
		SET @StartDate = dbo.NextDateByFrequency(@Frequency,@StartDate,@EndDate,@ByDay,IIF(@Frequency = 'monthly',@ByDay,NULL),DEFAULT,DEFAULT)	
	ELSE IF(@ByMonthDay IS NOT NULL AND @Frequency = 'monthly' AND DAY(@StartDate) <> @ByMonthDay)--  AND (@EndDate IS NULL OR @EndDate > @StartDate))
		SET @StartDate = dbo.NextDateByFrequency(@Frequency,@StartDate,@EndDate,@ByMonthDay,DEFAULT,DEFAULT,DEFAULT)
	ELSE IF(@ByMonth IS NOT NULL AND @Frequency = 'yearly' AND DATEPART(MM,@StartDate) <> @ByMonth)--  AND (@EndDate IS NULL OR @EndDate > @StartDate))
		SET @StartDate = dbo.NextDateByFrequency(@Frequency,@StartDate,@EndDate,@ByMonth,DEFAULT,DEFAULT,DEFAULT)
	
	SET @TempEndDate = (CAST(CONVERT(DATE, @StartDate) AS DATETIME) + IIF(@EndTime IS NOT NULL, CAST(@EndTime AS DATETIME), CAST(CONVERT(TIME, ISNULL(@EndDate,@StartDate)) AS DATETIME)))
	IF CONVERT(TIME, @TempEndDate) < CONVERT(TIME, @StartDate)
		SET @TempEndDate = DATEADD(dd, 1, @TempEndDate)	

	;WITH cteRange(RowNumber,StartDate, EndDate) AS 
	(
		SELECT 
			1
			,@StartDate 
			,IIF(@EndDate IS NOT NULL,@TempEndDate,NULL)
		UNION ALL
		SELECT 
			RowNumber + 1,
			CASE
				WHEN @Frequency IN ('hourly','P1H')			THEN DATEADD(HH, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)
				WHEN @Frequency IN ('daily','P1D')			THEN DATEADD(dd, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)				
				WHEN @Frequency IN ('weekly','P7D','P1W')	THEN DATEADD(ww, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)
				WHEN @Frequency IN ('monthly','P30D','P1M') THEN DATEADD(mm, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)
				WHEN @Frequency IN ('yearly','P365D','P1Y') THEN DATEADD(YY, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)
			END,
			CASE
				WHEN @EndDate IS NOT NULL
				THEN 
					CASE
						WHEN @Frequency IN ('hourly','P1H')			THEN DATEADD(HH, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)						
						WHEN @Frequency IN ('daily','P1D')			THEN DATEADD(dd, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
						WHEN @Frequency IN ('weekly','P7D','P1W')	THEN DATEADD(ww, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
						WHEN @Frequency IN ('monthly','P30D','P1M') THEN DATEADD(mm, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
						WHEN @Frequency IN ('yearly','P365D','P1Y') THEN DATEADD(YY, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
					END
				ELSE NULL
			END 	
		FROM	cteRange
		WHERE	(@EndDate IS NULL AND RowNumber < @OccCount)
				OR 
				CONVERT(DATE,StartDate) <= 
					CASE
						WHEN @Frequency IN ('hourly','P1H')			THEN DATEADD(HH, -1, @EndDate)						
						WHEN @Frequency IN ('daily','P1D')			THEN DATEADD(dd, -1, @EndDate)
						WHEN @Frequency IN ('weekly','P7D','P1W')	THEN DATEADD(ww, -1, @EndDate)
						WHEN @Frequency IN ('monthly','P30D','P1M') THEN DATEADD(mm, -1, @EndDate)
						WHEN @Frequency IN ('yearly','P365D','P1Y') THEN DATEADD(YY, -1, @EndDate)
					END
	)
          
	INSERT INTO @SelectedRange (StartDate,EndDate)
	SELECT StartDate,EndDate FROM cteRange
	WHERE StartDate IS NOT NULL	
	OPTION (MAXRECURSION 3660);
	RETURN
END
GO
/****** Object:  UserDefinedFunction [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('daily','2018-09-01','2018-09-30','18:15','19:15',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('weekly','2018-09-01','2018-09-30','18:15','19:15',1,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('monthly','2018-09-01','2018-09-30','18:15','19:15',NULL,1,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('monthly','2018-09-02','2018-10-30','18:15','19:15',1,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('yearly','2018-09-01','2020-10-30','18:15','19:15',NULL,NULL,1,DEFAULT)
															
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('daily','2018-09-01',NULL,'18:15','19:15',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('weekly','2018-09-01',NULL,'18:15','19:15',1,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('monthly','2018-09-01',NULL,'18:15','19:15',1,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('yearly','2018-09-01',NULL,'18:15','19:15',NULL,NULL,1,DEFAULT)
															 
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('P1W','2018-09-01','2018-09-30','18:15','19:15',1,NULL,NULL,NULL,2,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] ('hourly','2018-09-01','2018-09-5','18:15','19:15',NULL,NULL,NULL,NULL,NULL,DEFAULT)
CREATE FUNCTION [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601]
(     
	@Frequency			NVARCHAR(50)			 
	,@StartDate			DATETIME	 
	,@EndDate			DATETIME	 
	,@StartTime			TIME        
	,@EndTime			TIME
	,@ByDay				INT
	,@ByMonthDay		INT	
	,@ByMonth			INT
	,@RepeatCount		INT
	,@RepeatFrequency	NVARCHAR(50)
	,@OccCount			INT = 12
)
RETURNS  
	@SelectedRange TABLE 
	(StartDate DATETIME, EndDate DATETIME)
AS 
BEGIN
	DECLARE @TempEndDate DATETIME;

	SET @StartDate = IIF(@StartTime IS NOT NULL, CAST(CONVERT(DATE, @StartDate) AS DATETIME) + CAST(@StartTime AS DATETIME), @StartDate) 

	IF(@ByDay IS NOT NULL AND (@Frequency in ('weekly','P7D','P1W') OR @Frequency in ('monthly','P30D','P1M')) AND DATEPART(DW,@StartDate) <> @ByDay)--  AND (@EndDate IS NULL OR @EndDate > @StartDate))
		SET @StartDate = dbo.NextDateByFrequency_ISO8601(@Frequency,@StartDate,@EndDate,@ByDay,IIF(@Frequency in ('monthly','P30D','P1M'),@ByDay,NULL),DEFAULT,DEFAULT)	
	ELSE IF(@ByMonthDay IS NOT NULL AND @Frequency in ('monthly','P30D','P1M') AND DAY(@StartDate) <> @ByMonthDay)--  AND (@EndDate IS NULL OR @EndDate > @StartDate))
		SET @StartDate = dbo.NextDateByFrequency_ISO8601(@Frequency,@StartDate,@EndDate,@ByMonthDay,DEFAULT,DEFAULT,DEFAULT)
	ELSE IF(@ByMonth IS NOT NULL AND @Frequency in ('yearly','P365D','P1Y') AND DATEPART(MM,@StartDate) <> @ByMonth)--  AND (@EndDate IS NULL OR @EndDate > @StartDate))
		SET @StartDate = dbo.NextDateByFrequency_ISO8601(@Frequency,@StartDate,@EndDate,@ByMonth,DEFAULT,DEFAULT,DEFAULT)
	
	SET @TempEndDate = (CAST(CONVERT(DATE, @StartDate) AS DATETIME) + IIF(@EndTime IS NOT NULL, CAST(@EndTime AS DATETIME), CAST(CONVERT(TIME, ISNULL(@EndDate,@StartDate)) AS DATETIME)))
	IF CONVERT(TIME, @TempEndDate) < CONVERT(TIME, @StartDate)
		SET @TempEndDate = DATEADD(dd, 1, @TempEndDate)	

	;WITH cteRange(RowNumber,StartDate, EndDate) AS 
	(
		SELECT 
			1
			,@StartDate 
			,IIF(@EndDate IS NOT NULL,@TempEndDate,NULL)
		UNION ALL
		SELECT 
			RowNumber + 1,
			CASE
				WHEN @Frequency IN ('daily','P1D')			THEN DATEADD(dd, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)				
				WHEN @Frequency IN ('hourly','P1H')			THEN DATEADD(HH, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)				
				WHEN @Frequency IN ('weekly','P7D','P1W')	THEN DATEADD(ww, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)
				WHEN @Frequency IN ('monthly','P30D','P1M') THEN DATEADD(mm, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)
				WHEN @Frequency IN ('yearly','P365D','P1Y') THEN DATEADD(YY, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), StartDate)
			END,
			CASE
				WHEN @EndDate IS NOT NULL
				THEN 
					CASE
						WHEN @Frequency IN ('daily','P1D')			THEN DATEADD(dd, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
						WHEN @Frequency IN ('hourly','P1H')			THEN DATEADD(HH, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
						WHEN @Frequency IN ('weekly','P7D','P1W')	THEN DATEADD(ww, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
						WHEN @Frequency IN ('monthly','P30D','P1M') THEN DATEADD(mm, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
						WHEN @Frequency IN ('yearly','P365D','P1Y') THEN DATEADD(YY, IIF(ISNUMERIC(@RepeatFrequency) = 1,@RepeatFrequency,1), EndDate)
					END
				ELSE NULL
			END 	
		FROM	cteRange
		WHERE	(@EndDate IS NULL AND RowNumber < @OccCount)
				OR 
				CONVERT(DATE,StartDate) <= 
					CASE
						WHEN @Frequency IN ('daily','P1D')			THEN DATEADD(dd, -1, @EndDate)						
						WHEN @Frequency IN ('hourly','P1H')			THEN DATEADD(HH, -1, @EndDate)						
						WHEN @Frequency IN ('weekly','P7D','P1W')	THEN DATEADD(ww, -1, @EndDate)
						WHEN @Frequency IN ('monthly','P30D','P1M') THEN DATEADD(mm, -1, @EndDate)
						WHEN @Frequency IN ('yearly','P365D','P1Y') THEN DATEADD(YY, -1, @EndDate)
					END
	)
          
	INSERT INTO @SelectedRange (StartDate,EndDate)
	SELECT StartDate,EndDate FROM cteRange
	WHERE StartDate IS NOT NULL	
	OPTION (MAXRECURSION 3660);
	RETURN
END
GO
/****** Object:  UserDefinedFunction [dbo].[DateRangeByFrequencyWithMultipleReccurrence]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('daily','2018-09-01','2018-09-30','18:15','19:15',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('weekly','2018-09-01','2018-09-30','18:15','19:15',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('weekly','2018-09-01','2018-09-30','23:15','01:15',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('weekly','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('weekly','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL,'http://schema.org/Monday',NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('weekly','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL,'1',NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('weekly','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL,'http://schema.org/Monday,http://schema.org/Tuesday',NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('monthly','2018-09-01','2018-10-30','18:15','19:15',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('monthly','2018-09-01','2018-10-30','08:00','16:00','Sunday',NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('monthly','2018-09-02','2018-09-30','08:00','16:00',NULL,'[1,2,15]',NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('yearly','2018-09-01','2020-10-30','18:15','19:15',NULL,NULL,'[1,2]',DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('P7D','2018-03-06','2019-01-15','20:30','22:00','http://schema.org/Tuesday',NULL,NULL,DEFAULT)
-- SELECT ROW_NUMBER() OVER(ORDER BY StartDate ASC) AS Row#,* FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence] ('weekly','2018-09-01 10:00:00',NULL,NULL,NULL,'http://schema.org/Monday,http://schema.org/Tuesday',NULL,NULL,DEFAULT)
CREATE FUNCTION [dbo].[DateRangeByFrequencyWithMultipleReccurrence]
(     
	@Frequency			NVARCHAR(50)			 
	,@StartDate			DATETIME	 
	,@EndDate			DATETIME	 
	,@StartTime			TIME        
	,@EndTime			TIME 
	,@ByDay				NVARCHAR(500)   
	,@ByMonthDay		NVARCHAR(500)   		
	,@ByMonth			NVARCHAR(500)
	,@RepeatCount		INT
	,@RepeatFrequency	NVARCHAR(50)
	,@OccCount			INT = 12
)
RETURNS  
	@SelectedRange TABLE 
	(StartDate DATETIME, EndDate DATETIME)
AS 
BEGIN
	DECLARE @ByRecur INT, @StartIndex INT, @EndIndex INT, @Day NVARCHAR(50);		
	
    SET @StartIndex = 1

	IF(@ByDay IS NOT NULL AND @Frequency = 'weekly')
	BEGIN
		IF SUBSTRING(@ByDay, LEN(@ByDay) - 1, LEN(@ByDay)) <> ','
		SET @ByDay = @ByDay + ','

		WHILE CHARINDEX(',', @ByDay) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByDay)		
			
			SET @Day = SUBSTRING(@ByDay, @StartIndex, @EndIndex - 1)						 
			SET @ByRecur = IIF(ISNUMERIC(@Day) = 1,@Day,[dbo].[WeekDay_v1](REVERSE(LEFT(REVERSE(@Day), CHARINDEX('/', REVERSE(@Day)) - 1)),1))

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByRecur,NULL,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByDay = SUBSTRING(@ByDay, @EndIndex + 1, LEN(@ByDay))
		END
	END
	ELSE IF (@ByMonthDay IS NOT NULL AND @Frequency = 'monthly')
	BEGIN
		SET @ByMonthDay = REPLACE(REPLACE(@ByMonthDay,']',''),'[','')

		IF SUBSTRING(@ByMonthDay, LEN(@ByMonthDay) - 1, LEN(@ByMonthDay)) <> ','
		SET @ByMonthDay = @ByMonthDay + ','

		WHILE CHARINDEX(',', @ByMonthDay) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByMonthDay)				
								 
			SET @ByRecur = SUBSTRING(@ByMonthDay, @StartIndex, @EndIndex - 1)	

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,NULL,@ByRecur,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByMonthDay = SUBSTRING(@ByMonthDay, @EndIndex + 1, LEN(@ByMonthDay))
		END
	END	
	ELSE IF (@ByMonth IS NOT NULL AND @Frequency = 'yearly')
	BEGIN
		SET @ByMonth = REPLACE(REPLACE(@ByMonth,']',''),'[','')

		IF SUBSTRING(@ByMonth, LEN(@ByMonth) - 1, LEN(@ByMonth)) <> ','
		SET @ByMonth = @ByMonth + ','

		WHILE CHARINDEX(',', @ByMonth) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByMonth)

			SET @ByRecur = SUBSTRING(@ByMonth, @StartIndex, @EndIndex - 1)

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,NULL,NULL,@ByRecur,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByMonth = SUBSTRING(@ByMonth, @EndIndex + 1, LEN(@ByMonth))
		END
	END	
	ELSE 
	BEGIN	
		IF(CHARINDEX('/', @ByDay) > 0)
			SET @ByRecur = [dbo].[WeekDay_v1](REVERSE(LEFT(REVERSE(@ByDay), CHARINDEX('/', REVERSE(@ByDay)) - 1)),1)
		ELSE
			SET @ByRecur = IIF(ISNUMERIC(@ByDay) = 1, CONVERT(INT,@ByDay), [dbo].[WeekDay_v1](@ByDay,1))
		
		INSERT INTO @SelectedRange (StartDate,EndDate)
		SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByRecur,NULL,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
		ORDER BY StartDate ASC
	END	
	RETURN
END
GO
/****** Object:  UserDefinedFunction [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('daily','2018-09-01','2018-09-30','18:15','19:15',NULL,NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('weekly','2018-09-01','2018-09-30','18:15','19:15',NULL,NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('weekly','2018-09-01','2018-09-30','23:15','01:15',NULL,NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('weekly','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL,NULL,NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('weekly','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL,'http://schema.org/Monday',NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('weekly','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL,'1',NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('weekly','2018-09-01 10:00:00','2018-09-30 23:00:00',NULL,NULL,'http://schema.org/Monday,http://schema.org/Tuesday',NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('monthly','2018-09-01','2018-10-30','18:15','19:15',NULL,NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('monthly','2018-09-01','2018-10-30','08:00','16:00','Sunday',NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('monthly','2018-09-02','2018-09-30','08:00','16:00',NULL,'[1,2,15]',NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('yearly','2018-09-01','2020-10-30','18:15','19:15',NULL,NULL,'[1,2]',NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('P7D','2018-03-06','2019-01-15','20:30','22:00','http://schema.org/Tuesday',NULL,NULL,NULL,NULL,DEFAULT)
-- SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] ('yearly','2019-05-09','2019-08-08','18:15','19:15','1MO',NULL,'[5,6,7,8]',NULL,NULL,DEFAULT)
CREATE FUNCTION [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601]
(     
	 @Frequency			NVARCHAR(50)			 
	,@StartDate			DATETIME	 
	,@EndDate			DATETIME	 
	,@StartTime			TIME        
	,@EndTime			TIME 
	,@ByDay				NVARCHAR(500)   
	,@ByMonthDay		NVARCHAR(500)   		
	,@ByMonth			NVARCHAR(500)
	,@RepeatCount		INT
	,@RepeatFrequency	NVARCHAR(50)
	,@OccCount			INT = 12
)
RETURNS  
	@SelectedRange TABLE 
	(StartDate DATETIME, EndDate DATETIME)
AS 
BEGIN	
	DECLARE @ByRecur INT, @StartIndex INT, @EndIndex INT, @Day NVARCHAR(50);		

    SET @StartIndex = 1

	IF(@ByDay IS NOT NULL AND @Frequency IN ('weekly','P7D','P1W'))
	BEGIN
		IF SUBSTRING(@ByDay, LEN(@ByDay) - 1, LEN(@ByDay)) <> ','
		SET @ByDay = @ByDay + ','

		WHILE CHARINDEX(',', @ByDay) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByDay)		
			
			SET @Day = SUBSTRING(@ByDay, @StartIndex, @EndIndex - 1)						 
			SET @ByRecur = IIF(ISNUMERIC(@Day) = 1,@Day,[dbo].[WeekDay_v1](REVERSE(LEFT(REVERSE(@Day), CHARINDEX('/', REVERSE(@Day)) - 1)),1))

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByRecur,NULL,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByDay = SUBSTRING(@ByDay, @EndIndex + 1, LEN(@ByDay))
		END
	END	
	ELSE IF (@ByMonthDay IS NOT NULL AND @Frequency IN ('monthly','P30D','P1M'))
	BEGIN
		SET @ByMonthDay = REPLACE(REPLACE(@ByMonthDay,']',''),'[','')

		IF SUBSTRING(@ByMonthDay, LEN(@ByMonthDay) - 1, LEN(@ByMonthDay)) <> ','
		SET @ByMonthDay = @ByMonthDay + ','

		WHILE CHARINDEX(',', @ByMonthDay) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByMonthDay)				
								 
			SET @ByRecur = SUBSTRING(@ByMonthDay, @StartIndex, @EndIndex - 1)	

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,NULL,@ByRecur,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByMonthDay = SUBSTRING(@ByMonthDay, @EndIndex + 1, LEN(@ByMonthDay))
		END
	END	
	ELSE IF (@ByDay IS NOT NULL AND @ByMonthDay IS NULL AND @Frequency IN ('monthly','P30D','P1M'))
	BEGIN
		IF SUBSTRING(@ByDay, LEN(@ByDay) - 1, LEN(@ByDay)) <> ','
			SET @ByDay = @ByDay + ','

			IF(@EndDate IS NULL)
			SET @EndDate = DATEADD(MONTH,@OccCount,@StartDate);

			WHILE CHARINDEX(',', @ByDay) > 0
			BEGIN
				SET @EndIndex = CHARINDEX(',', @ByDay)

				SET @StartDate = CAST(CAST(@StartDate AS DATE) AS DATETIME) + CAST(CAST(@StartTime AS TIME) AS DATETIME);
				SET @EndDate =   CAST(CAST(@EndDate AS DATE) AS DATETIME) + CAST(CAST(@EndTime AS TIME) AS DATETIME);
						
				INSERT INTO @SelectedRange (StartDate,EndDate)
				SELECT * FROM [dbo].[DateRangeFrequencyOtherByDay]
				(
					@Frequency,
					@StartDate,
					@EndDate,
					@ByDay,
					@OccCount
				)
				ORDER BY StartDate
										
				SET @ByDay = SUBSTRING(@ByDay, @EndIndex + 1, LEN(@ByDay))
			END
	END
	ELSE IF (@ByMonth IS NOT NULL AND @ByDay IS NOT NULL AND @Frequency IN ('yearly','P365D','P1Y'))
	BEGIN	
		   
		SET @ByMonth = REPLACE(REPLACE(@ByMonth,']',''),'[','')

		IF SUBSTRING(@ByMonth, LEN(@ByMonth) - 1, LEN(@ByMonth)) <> ','
		SET @ByMonth = @ByMonth + ','

		IF(@EndDate IS NULL)
		SET @EndDate = DATEADD(YEAR,@OccCount,@StartDate);

		WHILE CHARINDEX(',', @ByMonth) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByMonth)

			SET @ByRecur = SUBSTRING(@ByMonth, @StartIndex, @EndIndex - 1)
				
			SET @StartDate = CAST(CAST(@StartDate AS DATE) AS DATETIME) + CAST(CAST(@StartTime AS TIME) AS DATETIME);
			SET @EndDate =   CAST(CAST(@EndDate AS DATE) AS DATETIME) + CAST(CAST(@EndTime AS TIME) AS DATETIME);

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeFrequencyOtherByDay]
			(
				@Frequency,
				@StartDate,
				@EndDate,
				@ByDay,
				@OccCount
			)
			WHERE DATEPART(MONTH, StartDate) = @ByRecur				
						
			SET @ByMonth = SUBSTRING(@ByMonth, @EndIndex + 1, LEN(@ByMonth))
		END		
	END	
	ELSE IF (@ByMonth IS NOT NULL AND @Frequency IN ('yearly','P365D','P1Y'))
	BEGIN	
		   
		SET @ByMonth = REPLACE(REPLACE(@ByMonth,']',''),'[','')

		IF SUBSTRING(@ByMonth, LEN(@ByMonth) - 1, LEN(@ByMonth)) <> ','
		SET @ByMonth = @ByMonth + ','

		WHILE CHARINDEX(',', @ByMonth) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByMonth)

			SET @ByRecur = SUBSTRING(@ByMonth, @StartIndex, @EndIndex - 1)

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,NULL,NULL,@ByRecur,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByMonth = SUBSTRING(@ByMonth, @EndIndex + 1, LEN(@ByMonth))
		END		
	END	
	ELSE 
	BEGIN	
		IF(CHARINDEX('/', @ByDay) > 0)
			SET @ByRecur = [dbo].[WeekDay_v1](REVERSE(LEFT(REVERSE(@ByDay), CHARINDEX('/', REVERSE(@ByDay)) - 1)),1)
		ELSE
			SET @ByRecur = IIF(ISNUMERIC(@ByDay) = 1, CONVERT(INT,@ByDay), [dbo].[WeekDay_v1](@ByDay,1))
		
		INSERT INTO @SelectedRange (StartDate,EndDate)
		SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria_ISO8601] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByRecur,NULL,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
		ORDER BY StartDate ASC
	END	
	RETURN
END
GO
/****** Object:  UserDefinedFunction [dbo].[DateRangeByFrequencyWithMultipleReccurrence_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- for this function, need to set datefirst 1
-- SET datefirst 1;SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_v1] ('monthly','2018-09-01','2020-10-30','18:15','19:15','http://schema.org/Monday',NULL,NULL,DEFAULT)
-- SET datefirst 1;SELECT * FROM [dbo].[DateRangeByFrequencyWithMultipleReccurrence_v1] ('yearly','2018-09-01','2020-10-30','18:15','19:15','http://schema.org/Monday',NULL,NULL,DEFAULT)
CREATE FUNCTION [dbo].[DateRangeByFrequencyWithMultipleReccurrence_v1]
(     
	@Frequency			NVARCHAR(50)			 
	,@StartDate			DATETIME	 
	,@EndDate			DATETIME	 
	,@StartTime			TIME        
	,@EndTime			TIME 
	,@ByDay				NVARCHAR(500)   
	,@ByMonthDay		NVARCHAR(500)   		
	,@ByMonth			NVARCHAR(500)
	,@RepeatCount		INT
	,@RepeatFrequency	NVARCHAR(50)
	,@OccCount			INT = 12
)
RETURNS  
	@SelectedRange TABLE 
	(StartDate DATETIME, EndDate DATETIME)
AS 
BEGIN	
	DECLARE @ByRecur INT, @StartIndex INT, @EndIndex INT, @Day NVARCHAR(50);		

    SET @StartIndex = 1

	IF(@ByDay IS NOT NULL AND @Frequency = 'weekly')
	BEGIN
		IF SUBSTRING(@ByDay, LEN(@ByDay) - 1, LEN(@ByDay)) <> ','
		SET @ByDay = @ByDay + ','

		WHILE CHARINDEX(',', @ByDay) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByDay)		
			
			SET @Day = SUBSTRING(@ByDay, @StartIndex, @EndIndex - 1)						 
			SET @ByRecur = IIF(ISNUMERIC(@Day) = 1,@Day,[dbo].[WeekDay_v1](REVERSE(LEFT(REVERSE(@Day), CHARINDEX('/', REVERSE(@Day)) - 1)),0))

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByRecur,NULL,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByDay = SUBSTRING(@ByDay, @EndIndex + 1, LEN(@ByDay))
		END
	END
	ELSE IF (@ByMonthDay IS NOT NULL AND @Frequency = 'monthly')
	BEGIN
		SET @ByMonthDay = REPLACE(REPLACE(@ByMonthDay,']',''),'[','')

		IF SUBSTRING(@ByMonthDay, LEN(@ByMonthDay) - 1, LEN(@ByMonthDay)) <> ','
		SET @ByMonthDay = @ByMonthDay + ','

		WHILE CHARINDEX(',', @ByMonthDay) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByMonthDay)				
								 
			SET @ByRecur = SUBSTRING(@ByMonthDay, @StartIndex, @EndIndex - 1)	

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,NULL,@ByRecur,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByMonthDay = SUBSTRING(@ByMonthDay, @EndIndex + 1, LEN(@ByMonthDay))
		END
	END	
	ELSE IF (@ByMonth IS NOT NULL AND @Frequency = 'yearly')
	BEGIN
		SET @ByMonth = REPLACE(REPLACE(@ByMonth,']',''),'[','')

		IF SUBSTRING(@ByMonth, LEN(@ByMonth) - 1, LEN(@ByMonth)) <> ','
		SET @ByMonth = @ByMonth + ','

		WHILE CHARINDEX(',', @ByMonth) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(',', @ByMonth)

			SET @ByRecur = SUBSTRING(@ByMonth, @StartIndex, @EndIndex - 1)

			INSERT INTO @SelectedRange (StartDate,EndDate)
			SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,NULL,NULL,@ByRecur,@RepeatCount,@RepeatFrequency,@OccCount)
			ORDER BY StartDate ASC
						
			SET @ByMonth = SUBSTRING(@ByMonth, @EndIndex + 1, LEN(@ByMonth))
		END
	END	
	ELSE 
	BEGIN	
		IF(CHARINDEX('/', @ByDay) > 0)
			SET @ByRecur = [dbo].[WeekDay_v1](REVERSE(LEFT(REVERSE(@ByDay), CHARINDEX('/', REVERSE(@ByDay)) - 1)),0)
		ELSE
			SET @ByRecur = IIF(ISNUMERIC(@ByDay) = 1, CONVERT(INT,@ByDay), [dbo].[WeekDay_v1](@ByDay,0))
		
		INSERT INTO @SelectedRange (StartDate,EndDate)
		SELECT * FROM [dbo].[DateRangeByFrequencyAndOtherCriteria] (@Frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByRecur,NULL,NULL,@RepeatCount,@RepeatFrequency,@OccCount)
		ORDER BY StartDate ASC
	END	
	RETURN
END
GO
/****** Object:  UserDefinedFunction [dbo].[DateRangeFrequencyOtherByDay]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <2019-05-06>
-- Description:	<>
-- =============================================
CREATE FUNCTION [dbo].[DateRangeFrequencyOtherByDay]
(	
	 @Frequency	 NVARCHAR(50)		
	,@StartDate	 DATETIME
	,@EndDate	 DATETIME
	,@ByDay      NVARCHAR(50)
	,@OccCount	 INT = 12
)
RETURNS  
	@SelectedRange TABLE 
	(StartDate DATETIME, EndDate DATETIME)
AS 
BEGIN
	DECLARE @Start DATETIME = @StartDate;
	DECLARE @End DATETIME = @EndDate;
	
	DECLARE @FirststartDate DATETIME =  DATEADD(MONTH, DATEDIFF(MONTH, 0, @Start), 0);
	DECLARE @LastEndDate DATETIME = DATEADD (dd, -1, DATEADD(mm, DATEDIFF(mm, 0, @End) + 1, 0));
	DECLARE @StartTime TIME(0) = CAST(@Start AS TIME);
	DECLARE @EndTime TIME(0) = CAST(@End AS TIME);
	SET @LastEndDate = DATEADD(DAY, DATEDIFF(DAY, 0, @LastEndDate),CAST(@EndTime AS NVARCHAR))

	DECLARE @DayName NVARCHAR(10);
	DECLARE @WeeklyNumber INT;
		
	SET @DayName = [dbo].[GetAlphsOrNumericValue](@ByDay,0);
	SET @WeeklyNumber = [dbo].[GetAlphsOrNumericValue](@ByDay,1);				 
	SET @DayName = [dbo].[GetWeekDayName]([dbo].[WeekDay_v1](@DayName,1));

		;WITH Cte AS
		(
			SELECT DATEADD(DAY, DATEDIFF(DAY, 0, @FirststartDate),CAST(@StartTime AS NVARCHAR)) AS StartDate,
			DATEADD(DAY, DATEDIFF(DAY, 0, @FirststartDate),CAST(@EndTime AS NVARCHAR)) AS EndDate,
			DATENAME(WEEKDAY,@FirststartDate) WeekDayName,1 LEVEL 
			UNION ALL
			SELECT DATEADD(DAY,1,StartDate),
			DATEADD(DAY,1,EndDate),
			DATENAME(WEEKDAY,DATEADD(DAY,1,StartDate)),LEVEL+1
			FROM CTE
			WHERE StartDate <= @LastEndDate
		),
		Cte2 AS 
		(
			SELECT StartDate,EndDate,WeekDayName,
			LEVEL-ROW_NUMBER() OVER(PARTITION BY DATEADD(MONTH,DATEDIFF(MONTH,0,StartDate),0) ORDER BY StartDate) RN,
			DATEADD(MONTH,DATEDIFF(MONTH,0,StartDate),0) MonthNames 
			FROM CTE WHERE WeekDayName = @DayName
		),
		Cte3 AS
		(
			SELECT StartDate,EndDate,WeekDayName,DATENAME(MONTH,MonthNames) MonthNames,
			DENSE_RANK() OVER(PARTITION BY MonthNames ORDER BY RN) WeekDay FROM CTE2 
		)
		INSERT INTO @SelectedRange(StartDate,EndDate)
		SELECT StartDate,EndDate
		FROM CTE3 
		WHERE WeekDay = @WeeklyNumber AND StartDate >= @Start  AND EndDate <= @End
		ORDER BY StartDate ASC
		OPTION(MAXRECURSION 0)
	RETURN
END
GO
/****** Object:  UserDefinedFunction [dbo].[GenderForGivenEvent]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT [dbo].[GenderForGivenEvent] (120220,'http://openactive.io/ns#female')
-- SELECT [dbo].[GenderForGivenEvent] (120220,'http://openactive.io/ns#male')
-- SELECT [dbo].[GenderForGivenEvent] (120220,'http://openactive.io/ns#none')
-- SELECT [dbo].[GenderForGivenEvent] (120220,'https://openactive.io/NoRestriction')
-- SELECT [dbo].[GenderForGivenEvent] (120220,'https://openactive.io/MaleOnly')
-- SELECT [dbo].[GenderForGivenEvent] (120220,'https://openactive.io/FemaleOnly')
CREATE FUNCTION [dbo].[GenderForGivenEvent] 
(
	@eventId			BIGINT,
	@GenderRestriction	NVARCHAR(MAX)
)
RETURNS NVARCHAR(50)
AS
BEGIN	
	DECLARE @result NVARCHAR(50) = NULL;	
	
	IF CHARINDEX('#', @GenderRestriction) > 0
	BEGIN
		SET @result = RIGHT(LTRIM(RTRIM(@GenderRestriction)), CHARINDEX('#', REVERSE(LTRIM(RTRIM(@GenderRestriction)))) - 1)
		--SET @result = IIF(@result = 'none',NULL,@result)
		SET @result = IIF(@result = 'none','mixed',@result)
	END
	ELSE IF CHARINDEX('/', REVERSE(LTRIM(RTRIM(@GenderRestriction)))) > 0
	BEGIN
		SET @result = RIGHT(LTRIM(RTRIM(@GenderRestriction)), CHARINDEX('/', REVERSE(LTRIM(RTRIM(@GenderRestriction)))) - 1)
		--SET @result = IIF((@result = 'none' OR @result = 'NoRestriction'),NULL,@result)
		SET @result = IIF((@result = 'none' OR @result = 'NoRestriction'),'mixed',@result)
	END
	ELSE
	BEGIN
		SET @result = @GenderRestriction
	END

	IF(LOWER(@result) = 'women' OR LOWER(@result) = 'woman' OR LOWER(@result) = 'femaleonly')
	BEGIN
		SET @result = 'female'
	END
	ELSE IF(LOWER(@result) = 'men' OR LOWER(@result) = 'man' OR LOWER(@result) = 'maleonly')
	BEGIN
		SET @result = 'male'
	END
	
	RETURN @result;
END

GO
/****** Object:  UserDefinedFunction [dbo].[GenerateWeekDaysByFrequency]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GenerateWeekDaysByFrequency]
(
	@Frequency	NVARCHAR(50),
	@ByDay		NVARCHAR(MAX)
)
RETURNS NVARCHAR(50)
AS
BEGIN
	DECLARE @WeekDays NVARCHAR(50) = NULL;	
	
	SET @WeekDays = (
		CASE
			WHEN @Frequency IS NOT NULL
			THEN
				CASE
					WHEN @Frequency = 'daily' OR @Frequency = 'weekly'
					THEN						
						(SELECT 
							DISTINCT STUFF(
								(
									SELECT 
										',' + CAST(Item AS nvarchar(MAX)) [text()] 
									FROM (
										SELECT 
											DISTINCT dbo.WeekDay(Item) as Item  
										FROM (
												SELECT REVERSE(LEFT(REVERSE(Item), CHARINDEX('/', REVERSE(Item)) - 1)) AS Item
												FROM dbo.SplitString(@ByDay, ',') 
											) AS tbl
									) AS tbl
									FOR XML PATH('')
								),1,1,'') Weekdays) 					
					WHEN @Frequency = 'monthly' OR @Frequency = 'yearly'
					THEN 						
						(
							SELECT CONVERT(NVARCHAR,dbo.WeekDay(SUBSTRING(@ByDay,2,3)))
						)
				END			
		END
	)

	RETURN @WeekDays;
END

GO
/****** Object:  UserDefinedFunction [dbo].[GetAlphsOrNumericValue]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetAlphsOrNumericValue]
	(@strAlphaNumeric VARCHAR(256),
	@IsAlphaNumeric BIT = 1
	)
RETURNS VARCHAR(256)
AS
BEGIN
	DECLARE @intAlpha INT;
	IF @IsAlphaNumeric = 1 /*Default Number allow*/
	BEGIN
		SET @intAlpha = PATINDEX('%[^0-9]%', @strAlphaNumeric)
		BEGIN
			WHILE @intAlpha > 0
				BEGIN
					SET @strAlphaNumeric = STUFF(@strAlphaNumeric, @intAlpha, 1,'')
					SET @intAlpha = PATINDEX('%[^0-9]%', @strAlphaNumeric )
				END
			END		
	END
	ELSE
	BEGIN
		SET @intAlpha = PATINDEX('%[^A-Za-z]%', @strAlphaNumeric)
		BEGIN
			WHILE @intAlpha > 0
				BEGIN
					SET @strAlphaNumeric = STUFF(@strAlphaNumeric, @intAlpha, 1,'')
					SET @intAlpha = PATINDEX('%[^A-Za-z]%', @strAlphaNumeric )
				END
			END
	END
	RETURN @strAlphaNumeric
END;
GO
/****** Object:  UserDefinedFunction [dbo].[GetDistanceBetween]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--SELECT [dbo].[GetDistanceBetween](51.61885000,-0.16261100,51.61885,-0.162611)
CREATE FUNCTION [dbo].[GetDistanceBetween]
(
    @Lat1 float,
    @Long1 float,
    @Lat2 float,
    @Long2 float
)
RETURNS float
AS
BEGIN
-- Return distance in miles
    DECLARE @RetVal float;
    SET @RetVal = ( SELECT geography::Point(@Lat1, @Long1, 4326).STDistance(geography::Point(@Lat2, @Long2, 4326)) / 1609.344 );

RETURN @RetVal;

END
GO
/****** Object:  UserDefinedFunction [dbo].[GetFrequencyInterval]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- select * from [dbo].[GetFrequencyInterval](NULL,'P7D')
-- select * from [dbo].[GetFrequencyInterval]('weekly',1)
-- select * from [dbo].[GetFrequencyInterval](NULL,1)
-- select * from [dbo].[GetFrequencyInterval]('weekly',NULL)
CREATE FUNCTION [dbo].[GetFrequencyInterval](
	@Frequency NVARCHAR(50),
	@RepeatFrequency NVARCHAR(50)
)
RETURNS  
	@result TABLE 
	(Frequency NVARCHAR(50), Interval NVARCHAR(50))
AS 
BEGIN
	DECLARE @Interval VARCHAR(100);
	DECLARE @AfterIntervalChar CHAR;
	
	IF(ISNULL(@RepeatFrequency,'') <> '' AND ISNUMERIC(@RepeatFrequency) = 0) 
			--AND @RepeatFrequency LIKE '^P(?=\d+[YMWD])(\d+Y)?(\d+M)?(\d+W)?(\d+D)?(T(?=\d+[HMS])(\d+H)?(\d+M)?(\d+S)?)?$')
	BEGIN
		SET @RepeatFrequency = (CASE									
									WHEN @RepeatFrequency = 'P7D' THEN 'P1W'
									WHEN @RepeatFrequency = 'P30D' THEN 'P1M'
									WHEN @RepeatFrequency = 'P365D' THEN 'P1Y'
									ELSE @RepeatFrequency
								END)


		INSERT INTO @result (Frequency,Interval)
		SELECT 
			(CASE
				WHEN data.frequency = 'H' THEN 'hourly'
				WHEN data.frequency = 'D' THEN 'daily'
				WHEN data.frequency = 'W' THEN 'weekly'
				WHEN data.frequency = 'M' THEN 'monthly'
				WHEN data.frequency = 'Y' THEN 'yearly'				
			END
			) as frequency
			,IIF(ISNULL(data.Interval,'') = '',1,data.Interval) AS Interval
		FROM (
			SELECT 
				LEFT(subsrt, PATINDEX('%[^0-9]%', subsrt + 't') - 1) as Interval
				,SUBSTRING(subsrt,PATINDEX('%[^0-9]%', subsrt + 't'),1) as frequency
			FROM (
				SELECT subsrt = SUBSTRING(@RepeatFrequency, PATINDEX('%[0-9]%', @RepeatFrequency), LEN(@RepeatFrequency))
			) t
		) AS data
	END
	ELSE	
	BEGIN
		INSERT INTO @result (Frequency,Interval)
		VALUES(@Frequency,@RepeatFrequency)
	END
	RETURN
END
GO
/****** Object:  UserDefinedFunction [dbo].[IsAgeRangeEligibleForGivenAge]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge](32181,15,17)
-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge](107060,NULL,NULL)
CREATE FUNCTION [dbo].[IsAgeRangeEligibleForGivenAge] 
(
	@eventId	BIGINT,
	@minAge		BIGINT = NULL,
	@maxAge		BIGINT = NULL
)
RETURNS BIT
AS
BEGIN
	DECLARE @Status BIT;
	DECLARE @Agerange NVARCHAR(MAX);
	DECLARE @minAgeFromTable BIGINT;
	DECLARE @maxAgeFromTable BIGINT;
	
	IF EXISTS(SELECT * FROM Event WITH (NOLOCK) WHERE id = @EventId)
	BEGIN
		SELECT @Agerange = ISNULL(Agerange,'') FROM Event WITH (NOLOCK) WHERE id = @EventId; 
		IF CHARINDEX('{', @Agerange) > 0 AND CHARINDEX('}', @Agerange) > 0
		BEGIN
			SELECT TOP 1 
				@minAgeFromTable = CAST(ROUND(RIGHT(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))), CHARINDEX(':', REVERSE(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))))) - 1),0,1) AS INT)
			FROM dbo.SplitString(@Agerange, ',') 
			where item like '%minvalue%'

			SELECT TOP 1 
				@maxAgeFromTable = CAST(ROUND(RIGHT(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))), CHARINDEX(':', REVERSE(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))))) - 1),0,1) AS INT)
			FROM dbo.SplitString(@Agerange, ',') 
			where item like '%maxvalue%'
		END
		ELSE IF CHARINDEX('-', @Agerange) > 0
		BEGIN
			SELECT 
				@minAgeFromTable = LTRIM(RTRIM(LEFT(@Agerange, CHARINDEX('-', @Agerange) - 1))),
				@maxAgeFromTable = LTRIM(RTRIM(RIGHT(@Agerange, CHARINDEX('-', REVERSE(@Agerange)) - 1)))
		END
		ELSE
		BEGIN
			SELECT @minAgeFromTable = LTRIM(RTRIM(@Agerange))
		END
	END		
	
	SET @Status = (
		CASE
			-- check if age is null
			WHEN @Agerange IS NULL THEN 1
			-- check if @minAge & @maxAge is null	
			WHEN @minAge IS NULL AND @maxAge IS NULL THEN 1			
			-- check if age is stored as an object with its min-max value
			WHEN (CHARINDEX('{', @Agerange) > 0 AND CHARINDEX('}', @Agerange) > 0) 
					OR CHARINDEX('-', @Agerange) > 0							
			THEN						
				CASE											
					--WHEN ISNULL(@minAgeFromTable,0) >= ISNULL(@minAge,0) AND ISNULL(@maxAgeFromTable,100) <= ISNULL(@maxAge,100) THEN 1
					WHEN  ISNULL(@minAge,0) >= ISNULL(@minAgeFromTable,0) 
							AND (@maxAge IS NULL OR @maxAgeFromTable IS NULL OR @maxAge <= @maxAgeFromTable) 
						THEN 1
					ELSE 0
				END			
			--WHEN @minAgeFromTable >= ISNULL(@minAge,0)
			WHEN ISNULL(@minAge,0) >= @minAgeFromTable 
			THEN
				CASE
					WHEN @maxAgeFromTable IS NULL AND (@maxAge IS NULL OR @minAgeFromTable <= @maxAge) THEN 1					
					--WHEN @maxAgeFromTable IS NOT NULL AND @maxAgeFromTable <= ISNULL(@maxAge,0) THEN 1
					WHEN @maxAgeFromTable IS NOT NULL AND (@maxAge IS NULL OR @maxAge <= @maxAgeFromTable) THEN 1
					ELSE 0
				END					
			ELSE 0
		END
	)	
	RETURN @Status;
END


GO
/****** Object:  UserDefinedFunction [dbo].[IsAgeRangeEligibleForGivenAge_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge_v1](107060,NULL,NULL)
-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge_v1](127870,15,30)
-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge_v1](126079,15,90)
-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge_v1](126070,15,90)
-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge_v1](126063,15,90)
-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge_v1](126076,15,90)

-- SELECT [dbo].[IsAgeRangeEligibleForGivenAge_v1](125615,12,20)
CREATE FUNCTION [dbo].[IsAgeRangeEligibleForGivenAge_v1] 
(
	@eventId	BIGINT,
	@minAge		INT = NULL,
	@maxAge		INT = NULL
)
RETURNS BIT
AS
BEGIN
	DECLARE @Status BIT;
	DECLARE @Agerange NVARCHAR(1000);
	DECLARE @minAgeOfEvent INT;
	DECLARE @maxAgeOfEvent INT;
	
	IF EXISTS(SELECT * FROM Event WITH (NOLOCK) WHERE id = @EventId)
	BEGIN
		SELECT @Agerange = ISNULL(Agerange,'') FROM Event WITH (NOLOCK) WHERE id = @EventId; 
		IF CHARINDEX('{', @Agerange) > 0 AND CHARINDEX('}', @Agerange) > 0
		BEGIN
			SELECT TOP 1 
				@minAgeOfEvent = CAST(ROUND(RIGHT(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))), CHARINDEX(':', REVERSE(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))))) - 1),0,1) AS INT)
			FROM dbo.SplitString(@Agerange, ',') 
			where item like '%minvalue%'

			SELECT TOP 1 
				@maxAgeOfEvent = CAST(ROUND(RIGHT(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))), CHARINDEX(':', REVERSE(LTRIM(RTRIM(REPLACE (REPLACE (REPLACE (Item, '"', ''), '{', ''), '}', ''))))) - 1),0,1) AS INT)
			FROM dbo.SplitString(@Agerange, ',') 
			where item like '%maxvalue%'
		END
		ELSE IF CHARINDEX('-', @Agerange) > 0
		BEGIN
			SELECT 
				@minAgeOfEvent = LTRIM(RTRIM(LEFT(@Agerange, CHARINDEX('-', @Agerange) - 1))),
				@maxAgeOfEvent = LTRIM(RTRIM(RIGHT(@Agerange, CHARINDEX('-', REVERSE(@Agerange)) - 1)))
		END
		ELSE
		BEGIN
			SELECT @minAgeOfEvent = LTRIM(RTRIM(@Agerange))
		END
	END		
	
	SET @Status = (
		CASE			
			WHEN @Agerange IS NULL THEN 1			
			WHEN @minAge IS NULL AND @maxAge IS NULL THEN 1			
			WHEN @maxAgeOfEvent IS NULL
			THEN
				CASE 
					WHEN @maxAge IS NULL AND ISNULL(@minAgeOfEvent,0) >= ISNULL(@minAge,0) THEN 1					
					WHEN @maxAge IS NOT NULL AND ISNULL(@minAgeOfEvent,0) BETWEEN ISNULL(@minAge,0) AND @maxAge THEN 1
					ELSE 0
				END
			WHEN @maxAgeOfEvent IS NOT NULL
			THEN 
				CASE
					WHEN  (ISNULL(@minAgeOfEvent,0) < ISNULL(@minAge,0) AND ISNULL(@maxAgeOfEvent,0) < ISNULL(@minAge,0))
					OR (ISNULL(@minAgeOfEvent,0) > ISNULL(@maxAge,0) AND ISNULL(@maxAgeOfEvent,0) > ISNULL(@maxAge,0)) 
						THEN 0
					ELSE 1
				END
		END
	)	
	RETURN @Status;
END


GO
/****** Object:  UserDefinedFunction [dbo].[IsEventEligibleForGivenWeekdays]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[IsEventEligibleForGivenWeekdays] 
(
	@EventId	BIGINT,
	@Weekdays	NVARCHAR(MAX)
)
RETURNS BIT
AS
BEGIN
	DECLARE @Status BIT;
	DECLARE @ByDay NVARCHAR(MAX);
	DECLARE @scheduledfrequency NVARCHAR(50);
	SELECT @scheduledfrequency = Frequency, @ByDay = ByDay FROM EventSchedule WITH (NOLOCK) WHERE EventId = @EventId; 
	
	SET @Status = (
		CASE
			WHEN @scheduledfrequency IS NOT NULL
			THEN
				CASE 
					-- check if it's scheduled as DAILY 
					WHEN @scheduledfrequency = 'daily' THEN 1
					-- check if it's scheduled as WEEKLY 
					WHEN @scheduledfrequency = 'weekly'
					THEN
						-- START => check occurances of weekdays as per given weekday
						CASE
							WHEN 
								(SELECT COUNT(*) FROM (
									SELECT * FROM (
										SELECT DISTINCT dbo.WeekDay(Item) as Item  FROM (
										SELECT REVERSE(LEFT(REVERSE(Item), CHARINDEX('/', REVERSE(Item)) - 1)) AS Item
										FROM dbo.SplitString(@ByDay, ',') 
									) AS tbl ) AS DaysFromTable
									WHERE DaysFromTable.Item IN (
										SELECT DISTINCT Item FROM (
											SELECT Item AS Item
											FROM dbo.SplitString(@Weekdays, ',') 
										) AS GivenWeekDays
									)
								) as total) > 0							
							THEN 1
							ELSE 0
						END	
						-- END	
					-- check if it's scheduled as MONTHLY OR YEARLY 
					WHEN @scheduledfrequency = 'monthly' OR @scheduledfrequency = 'yearly'
					THEN 
						-- START => check occurances of weekdays as per given weekday
						CASE
							WHEN
								(SELECT COUNT(*) FROM (
									SELECT * FROM ( SELECT dbo.WeekDay(SUBSTRING(@ByDay,2,3)) AS Item)AS days
									WHERE Item IN (
										SELECT DISTINCT Item FROM (
											SELECT Item AS Item
											FROM dbo.SplitString(@Weekdays, ',') 
										) AS GivenWeekDays
									)
								) as total) > 0							
							THEN 1
							ELSE 0
						END	
						-- END					
					ELSE 0
				END
			WHEN NOT EXISTS(SELECT * FROM EventSchedule WHERE EventId = @EventId)				
			THEN 
				CASE
					WHEN (SELECT COUNT(*) FROM (
							SELECT * FROM (
								select dbo.WeekDay(DATENAME(dw, (SELECT StartDate FROM Event WHERE id = @EventId))) as item
							) AS DaysFromTable
							WHERE DaysFromTable.Item IN (
								SELECT DISTINCT Item FROM (
									SELECT Item AS Item
									FROM dbo.SplitString(@Weekdays, ',') 
								) AS GivenWeekDays
							)
						) as total) > 0	
					THEN 1
					ELSE 0
				END
			ELSE 0
		END
	)


	RETURN @Status;

END
GO
/****** Object:  UserDefinedFunction [dbo].[IsEventEligibleForGivenWeekdays_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- select [dbo].[IsEventEligibleForGivenWeekdays_v1](126092,'1,2,3,4,5,6')
CREATE FUNCTION [dbo].[IsEventEligibleForGivenWeekdays_v1] 
(
	@EventId	BIGINT,
	@Weekdays	NVARCHAR(MAX)
)
RETURNS BIT
AS
BEGIN
	DECLARE @Status BIT;
	DECLARE @ByDay NVARCHAR(MAX);
	DECLARE @scheduledfrequency NVARCHAR(50);
	SELECT @scheduledfrequency = Frequency, @ByDay = ByDay FROM EventSchedule WITH (NOLOCK) WHERE EventId = @EventId; 
	
	SET @Status = (
		CASE
			WHEN @scheduledfrequency IS NOT NULL
			THEN
				CASE 
					-- check if it's scheduled as DAILY 
					WHEN @scheduledfrequency = 'daily' THEN 1
					-- check if it's scheduled as WEEKLY 
					WHEN @scheduledfrequency = 'weekly'
					THEN
						-- START => check occurances of weekdays as per given weekday
						CASE
							WHEN (
									SELECT COUNT(WeekDays.Item) FROM (
										SELECT DISTINCT dbo.WeekDay(REVERSE(LEFT(REVERSE(Item), CHARINDEX('/', REVERSE(Item)) - 1))) AS Item
										FROM dbo.SplitString(@ByDay, ',') 
									) AS WeekDays
									WHERE ',' + @Weekdays + ',' LIKE '%,' + CAST(WeekDays.Item AS NVARCHAR) + ',%'
								) > 0							
							THEN 1
							ELSE 0
						END	
						-- END	
					-- check if it's scheduled as MONTHLY OR YEARLY 
					WHEN @scheduledfrequency = 'monthly' OR @scheduledfrequency = 'yearly'
					THEN 
						-- START => check occurances of weekdays as per given weekday
						CASE
							WHEN (
									SELECT COUNT(*) FROM ( 
										SELECT 
											CASE
												WHEN CHARINDEX('/', @ByDay) > 0 THEN dbo.WeekDay(REVERSE(LEFT(REVERSE(@ByDay), CHARINDEX('/', REVERSE(@ByDay)) - 1)))
												ELSE dbo.WeekDay(SUBSTRING(@ByDay,2,3))
											END AS Item
									) AS days
									WHERE ',' + @Weekdays + ',' LIKE '%,' + CAST(days.Item AS NVARCHAR) + ',%'
								) > 0							
							THEN 1
							ELSE 0
						END	
						-- END					
					ELSE 0
				END
			WHEN NOT EXISTS(SELECT * FROM EventSchedule WHERE EventId = @EventId)				
			THEN 
				CASE
					WHEN (
						SELECT COUNT(*) FROM (
							SELECT dbo.WeekDay(DATENAME(dw, (SELECT StartDate FROM Event WHERE id = @EventId))) as item
						) AS DaysFromTable
						WHERE ',' + @Weekdays + ',' LIKE '%,' + CAST(DaysFromTable.Item AS NVARCHAR) + ',%'
					) > 0	
					THEN 1
					ELSE 0
				END
			ELSE 0
		END
	)


	RETURN @Status;

END
GO
/****** Object:  UserDefinedFunction [dbo].[IsValidRecurranceForSchedulerFrequency]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- SELECT [dbo].[IsValidRecurranceForSchedulerFrequency] (1,NULL,NULL,NULL,NULL,NULL,NULL,NULL)
-- SELECT [dbo].[IsValidRecurranceForSchedulerFrequency] (2,NULL,1,NULL,NULL,NULL,NULL,NULL)
-- SELECT [dbo].[IsValidRecurranceForSchedulerFrequency] (3,NULL,1,NULL,NULL,NULL,NULL,NULL)
-- SELECT [dbo].[IsValidRecurranceForSchedulerFrequency] (4,NULL,1,'Monday,Tuesday',NULL,NULL,NULL,NULL)
-- SELECT [dbo].[IsValidRecurranceForSchedulerFrequency] (5,NULL,NULL,NULL,'June,July,August',NULL,'1,2,3,4,-1','Sunday,Monday')
CREATE FUNCTION [dbo].[IsValidRecurranceForSchedulerFrequency]
(
	@SchedulerFrequencyId			INT,
	@LastExecutionDateTime			DATETIME,
	@RecurranceInterval				INT,
	@RecurranceDaysInWeek			NVARCHAR(200),
	@RecurranceMonths				NVARCHAR(200),
	@RecurranceDatesInMonth			NVARCHAR(200),
	@RecurranceWeekNos				NVARCHAR(50),
	@RecurranceDaysInWeekForMonth	NVARCHAR(200)
)
RETURNS BIT
AS
BEGIN
	
	DECLARE @Status BIT

	SET @Status = (
		CASE 
			--check for one time only
			WHEN @SchedulerFrequencyId = 1 
				THEN	
					CASE 
						--check if it has been run earlier once or not
						WHEN @LastExecutionDateTime IS NULL
							THEN 1
						ELSE 0
					END
			--check for hourly
			WHEN @SchedulerFrequencyId = 2 
				THEN	
					CASE 
						--check configured recurrance hours count
						WHEN DATEDIFF(HOUR, ISNULL(@LastExecutionDateTime, GETUTCDATE()), GETUTCDATE()) % @RecurranceInterval = 0 
							THEN 
								CASE 
									--check if task has been run on the same hour
									--WHEN @LastExecutionDateTime IS NOT NULL 
									--		AND DATEDIFF(HOUR, @LastExecutionDateTime, GETUTCDATE()) = 0 
									--	THEN 0
									WHEN @LastExecutionDateTime IS NOT NULL 
											AND (CAST(DATEDIFF(SS, ISNULL(@LastExecutionDateTime, GETUTCDATE()), GETUTCDATE()) AS DECIMAL(11, 2)) / 3600) < @RecurranceInterval
										THEN 0
									ELSE 1
								END
						ELSE 0
					END
			--check for daily
			WHEN @SchedulerFrequencyId = 3 
				THEN	
					CASE 
						--check configured recurrance day count
						WHEN DATEDIFF(DAY, ISNULL(@LastExecutionDateTime, GETUTCDATE()), GETUTCDATE()) % @RecurranceInterval = 0 
							THEN 
								CASE 
									--check if task has been run on the same day
									WHEN @LastExecutionDateTime IS NOT NULL 
											AND DATEDIFF(DAY, @LastExecutionDateTime, GETUTCDATE()) = 0 
										THEN 0
									ELSE 1
								END
						ELSE 0
					END
			--check for weekly
			WHEN @SchedulerFrequencyId = 4 
				THEN 
					CASE
						--check configured recurrance week count
						WHEN DATEDIFF(WEEK, ISNULL(@LastExecutionDateTime, GETUTCDATE()), GETUTCDATE()) % @RecurranceInterval = 0 
							THEN 
								CASE 
									--check configured week name in the week name list
									WHEN CHARINDEX(DATENAME(dw, GETUTCDATE()), @RecurranceDaysInWeek) > 0 
										THEN 
											CASE 
												-- check if task is already run on this day in week
												WHEN @LastExecutionDateTime IS NOT NULL 
														AND CHARINDEX(DATENAME(dw, @LastExecutionDateTime), @RecurranceDaysInWeek) > 0
													THEN 0
												ELSE 1
											END
									ELSE 0
								END
						ELSE 0
					END
			--check for monthly
			WHEN @SchedulerFrequencyId = 5 
				THEN 
					CASE
						--check configured months list
						WHEN CHARINDEX(DATENAME(MONTH, GETUTCDATE()), @RecurranceMonths) > 0 
							--check configured dates in the month list
								AND ((CHARINDEX(CONVERT(VARCHAR,DAY(GETUTCDATE())), @RecurranceDatesInMonth) > 0 
										--check if last date of the month is configured
										OR (CHARINDEX(CONVERT(VARCHAR, 'Last'), @RecurranceDatesInMonth) > 0
											--check if current date of the month is the last date of the month
											AND CAST(GETUTCDATE() as date) = EOMONTH(GETUTCDATE()))
									 )
									OR 
									--check configured week number in the week number list
									(CHARINDEX(Convert(VARCHAR,(DATEDIFF(WEEK, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETUTCDATE()), 0), GETUTCDATE()) + 1)), @RecurranceWeekNos) > 0
									 --check if last week of the month is configured
									 OR ( CHARINDEX(CONVERT(VARCHAR, '-1'), @RecurranceWeekNos) > 0
											--check if current week of the month is the last week of the month
										  AND (DATEDIFF(WEEK, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETUTCDATE()), 0), GETUTCDATE()) + 1) 
												= 
											  (DATEDIFF(WEEK, DATEADD(MONTH, DATEDIFF(MONTH, 0, GETUTCDATE()), 0), EOMONTH(GETUTCDATE())) + 1)
										)
									) 
									--check configured day name in the day name list
									AND CHARINDEX(DATENAME(dw, GETUTCDATE()), @RecurranceDaysInWeekForMonth) > 0 
									)
									
							THEN 
								CASE
									--check if task has been already run on the configured
									WHEN @LastExecutionDateTime IS NOT NULL 
										AND	CHARINDEX(DATENAME(MONTH, @LastExecutionDateTime), @RecurranceMonths) > 0 
											--check configured dates in the month list
											AND (CHARINDEX(CONVERT(VARCHAR, DAY(@LastExecutionDateTime)), @RecurranceDatesInMonth) > 0 
												OR 
												--check configured week number in the week number list
												(CHARINDEX(Convert(VARCHAR, (DATEDIFF(WEEK, DATEADD(MONTH, DATEDIFF(MONTH, 0, @LastExecutionDateTime), 0), @LastExecutionDateTime) + 1)), @RecurranceWeekNos) > 0 
												--check configured week name in the week name list
												AND CHARINDEX(DATENAME(dw, @LastExecutionDateTime), @RecurranceDaysInWeekForMonth) > 0 )
												)
										THEN 0
									ELSE 1
								END
						ELSE 0
					END
			ELSE 0
		END
	)

	RETURN @Status;
END
GO
/****** Object:  UserDefinedFunction [dbo].[LatLonRadiusDistance]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- select [dbo].[LatLonRadiusDistance](51.5073509,-0.1277583,'50.42086','-4.110758')
CREATE FUNCTION [dbo].[LatLonRadiusDistance] 
(
	@lat1Degrees decimal(15,12),
	@lon1Degrees decimal(15,12),
	@lat2Degrees decimal(15,12),
	@lon2Degrees decimal(15,12)
)
RETURNS decimal(9,1)
AS
BEGIN

	DECLARE @earthSphereRadiusKilometers as decimal(10,6)
	DECLARE @kilometerConversionToMilesFactor as decimal(7,6)
	SELECT @earthSphereRadiusKilometers = 6366.707019
	SELECT @kilometerConversionToMilesFactor = .621371

	-- convert degrees to radians
	DECLARE @lat1Radians decimal(15,12)
	DECLARE @lon1Radians decimal(15,12)
	DECLARE @lat2Radians decimal(15,12)
	DECLARE @lon2Radians decimal(15,12)
	SELECT @lat1Radians = (@lat1Degrees / 180) * PI()
	SELECT @lon1Radians = (@lon1Degrees / 180) * PI()
	SELECT @lat2Radians = (@lat2Degrees / 180) * PI()
	SELECT @lon2Radians = (@lon2Degrees / 180) * PI()

	-- formula for distance from [lat1,lon1] to [lat2,lon2]
	RETURN ROUND(2 * ASIN(SQRT(POWER(SIN((@lat1Radians - @lat2Radians) / 2) ,2)
        + COS(@lat1Radians) * COS(@lat2Radians) * POWER(SIN((@lon1Radians - @lon2Radians) / 2), 2)))
        * (@earthSphereRadiusKilometers * @kilometerConversionToMilesFactor), 4)
		* 1.60934 /*in kilometer*/
END

GO
/****** Object:  UserDefinedFunction [dbo].[NextDateByFrequency]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.NextDateByFrequency('weekly','2018-09-26 08:00:00.000','2018-09-30 08:00:00.000',1,DEFAULT,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency('monthly','2018-09-01 08:00:00.000','2018-09-30 08:00:00.000',1,DEFAULT,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency('monthly','2018-09-01 08:00:00.000','2018-09-30 08:00:00.000',1,1,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency('yearly','2018-09-01 08:00:00.000','2019-10-30 08:00:00.000',10,DEFAULT,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency('yearly','2018-09-01 08:00:00.000','2020-10-30 08:00:00.000',1,DEFAULT,DEFAULT,DEFAULT)
CREATE FUNCTION [dbo].[NextDateByFrequency](
	@Frequency	 NVARCHAR(50)		
	,@StartDate	 DATETIME
	,@EndDate	 DATETIME
	,@ByRecur	 INT
	,@ByDay		 INT = NULL
	,@ByMonthDay INT = NULL	
	,@ByMonth	 INT = NULL
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @idx INT, @ToBeReccur INT;
	
	-- set index to be reccurred
	SET @idx =	CASE
					WHEN @Frequency = 'weekly' THEN 6
					WHEN @Frequency = 'monthly' AND @ByDay IS NOT NULL THEN 6
					WHEN @Frequency = 'monthly' THEN (DAY(EOMONTH(@StartDate))) - 1
					WHEN @Frequency = 'yearly' THEN 11
				END

	---- set recurring count as per frequency
	--SET @ToBeReccur =	CASE
	--						WHEN @Frequency = 'weekly' THEN DATEPART(DW,@StartDate)
	--						WHEN @Frequency = 'monthly' AND @ByDay IS NOT NULL THEN DATEPART(DW,@StartDate)
	--						WHEN @Frequency = 'monthly' THEN DAY(@StartDate)
	--						WHEN @Frequency = 'yearly' THEN DATEPART(MM,@StartDate)
	--					END

	--WHILE @ToBeReccur <> @ByRecur
	--BEGIN			
	--	IF (@StartDate < @EndDate)
	--		SET @StartDate = IIF(@Frequency <> 'yearly',DATEADD(day,1,@StartDate),DATEADD(MM,1,@StartDate))

	--	SET @idx=@idx-1
	--	IF @idx < 0 
	--	BEGIN
	--		SET @StartDate = NULL 
	--		BREAK
	--	END
	--END

	IF(@Frequency = 'yearly')
	BEGIN
		--	SET @idx = 11
		--	WHILE DATEPART(MM,@StartDate) <> @ByRecur
		
		WHILE DATEPART(MM,@StartDate) <> @ByRecur
		BEGIN
			IF (@EndDate IS NULL OR @StartDate < @EndDate)
				SET @StartDate = DATEADD(MM,1,@StartDate)

			SET @idx=@idx-1
			IF @idx < 0 
			BEGIN
				SET @StartDate = NULL 
				BREAK
			END
		END
	END
	ELSE
	BEGIN
		--	SET @idx = IIF(@Frequency = 'weekly' OR @ByDay IS NOT NULL,6,(DAY(EOMONTH(@StartDate))) - 1)		
		--	WHILE IIF(@Frequency = 'weekly' OR @ByDay IS NOT NULL,DATEPART(DW,@StartDate),DAY(@StartDate)) <> @ByRecur
				
		WHILE IIF(@Frequency = 'weekly' OR @ByDay IS NOT NULL,DATEPART(DW,@StartDate),DAY(@StartDate)) <> @ByRecur
		BEGIN			
			IF (@EndDate IS NULL OR @StartDate < @EndDate)
				SET @StartDate = DATEADD(day,1,@StartDate)			

			SET @idx=@idx-1
			IF @idx < 0 
			BEGIN
				SET @StartDate = NULL 
				BREAK
			END
		END
	END
	RETURN @StartDate
END

GO
/****** Object:  UserDefinedFunction [dbo].[NextDateByFrequency_ISO8601]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.NextDateByFrequency_ISO8601('weekly','2018-09-26 08:00:00.000','2018-09-30 08:00:00.000',1,DEFAULT,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency_ISO8601('monthly','2018-09-01 08:00:00.000','2018-09-30 08:00:00.000',1,DEFAULT,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency_ISO8601('monthly','2018-09-01 08:00:00.000','2018-09-30 08:00:00.000',1,1,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency_ISO8601('yearly','2018-09-01 08:00:00.000','2019-10-30 08:00:00.000',10,DEFAULT,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency_ISO8601('yearly','2018-09-01 08:00:00.000','2020-10-30 08:00:00.000',1,DEFAULT,DEFAULT,DEFAULT)
-- SELECT dbo.NextDateByFrequency_ISO8601('P7D','2018-04-09 09:00:00.000','2018-04-12 17:00:00.000',2,DEFAULT,DEFAULT,DEFAULT)
CREATE FUNCTION [dbo].[NextDateByFrequency_ISO8601](
	@Frequency	 NVARCHAR(50)		
	,@StartDate	 DATETIME
	,@EndDate	 DATETIME
	,@ByRecur	 INT
	,@ByDay		 INT = NULL
	,@ByMonthDay INT = NULL	
	,@ByMonth	 INT = NULL
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @idx INT, @ToBeReccur INT;
	
	-- set index to be reccurred
	SET @idx =	CASE
					WHEN @Frequency in ('weekly','P7D','P1W') THEN 6
					WHEN @Frequency in ('monthly','P30D','P1M') AND @ByDay IS NOT NULL THEN 6
					WHEN @Frequency in ('monthly','P30D','P1M') THEN (DAY(EOMONTH(@StartDate))) - 1
					WHEN @Frequency in ('yearly','P365D','P1Y') THEN 11
				END

	---- set recurring count as per frequency
	--SET @ToBeReccur =	CASE
	--						WHEN @Frequency = 'weekly' THEN DATEPART(DW,@StartDate)
	--						WHEN @Frequency = 'monthly' AND @ByDay IS NOT NULL THEN DATEPART(DW,@StartDate)
	--						WHEN @Frequency = 'monthly' THEN DAY(@StartDate)
	--						WHEN @Frequency = 'yearly' THEN DATEPART(MM,@StartDate)
	--					END

	--WHILE @ToBeReccur <> @ByRecur
	--BEGIN			
	--	IF (@StartDate < @EndDate)
	--		SET @StartDate = IIF(@Frequency <> 'yearly',DATEADD(day,1,@StartDate),DATEADD(MM,1,@StartDate))

	--	SET @idx=@idx-1
	--	IF @idx < 0 
	--	BEGIN
	--		SET @StartDate = NULL 
	--		BREAK
	--	END
	--END

	IF(@Frequency in ('yearly','P365D','P1Y'))
	BEGIN
		--	SET @idx = 11
		--	WHILE DATEPART(MM,@StartDate) <> @ByRecur
		
		WHILE DATEPART(MM,@StartDate) <> @ByRecur
		BEGIN
			IF (@EndDate IS NULL OR @StartDate < @EndDate)
				SET @StartDate = DATEADD(MM,1,@StartDate)

			SET @idx=@idx-1
			IF @idx < 0 
			BEGIN
				SET @StartDate = NULL 
				BREAK
			END
		END
	END
	ELSE
	BEGIN
		--	SET @idx = IIF(@Frequency = 'weekly' OR @ByDay IS NOT NULL,6,(DAY(EOMONTH(@StartDate))) - 1)		
		--	WHILE IIF(@Frequency = 'weekly' OR @ByDay IS NOT NULL,DATEPART(DW,@StartDate),DAY(@StartDate)) <> @ByRecur
				
		WHILE IIF(@Frequency in ('weekly','P7D','P1W') OR @ByDay IS NOT NULL,DATEPART(DW,@StartDate),DAY(@StartDate)) <> @ByRecur
		BEGIN			
			IF (@EndDate IS NULL OR @StartDate < @EndDate)
				SET @StartDate = DATEADD(day,1,@StartDate)			

			SET @idx=@idx-1
			IF @idx < 0 
			BEGIN
				SET @StartDate = NULL 
				BREAK
			END
		END
	END
	RETURN @StartDate
END
GO
/****** Object:  UserDefinedFunction [dbo].[NextWeekDayDate]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.NextWeekDayDate('2018-08-26 08:00:00.000',1)
CREATE FUNCTION [dbo].[NextWeekDayDate](
	@StartDate	DATETIME
	,@ByDay		INT
)
RETURNS DATETIME
AS
BEGIN
	DECLARE @dx INT = 6
	WHILE DATEPART(DW,@StartDate) <> @ByDay
	BEGIN
		SET @StartDate = DATEADD(day,1,@StartDate)

		SET @dx=@dx-1
		IF @dx < 0 
		BEGIN
			SET @StartDate = NULL 
			BREAK
		END
	END
	RETURN @StartDate
END

GO
/****** Object:  UserDefinedFunction [dbo].[ReplaceStringFromDelimeterSeperatedString]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT dbo.ReplaceStringFromDelimeterSeperatedString('identifier,Identifier123,a,b', 'Identifier123', ',') 
CREATE FUNCTION [dbo].[ReplaceStringFromDelimeterSeperatedString]
(    
      @Input			NVARCHAR(MAX),
	  @ReplacableInput	NVARCHAR(MAX),
      @Character		CHAR(1)
)
RETURNS NVARCHAR(MAX)
AS
BEGIN
		DECLARE @StartIndex INT, @EndIndex INT, @output NVARCHAR(MAX), @text NVARCHAR(100)
 
		SET @StartIndex = 1
		IF SUBSTRING(@Input, LEN(@Input) - 1, LEN(@Input)) <> @Character
		BEGIN
			SET @Input = @Input + @Character
		END
 
		WHILE CHARINDEX(@Character, @Input) > 0
		BEGIN
			SET @EndIndex = CHARINDEX(@Character, @Input)
            SET @text = SUBSTRING(@Input, @StartIndex, @EndIndex - 1)
			
			--IF CONVERT(VARBINARY(250),LTRIM(RTRIM(@text))) <> CONVERT(VARBINARY(250),LTRIM(RTRIM(@ReplacableInput)))
			--BEGIN
			--	SET @output = CONCAT(@output, @text, ', ')
			--END 	
			IF LTRIM(RTRIM(@text)) <> LTRIM(RTRIM(@ReplacableInput)) COLLATE Latin1_General_CS_AS 
			BEGIN
				SET @output = CONCAT(@output, @text, ',')
			END 		
           
			SET @Input = SUBSTRING(@Input, @EndIndex + 1, LEN(@Input))
		END
		SET @output =   CASE     
							WHEN @output LIKE '%,' 
							THEN LTRIM(RTRIM(SUBSTRING(@output,1,LEN(@output)-1)))
	
							WHEN @output LIKE ',%' 
							THEN LTRIM(RTRIM(SUBSTRING(@output,2,LEN(@output))))
	
							ELSE LTRIM(RTRIM(@output))
						END
		--SET @output = LTRIM(RTRIM(SUBSTRING(@output,1,LEN(@output)-1)))
		RETURN @output
END

GO
/****** Object:  UserDefinedFunction [dbo].[SplitString]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT Item AS Item FROM dbo.SplitString('Gym', ',') 
CREATE FUNCTION [dbo].[SplitString]
(    
      @Input NVARCHAR(MAX),
      @Character CHAR(1)
)
RETURNS @Output TABLE (
      Item NVARCHAR(1000)
)
AS
BEGIN
      DECLARE @StartIndex INT, @EndIndex INT
 
      SET @StartIndex = 1
      IF SUBSTRING(@Input, LEN(@Input) - 1, LEN(@Input)) <> @Character
      BEGIN
            SET @Input = @Input + @Character
      END
 
      WHILE CHARINDEX(@Character, @Input) > 0
      BEGIN
            SET @EndIndex = CHARINDEX(@Character, @Input)
           
            INSERT INTO @Output(Item)
            SELECT SUBSTRING(@Input, @StartIndex, @EndIndex - 1)
           
            SET @Input = SUBSTRING(@Input, @EndIndex + 1, LEN(@Input))
      END
 
      RETURN
END



GO
/****** Object:  UserDefinedFunction [dbo].[SplitStringByComma]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT Item AS Item FROM dbo.SplitStringByComma('identifier,Identifier123,a,b') 
CREATE FUNCTION [dbo].[SplitStringByComma]
(    
      @Input NVARCHAR(MAX)
)
RETURNS @Output TABLE (
      Item NVARCHAR(1000)
)
AS
BEGIN
	INSERT INTO @Output (Item)	
	SELECT LTRIM(RTRIM(m.n.value('.[1]','varchar(8000)'))) AS Item	
	FROM
	(
		SELECT CAST('<XMLRoot><RowData>' + REPLACE(@Input,',','</RowData><RowData>') + '</RowData></XMLRoot>' AS XML) AS x
	)t
	CROSS APPLY x.nodes('/XMLRoot/RowData')m(n) 
    RETURN
END

GO
/****** Object:  UserDefinedFunction [dbo].[UNIX_TIMESTAMP]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[UNIX_TIMESTAMP]  
(
	@UTCDateTime DateTime
)
RETURNS INT
AS
BEGIN

  DECLARE @UNIXTimeStamp BIGINT

  SELECT @UNIXTimeStamp = DATEDIFF(SECOND,{D '1970-01-01'}, @UTCDateTime)

  return @UNIXTimeStamp

END
GO
/****** Object:  UserDefinedFunction [dbo].[WeekDay]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[WeekDay](
	@DayOfWeek Varchar(9)	
)
RETURNS INT
            AS
    BEGIN
    DECLARE @iDayofWeek INT
    SELECT @iDayofWeek = CASE @DayOfWeek
     --               WHEN 'Sunday' THEN 1
					--WHEN 'SU' THEN 1
     --               WHEN 'Monday' THEN 2
					--WHEN 'MO' THEN 2
     --               WHEN 'Tuesday' THEN 3
					--WHEN 'TU' THEN 3
     --               WHEN 'Wednesday' THEN 4
					--WHEN 'WE' THEN 4
     --               WHEN 'Thursday' THEN 5
					--WHEN 'TH' THEN 5
     --               WHEN 'Friday' THEN 6
					--WHEN 'FR' THEN 6
     --               WHEN 'Saturday' THEN 7
					--WHEN 'SA' THEN 7
					
                    WHEN 'Monday' THEN 1
					WHEN 'MO' THEN 1
                    WHEN 'Tuesday' THEN 2
					WHEN 'TU' THEN 2
                    WHEN 'Wednesday' THEN 3
					WHEN 'WE' THEN 3
                    WHEN 'Thursday' THEN 4
					WHEN 'TH' THEN 4
                    WHEN 'Friday' THEN 5
					WHEN 'FR' THEN 5
                    WHEN 'Saturday' THEN 6
					WHEN 'SA' THEN 6
					WHEN 'Sunday' THEN 7
					WHEN 'SU' THEN 7
        END
    RETURN (@iDayofWeek)
    END

GO
/****** Object:  UserDefinedFunction [dbo].[WeekDay_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT [dbo].[WeekDay_v1]('Monday',default)
CREATE FUNCTION [dbo].[WeekDay_v1](
	@DayOfWeek			VARCHAR(9)
	,@IsForSQLOperation BIT = 0
)
RETURNS INT
AS
BEGIN
DECLARE @iDayofWeek INT

IF(@IsForSQLOperation = 1)
BEGIN
	SELECT @iDayofWeek = CASE @DayOfWeek
							WHEN 'Sunday' THEN 1
							WHEN 'SU' THEN 1
							WHEN 'Monday' THEN 2
							WHEN 'MO' THEN 2
							WHEN 'Tuesday' THEN 3
							WHEN 'TU' THEN 3
							WHEN 'Wednesday' THEN 4
							WHEN 'WE' THEN 4
							WHEN 'Thursday' THEN 5
							WHEN 'TH' THEN 5
							WHEN 'Friday' THEN 6
							WHEN 'FR' THEN 6
							WHEN 'Saturday' THEN 7
							WHEN 'SA' THEN 7
						END
END
ELSE
BEGIN
	SELECT @iDayofWeek = CASE @DayOfWeek
							WHEN 'Monday' THEN 1
							WHEN 'MO' THEN 1
							WHEN 'Tuesday' THEN 2
							WHEN 'TU' THEN 2
							WHEN 'Wednesday' THEN 3
							WHEN 'WE' THEN 3
							WHEN 'Thursday' THEN 4
							WHEN 'TH' THEN 4
							WHEN 'Friday' THEN 5
							WHEN 'FR' THEN 5
							WHEN 'Saturday' THEN 6
							WHEN 'SA' THEN 6
							WHEN 'Sunday' THEN 7
							WHEN 'SU' THEN 7
						END
END
RETURN (@iDayofWeek)
END

GO
/****** Object:  UserDefinedFunction [dbo].[WeekDaysForGivenEvent]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- SELECT [dbo].[WeekDaysForGivenEvent](31611)
CREATE FUNCTION [dbo].[WeekDaysForGivenEvent] 
(
	@EventId BIGINT	
)
RETURNS NVARCHAR(50)
AS
BEGIN
	DECLARE @resultedWeekDays NVARCHAR(50) = NULL;
	DECLARE @ByDay NVARCHAR(MAX);
	DECLARE @scheduledfrequency NVARCHAR(50);
	SELECT @scheduledfrequency = Frequency, @ByDay = ByDay FROM EventSchedule WITH (NOLOCK) WHERE EventId = @EventId; 
	
	SET @resultedWeekDays = (
		CASE
			WHEN @scheduledfrequency IS NOT NULL
			THEN
				CASE
					WHEN @scheduledfrequency = 'daily' OR @scheduledfrequency = 'weekly'
					THEN						
						(SELECT 
							DISTINCT STUFF(
								(
									SELECT 
										',' + CAST(Item AS nvarchar(MAX)) [text()] 
									FROM (
										SELECT 
											DISTINCT dbo.WeekDay(Item) as Item  
										FROM (
												SELECT REVERSE(LEFT(REVERSE(Item), CHARINDEX('/', REVERSE(Item)) - 1)) AS Item
												FROM dbo.SplitString(@ByDay, ',') 
											) AS tbl
									) AS tbl
									FOR XML PATH('')
								),1,1,'') Weekdays) 					
					WHEN @scheduledfrequency = 'monthly' OR @scheduledfrequency = 'yearly'
					THEN 						
						(
							SELECT CONVERT(NVARCHAR,dbo.WeekDay(SUBSTRING(@ByDay,2,3)))
						)
				END			
		END
	)

	RETURN @resultedWeekDays;
END

GO
/****** Object:  UserDefinedFunction [dbo].[WeekDaysForGivenEvent_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[WeekDaysForGivenEvent_v1] 
(
	@EventId BIGINT	
)
RETURNS NVARCHAR(50)
AS
BEGIN
	DECLARE @resultedWeekDays NVARCHAR(50) = NULL;	
	
	SET @resultedWeekDays = (SELECT DISTINCT STUFF(
								(
									SELECT 
										',' + CAST(Item AS nvarchar(MAX)) [text()] 
									FROM (
										SELECT DISTINCT dbo.WeekDay(WeekName) AS item
										FROM EventOccurrence WITH (NOLOCK) WHERE EventId = @EventId
									) AS tbl
									FOR XML PATH('')
								),1,1,'') Weekdays
							) 
	RETURN @resultedWeekDays;
END
GO
/****** Object:  UserDefinedFunction [dbo].[CIRCLEDISTANCE]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--select * from [dbo].[CIRCLEDISTANCE](51.61885000,-0.16261100,51.61885,-0.162611)
CREATE function [dbo].[CIRCLEDISTANCE] 
(
  @LAT1 varchar(250), 
  @LNG1 varchar(250), 
  @LAT2 varchar(250),  
  @LNG2 varchar(250)
)
returns table as return
(
  --select acos(sin(radians(cast(@LAT1 as float)))*
  --       sin(radians(cast(@LAT2 as float)))+
  --       cos(radians(cast(@LAT1 as float)))*
  --       cos(radians(cast(@LAT2 as float)))*
  --       cos(radians(cast(@LNG2 as float))-radians(cast(@LNG1 as float)))
  --       )*6371 as Distance
  SELECT 
		CASE  
		  WHEN (cast(@LAT1 as float) = cast(@LAT2 as float) AND cast(@LNG1 as float) = cast(@LNG2 as float)) 
		  THEN cast(0.00 as float)
		  ELSE 
			acos(sin(radians(cast(@LAT1 as float)))*
			 sin(radians(cast(@LAT2 as float)))+
			 cos(radians(cast(@LAT1 as float)))*
			 cos(radians(cast(@LAT2 as float)))*
			 cos(radians(cast(@LNG2 as float))-radians(cast(@LNG1 as float)))
			 )*6371
		END AS Distance
)
GO
/****** Object:  UserDefinedFunction [dbo].[GetWeekDayName]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetWeekDayName](@d int)
RETURNS VARCHAR(9)
WITH SCHEMABINDING, RETURNS NULL ON NULL INPUT
AS
BEGIN
RETURN 
(
SELECT 
    CASE @d
        WHEN 1 THEN 'Sunday'
        WHEN 2 THEN 'Monday'
        WHEN 3 THEN 'Tuesday'
        WHEN 4 THEN 'Wednesday'
        WHEN 5 THEN 'Thursday'
        WHEN 6 THEN 'Friday'
        WHEN 7 THEN 'Saturday'
    END
)
END
GO
/****** Object:  Table [dbo].[AccessToken]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AccessToken](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AccessToken] [nvarchar](max) NOT NULL,
	[IsDeleted] [bit] NULL,
 CONSTRAINT [PK_AccessToken] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AmenityFeature]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AmenityFeature](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[PlaceId] [bigint] NULL,
	[Type] [nvarchar](50) NULL,
	[Name] [nvarchar](50) NULL,
	[Value] [bit] NULL,
 CONSTRAINT [PK_AmenityFeature] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CustomFeedData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomFeedData](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NOT NULL,
	[ColumnName] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](max) NULL,
 CONSTRAINT [PK_CustomFeedData] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ErrorLog]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ErrorLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ModuleName] [nvarchar](255) NOT NULL,
	[MethodName] [nvarchar](255) NOT NULL,
	[Exception] [nvarchar](max) NOT NULL,
	[InnerException] [nvarchar](max) NULL,
	[StackTrace] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NULL,
	[FeedProviderId] [bigint] NULL,
 CONSTRAINT [PK_ErrorLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ErrorLog_18Sep19_13_19]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ErrorLog_18Sep19_13_19](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[ModuleName] [nvarchar](255) NOT NULL,
	[MethodName] [nvarchar](255) NOT NULL,
	[Exception] [nvarchar](max) NOT NULL,
	[InnerException] [nvarchar](max) NULL,
	[StackTrace] [nvarchar](max) NULL,
	[CreatedOn] [datetime] NULL,
	[FeedProviderId] [bigint] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Event]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Event](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[FeedProviderId] [int] NOT NULL,
	[FeedId] [nvarchar](50) NULL,
	[State] [nvarchar](50) NULL,
	[ModifiedDate] [datetime] NULL,
	[Name] [nvarchar](200) NULL,
	[Description] [nvarchar](max) NULL,
	[Image] [nvarchar](max) NULL,
	[ImageThumbnail] [nvarchar](max) NULL,
	[StartDate] [datetime2](7) NULL,
	[EndDate] [datetime2](7) NULL,
	[Duration] [nvarchar](50) NULL,
	[MaximumAttendeeCapacity] [nvarchar](max) NULL,
	[RemainingAttendeeCapacity] [nvarchar](max) NULL,
	[EventStatus] [nvarchar](50) NULL,
	[SuperEventId] [bigint] NULL,
	[Category] [nvarchar](max) NULL,
	[AgeRange] [nvarchar](max) NULL,
	[MinAge] [int] NULL,
	[MaxAge] [int] NULL,
	[GenderRestriction] [nvarchar](max) NULL,
	[Gender] [nvarchar](50) NULL,
	[AttendeeInstructions] [nvarchar](max) NULL,
	[AccessibilitySupport] [nvarchar](max) NULL,
	[AccessibilityInformation] [nvarchar](max) NULL,
	[IsCoached] [bit] NULL,
	[Level] [nvarchar](max) NULL,
	[MeetingPoint] [nvarchar](max) NULL,
	[Identifier] [nvarchar](max) NULL,
	[URL] [nvarchar](max) NULL,
	[ModifiedDateTimestamp] [bigint] NULL,
	[InsertedDateTime] [datetime] NULL,
	[UpdatedDateTime] [datetime] NULL,
 CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventLog]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventLog](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FeedProviderID] [int] NULL,
	[CurrentPageUrl] [nvarchar](max) NULL,
	[NextPageUrl] [nvarchar](max) NULL,
	[ModuleName] [nvarchar](250) NULL,
	[MethodName] [nvarchar](250) NULL,
	[CreatedOn] [datetime] NULL,
	[Note] [nvarchar](500) NULL,
 CONSTRAINT [PK__EventLog__3214EC070080D3EA] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventOccurrence]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventOccurrence](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[SubEventId] [bigint] NULL,
	[StartDate] [datetime2](7) NULL,
	[EndDate] [datetime2](7) NULL,
	[WeekDay] [int] NULL,
	[WeekName] [nvarchar](50) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_Event_Occurrence] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[EventSchedule]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EventSchedule](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[StartDate] [date] NOT NULL,
	[EndDate] [date] NULL,
	[StartTime] [time](7) NULL,
	[EndTime] [time](7) NULL,
	[Frequency] [nvarchar](50) NULL,
	[ByDay] [nvarchar](max) NULL,
	[ByMonth] [nvarchar](max) NULL,
	[ByMonthDay] [nvarchar](max) NULL,
	[RepeatCount] [int] NULL,
	[RepeatFrequency] [nvarchar](50) NULL,
	[ExceptDate] [nvarchar](max) NULL,
	[ActualFrequency] [nvarchar](50) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_EventSchedule] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FacilityUse]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FacilityUse](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[ParentId] [bigint] NULL,
	[TypeId] [int] NULL,
	[URL] [varchar](max) NULL,
	[Identifier] [nvarchar](255) NULL,
	[Name] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
	[Provider] [nvarchar](max) NULL,
	[Image] [nvarchar](max) NULL,
 CONSTRAINT [PK__Facility__3214EC078FA7A808] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FeedDataType]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FeedDataType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
 CONSTRAINT [PK_FeedDataType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FeedMapping]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FeedMapping](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentId] [bigint] NULL,
	[FeedProviderId] [int] NOT NULL,
	[TableName] [nvarchar](100) NULL,
	[ColumnName] [nvarchar](500) NOT NULL,
	[ColumnDataType] [nvarchar](50) NULL,
	[IsCustomFeedKey] [bit] NULL,
	[FeedKey] [nvarchar](50) NULL,
	[FeedKeyPath] [nvarchar](200) NULL,
	[ActualFeedKeyPath] [nvarchar](200) NULL,
	[Constraint] [nvarchar](max) NULL,
	[IsDeleted] [bit] NULL,
	[Position] [bigint] NULL,
 CONSTRAINT [PK_FeedMappint] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FeedProvider]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FeedProvider](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[Source] [nvarchar](max) NOT NULL,
	[IsIminConnector] [bit] NOT NULL,
	[FeedDataTypeId] [int] NULL,
	[IsUsesTimestamp] [bit] NOT NULL,
	[IsUtcTimestamp] [bit] NOT NULL,
	[IsUsesChangenumber] [bit] NOT NULL,
	[IsUsesUrlSlug] [bit] NOT NULL,
	[EndpointUp] [bit] NOT NULL,
	[UsesPagingSpec] [bit] NOT NULL,
	[IsOpenActiveCompatible] [bit] NOT NULL,
	[IncludesCoordinates] [bit] NOT NULL,
	[IsFeedMappingConfirmed] [bit] NOT NULL,
	[IsDeleted] [bit] NOT NULL,
	[JSONTreeFileName] [nvarchar](max) NULL,
	[SampleJSONFIleName] [nvarchar](max) NULL,
	[JsonTreeWithDisabledKeysFileName] [nvarchar](max) NULL,
	[InsertedDatetime] [datetime] NULL,
	[UpdateDatetime] [datetime] NULL,
 CONSTRAINT [PK_FeedProvider] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[FilterCriteria]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[FilterCriteria](
	[FilterCriteriaId] [int] IDENTITY(1,1) NOT NULL,
	[ParentId] [int] NULL,
	[RuleId] [int] NULL,
	[FieldMappingId] [int] NULL,
	[OperatorId] [int] NULL,
	[Value] [nvarchar](max) NULL,
	[OperationId] [int] NULL,
	[JsonFeedId] [nvarchar](255) NULL,
 CONSTRAINT [PK_FilterCriteria] PRIMARY KEY CLUSTERED 
(
	[FilterCriteriaId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[IntelligentMapping]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[IntelligentMapping](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[ParentId] [bigint] NULL,
	[TableName] [nvarchar](100) NOT NULL,
	[ColumnName] [nvarchar](500) NOT NULL,
	[ColumnDataType] [nvarchar](50) NULL,
	[PossibleMatches] [nvarchar](max) NULL,
	[PossibleHierarchies] [nvarchar](max) NULL,
	[PossibleMatchesByManually] [nvarchar](max) NULL,
	[PossibleHierarchiesByManually] [nvarchar](max) NULL,
	[CustomCriteria] [nvarchar](max) NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsCustomFeedKey] [bit] NOT NULL,
	[Position] [bigint] NULL,
 CONSTRAINT [PK_Intelligent_Mapping] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[JsonImportData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[JsonImportData](
	[JsonID] [bigint] IDENTITY(1,1) NOT NULL,
	[EventID] [bigint] NOT NULL,
	[FeedProviderID] [bigint] NOT NULL,
	[FeedID] [nvarchar](100) NOT NULL,
	[JsonData] [nvarchar](max) NOT NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_JsonImportData] PRIMARY KEY CLUSTERED 
(
	[JsonID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Offer]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Offer](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[SlotId] [bigint] NULL,
	[Identifier] [nvarchar](50) NULL,
	[Name] [nvarchar](max) NULL,
	[Price] [nvarchar](50) NULL,
	[PriceCurrency] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_Offer] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Operation]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Operation](
	[OperationId] [int] IDENTITY(1,1) NOT NULL,
	[RuleId] [int] NULL,
	[FieldId] [int] NULL,
	[Value] [nvarchar](max) NULL,
	[CurrentWord] [nvarchar](max) NULL,
	[NewWord] [nvarchar](max) NULL,
	[Sentance] [nvarchar](max) NULL,
	[FirstFieldId] [int] NULL,
	[SecondFieldId] [int] NULL,
	[OperationTypeId] [int] NOT NULL,
 CONSTRAINT [PK_Operation] PRIMARY KEY CLUSTERED 
(
	[OperationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OperationType]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OperationType](
	[OperationTypeId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
 CONSTRAINT [PK_OperationType] PRIMARY KEY CLUSTERED 
(
	[OperationTypeId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Operator]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Operator](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NULL,
	[SupportedDataType] [nvarchar](500) NULL,
	[OperatorExpression] [nvarchar](10) NULL,
	[IsAvtive] [bit] NOT NULL,
 CONSTRAINT [PK_Operator] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Organization]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Organization](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](max) NULL,
	[Email] [nvarchar](100) NULL,
	[Image] [nvarchar](max) NULL,
	[URL] [nvarchar](max) NULL,
	[Telephone] [nvarchar](50) NULL,
 CONSTRAINT [PK_Organization] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Person]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Person](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[IsLeader] [bit] NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
	[Email] [nvarchar](100) NULL,
	[Image] [nvarchar](max) NULL,
	[URL] [nvarchar](max) NULL,
	[Telephone] [nvarchar](50) NULL,
 CONSTRAINT [PK_Person] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PhysicalActivity]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PhysicalActivity](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[PrefLabel] [nvarchar](50) NULL,
	[AltLabel] [nvarchar](50) NULL,
	[InScheme] [nvarchar](200) NULL,
	[Notation] [nvarchar](50) NULL,
	[BroaderId] [bigint] NULL,
	[NarrowerId] [bigint] NULL,
	[Image] [nvarchar](255) NULL,
	[Description] [nvarchar](max) NULL,
 CONSTRAINT [PK_PhysicalActivity] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Place]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Place](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[ParentId] [bigint] NULL,
	[PlaceTypeId] [int] NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
	[Image] [nvarchar](max) NULL,
	[Address] [nvarchar](max) NULL,
	[Lat] [nvarchar](50) NULL,
	[Long] [nvarchar](50) NULL,
	[Telephone] [nvarchar](12) NULL,
	[FaxNumber] [nvarchar](20) NULL,
	[URL] [nvarchar](max) NULL,
	[StreetAddress] [nvarchar](max) NULL,
	[AddressLocality] [nvarchar](max) NULL,
	[PostalCode] [nvarchar](255) NULL,
	[Region] [nvarchar](255) NULL,
	[OpeningHoursSpecification] [nvarchar](max) NULL,
	[IsUpdate] [bit] NULL,
 CONSTRAINT [PK_Place] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[PlaceType]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PlaceType](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_PlaceType] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Programme]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Programme](
	[id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventId] [bigint] NULL,
	[Name] [nvarchar](50) NULL,
	[Description] [nvarchar](max) NULL,
	[Image] [nvarchar](max) NULL,
	[URL] [nvarchar](max) NULL,
 CONSTRAINT [PK_Programme] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Rule]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Rule](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FeedProviderId] [int] NULL,
	[RuleName] [nvarchar](100) NULL,
	[IsEnabled] [bit] NULL,
	[IsDeleted] [bit] NULL,
	[CreatedOn] [datetime] NULL,
	[UpdatedOn] [datetime] NULL,
 CONSTRAINT [PK_Rules] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[RuleOperator]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[RuleOperator](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NULL,
 CONSTRAINT [PK_RuleOperator] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SchedulerFrequency]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchedulerFrequency](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NOT NULL,
 CONSTRAINT [PK_SchedulerFrequency] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SchedulerLastExecutionLog]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchedulerLastExecutionLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[FeedProviderId] [int] NULL,
	[SchedulerSettingsId] [int] NULL,
	[LastExecutionDateTime] [datetime] NULL,
	[InsertedDatetime] [datetime] NULL,
 CONSTRAINT [PK_LastExecutionLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SchedulerLog]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchedulerLog](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[FeedProviderId] [bigint] NULL,
	[StartDate] [datetime2](7) NULL,
	[EndDate] [datetime2](7) NULL,
	[Status] [nvarchar](50) NULL,
	[Note] [nvarchar](500) NULL,
	[AffectedEvents] [bigint] NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_ScheduledJobLog] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[SchedulerSettings]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[SchedulerSettings](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[FeedProviderId] [int] NOT NULL,
	[StartDateTime] [datetime] NOT NULL,
	[ExpiryDateTime] [datetime] NULL,
	[LastExecutionDateTime] [datetime] NULL,
	[NextPageUrlAfterExecution] [nvarchar](max) NULL,
	[NextPageNumberAfterExecution] [int] NULL,
	[IsEnabled] [bit] NOT NULL,
	[SchedulerFrequencyId] [int] NOT NULL,
	[RecurranceInterval] [int] NULL,
	[RecurranceDaysInWeek] [nvarchar](200) NULL,
	[RecurranceMonths] [nvarchar](200) NULL,
	[RecurranceDatesInMonth] [nvarchar](200) NULL,
	[RecurranceWeekNos] [nvarchar](50) NULL,
	[RecurranceDaysInWeekForMonth] [nvarchar](200) NULL,
	[IsDeleted] [bit] NOT NULL,
	[IsStarted] [bit] NULL,
	[IsCompleted] [bit] NULL,
	[IsTerminated] [bit] NULL,
	[SchedulerLastStartTime] [datetime] NULL,
	[SchedulerLastStartTimeStamp] [bigint] NULL,
	[InsertedDatetime] [datetime] NULL,
	[UpdatedDatetime] [datetime] NULL,
 CONSTRAINT [PK_SchedulerSettings] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[ServiceLog]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ServiceLog](
	[ServiceLogID] [bigint] IDENTITY(1,1) NOT NULL,
	[MethodName] [nvarchar](max) NULL,
	[Model] [nvarchar](max) NULL,
	[RequestTime] [datetime] NULL,
	[ResponseTime] [datetime] NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_ServiceLog] PRIMARY KEY CLUSTERED 
(
	[ServiceLogID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Slot]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Slot](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[EventID] [bigint] NULL,
	[Identifier] [varchar](50) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[Duration] [nvarchar](15) NULL,
	[OfferID] [bigint] NULL,
	[RemainingUses] [varchar](10) NULL,
	[MaximumUses] [varchar](10) NULL,
 CONSTRAINT [PK_Slot] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[UKPostalCode]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UKPostalCode](
	[UKPostalCodeID] [bigint] IDENTITY(1,1) NOT NULL,
	[FeedProviderID] [int] NULL,
	[PostalCode] [nvarchar](50) NULL,
	[Lat] [nvarchar](50) NULL,
	[Long] [nvarchar](50) NULL,
	[CreatedOn] [datetime] NULL,
 CONSTRAINT [PK_PlaceLatLongData] PRIMARY KEY CLUSTERED 
(
	[UKPostalCodeID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[User]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[User](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Email] [nvarchar](50) NOT NULL,
	[Password] [nvarchar](50) NOT NULL,
	[PasswordResetToken] [uniqueidentifier] NULL,
	[PasswordResetExpiration] [datetime] NULL,
	[IsDeleted] [bit] NOT NULL,
	[CreatedBy] [int] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[UpdatedBy] [int] NULL,
	[UpdatedOn] [datetime] NULL,
 CONSTRAINT [PK_User] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_CustomFeedData_EventId_ColumnName_463B0]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_CustomFeedData_EventId_ColumnName_463B0] ON [dbo].[CustomFeedData]
(
	[EventId] ASC,
	[ColumnName] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IDX_EventID_AgeRange]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IDX_EventID_AgeRange] ON [dbo].[Event]
(
	[id] ASC
)
INCLUDE([AgeRange]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IX_Event_EndDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_Event_EndDate] ON [dbo].[Event]
(
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Event_FeedId_id_FeedProviderID_StartDate_EndDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_Event_FeedId_id_FeedProviderID_StartDate_EndDate] ON [dbo].[Event]
(
	[FeedId] ASC
)
INCLUDE([id],[FeedProviderId],[StartDate],[EndDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Event_FeedId_id_StartDate_EndDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_Event_FeedId_id_StartDate_EndDate] ON [dbo].[Event]
(
	[FeedProviderId] ASC,
	[FeedId] ASC
)
INCLUDE([id],[StartDate],[EndDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Event_StartDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_Event_StartDate] ON [dbo].[Event]
(
	[StartDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Event_SuperEventId]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_Event_SuperEventId] ON [dbo].[Event]
(
	[SuperEventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_D_Event_ID_FPID_Start_EndDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_D_Event_ID_FPID_Start_EndDate] ON [dbo].[Event]
(
	[id] ASC,
	[FeedProviderId] ASC,
	[StartDate] ASC,
	[EndDate] ASC
)
INCLUDE([GenderRestriction],[FeedId],[State],[MinAge],[MaxAge]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Event_FeedID]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Event_FeedID] ON [dbo].[Event]
(
	[FeedId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Event_FeedId_SuperEventId]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Event_FeedId_SuperEventId] ON [dbo].[Event]
(
	[FeedId] ASC,
	[SuperEventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_Event_FeedProviderId]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Event_FeedProviderId] ON [dbo].[Event]
(
	[FeedProviderId] ASC
)
INCLUDE([id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_Event_MaxAge]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Event_MaxAge] ON [dbo].[Event]
(
	[MaxAge] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_Event_MinAge]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Event_MinAge] ON [dbo].[Event]
(
	[MinAge] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Event_State]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Event_State] ON [dbo].[Event]
(
	[State] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Event_State_FeedId]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Event_State_FeedId] ON [dbo].[Event]
(
	[State] ASC,
	[FeedId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Event_State_ModifiedDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Event_State_ModifiedDate] ON [dbo].[Event]
(
	[State] ASC,
	[ModifiedDate] ASC
)
INCLUDE([id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_D_EventOccurence_EventID_Wname]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_D_EventOccurence_EventID_Wname] ON [dbo].[EventOccurrence]
(
	[EventId] ASC,
	[WeekName] ASC
)
INCLUDE([WeekDay]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_EventOccurrence_EventId_StartDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_EventOccurrence_EventId_StartDate] ON [dbo].[EventOccurrence]
(
	[EventId] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_EventOccurrence_StartDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_EventOccurrence_StartDate] ON [dbo].[EventOccurrence]
(
	[StartDate] ASC
)
INCLUDE([EventId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventSchedule_EndDate]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_EventSchedule_EndDate] ON [dbo].[EventSchedule]
(
	[EndDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_EventSchedule_StartTime_EndTime]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_EventSchedule_StartTime_EndTime] ON [dbo].[EventSchedule]
(
	[EndDate] ASC,
	[StartTime] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_EventSchedule_EventId_92A3E]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_EventSchedule_EventId_92A3E] ON [dbo].[EventSchedule]
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_EventSchedule_EventId_StartDate_37FF8]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_EventSchedule_EventId_StartDate_37FF8] ON [dbo].[EventSchedule]
(
	[EventId] ASC,
	[StartDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_FeedMapping_FeedProviderId]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_FeedMapping_FeedProviderId] ON [dbo].[FeedMapping]
(
	[FeedProviderId] ASC
)
INCLUDE([id],[ParentId],[TableName],[ColumnName],[ColumnDataType],[IsCustomFeedKey],[FeedKey],[FeedKeyPath],[ActualFeedKeyPath],[IsDeleted]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IX_FeedProvider_IsDeleted]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_FeedProvider_IsDeleted] ON [dbo].[FeedProvider]
(
	[IsDeleted] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_JsonImportData_FeedProviderID]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_JsonImportData_FeedProviderID] ON [dbo].[JsonImportData]
(
	[FeedProviderID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
/****** Object:  Index [IX_Offer]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_Offer] ON [dbo].[Offer]
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Offer_EventID_Price_PriceCurrency]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_Offer_EventID_Price_PriceCurrency] ON [dbo].[Offer]
(
	[EventId] ASC,
	[Price] ASC,
	[PriceCurrency] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Organization_EventId_Name]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Organization_EventId_Name] ON [dbo].[Organization]
(
	[EventId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Person_EventId_Name]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Person_EventId_Name] ON [dbo].[Person]
(
	[EventId] ASC,
	[Name] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_PhysicalActivity_EventId_D40EB]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_PhysicalActivity_EventId_D40EB] ON [dbo].[PhysicalActivity]
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_PhysicalActivity_EventId_PrefLabel_C4F53]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_PhysicalActivity_EventId_PrefLabel_C4F53] ON [dbo].[PhysicalActivity]
(
	[EventId] ASC,
	[PrefLabel] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_PhysicalActivity_PrefLabel]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_PhysicalActivity_PrefLabel] ON [dbo].[PhysicalActivity]
(
	[PrefLabel] ASC
)
INCLUDE([EventId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_Place_EventID]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Place_EventID] ON [dbo].[Place]
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IXNC_Place_EventId_C66E4]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Place_EventId_C66E4] ON [dbo].[Place]
(
	[EventId] ASC
)
INCLUDE([Lat],[Long]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Place_Lat]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Place_Lat] ON [dbo].[Place]
(
	[Lat] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Place_Lat_Long_PostalCode]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Place_Lat_Long_PostalCode] ON [dbo].[Place]
(
	[Lat] ASC,
	[Long] ASC,
	[PostalCode] ASC
)
INCLUDE([id],[EventId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IXNC_Place_Long]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IXNC_Place_Long] ON [dbo].[Place]
(
	[Long] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
/****** Object:  Index [IX_Programme_EventId]    Script Date: 13-02-2020 18:14:40 ******/
CREATE NONCLUSTERED INDEX [IX_Programme_EventId] ON [dbo].[Programme]
(
	[EventId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ErrorLog] ADD  CONSTRAINT [DF_ErrorLog_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Event] ADD  CONSTRAINT [DF_Event_InsertedDateTime]  DEFAULT (getdate()) FOR [InsertedDateTime]
GO
ALTER TABLE [dbo].[EventOccurrence] ADD  CONSTRAINT [DF_EventOccurrence_CreatedOn]  DEFAULT (getutcdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[EventSchedule] ADD  CONSTRAINT [DF_EventSchedule_CreatedOn]  DEFAULT (getutcdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[FeedMapping] ADD  CONSTRAINT [DF_FeedMapping_IsCustomFeedKey]  DEFAULT ((0)) FOR [IsCustomFeedKey]
GO
ALTER TABLE [dbo].[FeedMapping] ADD  CONSTRAINT [DF_FeedMapping_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IsIminConnector]  DEFAULT ((0)) FOR [IsIminConnector]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IsUsesTimestamp]  DEFAULT ((0)) FOR [IsUsesTimestamp]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IsUtcTimestamp]  DEFAULT ((0)) FOR [IsUtcTimestamp]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IsUsesChangenumber]  DEFAULT ((0)) FOR [IsUsesChangenumber]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IsUsesUrlSlug]  DEFAULT ((0)) FOR [IsUsesUrlSlug]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_EndpointUp]  DEFAULT ((0)) FOR [EndpointUp]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_UsesPagingSpec]  DEFAULT ((0)) FOR [UsesPagingSpec]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IsOpenActiveCompatible]  DEFAULT ((0)) FOR [IsOpenActiveCompatible]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IncludesCoordinates]  DEFAULT ((0)) FOR [IncludesCoordinates]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IsFeedMappingConfirmed]  DEFAULT ((0)) FOR [IsFeedMappingConfirmed]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[FeedProvider] ADD  CONSTRAINT [DF_FeedProvider_InsertedDatetime]  DEFAULT (getdate()) FOR [InsertedDatetime]
GO
ALTER TABLE [dbo].[IntelligentMapping] ADD  CONSTRAINT [DF_IntelligentMapping_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[IntelligentMapping] ADD  CONSTRAINT [DF_IntelligentMapping_IsCustomFeedKey]  DEFAULT ((0)) FOR [IsCustomFeedKey]
GO
ALTER TABLE [dbo].[JsonImportData] ADD  CONSTRAINT [DF_JsonImportData_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[Operator] ADD  CONSTRAINT [DF_Operator_IsAvtive]  DEFAULT ((1)) FOR [IsAvtive]
GO
ALTER TABLE [dbo].[Place] ADD  CONSTRAINT [DF_Place_IsUpdate]  DEFAULT ((0)) FOR [IsUpdate]
GO
ALTER TABLE [dbo].[Rule] ADD  CONSTRAINT [DF_Rules_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Rule] ADD  CONSTRAINT [DF_Rule_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[SchedulerLastExecutionLog] ADD  CONSTRAINT [DF_LastExecutionLog_InsertedDatetime]  DEFAULT (getdate()) FOR [InsertedDatetime]
GO
ALTER TABLE [dbo].[SchedulerLog] ADD  CONSTRAINT [DF_SchedulerLog_CreatedOn]  DEFAULT (getutcdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[SchedulerSettings] ADD  CONSTRAINT [DF_SchedulerSettings_IsEnabled]  DEFAULT ((0)) FOR [IsEnabled]
GO
ALTER TABLE [dbo].[SchedulerSettings] ADD  CONSTRAINT [DF_SchedulerSettings_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[SchedulerSettings] ADD  CONSTRAINT [DF_SchedulerSettings_IsStarted]  DEFAULT ((0)) FOR [IsStarted]
GO
ALTER TABLE [dbo].[SchedulerSettings] ADD  CONSTRAINT [DF_SchedulerSettings_IsCompleted]  DEFAULT ((0)) FOR [IsCompleted]
GO
ALTER TABLE [dbo].[SchedulerSettings] ADD  CONSTRAINT [DF_SchedulerSettings_IsTerminated]  DEFAULT ((0)) FOR [IsTerminated]
GO
ALTER TABLE [dbo].[SchedulerSettings] ADD  CONSTRAINT [DF_SchedulerSettings_InsertedDatetime]  DEFAULT (getdate()) FOR [InsertedDatetime]
GO
ALTER TABLE [dbo].[ServiceLog] ADD  CONSTRAINT [DF_ServiceLog_CreatedOn]  DEFAULT (getdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[UKPostalCode] ADD  CONSTRAINT [DF_UKPostalCode_CreatedOn]  DEFAULT (getutcdate()) FOR [CreatedOn]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_CreatedBy]  DEFAULT ((0)) FOR [CreatedBy]
GO
ALTER TABLE [dbo].[User] ADD  CONSTRAINT [DF_User_CreatedOn]  DEFAULT (getutcdate()) FOR [CreatedOn]
GO
/****** Object:  StoredProcedure [dbo].[AccessToken_Authorize]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[AccessToken_Authorize] '4b394f8e-39e1-4be7-8840-0a017726402g'
CREATE PROCEDURE [dbo].[AccessToken_Authorize]
	@AccessToken NVARCHAR(MAX)	
AS
BEGIN	
	DECLARE @IsAuthorized BIT = 0

	SET @IsAuthorized = IIF(EXISTS(SELECT 1 FROM [dbo].[AccessToken] WITH (NOLOCK) WHERE [AccessToken] = @AccessToken AND ([IsDeleted] IS NULL OR [IsDeleted] = 0)),1,0)
	SELECT @IsAuthorized AS IsAuthorized
END

GO
/****** Object:  StoredProcedure [dbo].[AmenityFeature_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[AmenityFeature_Delete]
	@EventId BIGINT
AS
BEGIN	
	DELETE FROM [dbo].[AmenityFeature]
	WHERE [EventId] = @EventId	
END

GO
/****** Object:  StoredProcedure [dbo].[AmenityFeature_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AmenityFeature_Insert]
	@EventId BIGINT,
	@PlaceId BIGINT,
	@Type NVARCHAR(50),
	@Name NVARCHAR(50),
	@Value BIT,
	@AmenityFeatureId BIGINT OUT
AS
BEGIN	
	IF NOT EXISTS (SELECT [Id] 
					FROM [dbo].[AmenityFeature] WITH (NOLOCK) 
					WHERE [EventId] = @EventId 
					AND [PlaceId] = @PlaceId 
					AND [Name] = @Name)
	BEGIN
		INSERT INTO [dbo].[AmenityFeature]
				   ([EventId]
				   ,[PlaceId]
				   ,[Type]
				   ,[Name]
				   ,[Value])
			 VALUES
				   (@EventId
				   ,@PlaceId
				   ,@Type
				   ,@Name
				   ,@Value)
	
		SET @AmenityFeatureId = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		SELECT	@AmenityFeatureId = [id]
		FROM	[AmenityFeature] WITH (NOLOCK) 
		WHERE	[EventId] = @EventId
				AND [PlaceId] = @PlaceId
				AND [Name] = @Name

		UPDATE [dbo].[AmenityFeature] SET
		[PlaceId] = @PlaceId,
		[Type] = @Type,
		[Name] = @Name,
		[Value] = @Value
		WHERE [Id] = @AmenityFeatureId AND
		[EventId] = @EventId
	END	
END
GO
/****** Object:  StoredProcedure [dbo].[Clear_Database]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Clear_Database]
AS
BEGIN	
	TRUNCATE TABLE [dbo].[AmenityFeature]
	TRUNCATE TABLE [dbo].[CustomFeedData]
	TRUNCATE TABLE [dbo].[Event]
	TRUNCATE TABLE [dbo].[ErrorLog]
	TRUNCATE TABLE [dbo].[EventSchedule]
	TRUNCATE TABLE [dbo].[FeedMapping]
	TRUNCATE TABLE [dbo].[FeedProvider]	
	TRUNCATE TABLE [dbo].[Offer]
	TRUNCATE TABLE [dbo].[Organization]
	TRUNCATE TABLE [dbo].[Person]
	TRUNCATE TABLE [dbo].[PhysicalActivity]
	TRUNCATE TABLE [dbo].[Place]
	TRUNCATE TABLE [dbo].[Programme]
	TRUNCATE TABLE [dbo].[SchedulerSettings]
	TRUNCATE TABLE [dbo].[SchedulerLog]
	TRUNCATE TABLE [dbo].[EventOccurrence]
	TRUNCATE TABLE [dbo].[EventLog]
	TRUNCATE TABLE [dbo].[SchedulerLastExecutionLog]
	TRUNCATE TABLE [dbo].[Rule]
	TRUNCATE TABLE [dbo].[FilterCriteria]
	TRUNCATE TABLE [dbo].[Operation]
    TRUNCATE TABLE [dbo].[Slot]
    TRUNCATE TABLE [dbo].[FacilityUse]
	DELETE FROM [dbo].[IntelligentMapping] WHERE [IsCustomFeedKey] = 1
	UPDATE [dbo].[IntelligentMapping] 
	SET [PossibleHierarchies] = PossibleHierarchiesByManually,
	    [PossibleMatches] = [PossibleMatchesByManually]
END
GO
/****** Object:  StoredProcedure [dbo].[CustomFeedData_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[CustomFeedData_Delete]
	@EventId BIGINT
AS
BEGIN
	DELETE	FROM [dbo].[CustomFeedData]
	WHERE	[EventId] = @EventId
END



GO
/****** Object:  StoredProcedure [dbo].[CustomFeedData_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[CustomFeedData_Insert]
	@EventId BIGINT,
	@ColumnName NVARCHAR(50),
	@Value NVARCHAR(MAX)
AS
BEGIN	
	IF NOT EXISTS (SELECT	[id] 
					FROM	[dbo].[CustomFeedData] WITH (NOLOCK)
					WHERE	[EventId] = @EventId 
							AND [ColumnName] = @ColumnName)
	BEGIN
		INSERT INTO [dbo].[CustomFeedData]
				   ([EventId]
				   ,[ColumnName]
				   ,[Value])
			 VALUES
				   (@EventId
				   ,@ColumnName
				   ,@Value)
	END
	ELSE
	BEGIN
		UPDATE [dbo].[CustomFeedData]
		SET [Value] = @Value
		WHERE [EventId] = @EventId
				AND [ColumnName] = @ColumnName
	END
END



GO
/****** Object:  StoredProcedure [dbo].[DeleteCustomFeedsByFeedProvider]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DeleteCustomFeedsByFeedProvider]
	@FeedProviderId INT
	,@isCustomFeedKey BIT = 1
AS
BEGIN	
	UPDATE		FM
	SET			FM.[IsDeleted] = IM.[IsDeleted]
	FROM		[dbo].[FeedMapping] AS FM
	INNER JOIN	[dbo].[IntelligentMapping] AS IM
					ON	FM.[TableName] = IM.[TableName] 
					AND FM.[ColumnName] = IM.[ColumnName]
					AND FM.[Position] = IM.[Position]
					AND IM.[IsCustomFeedKey] = @isCustomFeedKey
					AND	IM.[IsDeleted] = 1
	WHERE		FM.[IsCustomFeedKey] = @isCustomFeedKey
	AND			FM.[IsDeleted] = 0
	AND			FM.[FeedProviderId] = @FeedProviderId
END

GO
/****** Object:  StoredProcedure [dbo].[DeleteEventsOldData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <2019-05-15>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[DeleteEventsOldData]
	@EventId BIGINT
AS
BEGIN
	/*Delete old superevent data*/
	--DELETE FROM [dbo].[Event] 
	--WHERE SuperEventId = @EventId
	EXEC Event_DeleteOldData @EventId
END
GO
/****** Object:  StoredProcedure [dbo].[ErrorLog_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ErrorLog_Insert]
	@ModuleName NVARCHAR(255),
	@MethodName NVARCHAR(255),
	@Exception NVARCHAR(MAX),
	@InnerException NVARCHAR(MAX),
	@StackTrace NVARCHAR(MAX),
	@FeedProviderId BIGINT = NULL
AS
BEGIN	
	INSERT INTO [dbo].[ErrorLog]
			   ([ModuleName]
			   ,[MethodName]
			   ,[Exception]
			   ,[InnerException]
			   ,[StackTrace]
			   ,[FeedProviderId])
		 VALUES
			   (@ModuleName
			   ,@MethodName
			   ,@Exception
			   ,@InnerException
			   ,@StackTrace
			   ,@FeedProviderId)
END
GO
/****** Object:  StoredProcedure [dbo].[Event_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Event_Delete]	
AS
BEGIN	
	-- BEFORE 21-01-2019 DELETE FROM [dbo].[Event] WHERE State = 'deleted' --and ModifiedDate <= DATEADD(day,-7, GETDATE())
	DELETE FROM [dbo].[Event] WHERE [State] = 'deleted' AND ModifiedDate <= DATEADD(day,-1, GETDATE())
END
GO
/****** Object:  StoredProcedure [dbo].[Event_DeleteOldData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Event_DeleteOldData]
	@EventId BIGINT
AS
BEGIN
	
	EXEC Organization_Delete @EventId

	EXEC Person_Delete @EventId

	EXEC Programme_Delete @EventId

	EXEC EventSchedule_Delete @EventId

	EXEC Place_Delete @EventId

	EXEC AmenityFeature_Delete @EventId

	EXEC PhysicalActivity_Delete @EventId

	EXEC Event_DeleteSuperSubEvent @EventId

	EXEC CustomFeedData_Delete @EventId

	EXEC EventOccurrence_Delete @EventId

	EXEC Offer_Delete @EventId

    EXEC Slot_Delete @EventId

    EXEC FacilityUse_Delete @EventId

END
GO
/****** Object:  StoredProcedure [dbo].[Event_DeletePastOccurrence]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Event_DeletePastOccurrence]	
AS
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION tran_delete_past_occurred_event

		-- delete from event and it's dependent tables
		DECLARE @EventId BIGINT
		DECLARE db_cursor_delete_event CURSOR STATIC FOR
	 
		SELECT DISTINCT eventid FROM (
			SELECT [EventId], SUM(IIF([StartDate] >= GETDATE(),1,0)) AS FutureOccurrences FROM [dbo].[EventOccurrence] WITH (NOLOCK)		
			GROUP BY eventid
		) data
		WHERE data.FutureOccurrences <= 0

		OPEN db_cursor_delete_event  
		FETCH NEXT FROM db_cursor_delete_event INTO @EventId  

		WHILE @@FETCH_STATUS = 0  
		BEGIN  
			  EXEC Event_DeleteOldData @EventId
			  DELETE FROM [dbo].[Event] WHERE [id] = @EventId
			  FETCH NEXT FROM db_cursor_delete_event INTO @EventId 
		END 

		CLOSE db_cursor_delete_event  
		DEALLOCATE db_cursor_delete_event
		
		COMMIT TRANSACTION tran_delete_past_occurred_event
		PRINT 'Deleted the Record Successfully'
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION tran_delete_past_occurred_event
		PRINT 'Sorry, Error occurred !!'
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[Event_DeleteSuperSubEvent]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Event_DeleteSuperSubEvent]
	@EventId BIGINT
AS
BEGIN	
	-- delete sub/super events
	DELETE	FROM [dbo].[Event]
	WHERE	[FeedId] IS NULL
			AND 
			(
				[SuperEventId] = @EventId
				OR (
					[Id] = (SELECT TOP 1 [SuperEventId] FROM [dbo].[Event] WITH (NOLOCK) WHERE [id] = @EventId)
				)
			)
END


GO
/****** Object:  StoredProcedure [dbo].[Event_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Event_Insert]
	@FeedProviderId				INT,
	@FeedId						NVARCHAR(50),
	@State						NVARCHAR(50),
	@ModifiedDate				DATETIME,
	@Name						NVARCHAR(200),
	@Description				NVARCHAR(MAX),
	@Image						NVARCHAR(MAX),
	@ImageThumbnail				NVARCHAR(MAX),
	@StartDate					DATETIME2,
	@EndDate					DATETIME2,
	@Duration					NVARCHAR(50),
	@MaximumAttendeeCapacity	NVARCHAR(MAX),
	@RemainingAttendeeCapacity	NVARCHAR(MAX),
	@EventStatus				NVARCHAR(50),
	@SuperEventId				BIGINT,
	@Category					NVARCHAR(MAX),
	@AgeRange					NVARCHAR(MAX),
	@GenderRestriction			NVARCHAR(MAX),
	@AttendeeInstructions		NVARCHAR(MAX),
	@AccessibilitySupport		NVARCHAR(MAX),
	@AccessibilityInformation	NVARCHAR(MAX),
	@IsCoached					BIT,
	@Level						NVARCHAR(MAX),
	@MeetingPoint				NVARCHAR(MAX),
	@Identifier					NVARCHAR(MAX),
	@Url						NVARCHAR(MAX),
	@MinAge						INT = NULL,
	@MaxAge						INT = NULL,
	@EventId					BIGINT OUT
AS
BEGIN
	
	IF EXISTS (SELECT 1 FROM [dbo].[Event] WITH (NOLOCK)
				WHERE	[FeedProviderId] = @FeedProviderId 
						AND [FeedId] = @FeedId )
	BEGIN
		UPDATE [dbo].[Event]
		   SET [State] = @State
			  ,[ModifiedDate] = @ModifiedDate
			  ,[Name] = @Name
			  ,[Description] = @Description
			  ,[Image] = @Image
			  ,[ImageThumbnail] = @ImageThumbnail
			  ,[StartDate] = @StartDate
			  ,[EndDate] = @EndDate
			  ,[Duration] = @Duration
			  ,[MaximumAttendeeCapacity] = @MaximumAttendeeCapacity
			  ,[RemainingAttendeeCapacity] = @RemainingAttendeeCapacity
			  ,[EventStatus] = @EventStatus
			  ,[SuperEventId] = @SuperEventId
			  ,[Category] = @Category
			  ,[AgeRange] = @AgeRange
			  ,[GenderRestriction] = @GenderRestriction
			  ,[Gender] = [dbo].[GenderForGivenEvent](0,@GenderRestriction)
			  ,[AttendeeInstructions] = @AttendeeInstructions
			  ,[AccessibilitySupport] = @AccessibilitySupport
			  ,[AccessibilityInformation] = @AccessibilityInformation
			  ,[IsCoached] = @IsCoached
			  ,[Level] = @Level
			  ,[MeetingPoint] = @MeetingPoint
			  ,[Identifier] = @Identifier
			  ,[URL] = @URL
			  ,[MinAge] = IIF(@AgeRange IS NULL,18,@MinAge)
			  ,[MaxAge] = @MaxAge
			  ,[ModifiedDateTimestamp] = dbo.UNIX_TIMESTAMP(@ModifiedDate)
			  ,[UpdatedDateTime] = GETDATE()
		 WHERE	[FeedProviderId] = @FeedProviderId
				AND [FeedId] = @FeedId

		SELECT	@EventId = id 
		FROM	[dbo].[Event] WITH (NOLOCK)
		WHERE	[FeedProviderId] = @FeedProviderId
				AND [FeedId] = @FeedId
	END
	ELSE
	BEGIN
		IF(ISNULL(@State,'') <> 'deleted')
		BEGIN
			INSERT INTO [dbo].[Event]
					   ([FeedProviderId]
					   ,[FeedId]
					   ,[State]
					   ,[ModifiedDate]
					   ,[Name]
					   ,[Description]
					   ,[Image]
					   ,[ImageThumbnail]
					   ,[StartDate]
					   ,[EndDate]
					   ,[Duration]
					   ,[MaximumAttendeeCapacity]
					   ,[RemainingAttendeeCapacity]
					   ,[EventStatus]
					   ,[SuperEventId]
					   ,[Category]
					   ,[AgeRange]
					   ,[GenderRestriction]
					   ,[Gender]
					   ,[AttendeeInstructions]
					   ,[AccessibilitySupport]
					   ,[AccessibilityInformation]
					   ,[IsCoached]
					   ,[Level]
					   ,[MeetingPoint]
					   ,[Identifier]
					   ,[URL]
					   ,[MinAge]
					   ,[MaxAge]
					   ,[ModifiedDateTimestamp])
				 VALUES
					   (@FeedProviderId
					   ,@FeedId
					   ,@State
					   ,@ModifiedDate
					   ,@Name
					   ,@Description
					   ,@Image
					   ,@ImageThumbnail
					   ,@StartDate
					   ,@EndDate
					   ,@Duration
					   ,@MaximumAttendeeCapacity
					   ,@RemainingAttendeeCapacity
					   ,@EventStatus
					   ,@SuperEventId
					   ,@Category
					   ,@AgeRange
					   ,@GenderRestriction
					   ,[dbo].[GenderForGivenEvent](0,@GenderRestriction)
					   ,@AttendeeInstructions
					   ,@AccessibilitySupport
					   ,@AccessibilityInformation
					   ,@IsCoached
					   ,@Level
					   ,@MeetingPoint
					   ,@Identifier
					   ,@URL
					   ,IIF(@AgeRange IS NULL,18,@MinAge)
					   ,@MaxAge
					   ,dbo.UNIX_TIMESTAMP(@ModifiedDate))

			SET @EventId = SCOPE_IDENTITY();
		END		
	END


	-- Update Parent Event Enddate according to sub event's enddate
	IF(@SuperEventId IS NOT NULL AND @EndDate IS NOT NULL)
	BEGIN		
		UPDATE [dbo].[Event] SET [EndDate] = @EndDate
		WHERE [id] = @SuperEventId
	END
END
GO
/****** Object:  StoredProcedure [dbo].[EventLog_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventLog_Insert]
	@FeedProviderId		BIGINT
	,@CurrentPageUrl	NVARCHAR(MAX)
	,@NextPageUrl		NVARCHAR(MAX)
	,@ModuleName		NVARCHAR(250)
	,@MethodName		NVARCHAR(250)
	,@Note				NVARCHAR(500) = NULL
AS
BEGIN
	INSERT INTO [dbo].[EventLog]
			   ([FeedProviderID]
			   ,[CurrentPageUrl]
			   ,[NextPageUrl]
			   ,[ModuleName]
			   ,[MethodName]
			   ,[CreatedOn]
			   ,[Note])
		 VALUES
			   (@FeedProviderId
			   ,@CurrentPageUrl
			   ,@NextPageUrl
			   ,@ModuleName
			   ,@MethodName
			   ,GETDATE()
			   ,@Note)
END



GO
/****** Object:  StoredProcedure [dbo].[EventOccurrence_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventOccurrence_Delete]
	@EventId BIGINT
AS
BEGIN
	DELETE	FROM [dbo].[EventOccurrence]
	WHERE	[EventId] = @EventId
END

GO
/****** Object:  StoredProcedure [dbo].[EventOccurrence_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[EventOccurrence_Insert] 7          -- subevent is available
-- [dbo].[EventOccurrence_Insert] 1635       -- subvent is available with event schedule
-- [dbo].[EventOccurrence_Insert] 1065       -- event schedule is available
-- [dbo].[EventOccurrence_Insert] 576       -- subevent and event schedule is available individually
CREATE PROCEDURE [dbo].[EventOccurrence_Insert]
	@EventId BIGINT	
AS
BEGIN
	DECLARE @SubEventCount BIGINT;

	DECLARE @frequency  NVARCHAR(100), 
			@StartDate  Date, 
			@EndDate    DATE,			
			@StartTime  TIME,
			@EndTime    TIME,
			@ByDay      NVARCHAR(100),
			@ByMonth    NVARCHAR(100),
			@ByMonthDay NVARCHAR(100);

	SELECT @SubEventCount = COUNT(*) FROM [dbo].[Event] WITH (NOLOCK) WHERE [SuperEventId] = @EventId AND [FeedId] IS NULL 

	BEGIN TRY
		BEGIN TRANSACTION tran_Occurrence_Insert

		IF(@SubEventCount > 0)
		BEGIN
			DECLARE @SubEventId BIGINT
			DECLARE db_cursor_insert_occurrence CURSOR FOR
	 
			SELECT [id] FROM [dbo].[Event] WITH (NOLOCK) WHERE [SuperEventId] = @EventId AND [FeedId] IS NULL 

			OPEN db_cursor_insert_occurrence  
			FETCH NEXT FROM db_cursor_insert_occurrence INTO @SubEventId  																															
			WHILE @@FETCH_STATUS = 0  
			BEGIN 	
				--PRINT @SubEventId		
				IF EXISTS(SELECT 1 FROM [dbo].[EventSchedule] WITH (NOLOCK) WHERE [EventId] = @SubEventId)
				BEGIN
					SELECT 
						@frequency = [Frequency],
						@StartDate = [StartDate],
						@EndDate = ISNULL([EndDate],[StartDate]),
						@StartTime = [StartTime],
						@EndTime = [EndTime],
						@ByDay = [ByDay],
						@ByMonth = [ByMonth],
						@ByMonthDay = [ByMonthDay]
					FROM [dbo].[EventSchedule] WITH(NOLOCK) 
					WHERE [EventId] = @SubEventId
					
					INSERT INTO [dbo].[EventOccurrence]
					(
						[EventId],
						[SubEventId],
						[StartDate],
						[EndDate]
					)
					SELECT DISTINCT
						@EventId,
						@SubEventId, 
						[StartDate],
						[EndDate]
					FROM [dbo].[DateRangeByFrequencyAndMultipleDay] (@frequency,@ByDay,@StartDate,@EndDate,@StartTime,@EndTime)
					ORDER BY [StartDate]
				END				
				ELSE --IF NOT EXISTS(SELECT 1 FROM [dbo].[EventSchedule] WHERE EventId = @SubEventId)
				BEGIN
					INSERT INTO [dbo].[EventOccurrence]
					(
						[EventId],
						[SubEventId],
						[StartDate],
						[EndDate]
					)
					SELECT 
						@EventId,
						@SubEventId, 
						[StartDate],
						[EndDate]
					FROM [dbo].[Event] WITH(NOLOCK)
					WHERE [Id] = @SubEventId;				
				END

				FETCH NEXT FROM db_cursor_insert_occurrence INTO @SubEventId 
			END 

			CLOSE db_cursor_insert_occurrence  
			DEALLOCATE db_cursor_insert_occurrence
		END
		ELSE IF EXISTS(SELECT 1 FROM EventSchedule WITH (NOLOCK) WHERE EventId = @EventId)
		BEGIN
			SELECT 
				@frequency = [Frequency],
				@StartDate = [StartDate],
				@EndDate = ISNULL([EndDate],[StartDate]),
				@StartTime = [StartTime],
				@EndTime = [EndTime],
				@ByDay = [ByDay],
				@ByMonth = [ByMonth],
				@ByMonthDay = [ByMonthDay]
			FROM [dbo].[EventSchedule] WITH (NOLOCK)
			WHERE [EventId] = @EventId

			INSERT INTO [dbo].[EventOccurrence]
			(
				[EventId],
				[StartDate],
				[EndDate]
			)
			SELECT DISTINCT
				@EventId, 
				[StartDate],
				[EndDate]
			FROM [dbo].[DateRangeByFrequencyAndMultipleDay] (@frequency,@ByDay,@StartDate,@EndDate,@StartTime,@EndTime)
			ORDER BY [StartDate]
		END
		ELSE --IF NOT EXISTS(SELECT 1 FROM EventSchedule WHERE EventId = @EventId)
		BEGIN
			INSERT INTO [dbo].[EventOccurrence]
			(
				[EventId],
				[StartDate],
				[EndDate]
			)
			SELECT 
				[Id], 
				[StartDate],
				ISNULL([EndDate],[StartDate])
			FROM [dbo].[Event] WITH (NOLOCK)
			WHERE [Id] = @EventId;		
		END

		--IF @@ERROR = 0
		--	COMMIT TRAN tran_Occurrence_Insert	
		--ELSE 
		--	ROLLBACK TRAN tran_Occurrence_Insert

		COMMIT TRANSACTION tran_Occurrence_Insert
		PRINT 'Inserted the Record Successfully'
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION tran_Occurrence_Insert
		PRINT 'Sorry, Error occurred !!'
	END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[EventOccurrence_Insert_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[EventOccurrence_Insert_v1]
	@EventId BIGINT	
	,@OccCount INT = 12
AS
BEGIN
	DECLARE @SubEventCount  BIGINT;
	--set datefirst 1;

	/* 
		Note :- 
			For datefirst = 7 , DateRangeByFrequencyWithMultipleReccurrence is being used as it is made according to datefirst 7
			For datefirst = 1 , DateRangeByFrequencyWithMultipleReccurrence_v1 is being used as it is made according to datefirst 1
	*/
	/* 
		Note :- 
			For frequency with ISO 8601 format , DateRangeByFrequencyWithMultipleReccurrence_ISO8601 is needed to used 
			Otherwise, DateRangeByFrequencyWithMultipleReccurrence is needed to used 
	*/

	DECLARE @frequency			NVARCHAR(100), 
			@StartDate			DATE, 
			@EndDate			DATE,			
			@StartTime			TIME,
			@EndTime			TIME,
			@ByDay				NVARCHAR(500),
			@ByMonth			NVARCHAR(100),
			@ByMonthDay			NVARCHAR(100),
			@RepeatCount		INT,
			@RepeatFrequency	NVARCHAR(50),
			@ExceptDate			NVARCHAR(MAX),
			@Evt_EndDate		DATE ;	

	BEGIN TRY
	    -- Get total subevents
		SELECT @SubEventCount = COUNT(1) FROM [dbo].[Event] WITH (NOLOCK) 
		WHERE [SuperEventId] = @EventId 
		AND [FeedId] IS NULL 		

		BEGIN TRANSACTION tran_Occurrence_Insert		
		IF EXISTS(SELECT 1 FROM [dbo].[EventSchedule] WITH (NOLOCK) WHERE [EventId] = @EventId)
		BEGIN
			-- Get event end date if it's not available then get last subevent's startdate 
			SELECT @Evt_EndDate = [EndDate] FROM [dbo].[Event] WITH (NOLOCK) WHERE [id] = @EventId
			IF ISNULL(@Evt_EndDate,'') = ''
			BEGIN
				SELECT TOP 1 @Evt_EndDate = [StartDate] FROM [dbo].[Event] WITH (NOLOCK) 
				WHERE [SuperEventId] = @EventId 
				AND [FeedId] IS NULL 
				ORDER BY Id Desc
			END

			SELECT 
				@frequency			= ISNULL([Frequency],[RepeatFrequency]),
				@StartDate			= es.[StartDate],					
				@EndDate			= ISNULL(es.[EndDate],@Evt_EndDate),
				@StartTime			= [StartTime],
				@EndTime			= [EndTime],
				@ByDay				= [ByDay],
				@ByMonth			= [ByMonth],
				@ByMonthDay			= [ByMonthDay],
				@RepeatCount		= [RepeatCount],
				@RepeatFrequency	= [RepeatFrequency],
				@ExceptDate			= [ExceptDate]
			FROM [dbo].[EventSchedule] es WITH (NOLOCK)
			WHERE [EventId] = @EventId

			INSERT INTO [dbo].[EventOccurrence]
			(
				[EventId]
				,[StartDate]
				,[EndDate]
				,[WeekDay]
				,[WeekName]
			)
			SELECT DISTINCT
				@EventId
				,Occ.[StartDate]
				,Occ.[EndDate]
				,DATEPART(dw,Occ.[StartDate])
				,DATENAME(WEEKDAY,Occ.[StartDate])
			FROM (
				SELECT 
					ROW_NUMBER() OVER(ORDER BY [StartDate] ASC) AS Row#,
					[StartDate],
					[EndDate]
				FROM  [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] (@frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByDay,@ByMonthDay,@ByMonth,@RepeatCount,@RepeatFrequency,@OccCount)
				WHERE [StartDate] IS NOT NULL
				AND CONVERT(DATE,[StartDate]) NOT IN (
					SELECT 
						DISTINCT CONVERT(DATE,[ExceptDate]) 
					FROM (
							SELECT LTRIM(RTRIM(Item)) as ExceptDate
							FROM dbo.SplitString(@ExceptDate, ',') 
						) AS ExceptDate
				)
			) AS Occ
			WHERE [EndDate] IS NOT NULL OR ([EndDate] IS NULL AND Occ.Row# <= @OccCount)

		END			
		ELSE IF(@SubEventCount > 0)
		BEGIN
			DECLARE @SubEventId BIGINT
			DECLARE db_cursor_insert_occurrence CURSOR STATIC FOR
	 
			SELECT [id] FROM [dbo].[Event] WITH (NOLOCK) 
			WHERE [SuperEventId] = @EventId 
			AND [FeedId] IS NULL 

			OPEN db_cursor_insert_occurrence  
			FETCH NEXT FROM db_cursor_insert_occurrence INTO @SubEventId  																															
			WHILE @@FETCH_STATUS = 0  
			BEGIN 	
				--PRINT @SubEventId		
				IF EXISTS(SELECT 1 FROM [dbo].[EventSchedule] WITH (NOLOCK) WHERE [EventId] = @SubEventId)
				BEGIN
					-- Get event end date if it's not available then get last subevent's startdate 
					SELECT @Evt_EndDate = [EndDate] FROM [dbo].[Event] WITH (NOLOCK) WHERE [id] = @SubEventId
					IF ISNULL(@Evt_EndDate,'') = ''
					BEGIN
						SELECT TOP 1 @Evt_EndDate = [StartDate] FROM [dbo].[Event] WITH (NOLOCK) WHERE [SuperEventId] = @SubEventId
						ORDER BY Id Desc
					END

					SELECT 
						@frequency			= ISNULL([Frequency],[RepeatFrequency]),
						@StartDate			= [StartDate],								
						@EndDate			= ISNULL([EndDate],@Evt_EndDate),
						@StartTime			= [StartTime],
						@EndTime			= [EndTime],
						@ByDay				= [ByDay],
						@ByMonth			= [ByMonth],
						@ByMonthDay			= [ByMonthDay],
						@RepeatCount		= [RepeatCount],
						@RepeatFrequency	= [RepeatFrequency],
						@ExceptDate = [ExceptDate]
					FROM [dbo].[EventSchedule] WITH (NOLOCK)
					WHERE [EventId] = @SubEventId

					INSERT INTO [dbo].[EventOccurrence]
					(
						[EventId]
						,[SubEventId]
						,[StartDate]
						,[EndDate]
						,[WeekDay]
						,[WeekName]
					)
					SELECT DISTINCT
						@EventId
						,@SubEventId 
						,Occ.[StartDate]
						,Occ.[EndDate]
						,DATEPART(DW,Occ.[StartDate])
						,DATENAME(WEEKDAY,Occ.[StartDate])
					FROM (
						SELECT DISTINCT
							ROW_NUMBER() OVER(ORDER BY [StartDate] ASC) AS Row#,
							[StartDate],
							[EndDate]
						FROM  [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] (@frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByDay,@ByMonthDay,@ByMonth,@RepeatCount,@RepeatFrequency,@OccCount)
						WHERE [StartDate] IS NOT NULL
						AND CONVERT(DATE,[StartDate]) NOT IN (
							SELECT 
								DISTINCT CONVERT(DATE,[ExceptDate]) 
							FROM (
									SELECT LTRIM(RTRIM(Item)) as ExceptDate
									FROM dbo.SplitString(@ExceptDate, ',') 
								) AS ExceptDate
						)
					) AS Occ
					WHERE [EndDate] IS NOT NULL OR ([EndDate] IS NULL AND Occ.Row# <= @OccCount)

				END				
				ELSE 
				BEGIN
					INSERT INTO [dbo].[EventOccurrence]
					(
						[EventId]
						,[SubEventId]
						,[StartDate]
						,[EndDate]
						,[WeekDay]
						,[WeekName]
					)
					SELECT 
						@EventId
						,@SubEventId
						,[StartDate]
						,[EndDate]
						,DATEPART(DW,[StartDate])
						,DATENAME(WEEKDAY,[StartDate])
					FROM [dbo].[Event] WITH (NOLOCK)
					WHERE Id = @SubEventId;				
				END

				FETCH NEXT FROM db_cursor_insert_occurrence INTO @SubEventId 
			END 

			CLOSE db_cursor_insert_occurrence  
			DEALLOCATE db_cursor_insert_occurrence
		END			
		ELSE
		BEGIN
			INSERT INTO [dbo].[EventOccurrence]
			(
				[EventId]
				,[StartDate]
				,[EndDate]
				,[WeekDay]
				,[WeekName]
			)
			SELECT 
				[Id]
				,[StartDate]
				,[EndDate]
				,DATEPART(DW,[StartDate])
				,DATENAME(WEEKDAY,[StartDate])			
			FROM [dbo].[Event] WITH (NOLOCK)
			WHERE [Id] = @EventId
			AND [StartDate] IS NOT NULL;		
		END
		
		COMMIT TRANSACTION tran_Occurrence_Insert
		PRINT 'Inserted the Record Successfully'
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION tran_Occurrence_Insert
		PRINT 'Sorry, Error occurred !!'
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[EventOccurrence_Insert_v1_29-04-2019-Backup]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[EventOccurrence_Insert_v1] 1
CREATE PROCEDURE [dbo].[EventOccurrence_Insert_v1_29-04-2019-Backup]
	@EventId BIGINT	
	,@OccCount INT = 12
AS
BEGIN
	DECLARE @SubEventCount  BIGINT;
	--set datefirst 1;

	/* 
		Note :- 
			For datefirst = 7 , DateRangeByFrequencyWithMultipleReccurrence is being used as it is made according to datefirst 7
			For datefirst = 1 , DateRangeByFrequencyWithMultipleReccurrence_v1 is being used as it is made according to datefirst 1
	*/
	/* 
		Note :- 
			For frequency with ISO 8601 format , DateRangeByFrequencyWithMultipleReccurrence_ISO8601 is needed to used 
			Otherwise, DateRangeByFrequencyWithMultipleReccurrence is needed to used 
	*/

	DECLARE @frequency			NVARCHAR(100), 
			@StartDate			DATE, 
			@EndDate			DATE,			
			@StartTime			TIME,
			@EndTime			TIME,
			@ByDay				NVARCHAR(500),
			@ByMonth			NVARCHAR(100),
			@ByMonthDay			NVARCHAR(100),
			@RepeatCount		INT,
			@RepeatFrequency	NVARCHAR(50),
			@ExceptDate			NVARCHAR(MAX),
			@Evt_EndDate		DATE ;	

	BEGIN TRY
	    -- Get total subevents
		SELECT @SubEventCount = COUNT(1) FROM Event WITH (NOLOCK) WHERE SuperEventId = @EventId AND FeedId IS NULL 		

		BEGIN TRANSACTION tran_Occurrence_Insert

		IF EXISTS(SELECT 1 FROM EventSchedule WITH (NOLOCK) WHERE EventId = @EventId)
		BEGIN
			-- Get event end date if it's not available then get last subevent's startdate 
			SELECT @Evt_EndDate = EndDate FROM EVENT WITH (NOLOCK) WHERE id = @EventId
			IF ISNULL(@Evt_EndDate,'') = ''
			BEGIN
				SELECT TOP 1 @Evt_EndDate = StartDate FROM Event WITH (NOLOCK) WHERE SuperEventId = @EventId AND FeedId IS NULL 
				ORDER BY Id Desc
			END

			SELECT 
				@frequency			= ISNULL(Frequency,RepeatFrequency),
				@StartDate			= es.StartDate,					
				@EndDate			= ISNULL(es.EndDate,@Evt_EndDate),
				@StartTime			= StartTime,
				@EndTime			= EndTime,
				@ByDay				= ByDay,
				@ByMonth			= ByMonth,
				@ByMonthDay			= ByMonthDay,
				@RepeatCount		= RepeatCount,
				@RepeatFrequency	= RepeatFrequency,
				@ExceptDate			= ExceptDate
			FROM EventSchedule es WITH (NOLOCK)
			WHERE EventId = @EventId

			INSERT INTO [dbo].[EventOccurrence]
			(
				[EventId]
				,[StartDate]
				,[EndDate]
				,[WeekDay]
				,[WeekName]
			)
			SELECT DISTINCT
				@EventId
				,Occ.StartDate
				,Occ.EndDate
				,DATEPART(dw,Occ.StartDate)
				,DATENAME(weekday,Occ.StartDate)
			FROM (
				SELECT 
					ROW_NUMBER() OVER(ORDER BY StartDate ASC) AS Row#,
					StartDate,
					EndDate
				FROM  [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] (@frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByDay,@ByMonthDay,@ByMonth,@RepeatCount,@RepeatFrequency,@OccCount)
				WHERE StartDate IS NOT NULL
				AND CONVERT(DATE,StartDate) NOT IN (
					SELECT 
						DISTINCT CONVERT(DATE,ExceptDate) 
					FROM (
							SELECT LTRIM(RTRIM(Item)) as ExceptDate
							FROM dbo.SplitString(@ExceptDate, ',') 
						) AS ExceptDate
				)
			) AS Occ
			WHERE EndDate IS NOT NULL OR (EndDate IS NULL AND Occ.Row# <= @OccCount)

		END	
		ELSE IF(@SubEventCount > 0)
		BEGIN
			DECLARE @SubEventId BIGINT
			DECLARE db_cursor_insert_occurrence CURSOR STATIC FOR
	 
			SELECT id FROM [dbo].[Event] WITH (NOLOCK) WHERE SuperEventId = @EventId AND FeedId IS NULL 

			OPEN db_cursor_insert_occurrence  
			FETCH NEXT FROM db_cursor_insert_occurrence INTO @SubEventId  																															
			WHILE @@FETCH_STATUS = 0  
			BEGIN 	
				--PRINT @SubEventId		
				IF EXISTS(SELECT 1 FROM [dbo].[EventSchedule] WITH (NOLOCK) WHERE EventId = @SubEventId)
				BEGIN
					-- Get event end date if it's not available then get last subevent's startdate 
					SELECT @Evt_EndDate = EndDate FROM EVENT WITH (NOLOCK) WHERE id = @SubEventId
					IF ISNULL(@Evt_EndDate,'') = ''
					BEGIN
						SELECT TOP 1 @Evt_EndDate = StartDate FROM Event WITH (NOLOCK) WHERE SuperEventId = @SubEventId
						ORDER BY Id Desc
					END

					SELECT 
						@frequency			= ISNULL(Frequency,RepeatFrequency),
						@StartDate			= StartDate,								
						@EndDate			= ISNULL(EndDate,@Evt_EndDate),
						@StartTime			= StartTime,
						@EndTime			= EndTime,
						@ByDay				= ByDay,
						@ByMonth			= ByMonth,
						@ByMonthDay			= ByMonthDay,
						@RepeatCount		= RepeatCount,
						@RepeatFrequency	= RepeatFrequency,
						@ExceptDate = ExceptDate
					FROM EventSchedule WITH (NOLOCK)
					WHERE EventId = @SubEventId

					INSERT INTO [dbo].[EventOccurrence]
					(
						[EventId]
						,[SubEventId]
						,[StartDate]
						,[EndDate]
						,[WeekDay]
						,[WeekName]
					)
					SELECT DISTINCT
						@EventId
						,@SubEventId 
						,Occ.StartDate
						,Occ.EndDate
						,DATEPART(dw,Occ.StartDate)
						,DATENAME(weekday,Occ.StartDate)
					FROM (
						SELECT DISTINCT
							ROW_NUMBER() OVER(ORDER BY StartDate ASC) AS Row#,
							StartDate,
							EndDate
						FROM  [dbo].[DateRangeByFrequencyWithMultipleReccurrence_ISO8601] (@frequency,@StartDate,@EndDate,@StartTime,@EndTime,@ByDay,@ByMonthDay,@ByMonth,@RepeatCount,@RepeatFrequency,@OccCount)
						WHERE StartDate IS NOT NULL
						AND CONVERT(DATE,StartDate) NOT IN (
							SELECT 
								DISTINCT CONVERT(DATE,ExceptDate) 
							FROM (
									SELECT LTRIM(RTRIM(Item)) as ExceptDate
									FROM dbo.SplitString(@ExceptDate, ',') 
								) AS ExceptDate
						)
					) AS Occ
					WHERE EndDate IS NOT NULL OR (EndDate IS NULL AND Occ.Row# <= @OccCount)

				END				
				ELSE 
				BEGIN
					INSERT INTO [dbo].[EventOccurrence]
					(
						[EventId]
						,[SubEventId]
						,[StartDate]
						,[EndDate]
						,[WeekDay]
						,[WeekName]
					)
					SELECT 
						@EventId
						,@SubEventId
						,StartDate
						,EndDate
						,DATEPART(dw,StartDate)
						,DATENAME(weekday,StartDate)
					FROM [dbo].[Event] WITH (NOLOCK)
					WHERE Id = @SubEventId;				
				END

				FETCH NEXT FROM db_cursor_insert_occurrence INTO @SubEventId 
			END 

			CLOSE db_cursor_insert_occurrence  
			DEALLOCATE db_cursor_insert_occurrence
		END	
		ELSE
		BEGIN
			INSERT INTO [dbo].[EventOccurrence]
			(
				[EventId]
				,[StartDate]
				,[EndDate]
				,[WeekDay]
				,[WeekName]
			)
			SELECT 
				Id
				,StartDate
				--,ISNULL(EndDate,StartDate)
				,EndDate
				,DATEPART(dw,StartDate)
				,DATENAME(weekday,StartDate)			
			FROM [dbo].[Event] WITH (NOLOCK)
			WHERE Id = @EventId
			AND StartDate IS NOT NULL;		
		END
		
		COMMIT TRANSACTION tran_Occurrence_Insert
		PRINT 'Inserted the Record Successfully'
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION tran_Occurrence_Insert
		PRINT 'Sorry, Error occurred !!'
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[EventSchedule_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[EventSchedule_Delete]
	@EventId BIGINT
AS
BEGIN
	DELETE	FROM [dbo].[EventSchedule]
	WHERE [EventId] = @EventId
END


GO
/****** Object:  StoredProcedure [dbo].[EventSchedule_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[EventSchedule_Insert]
	@EventId			BIGINT,
	@StartDate			DATE,
	@EndDate			DATE,
	@StartTime			TIME(7),
	@EndTime			TIME(7),
	@Frequency			NVARCHAR(50),
	@ByDay				NVARCHAR(MAX),
	@ByMonth			NVARCHAR(MAX),
	@ByMonthDay			NVARCHAR(MAX),
	@RepeatCount		INT,
	@RepeatFrequency	NVARCHAR(50),
	@EventScheduleId	BIGINT OUT
AS
BEGIN
	IF(@StartDate IS NOT NULL
		OR @EndDate IS NOT NULL
		OR @StartTime IS NOT NULL
		OR @EndTime IS NOT NULL
		OR ISNULL(@Frequency, '') <> ''
		OR ISNULL(@ByDay, '') <> ''
		OR ISNULL(@ByMonth, '') <> ''
		OR ISNULL(@ByMonthDay, '') <> ''
		)
	BEGIN
		IF NOT EXISTS(SELECT id 
				FROM EventSchedule WITH (NOLOCK)
				WHERE  EventId = @EventId
						and StartDate = @StartDate)
		BEGIN
			INSERT INTO [dbo].[EventSchedule]
					   ([EventId]
					   ,[StartDate]
					   ,[EndDate]
					   ,[StartTime]
					   ,[EndTime]
					   ,[Frequency]
					   ,[ByDay]
					   ,[ByMonth]
					   ,[ByMonthDay]
					   ,[RepeatCount]
					   ,[RepeatFrequency])
				 VALUES
					   (@EventId
					   ,@StartDate
					   ,@EndDate
					   ,@StartTime
					   ,@EndTime
					   ,@Frequency
					   ,@ByDay
					   ,@ByMonth
					   ,@ByMonthDay
					   ,@RepeatCount
					   ,@RepeatFrequency)
	
			SET @EventScheduleId = SCOPE_IDENTITY()
		END
		ELSE
		BEGIN
			SELECT	@EventScheduleId = id
			FROM	EventSchedule WITH (NOLOCK)
			WHERE	EventId = @EventId
					AND StartDate = @StartDate
		END
	END
END

GO
/****** Object:  StoredProcedure [dbo].[EventSchedule_Insert_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EventSchedule_Insert_v1]
	@EventId			BIGINT,
	@StartDate			DATE,
	@EndDate			DATE,
	@StartTime			TIME(7),
	@EndTime			TIME(7),
	@Frequency			NVARCHAR(50),
	@ByDay				NVARCHAR(MAX),
	@ByMonth			NVARCHAR(MAX),
	@ByMonthDay			NVARCHAR(MAX),
	@RepeatCount		INT,
	@RepeatFrequency	NVARCHAR(50),
	@ExceptDate			NVARCHAR(MAX),
	@EventScheduleId	BIGINT OUT
AS
BEGIN
	IF(@StartDate IS NOT NULL
		OR @EndDate IS NOT NULL
		OR @StartTime IS NOT NULL
		OR @EndTime IS NOT NULL
		OR ISNULL(@Frequency, '') <> ''
		OR ISNULL(@ByDay, '') <> ''
		OR ISNULL(@ByMonth, '') <> ''
		OR ISNULL(@ByMonthDay, '') <> ''
		)
	BEGIN	

		IF NOT EXISTS(SELECT [id] 
				FROM [dbo].[EventSchedule] WITH (NOLOCK)
				WHERE  [EventId] = @EventId
						AND [StartDate] = @StartDate)
		BEGIN
			INSERT INTO [dbo].[EventSchedule]
					   ([EventId]
					   ,[StartDate]
					   ,[EndDate]
					   ,[StartTime]
					   ,[EndTime]
					   ,[Frequency]
					   ,[ByDay]
					   ,[ByMonth]
					   ,[ByMonthDay]
					   ,[RepeatCount]
					   ,[RepeatFrequency]
					   ,[ExceptDate]
					   ,[ActualFrequency]					   
					)
				SELECT
					@EventId
					,@StartDate
					,@EndDate
					,@StartTime
					,@EndTime					
					,F.Frequency
					,@ByDay
					,@ByMonth
					,@ByMonthDay
					,@RepeatCount					
					,F.Interval
					,@ExceptDate
					,@RepeatFrequency
				FROM [dbo].[GetFrequencyInterval](@Frequency,@RepeatFrequency) AS F
				
			SET @EventScheduleId = SCOPE_IDENTITY()
		END
		ELSE
		BEGIN
			SELECT	@EventScheduleId = [id]
			FROM	[dbo].[EventSchedule] WITH (NOLOCK)
			WHERE	[EventId] = @EventId
					AND [StartDate] = @StartDate
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[FacilityUse_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FacilityUse_Delete]
(
    @EventId BIGINT
) AS
BEGIN
    DELETE FROM [dbo].[FacilityUse]
    WHERE [EventId] = @EventId
END
GO
/****** Object:  StoredProcedure [dbo].[FacilityUse_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <18-03-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[FacilityUse_Insert]
(
    @EventId BIGINT,
    @ParentId BIGINT = NULL,
    @TypeID INT = NULL,
    @url NVARCHAR(MAX) = NULL,
    @identifier NVARCHAR(255) = NULL,
    @name NVARCHAR(255) = NULL,
    @description NVARCHAR(MAX) = NULL,
    @provider NVARCHAR(MAX) = NULL,
    @image NVARCHAR(MAX) = NULL,
    @facilityUsedId BIGINT OUT
) AS
BEGIN
    INSERT INTO [dbo].[FacilityUse]
    (
        [EventId],
        [ParentId],
        [TypeId],
        [Url],
        [Identifier],
        [Name],
        [Description],
        [Provider],
        [Image]
    )
    VALUES
    (
        @EventId,
        @parentId,
        @typeId,
        @url,
        @identifier,
        @name,
        @description,
        @provider,
        @image
    )
    SET @facilityUsedId = SCOPE_IDENTITY()
END
GO
/****** Object:  StoredProcedure [dbo].[FeedMapping_ActivateDeactivateFeed]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FeedMapping_ActivateDeactivateFeed]
	@Id			BIGINT,
	@IsActive	BIT
AS
BEGIN	
	DECLARE @IsCustomFeedKey BIT = 0
	SELECT @IsCustomFeedKey = IsCustomFeedKey FROM FeedMapping WITH (NOLOCK) WHERE id = @Id 

	-- Delete/activate parent feed key
	UPDATE [dbo].[FeedMapping]
	SET 
		[IsDeleted] = IIF(@IsCustomFeedKey = 0,@IsActive,[IsDeleted])
		--,IsCustomKeyActive = IIF(@IsCustomFeedKey = 1,@IsActive,IsCustomKeyActive)
	WHERE	[id] = @Id 

	-- Delete/activate child feed key(s)
	UPDATE [dbo].[FeedMapping]
	SET 
		[IsDeleted] = @IsActive		
	WHERE [ParentId] = @Id
	AND [IsCustomFeedKey] = 0


	--IF @IsCustomFeedKey = 1
	--BEGIN
	--	UPDATE	FeedMapping
	--	SET		IsCustomKeyActive = @IsActive
	--	WHERE	id = @Id AND IsCustomFeedKey = 1
	--END
	--ELSE
	--BEGIN
	--	UPDATE	FeedMapping
	--	SET		IsDeleted = @IsActive
	--	WHERE	id = @Id				
	--END
END

GO
/****** Object:  StoredProcedure [dbo].[FeedMapping_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[FeedMapping_Delete]
	@Id						BIGINT,
	@IsCustomFeedKey		BIT = 1,
	@EffectToInteMapping	BIT = 0
AS
BEGIN
	UPDATE	[dbo].[FeedMapping]
	SET		[IsDeleted] = 1
	WHERE	[id] = @Id
	AND		[IsCustomFeedKey] = @IsCustomFeedKey


	IF @EffectToInteMapping = 1
	BEGIN		
		UPDATE	[dbo].[IntelligentMapping] 
		SET		[IsDeleted] = 1
		FROM (
			SELECT [TableName],[ColumnName],[Position] FROM [dbo].[FeedMapping] WITH (NOLOCK) WHERE [id] = @Id) fm
		WHERE	IntelligentMapping.[TableName] = fm.[TableName] 
		AND		IntelligentMapping.[ColumnName] = fm.[ColumnName]
		AND		IntelligentMapping.[Position] = fm.[Position]
		AND		[IsCustomFeedKey] = @IsCustomFeedKey
	END
END
GO
/****** Object:  StoredProcedure [dbo].[FeedMapping_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[FeedMapping_Insert]
	@FeedProviderId		INT,
	@ParentId			BIGINT,
	@TableName			NVARCHAR(50),
	@ColumnName			NVARCHAR(50),
	@FeedKey			NVARCHAR(50),
	@FeedKeyPath		NVARCHAR(200),
	@ActualFeedKeyPath	NVARCHAR(200),
	@Constraint			NVARCHAR(MAX),
	@FeedMappingId		INT OUT
AS
BEGIN
	IF NOT EXISTS(SELECT	[id] 
					FROM	[dbo].[FeedMapping] WITH (NOLOCK)
					WHERE	[FeedProviderId] = @FeedProviderId
							AND ISNULL([ParentId],0) = ISNULL(@ParentId,0)
							AND [TableName] = @TableName 
							AND [ColumnName] = @ColumnName)
	BEGIN
		INSERT INTO [dbo].[FeedMapping]
				   ([ParentId]
				   ,[FeedProviderId]
				   ,[TableName]
				   ,[ColumnName]
				   ,[FeedKey]
				   ,[FeedKeyPath]
				   ,[ActualFeedKeyPath]
				   ,[Constraint])
			 VALUES
				   (@ParentId
				   ,@FeedProviderId
				   ,@TableName
				   ,@ColumnName
				   ,@FeedKey
				   ,@FeedKeyPath
				   ,@ActualFeedKeyPath
				   ,@Constraint)

		SET @FeedMappingId = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		UPDATE	[dbo].[FeedMapping]
		SET		[FeedKey] = @FeedKey
				,[FeedKeyPath] = @FeedKeyPath
				,[ActualFeedKeyPath] = @ActualFeedKeyPath
				,[Constraint] = @Constraint
		WHERE	[FeedProviderId] = @FeedProviderId
				AND ISNULL([ParentId],0) = ISNULL(@ParentId,0)
				AND [TableName] = @TableName
				AND [ColumnName] = @ColumnName

		SELECT @FeedMappingId = [id] 
		FROM [dbo].[FeedMapping] WITH (NOLOCK)
		WHERE	[FeedProviderId] = @FeedProviderId
				AND ISNULL([ParentId],0) = ISNULL(@ParentId,0)
				AND [TableName] = @TableName
				AND [ColumnName] = @ColumnName
	END
END

GO
/****** Object:  StoredProcedure [dbo].[FeedMapping_InsertUpdate]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FeedMapping_InsertUpdate]
	@FeedProviderId		INT,
	@ParentId			BIGINT,
	@TableName			NVARCHAR(100),
	@ColumnName			NVARCHAR(500),
	@ColumnDataType		NVARCHAR(50),
	@IsCustomFeedKey	BIT,
	@FeedKey			NVARCHAR(50),
	@FeedKeyPath		NVARCHAR(200),
	@ActualFeedKeyPath	NVARCHAR(200),
	@Constraint			NVARCHAR(MAX),
	@FeedMappingId		INT OUT,
	@Position			BIGINT
AS
BEGIN
	
	IF(@FeedMappingId IS NOT NULL
		AND EXISTS (SELECT	[id] 
					FROM	[dbo].[FeedMapping] WITH (NOLOCK)
					WHERE	[id] = @FeedMappingId
							AND ISNULL([ParentId],0) = ISNULL(@ParentId,0)
							AND [FeedProviderId] = @FeedProviderId
							AND [IsCustomFeedKey] = @IsCustomFeedKey
							AND [TableName] = @TableName
							AND [ColumnName] = @ColumnName --added on 05/12/2018 due to existing critetia was making confiction(such as id and tablename only)
							AND [IsDeleted] = 0))
	BEGIN
		--Update existing feedmapping when custom feed key name is changed
		UPDATE	[dbo].[FeedMapping]
		SET		[IsCustomFeedKey] = @IsCustomFeedKey
				,[ColumnName] = @ColumnName
				,[ColumnDataType] = @ColumnDataType
				,[FeedKey] = @FeedKey
				,[FeedKeyPath] = @FeedKeyPath
				,[ActualFeedKeyPath] = @ActualFeedKeyPath
				,[Constraint] = @Constraint
				,[IsDeleted] = 0
				,[Position] = @Position
		WHERE	[id] = @FeedMappingId
				AND [FeedProviderId] = @FeedProviderId
				AND ISNULL([ParentId],0) = ISNULL(@ParentId, 0)
				AND [TableName] = @TableName
	END
	ELSE
	BEGIN
		IF NOT EXISTS(SELECT	[id] 
						FROM	[dbo].[FeedMapping] WITH (NOLOCK)
						WHERE	[FeedProviderId] = @FeedProviderId
								AND ISNULL([ParentId],0) = ISNULL(@ParentId,0)
								AND [IsCustomFeedKey] = @IsCustomFeedKey
								AND [TableName] = @TableName 
								AND [ColumnName] = @ColumnName
								AND [IsDeleted] = 0)
		BEGIN
			--add new feed mapping after analysis or add new custom feed key			
			IF @IsCustomFeedKey = 1
			BEGIN
				SET @Position = (SELECT [Position] FROM [dbo].[IntelligentMapping] WITH (NOLOCK)
										WHERE ISNULL([ParentId],0) = ISNULL(@ParentId,0)
										AND [IsCustomFeedKey] = @IsCustomFeedKey
										AND [TableName] = @TableName 
										AND [ColumnName] = @ColumnName
										AND [IsDeleted] = 0)
			END

			INSERT INTO [dbo].[FeedMapping]
				   ([ParentId]
				   ,[FeedProviderId]
				   ,[TableName]
				   ,[ColumnName]
				   ,[ColumnDataType]
				   ,[IsCustomFeedKey]
				   ,[FeedKey]
				   ,[FeedKeyPath]
				   ,[ActualFeedKeyPath]
				   ,[Position]
				   ,[Constraint])
			 VALUES
				   (@ParentId
				   ,@FeedProviderId
				   ,@TableName
				   ,@ColumnName
				   ,@ColumnDataType
				   ,@IsCustomFeedKey
				   ,@FeedKey
				   ,@FeedKeyPath
				   ,@ActualFeedKeyPath
				   ,@Position
				   ,@Constraint)

			SET @FeedMappingId = SCOPE_IDENTITY()
		END
		ELSE
		BEGIN
			-- update key position which are moved from non matched to matched
			IF (ISNULL(@IsCustomFeedKey,0) = 0 AND @Position IS NULL AND @ActualFeedKeyPath IS NOT NULL)
			BEGIN
				SET @Position = (SELECT [Position] FROM [dbo].[IntelligentMapping] WITH (NOLOCK)
								 WHERE [TableName] = @TableName 
								 AND [ColumnName] = @ColumnName
								 AND ([IsCustomFeedKey] = 0 OR [IsCustomFeedKey] IS NULL)
								 AND [IsDeleted] = 0)
			END

			-- update feed mapping after analysis or confirm feed analysis
			UPDATE	[dbo].[FeedMapping]
			SET		[IsCustomFeedKey] = @IsCustomFeedKey
					,[ColumnDataType] = @ColumnDataType
					,[FeedKey] = @FeedKey
					,[FeedKeyPath] = @FeedKeyPath
					,[ActualFeedKeyPath] = @ActualFeedKeyPath
					,[Constraint] = @Constraint
					,[IsDeleted] = 0
					,[Position] = @Position
			WHERE	[FeedProviderId] = @FeedProviderId
					AND ISNULL([ParentId],0) = ISNULL(@ParentId,0)
					AND [TableName] = @TableName
					AND [ColumnName] = @ColumnName

			SELECT	@FeedMappingId = [Id]
			FROM	[dbo].[FeedMapping] WITH (NOLOCK)
			WHERE	[FeedProviderId] = @FeedProviderId
					AND ISNULL([ParentId],0) = ISNULL(@ParentId, 0)
					AND [TableName] = @TableName
					AND [ColumnName] = @ColumnName
		END
	END
END
GO
/****** Object:  StoredProcedure [dbo].[FeedProvider_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FeedProvider_Delete]
	@FeedProviderId INT,
	@IsDeleted		BIT
AS
BEGIN
	-- delete from feed provider	
	UPDATE	[dbo].[FeedProvider]
	SET		[IsDeleted] = @IsDeleted
			,[JSONTreeFileName] = NULL
			,[SampleJSONFIleName] = NULL
			,[JsonTreeWithDisabledKeysFileName] = NULL
			,[UpdateDatetime]=GETUTCDATE()
	WHERE	[id] = @FeedProviderId

	-- delete from feed mapping	
	--UPDATE	FeedMapping
	--SET		IsDeleted = @IsDeleted
	--WHERE	FeedProviderId = @FeedProviderId
	
		DELETE [dbo].[FeedMapping]
		WHERE	[FeedProviderId] = @FeedProviderId

	-- delete from scheduler setting
	--UPDATE	SchedulerSettings
	--SET		IsDeleted = @IsDeleted
	--WHERE	FeedProviderId = @FeedProviderId	
		
		DELETE [dbo].[SchedulerSettings] 
		WHERE	[FeedProviderId] = @FeedProviderId

	--Delete Rule
	UPDATE [dbo].[Rule] SET [IsDeleted] = 1 ,[UpdatedOn]=GETUTCDATE(),[IsEnabled]=0
	WHERE [FeedProviderId] = @FeedProviderId

	DELETE FC
	FROM [dbo].[FilterCriteria] FC
	INNER JOIN [dbo].[Rule] R ON  R.[Id] = FC.[RuleId] AND R.[IsDeleted] = 1
	WHERE R.[FeedProviderId] = @FeedProviderId

	DELETE OP
	FROM [dbo].[Operation] OP
	INNER JOIN [dbo].[Rule] R ON  R.[Id] = OP.[RuleId] AND R.[IsDeleted] = 1
	WHERE R.[FeedProviderId] = @FeedProviderId

	-- delete from event and it's dependent tables
	DECLARE @EventId BIGINT
	DECLARE db_cursor_delete_event CURSOR STATIC FOR
	 
	SELECT [id] FROM [dbo].[Event] WITH(NOLOCK) WHERE [FeedProviderId] = @FeedProviderId

	OPEN db_cursor_delete_event  
	FETCH NEXT FROM db_cursor_delete_event INTO @EventId  

	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		  EXEC Event_DeleteOldData @EventId
		  DELETE FROM [dbo].[Event] WHERE [id] = @EventId
		  FETCH NEXT FROM db_cursor_delete_event INTO @EventId 
	END 

	CLOSE db_cursor_delete_event  
	DEALLOCATE db_cursor_delete_event
END
GO
/****** Object:  StoredProcedure [dbo].[FeedProvider_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[FeedProvider_Insert]
	@Name					NVARCHAR(100),
	@Source					NVARCHAR(MAX),
	@IsIminConnector		BIT,
	@FeedDataTypeId			INT,
	@EndpointUp				BIT,
	@UsesPagingSpec			BIT,
	@IsOpenActiveCompatible BIT,
	@IncludesCoordinates	BIT,
	@FeedProviderId			INT OUT
AS
BEGIN	
	INSERT INTO [dbo].[FeedProvider]
           ([Name]
           ,[Source]
           ,[IsIminConnector]
           ,[FeedDataTypeId]
           ,[EndpointUp]
           ,[UsesPagingSpec]
           ,[IsOpenActiveCompatible]
           ,[IncludesCoordinates]
           ,[InsertedDatetime])
     VALUES
           (@Name
           ,@Source
           ,@IsIminConnector
           ,@FeedDataTypeId
           ,@EndpointUp
           ,@UsesPagingSpec
           ,@IsOpenActiveCompatible
           ,@IncludesCoordinates
           ,GETDATE())

	SET @FeedProviderId = SCOPE_IDENTITY();
END


GO
/****** Object:  StoredProcedure [dbo].[FeedProvider_UpdateFeedMappingConfirmStatus]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[FeedProvider_UpdateFeedMappingConfirmStatus]
	@FeedProviderId			INT,
	@IsFeedMappingConfirmed BIT
AS
BEGIN	
	UPDATE	[dbo].[FeedProvider]
	SET		[IsFeedMappingConfirmed] = @IsFeedMappingConfirmed
	WHERE	[Id] = @FeedProviderId
	AND		[IsDeleted] = 0
END
GO
/****** Object:  StoredProcedure [dbo].[FeedProvider_UpdateJSONFileName]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[FeedProvider_UpdateJSONFileName]
	@FeedProviderId						INT,
	@JSONTreeFileName					NVARCHAR(MAX),
	@SampleJSONFIleName					NVARCHAR(MAX),
	@JsonTreeWithDisabledKeysFileName	NVARCHAR(MAX)
AS
BEGIN	
	UPDATE	[dbo].[FeedProvider]
	SET		[JSONTreeFileName] = @JSONTreeFileName,
			[SampleJSONFIleName] = @SampleJSONFIleName,
			[JsonTreeWithDisabledKeysFileName] = @JsonTreeWithDisabledKeysFileName
	WHERE	[Id] = @FeedProviderId
	AND		[IsDeleted] = 0
END

GO
/****** Object:  StoredProcedure [dbo].[FeedProvider_UpdateOrderingStrategy]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[FeedProvider_UpdateOrderingStrategy]
	@FeedProviderId		INT,
	@IsUsesTimestamp	BIT,
	@IsUtcTimestamp		BIT,
	@IsUsesChangenumber BIT,
	@IsUsesUrlSlug		BIT
AS
BEGIN	
	UPDATE	[dbo].[FeedProvider]
	SET		[IsUsesTimestamp] = @IsUsesTimestamp
			,[IsUtcTimestamp] = @IsUtcTimestamp
			,[IsUsesChangenumber] = @IsUsesChangenumber
			,[IsUsesUrlSlug] = @IsUsesUrlSlug
	WHERE	[Id] = @FeedProviderId
	AND		[IsDeleted] = 0
END

GO
/****** Object:  StoredProcedure [dbo].[FeedProviderNameAvailable]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[FeedProviderNameAvailable]
	@Name NVARCHAR(255),
	@Id INT
AS
BEGIN
	DECLARE @Result BIT = 0;
	IF EXISTS(SELECT [Id] 
				FROM [dbo].[FeedProvider] WITH(NOLOCK) 
				WHERE LOWER([Name]) = LOWER(@Name) 
				AND [IsDeleted] = 0
				AND 1 = CASE WHEN @ID = 0 THEN 1 WHEN @Id <> id THEN 1 ELSE 0 END
				)
	BEGIN
		SET @Result = 1;
	END
	SELECT @Result AS Result
END
GO
/****** Object:  StoredProcedure [dbo].[FeedProviderNameUpdate]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <23-04-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[FeedProviderNameUpdate]
	@Name NVARCHAR(255),
	@Id INT
AS
BEGIN
	UPDATE [dbo].[FeedProvider] 
	SET [Name] = @Name
	WHERE [id] = @Id
END
GO
/****** Object:  StoredProcedure [dbo].[GetActivities]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- [dbo].[GetActivities] 51.5073509,-0.1277583,30
-- [dbo].[GetActivities] 51.5595,-0.3167933,50
-- [dbo].[GetActivities] 51.5595,-0.3167933,50,NULL,'Female'
CREATE PROCEDURE [dbo].[GetActivities]	
	@lat				DECIMAL(10,8),
	@long				DECIMAL(10,8),
	@radius				DECIMAL(10,8),
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,	
	@weekdays			NVARCHAR(50) = NULL,
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,
	@source				NVARCHAR(MAX) = NULL,
	@kind				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@agerange			NVARCHAR(MAX)	= NULL
AS
BEGIN
	DECLARE @min_Minute INT = DATEDIFF(MINUTE, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(MINUTE, '00:00:00', '23:59:59.999999');

	SELECT 
		DISTINCT RTRIM(LTRIM(PrefLabel)) AS PrefLabel
	FROM [dbo].[PhysicalActivity] 
	WHERE EventId IN 
	(
		SELECT
			e.id
		FROM [dbo].[Event] e WITH (NOLOCK)
		INNER JOIN [dbo].[Place] p ON p.EventId = e.Id	
		LEFT JOIN [dbo].[EventSchedule] es ON es.EventId = e.Id								
		WHERE e.state = 'updated' 
		AND e.FeedId IS NOT NULL
		--AND ISNULL(p.Lat,'') <> '' AND ISNULL(p.Long,'') <> ''
		AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL			
		AND CONVERT(DECIMAL(10,8),p.Lat) >= @lat
		AND CONVERT(DECIMAL(10,8),p.Long) >= @long
		--AND CONVERT(DECIMAL(10,8),IIF(TRY_PARSE(Lat AS DECIMAL(10,8)) IS NOT NULL,p.Lat,'0.0')) >= @lat
		--AND CONVERT(DECIMAL(10,8),IIF(TRY_PARSE(Lat AS DECIMAL(10,8)) IS NOT NULL,p.Long,'0.0')) >= @long
		--AND dbo.LatLonRadiusDistance(@lat,@long,CONVERT(DECIMAL(10,8),p.Lat) ,CONVERT(DECIMAL(10,8),p.Long)) <= @radius
		AND dbo.LatLonRadiusDistance(@lat,@long,p.Lat,p.Long) <= @radius				
		AND DATEDIFF_BIG(minute, '00:00:00', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ISNULL(@mintime,@min_Minute)
		AND DATEDIFF_BIG(minute, '00:00:00', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ISNULL(@maxtime,@max_Minute)
		AND [dbo].[IsAgeRangeEligibleForGivenAge](e.Id,@minAge,@maxAge) = 1
		AND (ISNULL(@gender,'') = '' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = @gender)
		AND (ISNULL(@from,'') = '' OR e.StartDate >= @from)
		AND (ISNULL(@to,'') = '' OR e.EndDate >= @to)			
		AND (ISNULL(@disabilitySupport,'')  = '' 
			OR (
					SELECT COUNT(*) FROM (
						SELECT * FROM (
							SELECT DISTINCT ITEM FROM (
								SELECT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
								FROM dbo.SplitString(e.AccessibilitySupport, ',') 
							) AS AccessibilitySupport
						)AS AccessibilitySupportInTable
						WHERE AccessibilitySupportInTable.ITEM IN (
							SELECT DISTINCT Item FROM (
								SELECT Item AS Item
								FROM dbo.SplitString(@disabilitySupport, ',') 
							) AS DisabilitySupport
						)
					) AS TotalCOunt 									
			) > 0
		)
		AND (ISNULL(@weekdays,'') = '' 
			OR (
				[dbo].[IsEventEligibleForGivenWeekdays] (e.Id,@weekdays) = 1
			)
		)
		AND (ISNULL(@tag,'')  = ''					 
			OR (
					SELECT COUNT(*) FROM (
						SELECT * FROM (
							SELECT DISTINCT Item FROM (
								SELECT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
								FROM dbo.SplitString(e.Category, ',') 
							) AS Category
						)AS CategoryInTable
						WHERE CategoryInTable.Item IN (									
							SELECT DISTINCT Item FROM (
								SELECT Item AS Item
								FROM dbo.SplitString(@tag, ',')
							) AS Tag
							WHERE ISNULL(@excludeTag,'') = ''
								OR 
								Tag.Item NOT IN (
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(@excludeTag, ',')
									) AS ExcludedTag
							)
						)
					) AS TotalCount 									
			) > 0
		)	
	)
END

GO
/****** Object:  StoredProcedure [dbo].[GetActivities_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- [dbo].[GetActivities_v1] 51.5073509,-0.1277583,30
-- [dbo].[GetActivities_v1] 51.5595,-0.3167933,50
-- [dbo].[GetActivities_v1] 51.5595,-0.3167933,30,NULL,'Female'
CREATE PROCEDURE [dbo].[GetActivities_v1]	
	@lat				DECIMAL(17, 14),
	@long				DECIMAL(17, 14),
	@radius				DECIMAL(17, 14),
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,	
	@weekdays			NVARCHAR(50) = NULL,
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,
	@source				NVARCHAR(MAX) = NULL,
	@kind				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@agerange			NVARCHAR(MAX)	= NULL
AS
BEGIN
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	DECLARE @min_Minute INT = DATEDIFF(MINUTE, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(MINUTE, '00:00:00', '23:59:59.999999');

	BEGIN TRY
		SELECT DISTINCT RTRIM(LTRIM(PA.PrefLabel)) AS PrefLabel
		FROM [dbo].[PhysicalActivity] PA WITH (NOLOCK)  
		INNER JOIN [dbo].[Event] e WITH (NOLOCK) ON e.[id] = PA.[EventId]  AND e.[FeedId] IS NOT NULL
		INNER JOIN [dbo].[Place] p WITH (NOLOCK) 
					ON p.[EventId] = e.[Id]
					AND TRY_CAST(p.[Lat] AS DECIMAL(10,8)) IS NOT NULL 
					AND TRY_CAST(p.[Long] AS DECIMAL(10,8)) IS NOT NULL	
			LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.[EventId] = e.[Id]
			CROSS APPLY dbo.CIRCLEDISTANCE(@lat,@long,p.Lat,p.Long) as C							
			WHERE CAST(ISNULL(es.[StartDate],e.[StartDate]) AS DATETIME2)  < (DATEADD(WEEK,4,GETUTCDATE())) 
			AND C.[Distance] <= @radius
			AND DATEDIFF_BIG(MINUTE, '00:00:00', CAST(ISNULL(es.[StartTime],e.[StartDate]) AS TIME)) >= ISNULL(@mintime,@min_Minute)
			AND DATEDIFF_BIG(MINUTE, '00:00:00', CAST(ISNULL(es.[EndTime],e.[EndDate]) AS TIME)) <= ISNULL(@maxtime,@max_Minute)		
			AND (ISNULL(@gender,'') = '' OR E.[Gender] = @gender)
			AND (ISNULL(@from,'') = '' OR e.[StartDate] >= @from)
			AND (ISNULL(@to,'') = '' OR e.[EndDate] >= @to)			
			AND (ISNULL(@disabilitySupport,'') = '' 
				OR (										
						SELECT COUNT(1) FROM (
							SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
							FROM dbo.SplitString(e.[AccessibilitySupport], ',') 
						)AS AccessibilitySupports
						WHERE ',' + ISNULL(@disabilitySupport,'') + ',' LIKE '%,' + AccessibilitySupports.ITEM + ',%'
				) > 0
			)
			AND (ISNULL(@weekdays,'') = '' 
				OR E.[Id] IN (							
					SELECT DISTINCT [EventId] FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
					WHERE ',' + ISNULL(@weekdays,'') + ',' like '%,' + CONVERT(NVARCHAR,dbo.WeekDay([WeekName])) + ',%'
				)	
			)
			AND (ISNULL(@tag,'')  = ''					 
				OR (
						SELECT COUNT(*) FROM (
							SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
							FROM dbo.SplitString(e.[Category], ',') 
						)AS Category
						WHERE Category.Item IN (
							SELECT DISTINCT Item
							FROM dbo.SplitString(ISNULL(@tag,''), ',') 
							WHERE ISNULL(@excludeTag,'') = ''
								OR ',' + ISNULL(@excludeTag,'') + ',' NOT LIKE '%,' + Item + ',%'
						)									
				) > 0
			)
			AND 1 = (
				CASE
					WHEN (@minAge IS NULL AND @maxAge IS NULL)
					THEN 1
					WHEN (ISNULL(e.[MinAge],0) BETWEEN ISNULL(@minAge,0)
								AND ISNULL(@maxAge,200)) 
						OR (e.[MaxAge] IS NOT NULL AND e.MaxAge BETWEEN ISNULL(@minAge,0)
								AND ISNULL(@maxAge,200))
					THEN 1
					ELSE 0
				END
			)
			ORDER BY PrefLabel
	PRINT 'Record fetched Successfully'
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetActivities - sp(GetActivities_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetActivities_v1_07-03-2019]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[GetActivities_v1_07-03-2019] 51.5073509,-0.1277583,30
-- [dbo].[GetActivities_v1_07-03-2019] 51.5595,-0.3167933,50
-- [dbo].[GetActivities_v1_07-03-2019] 51.5595,-0.3167933,50,NULL,'Female'
CREATE PROCEDURE [dbo].[GetActivities_v1_07-03-2019]	
	@lat				DECIMAL(10,8),
	@long				DECIMAL(10,8),
	@radius				DECIMAL(10,8),
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,	
	@weekdays			NVARCHAR(50) = NULL,
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,
	@source				NVARCHAR(MAX) = NULL,
	@kind				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@agerange			NVARCHAR(MAX)	= NULL
AS
BEGIN
	DECLARE @min_Minute INT = DATEDIFF(minute, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(minute, '00:00:00', '23:59:59.999999');

	BEGIN TRY
		SELECT 
			DISTINCT RTRIM(LTRIM(PrefLabel)) AS PrefLabel
		FROM [dbo].[PhysicalActivity] WITH (NOLOCK) 
		WHERE EventId IN 
		(
			SELECT
				e.id
			FROM [dbo].[Event] e WITH (NOLOCK)
			INNER JOIN [dbo].[Place] p WITH (NOLOCK) 
					ON p.EventId = e.Id 
					AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
					AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL	
			LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.EventId = e.Id
			CROSS APPLY dbo.CIRCLEDISTANCE(@lat,@long,p.Lat,p.Long) as C							
			WHERE e.FeedId IS NOT NULL
			AND C.Distance <= @radius
			AND DATEDIFF_BIG(minute, '00:00:00', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ISNULL(@mintime,@min_Minute)
			AND DATEDIFF_BIG(minute, '00:00:00', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ISNULL(@maxtime,@max_Minute)		
			AND (ISNULL(@gender,'') = '' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = @gender)
			AND (ISNULL(@from,'') = '' OR e.StartDate >= @from)
			AND (ISNULL(@to,'') = '' OR e.EndDate >= @to)			
			AND (ISNULL(@disabilitySupport,'') = '' 
				OR (										
						SELECT COUNT(1) FROM (
							SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
							FROM dbo.SplitString(e.AccessibilitySupport, ',') 
						)AS AccessibilitySupports
						WHERE ',' + ISNULL(@disabilitySupport,'') + ',' LIKE '%,' + AccessibilitySupports.ITEM + ',%'
				) > 0
			)
			AND (ISNULL(@weekdays,'') = '' 
				OR E.Id IN (							
					SELECT DISTINCT EventId FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
					WHERE ',' + ISNULL(@weekdays,'') + ',' like '%,' + CONVERT(NVARCHAR,dbo.WeekDay(WeekName)) + ',%'
				)	
			)
			AND (ISNULL(@tag,'')  = ''					 
				OR (
						SELECT COUNT(*) FROM (
							SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
							FROM dbo.SplitString(e.Category, ',') 
						)AS Category
						WHERE Category.Item IN (
							SELECT DISTINCT Item
							FROM dbo.SplitString(ISNULL(@tag,''), ',') 
							WHERE ISNULL(@excludeTag,'') = ''
								OR ',' + ISNULL(@excludeTag,'') + ',' NOT LIKE '%,' + Item + ',%'
						)									
				) > 0
			)
			AND 1 = (
				CASE
					WHEN (@minAge IS NULL AND @maxAge IS NULL)
					THEN 1
					WHEN (ISNULL(e.MinAge,0) BETWEEN ISNULL(@minAge,0)
								AND ISNULL(@maxAge,200)) 
						OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ISNULL(@minAge,0)
								AND ISNULL(@maxAge,200))
					THEN 1
					ELSE 0
				END
			)
		)
	PRINT 'Record fetched Successfully'
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetActivities - sp(GetActivities_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllActivities]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--[dbo].[GetAllActivities]	
CREATE PROCEDURE [dbo].[GetAllActivities]	
AS
BEGIN	
	SELECT DISTINCT [PrefLabel] 
	FROM [dbo].[PhysicalActivity] WITH (NOLOCK)
	ORDER BY 1
END

GO
/****** Object:  StoredProcedure [dbo].[GetAllOperationType]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <01-01-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetAllOperationType]
AS
BEGIN
	SELECT * FROM [dbo].[OperationType] WITH(NOLOCK)
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllOperator]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
CREATE PROCEDURE [dbo].[GetAllOperator]
AS
BEGIN
	SELECT * FROM [dbo].[Operator] WITH(NOLOCK)
	WHERE [IsAvtive] = 1
END
GO
/****** Object:  StoredProcedure [dbo].[GetAllRuleByFeedID]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <26-12-2018>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetAllRuleByFeedID]
	@FeedProviderId INT,
	@Offset			INT = 0,
	@pageSize		INT = 10,
	@sortColumnNo	INT = 0,
	@sortDirection	NVARCHAR(50) = 'ASC',
	@searchText		NVARCHAR(50) = ''
AS
BEGIN
	
DECLARE @SQL AS NVARCHAR(MAX) = ''

	DECLARE @SortColumn AS NVARCHAR(255) = ''

	IF @sortColumnNo = 0 
	BEGIN
		SET @SortColumn = 'R.[RuleName]'
	END
	ELSE IF @sortColumnNo = 1
	BEGIN
		SET @SortColumn = 'R.[IsEnabled]'
	END

	SET @SQL = 'SELECT R.[Id],R.[FeedProviderId],R.[RuleName],R.[IsEnabled] FROM [Rule] R  WITH (NOLOCK)
				INNER JOIN FeedProvider FP WITH (NOLOCK) ON FP.[id] = R.[FeedProviderId]
				WHERE R.[FeedProviderId] = '+CAST(@FeedProviderId AS NVARCHAR)+' AND R.[IsDeleted] = 0 
						AND (
								''' + @searchText + ''' = ''''
								OR R.[RuleName] LIKE ''%' + @searchText + '%''								
							)
				ORDER BY ' + @SortColumn + ' ' + @sortDirection + '
				OFFSET (' + CONVERT(NVARCHAR, @Offset) + ') *  (' + CONVERT(NVARCHAR, @pageSize) + ' - 1) ROWS
				FETCH NEXT ' + CONVERT(NVARCHAR, @pageSize) + ' ROWS ONLY '

	PRINT @SQL
	EXEC SP_EXECUTESQL @SQL

	SELECT	COUNT(1) 
	FROM	[Rule] WITH (NOLOCK)
	WHERE	[IsDeleted] = 0 AND
			[FeedProviderId] = @FeedProviderId

	SELECT	COUNT(1) 
	FROM	[Rule] R WITH (NOLOCK)
	WHERE	R.[IsDeleted] = 0
			AND (
					@searchText = ''
					OR R.[RuleName] LIKE '%' + @searchText + '%'					
				)
			AND [FeedProviderId] = @FeedProviderId

END
GO
/****** Object:  StoredProcedure [dbo].[GetALLRuleOperator]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <27-12-2018>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetALLRuleOperator]
AS
BEGIN
	SELECT * FROM [dbo].[RuleOperator] WITH(NOLOCK)
END
GO
/****** Object:  StoredProcedure [dbo].[GETAllRulesByFeedProvider]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <02-01-2018>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GETAllRulesByFeedProvider]
	@FeedProviderId INT
AS
BEGIN
	
	SELECT R.[Id],
		   R.[FeedProviderId],
		   R.[RuleName],
		   R.[IsEnabled],
		   FP.[Name]
	FROM [dbo].[Rule] R WITH (NOLOCK)
	INNER JOIN [dbo].[FeedProvider] FP WITH (NOLOCK) 
	ON FP.[id] = @FeedProviderId 
	AND FP.[IsDeleted] = 0 
	AND R.[FeedProviderId] = FP.[id]

	SELECT FC.*,
		   FM.[TableName],
		   FM.[ColumnName],
		   FM.[ColumnDataType],
		   FM.[FeedKey],
		   FM.[FeedKeyPath],
		   FM.[ActualFeedKeyPath],
		   FM.[Position]
	FROM [dbo].[FilterCriteria] FC WITH (NOLOCK)
	INNER JOIN [dbo].[Rule] R WITH (NOLOCK) ON R.[Id] = FC.[RuleId] AND R.[IsDeleted] = 0
	INNER JOIN [dbo].[FeedProvider] FP WITH (NOLOCK) ON FP.[id] = @FeedProviderId AND FP.[IsDeleted] = 0 AND R.[FeedProviderId] = FP.[id]
	INNER JOIN [dbo].[FeedMapping] FM WITH (NOLOCK) ON FM.[id] = FC.[FieldMappingId] AND FM.[FeedProviderId] = @FeedProviderId

	SELECT O.* 
	FROM [dbo].[Operation] O WITH (NOLOCK)
	INNER JOIN [dbo].[FeedProvider] FP WITH (NOLOCK) ON FP.[id] = @FeedProviderId AND FP.[IsDeleted] = 0 
	INNER JOIN [dbo].[Rule] R WITH (NOLOCK) ON R.[FeedProviderId] = FP.[id] AND R.Id = O.[RuleId]
	
END
GO
/****** Object:  StoredProcedure [dbo].[GetDisabilities]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- GetDisabilities 51.5073509,-0.1277583,30
CREATE PROCEDURE [dbo].[GetDisabilities]	
	@lat		DECIMAL(10,8),
	@long		DECIMAL(10,8),
	@radius		DECIMAL(10,8),
	@activity	NVARCHAR(MAX) = NULL,		
	@gender		NVARCHAR(50) = NULL,
	@mintime	BIGINT = NULL,
	@maxtime	BIGINT = NULL,	
	@minAge		BIGINT = NULL,
	@maxAge		BIGINT = NULL,	
	@weekdays	NVARCHAR(50) = NULL,
	@from		NVARCHAR(MAX) = NULL,
	@to			NVARCHAR(MAX) = NULL,
	@source		NVARCHAR(MAX) = NULL,
	@kind		NVARCHAR(MAX) = NULL,
	@tag		NVARCHAR(MAX) = NULL,
	@excludeTag NVARCHAR(MAX) = NULL,
	@minCost	DECIMAL(10,8) = NULL,	
	@maxCost	DECIMAL(10,8) = NULL,
	@agerange	NVARCHAR(MAX)	= NULL
AS
BEGIN	
	DECLARE @combinedString VARCHAR(MAX);
	
	DECLARE @min_Minute INT = DATEDIFF(minute, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(minute, '00:00:00', '23:59:59.999999');

	SELECT 
		@combinedString = COALESCE(@combinedString + ', ', '') + ISNULL(e.AccessibilitySupport,'')
	FROM [dbo].[Event] e WITH (NOLOCK)
	INNER JOIN [dbo].[Place] p ON p.EventId = e.Id	
	LEFT JOIN [dbo].[EventSchedule] es ON es.EventId = e.Id
	WHERE state = 'updated' 
	AND E.FeedId IS NOT NULL
	--AND ISNULL(p.Lat,'') <> '' AND ISNULL(p.Long,'') <> ''	
	AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL
	AND CONVERT(DECIMAL(10,8),p.Lat) >= @lat
	AND CONVERT(DECIMAL(10,8),p.Long) >= @long			
	--AND CONVERT(DECIMAL(10,8),IIF(TRY_PARSE(Lat AS DECIMAL(10,8)) IS NOT NULL,p.Lat,'0.0')) >= @lat
	--AND CONVERT(DECIMAL(10,8),IIF(TRY_PARSE(Lat AS DECIMAL(10,8)) IS NOT NULL,p.Long,'0.0')) >= @long
	--AND dbo.LatLonRadiusDistance(@lat,@long,CONVERT(DECIMAL(10,8),p.Lat) ,CONVERT(DECIMAL(10,8),p.Long)) <= @radius
	AND dbo.LatLonRadiusDistance(@lat, @long, p.Lat, p.Long) <= @radius
	AND DATEDIFF(minute, '00:00:00', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ISNULL(@mintime,@min_Minute)
	AND DATEDIFF(minute, '00:00:00', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ISNULL(@maxtime,@max_Minute)
	AND [dbo].[IsAgeRangeEligibleForGivenAge](e.Id,@minAge,@maxAge) = 1
	AND (ISNULL(@gender,'')='' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = @gender)
	AND (ISNULL(@from,'') = '' OR e.StartDate >= @from)
	AND (ISNULL(@to,'') = '' OR e.EndDate >= @to)	
	AND (ISNULL(@activity,'') = '' 
			OR E.Id IN 
			(
				SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	 	
				WHERE (PrefLabel IN (
						SELECT DISTINCT Item FROM (
							SELECT Item AS Item
							FROM dbo.SplitString(@activity, ',') 
						) AS tbl)
					)				
			)					
	)	
	AND (ISNULL(@weekdays,'') = '' 
		OR (
			[dbo].[IsEventEligibleForGivenWeekdays] (e.Id,@weekdays) = 1
		)
	)
	AND (ISNULL(@tag,'')  = ''					 
		OR (
				SELECT COUNT(*) FROM (
					SELECT * FROM (
						SELECT DISTINCT Item FROM (
							SELECT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
							FROM dbo.SplitString(e.Category, ',') 
						) AS Category
					)AS CategoryInTable
					WHERE CategoryInTable.Item IN (									
						SELECT DISTINCT Item FROM (
							SELECT Item AS Item
							FROM dbo.SplitString(@tag, ',')
						) AS Tag
						WHERE ISNULL(@excludeTag,'') = ''
							OR 
							Tag.Item NOT IN (
								SELECT DISTINCT Item FROM (
									SELECT Item AS Item
									FROM dbo.SplitString(@excludeTag, ',')
								) AS ExcludedTag
						)
					)
				) AS TotalCount 									
		) > 0
	)

	--SELECT @combinedString

	SELECT DISTINCT Item AS AccessibilitySupport FROM (
		SELECT RTRIM(LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', ''))) as Item
		FROM dbo.SplitString(@combinedString, ',') 
	) AS tbl
	WHERE ISNULL(Item,'') <> ''
END

GO
/****** Object:  StoredProcedure [dbo].[GetDisabilities_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- GetDisabilities_v1 51.5073509,-0.1277583,30
CREATE PROCEDURE [dbo].[GetDisabilities_v1]	
	@lat		DECIMAL(17, 14),
	@long		DECIMAL(17, 14),
	@radius		DECIMAL(17, 14),
	@activity	NVARCHAR(MAX) = NULL,		
	@gender		NVARCHAR(50) = NULL,
	@mintime	BIGINT = NULL,
	@maxtime	BIGINT = NULL,	
	@minAge		BIGINT = NULL,
	@maxAge		BIGINT = NULL,	
	@weekdays	NVARCHAR(50) = NULL,
	@from		NVARCHAR(MAX) = NULL,
	@to			NVARCHAR(MAX) = NULL,
	@source		NVARCHAR(MAX) = NULL,
	@kind		NVARCHAR(MAX) = NULL,
	@tag		NVARCHAR(MAX) = NULL,
	@excludeTag NVARCHAR(MAX) = NULL,
	@minCost	DECIMAL(10,8) = NULL,	
	@maxCost	DECIMAL(10,8) = NULL,
	@agerange	NVARCHAR(MAX)	= NULL
AS
BEGIN	
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	DECLARE @combinedString VARCHAR(MAX);
	
	DECLARE @min_Minute INT = DATEDIFF(MINUTE, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(MINUTE, '00:00:00', '23:59:59.999999');	

	BEGIN TRY
		SET @combinedString = (SELECT 
									DISTINCT STUFF((SELECT ',' + CAST(e.[AccessibilitySupport] AS NVARCHAR(MAX)) [text()] 
								FROM [dbo].[Event] e WITH (NOLOCK)
								INNER JOIN [dbo].[Place] p WITH (NOLOCK)
										ON p.[EventId] = e.[Id] AND e.[FeedId] IS NOT NULL AND e.[AccessibilitySupport] IS NOT NULL
										AND TRY_CAST(p.[Lat] as DECIMAL(10,8)) IS NOT NULL 
										AND TRY_CAST(p.[Long] as DECIMAL(10,8)) IS NOT NULL	
								LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.[EventId] = e.[Id]
								CROSS APPLY dbo.CIRCLEDISTANCE(@lat,@long,p.[Lat],p.[Long]) as C
								WHERE CAST(ISNULL(es.[StartDate],e.[StartDate]) AS DATETIME2)  < (DATEADD(WEEK,4,GETUTCDATE())) 
								AND C.[Distance] <= @radius
								AND DATEDIFF_BIG(MINUTE, '00:00:00', CAST(ISNULL(es.[StartTime],e.[StartDate]) AS TIME)) >= ISNULL(@mintime,@min_Minute)
								AND DATEDIFF_BIG(MINUTE, '00:00:00', CAST(ISNULL(es.[EndTime],e.[EndDate]) AS TIME)) <= ISNULL(@maxtime,@max_Minute)
								AND (ISNULL(@gender,'')='' OR E.[Gender] = @gender)
								AND (ISNULL(@from,'') = '' OR e.[StartDate] >= @from)
								AND (ISNULL(@to,'') = '' OR e.[EndDate] >= @to)	
								AND (ISNULL(@activity,'') = '' 
									OR E.[Id] IN 
									(
										SELECT DISTINCT [EventId] FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
										WHERE ',' + ISNULL(@activity,'') + ',' LIKE '%,' +[PrefLabel] + ',%'		
									)					
								)	
								AND (ISNULL(@weekdays,'') = '' 
									OR E.Id IN 
									(							
										SELECT DISTINCT [EventId] FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
										WHERE ',' + ISNULL(@weekdays,'') + ',' LIKE '%,' + CONVERT(NVARCHAR,dbo.WeekDay([WeekName])) + ',%'
									)	
								)
								AND (ISNULL(@tag,'')  = ''					 
									OR (
											SELECT COUNT(*) FROM 
											(
												SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
												FROM dbo.SplitString(e.[Category], ',') 
											)AS Category
											WHERE Category.Item IN (
												SELECT DISTINCT Item
												FROM dbo.SplitString(ISNULL(@tag,''), ',') 
												WHERE ISNULL(@excludeTag,'') = ''
													OR ',' + ISNULL(@excludeTag,'') + ',' NOT LIKE '%,' + Item + ',%'
											)									
									) > 0
								)
								AND 1 = (
									CASE
										WHEN (@minAge IS NULL AND @maxAge IS NULL)
										THEN 1
										WHEN (ISNULL(e.[MinAge],0) BETWEEN ISNULL(@minAge,0)
													AND ISNULL(@maxAge,200)) 
											OR (e.[MaxAge] IS NOT NULL AND e.MaxAge BETWEEN ISNULL(@minAge,0)
													AND ISNULL(@maxAge,200))
										THEN 1
										ELSE 0
									END
								)
								FOR XML PATH('')),1,1,''
							) AccessibilitySupport)

		SELECT DISTINCT Item AS AccessibilitySupport FROM (
			SELECT RTRIM(LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', ''))) as Item
			FROM dbo.SplitString(@combinedString, ',') 
		) AS tbl
		WHERE ISNULL(Item,'') <> ''
	PRINT 'Record fetched Successfully'
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);		
		SET @err_inner_exc += ', activity = '			+ ISNULL(@activity,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetDisabilities - sp(GetDisabilities_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetDisabilities_v1_07-03-2019]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- GetDisabilities_v1 51.5073509,-0.1277583,30
CREATE PROCEDURE [dbo].[GetDisabilities_v1_07-03-2019]	
	@lat		DECIMAL(10,8),
	@long		DECIMAL(10,8),
	@radius		DECIMAL(10,8),
	@activity	NVARCHAR(MAX) = NULL,		
	@gender		NVARCHAR(50) = NULL,
	@mintime	BIGINT = NULL,
	@maxtime	BIGINT = NULL,	
	@minAge		BIGINT = NULL,
	@maxAge		BIGINT = NULL,	
	@weekdays	NVARCHAR(50) = NULL,
	@from		NVARCHAR(MAX) = NULL,
	@to			NVARCHAR(MAX) = NULL,
	@source		NVARCHAR(MAX) = NULL,
	@kind		NVARCHAR(MAX) = NULL,
	@tag		NVARCHAR(MAX) = NULL,
	@excludeTag NVARCHAR(MAX) = NULL,
	@minCost	DECIMAL(10,8) = NULL,	
	@maxCost	DECIMAL(10,8) = NULL,
	@agerange	NVARCHAR(MAX)	= NULL
AS
BEGIN	
	DECLARE @combinedString VARCHAR(MAX);
	
	DECLARE @min_Minute INT = DATEDIFF(minute, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(minute, '00:00:00', '23:59:59.999999');	

	BEGIN TRY
		SET @combinedString = (SELECT 
									DISTINCT STUFF((SELECT ',' + CAST(e.AccessibilitySupport AS nvarchar(MAX)) [text()] FROM [dbo].[Event] e WITH (NOLOCK)
								INNER JOIN [dbo].[Place] p WITH (NOLOCK)
										ON p.EventId = e.Id 
										AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
										AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL	
								LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.EventId = e.Id
								CROSS APPLY dbo.CIRCLEDISTANCE(@lat,@long,p.Lat,p.Long) as C
								WHERE E.FeedId IS NOT NULL
								AND C.Distance <= @radius
								AND DATEDIFF_BIG(minute, '00:00:00', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ISNULL(@mintime,@min_Minute)
								AND DATEDIFF_BIG(minute, '00:00:00', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ISNULL(@maxtime,@max_Minute)
								AND (ISNULL(@gender,'')='' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = @gender)
								AND (ISNULL(@from,'') = '' OR e.StartDate >= @from)
								AND (ISNULL(@to,'') = '' OR e.EndDate >= @to)	
								AND (ISNULL(@activity,'') = '' 
									OR E.Id IN 
									(
										SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
										WHERE ',' + ISNULL(@activity,'') + ',' like '%,' +PrefLabel + ',%'		
									)					
								)	
								AND (ISNULL(@weekdays,'') = '' 
									OR E.Id IN (							
										SELECT DISTINCT EventId FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
										WHERE ',' + ISNULL(@weekdays,'') + ',' like '%,' + CONVERT(NVARCHAR,dbo.WeekDay(WeekName)) + ',%'
									)	
								)
								AND (ISNULL(@tag,'')  = ''					 
									OR (
											SELECT COUNT(*) FROM (
												SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
												FROM dbo.SplitString(e.Category, ',') 
											)AS Category
											WHERE Category.Item IN (
												SELECT DISTINCT Item
												FROM dbo.SplitString(ISNULL(@tag,''), ',') 
												WHERE ISNULL(@excludeTag,'') = ''
													OR ',' + ISNULL(@excludeTag,'') + ',' NOT LIKE '%,' + Item + ',%'
											)									
									) > 0
								)
								AND 1 = (
									CASE
										WHEN (@minAge IS NULL AND @maxAge IS NULL)
										THEN 1
										WHEN (ISNULL(e.MinAge,0) BETWEEN ISNULL(@minAge,0)
													AND ISNULL(@maxAge,200)) 
											OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ISNULL(@minAge,0)
													AND ISNULL(@maxAge,200))
										THEN 1
										ELSE 0
									END
								)
								FOR XML PATH('')),1,1,''
							) AccessibilitySupport)

		SELECT DISTINCT Item AS AccessibilitySupport FROM (
			SELECT RTRIM(LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', ''))) as Item
			FROM dbo.SplitString(@combinedString, ',') 
		) AS tbl
		WHERE ISNULL(Item,'') <> ''
	PRINT 'Record fetched Successfully'
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);		
		SET @err_inner_exc += ', activity = '			+ ISNULL(@activity,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetDisabilities - sp(GetDisabilities_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetEventActivityWise]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetEventActivityWise]	
	@activityName	NVARCHAR(50),
	@location		NVARCHAR(50),
	@lat			NVARCHAR(50),
	@long			NVARCHAR(50)
AS
BEGIN	
	SELECT 
		E.[id]
		,E.[FeedProviderId]
		,E.[FeedId]
		,E.[State]
		,E.[ModifiedDate]
		,dbo.UNIX_TIMESTAMP(ModifiedDate) as ModifiedDateTimestamp
		,E.[Name]
		,E.[Description]
		,E.[Image]
		,E.[ImageThumbnail]
		,E.[StartDate]
		,E.[EndDate]
		,E.[Duration]
		,E.[MaximumAttendeeCapacity]
		,E.[RemainingAttendeeCapacity]
		,E.[EventStatus]
		,E.[SuperEventId]
		,E.[Category]
		,E.[AgeRange]
		,E.[GenderRestriction]
		,E.[AttendeeInstructions]
		,E.[AccessibilitySupport]
		,E.[AccessibilityInformation]
		,E.[IsCoached]
		,E.[Level]
		,E.[MeetingPoint]
		,E.[Identifier]
		,E.[URL]
	FROM [dbo].[Event] E WITH (NOLOCK)
	WHERE E.[FeedId] IS NOT NULL
	AND E.[Id] IN 
	(
		SELECT pa.[EventId] FROM [dbo].[PhysicalActivity] pa WITH (NOLOCK)
		INNER JOIN [dbo].[Place] p WITH (NOLOCK) ON p.[EventId] = pa.[EventId]
		WHERE [PrefLabel] = @activityName
		AND p.[Address] LIKE '%' + @location + '%'
		AND p.[Lat] = @lat
		AND p.[Long] =  @long
	)	 
END

GO
/****** Object:  StoredProcedure [dbo].[GetEventById]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- [dbo].[GetEventById] 32181
CREATE PROCEDURE [dbo].[GetEventById]
	@eventId BIGINT
AS
BEGIN
	SELECT 
		e.[id]
		,e.[FeedProviderId]
		,LOWER(REPLACE(fp.[Name], ' ', '' )) AS FeedName
		,e.[FeedId]
		,e.[State]
		,e.[ModifiedDate]
		,e.[ModifiedDateTimestamp]
		,e.[Name]
		,e.[Description]
		,e.[Image]
		,e.[ImageThumbnail]		
		,CAST(ISNULL(es.[StartDate],e.[StartDate]) AS DATETIME) + cast(ISNULL(es.[StartTime],e.[StartDate]) AS DATETIME) AS StartDate
		,CAST(ISNULL(es.[EndDate],e.[EndDate]) AS DATETIME) + cast(ISNULL(es.[EndTime],e.[EndDate]) AS DATETIME) AS EndDate
		,REPLACE(e.[Duration],'-','') Duration
		,e.[MaximumAttendeeCapacity]
		,e.[RemainingAttendeeCapacity]
		,e.[EventStatus]
		,e.[SuperEventId]
		,e.[Category]		
		,e.[MinAge]
		,e.[MaxAge]
		,e.[Gender] AS GenderRestriction
		,e.[AttendeeInstructions]
		,e.[AccessibilitySupport]
		,e.[AccessibilityInformation]
		,e.[IsCoached]
		,e.[Level]
		,e.[MeetingPoint]
		,e.[Identifier]
		,e.[URL]			
		,(SELECT DISTINCT STUFF((SELECT ',' + CAST([PrefLabel] AS nvarchar(MAX)) [text()] FROM [dbo].[PhysicalActivity] WITH (NOLOCK) WHERE [EventId] = e.[Id] FOR XML PATH('')),1,1,'') Activities) AS Activity
		,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
		, NULL AS Distance
	FROM [dbo].[Event] e WITH (NOLOCK)
	INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.[Id] = e.[FeedProviderId] AND fp.[IsDeleted] = 0	
	LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.[EventId] = e.[Id]
	WHERE e.[id] = @eventId	
	AND e.[FeedId] IS NOT NULL	
END

GO
/****** Object:  StoredProcedure [dbo].[GetEventBySessionId]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--[dbo].[GetEventBySessionId] '27480622','gll'
CREATE PROCEDURE [dbo].[GetEventBySessionId]
	@sessionId NVARCHAR(50)
	,@FeedName NVARCHAR(100)
AS
BEGIN
	SELECT 
		e.[id]
		,e.[FeedProviderId]
		,LOWER(REPLACE(fp.[Name], ' ', '' )) AS FeedName
		,e.[FeedId]
		,e.[State]
		,e.[ModifiedDate]
		,e.[ModifiedDateTimestamp]			
		,e.[Name]
		,e.[Description]
		,e.[Image]
		,e.[ImageThumbnail]		
		,CAST(ISNULL(es.[StartDate],e.[StartDate]) AS DATETIME) + CAST(ISNULL(es.[StartTime],e.[StartDate]) AS DATETIME) AS StartDate
		,CAST(ISNULL(es.[EndDate],e.[EndDate]) AS DATETIME) + CAST(ISNULL(es.[EndTime],e.[EndDate]) AS DATETIME) AS EndDate
		,REPLACE(e.[Duration],'-','') Duration
		,e.[MaximumAttendeeCapacity]
		,e.[RemainingAttendeeCapacity]
		,e.[EventStatus]
		,e.[SuperEventId]
		,e.[Category]	
		,e.[MinAge]
		,e.[MaxAge]	
		,e.[Gender] AS GenderRestriction
		,e.[AttendeeInstructions]
		,e.[AccessibilitySupport]
		,e.[AccessibilityInformation]
		,e.[IsCoached]
		,e.[Level]
		,e.[MeetingPoint]
		,e.[Identifier]
		,e.[URL]		
		,(SELECT DISTINCT STUFF((SELECT ',' + CAST([PrefLabel] AS nvarchar(MAX)) [text()] FROM [dbo].[PhysicalActivity] WITH (NOLOCK) WHERE [EventId] = e.[Id] FOR XML PATH('')),1,1,'') Activities) AS Activity
		,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
		, NULL AS Distance
	FROM [dbo].[Event] e WITH (NOLOCK)	
	INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.[Id] = e.[FeedProviderId] AND fp.[IsDeleted] = 0
	LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.[EventId] = e.[Id]								
	WHERE e.[FeedId] = @sessionId
	AND  e.[FeedId] IS NOT NULL	
	AND LOWER(REPLACE(fp.[Name], ' ', '' )) = @FeedName
END

GO
/****** Object:  StoredProcedure [dbo].[GetEventData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetEventData]
	@EventId BIGINT,
	@IsSingleOccurrence BIT = 0
AS
BEGIN
	-- get subevents
	SELECT	[id]
			,[FeedProviderId]
			,[FeedId]
			,[State]
			,[ModifiedDate]
			,dbo.UNIX_TIMESTAMP(ModifiedDate) as ModifiedDateTimestamp
			,[Name]
			,[Description]
			,[Image]
			,[ImageThumbnail]
			,[StartDate]
			,[EndDate]
			,[Duration]
			,[MaximumAttendeeCapacity]
			,[RemainingAttendeeCapacity]
			,[EventStatus]
			,[SuperEventId]
			,[Category]
			,[AgeRange]
			,[GenderRestriction]
			,[AttendeeInstructions]
			,[AccessibilitySupport]
			,[AccessibilityInformation]
			,[IsCoached]
			,[Level]
			,[MeetingPoint]
			,[Identifier]
			,[URL] 
	FROM	[dbo].[Event] WITH (NOLOCK)
	WHERE	[SuperEventId] = @EventId
			AND [FeedId] IS NULL

	-- get superevent
	SELECT	[id]
			,[FeedProviderId]
			,[FeedId]
			,[State]
			,[ModifiedDate]
			,dbo.UNIX_TIMESTAMP([ModifiedDate]) as ModifiedDateTimestamp
			,[Name]
			,[Description]
			,[Image]
			,[ImageThumbnail]
			,[StartDate]
			,[EndDate]
			,[Duration]
			,[MaximumAttendeeCapacity]
			,[RemainingAttendeeCapacity]
			,[EventStatus]
			,[SuperEventId]
			,[Category]
			,[AgeRange]
			,[GenderRestriction]
			,[AttendeeInstructions]
			,[AccessibilitySupport]
			,[AccessibilityInformation]
			,[IsCoached]
			,[Level]
			,[MeetingPoint]
			,[Identifier]
			,[URL] 
	FROM	[dbo].[Event] WITH (NOLOCK)
	WHERE	[Id] = (SELECT [SuperEventId] FROM [Event] WITH (NOLOCK) WHERE [Id] = @EventId)
			AND [FeedId] IS NULL

	-- get place
	SELECT	P.[id]
			,P.[EventId]
			,P.[ParentId]
			,P.[PlaceTypeId]
			,PT.[Name] AS PlaceTypeName
			,P.[Name]
			,P.[Description]
			,P.[Image]
			,ISNULL(P.[Address],' ') AS Address
			,P.[Lat]
			,P.[Long]
			,P.[Telephone]
			,P.[FaxNumber]
			,P.[URL]
            ,p.[OpeningHoursSpecification] 
	FROM	[dbo].[Place] P WITH (NOLOCK)
	LEFT JOIN [dbo].[PlaceType] PT WITH (NOLOCK) ON P.[PlaceTypeId] = PT.[Id]
	WHERE	[EventId] = @EventId
	AND p.[Lat] IS NOT NULL AND p.[Long] IS NOT NULL
	
	-- get amenity feature
	SELECT	[Id]
			,[EventId]
			,[PlaceId]
			,[Type]
			,[Name]
			,[Value] 
	FROM	[dbo].[AmenityFeature] WITH (NOLOCK)
	WHERE	[EventId] = @EventId

	-- get physical activity
	SELECT  PA.[id]
			,PA.[EventId]
			,PA.[PrefLabel]
			,PA.[AltLabel]
			,PA.[InScheme]
			,PA.[Notation]
			,PA.[BroaderId]
			,PA.[NarrowerId]
            ,[PA].[Image]
            ,[PA].[Description]
			,PAB.[PrefLabel] BroaderPrefLabel
			,PAB.[AltLabel] BroaderAltLabel
			,PAB.[InScheme] BroaderInScheme
			,PAB.[Notation] BroaderNotation
			,PAN.[PrefLabel] NarrowerPrefLabel
			,PAN.[AltLabel] NarrowerAltLabel
			,PAN.[InScheme] NarrowerInScheme
			,PAN.[Notation] NarrowerNotation
	FROM	[dbo].[PhysicalActivity] PA WITH (NOLOCK)
	LEFT JOIN [dbo].[PhysicalActivity] PAB WITH (NOLOCK) ON PA.[BroaderId] = PAB.[id] AND PA.[EventId] = PAB.[EventId]
	LEFT JOIN [dbo].[PhysicalActivity] PAN WITH (NOLOCK) ON PA.[NarrowerId] = PAN.[id] AND PA.[EventId] = PAN.[EventId]
	WHERE	PA.[EventId] = @EventId

	-- get event schedule
	SELECT  [id]
			,[EventId]
			,[StartDate]
			,[EndDate]
			,[StartTime]
			,[EndTime]
			,[Frequency]
			,[ByDay]
			,[ByMonth]
			,[ByMonthDay]
			,[RepeatCount]
			,[RepeatFrequency]
	FROM	[dbo].[EventSchedule] WITH (NOLOCK)
	WHERE	[EventId] = @EventId

	-- get organization
	SELECT  [id]
			,[EventId]
			,[Name]
			,[Description]
			,[Email]
			,[Image]
			,[URL]
			,[Telephone] 
	FROM	[dbo].[Organization] WITH (NOLOCK)
	WHERE	[EventId] = @EventId

	-- get person
	SELECT  [id]
			,[EventId]
			,[IsLeader]
			,[Name]
			,[Description]
			,[Email]
			,[Image]
			,[URL]
			,[Telephone] 
	FROM	[dbo].[Person] WITH (NOLOCK)
	WHERE	[EventId] = @EventId

	-- get programme
	SELECT  [id]
			,[EventId]
			,[Name]
			,[Description]
			,[Image]
			,[URL]
	FROM	[dbo].[Programme] WITH (NOLOCK)
	WHERE	[EventId] = @EventId


	-- get occurrence
	IF @IsSingleOccurrence = 1
	BEGIN
		SELECT TOP 1  [id]
				,[EventId]
				,[SubEventId]
				,[StartDate]
				,[EndDate]			
		FROM	[dbo].[EventOccurrence] WITH (NOLOCK)
		WHERE	[EventId] = @EventId
		AND [StartDate] >= GETDATE()
	END
	ELSE
	BEGIN
		SELECT  [id]
				,[EventId]
				,[SubEventId]
				,[StartDate]
				,[EndDate]			
		FROM	[dbo].[EventOccurrence] WITH (NOLOCK)
		WHERE	[EventId] = @EventId
		AND [StartDate] >= GETDATE()
	END


	-- get custom data
	SELECT  [id]
			,[EventId]
			,[ColumnName]
			,[Value]
	FROM	[dbo].[CustomFeedData] WITH (NOLOCK)
	WHERE	[EventId] = @EventId
	
	-- get offer data
	SELECT [id]
          ,[SlotId]
		  ,[Name]
		  ,[Price]
		  ,[PriceCurrency]
		  ,[Identifier]
		  ,[Description]
	FROM [dbo].[Offer] WITH(NOLOCK)
	WHERE [EventId] = @EventId

    -- get Slot data
    SELECT [Id]
        ,[Identifier]
        ,[StartDate]
        ,[EndDate]
        ,[Duration]
        ,[OfferID]
        ,[RemainingUses]
        ,[MaximumUses]
    FROM [dbo].[Slot] WITH(NOLOCK)
    WHERE [EventID] = @EventId

    -- get FacilityUse data
    SELECT [Id],
            [ParentId],
            [TypeId],
            [URL],
            [Name],
            [Identifier],
            [Description],
            [Provider],
            [Image]
    FROM [dbo].[FacilityUse] WITH(NOLOCK)
    WHERE [EventId] = @EventId
END
GO
/****** Object:  StoredProcedure [dbo].[GetEvents]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- [dbo].[GetEvents] 
CREATE PROCEDURE [dbo].[GetEvents]
	@AfterTimestamp BIGINT = NULL,
	@AfterId		BIGINT = NULL
AS
BEGIN
	SELECT	E.[id]
			,E.[FeedProviderId]
			,E.[FeedId]
			,E.[State]
			,E.[ModifiedDate]
			,E.[ModifiedDateTimestamp]
			,E.[Name]
			,E.[Description]
			,E.[Image]
			,E.[ImageThumbnail]
			,E.[StartDate]
			,E.[EndDate]
			,E.[Duration]
			,E.[MaximumAttendeeCapacity]
			,E.[RemainingAttendeeCapacity]
			,E.[EventStatus]
			,E.[SuperEventId]
			,E.[Category]
			,E.[AgeRange]
			,E.[GenderRestriction]
			,E.[AttendeeInstructions]
			,E.[AccessibilitySupport]
			,E.[AccessibilityInformation]
			,E.[IsCoached]
			,E.[Level]
			,E.[MeetingPoint]
			,E.[Identifier]
			,E.[URL]
	FROM	[Event] E WITH (NOLOCK)
	INNER JOIN FeedProvider FP WITH (NOLOCK) ON E.[FeedProviderId] = FP.[id] AND FP.[IsDeleted] = 0
	WHERE	[FeedId] IS NOT NULL
			AND 
			(
				([ModifiedDateTimestamp] = NULL
					AND E.[id] > ISNULL(@AfterId, 0)
				)
				OR 
				([ModifiedDateTimestamp] > ISNULL(@AfterTimestamp, 0))
			)		    
	ORDER By [ModifiedDate], [id]
	OFFSET 0 * 10 ROWS
	FETCH NEXT 10 ROWS ONLY
END





GO
/****** Object:  StoredProcedure [dbo].[GetFeedDataTypes]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetFeedDataTypes]
	
AS
BEGIN
	
	SELECT	[Id]
			,[Name] 
	FROM	[dbo].[FeedDataType]

END
GO
/****** Object:  StoredProcedure [dbo].[GetFeedIntelligentMappingByTableName]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFeedIntelligentMappingByTableName]
	@FeedProviderId INT = NULL,
	@TableName		NVARCHAR(50) = NULL
AS
BEGIN	
	SELECT	* 
	FROM
	(
		SELECT	IM.id
				,IM.ParentId
				,IM.TableName
				,IM.ColumnName
				,CASE 
					WHEN FM.FeedKey IS NOT NULL THEN FM.FeedKey
					ELSE IM.PossibleMatches
				END AS PossibleMatches
				,IM.PossibleHierarchies
				,IM.CustomCriteria 
				,FM.id as FeedMappingId
				,FM.ParentId as FeedMappingParentId
				,ISNULL(FM.IsCustomFeedKey, 0) AS IsCustomFeedKey
				,FM.ColumnDataType
				,FM.FeedKey
				,FM.FeedKeyPath
				,FM.ActualFeedKeyPath
				,ISNULL(FM.IsDeleted,0) AS IsDeleted
				,ISNULL(FM.Position,IM.Position) AS Position
		FROM	IntelligentMapping IM WITH (NOLOCK)
		INNER JOIN FeedMapping FM ON IM.ColumnName = FM.ColumnName 
									AND IM.TableName = FM.TableName
									AND FM.FeedProviderId = ISNULL(@FeedProviderId, 0)
									AND FM.IsCustomFeedKey = 0
		WHERE	IM.IsDeleted = 0
				AND (@TableName IS NULL
					OR IM.TableName = @TableName)

		UNION ALL

		SELECT	FM.id
				,FM.ParentId
				,FM.TableName
				,FM.ColumnName
				,NULL as PossibleMatches
				,NULL as PossibleHierarchies
				,NULL as CustomCriteria 
				,FM.id as FeedMappingId
				,FM.ParentId as FeedMappingParentId
				,FM.IsCustomFeedKey
				,FM.ColumnDataType
				,FM.FeedKey
				,FM.FeedKeyPath
				,FM.ActualFeedKeyPath
				,0 as IsDeleted
				,FM.Position
		FROM	FeedMapping FM WITH (NOLOCK)
		WHERE	FM.FeedProviderId = ISNULL(@FeedProviderId, 0)
				AND FM.IsDeleted = 0
				AND FM.IsCustomFeedKey = 1
	) AS TBL
	ORDER BY ID
END
GO
/****** Object:  StoredProcedure [dbo].[GetFeedMappingByTableName]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetFeedMappingByTableName]
	@FeedProviderId INT,
	@TableName		NVARCHAR(50) = NULL
AS
BEGIN
	DECLARE @IsOpenActiveCompatible BIT

	SELECT @IsOpenActiveCompatible = IsOpenActiveCompatible FROM FeedProvider WITH (NOLOCK)
	WHERE id = @FeedProviderId AND IsDeleted = 0

	IF (ISNULL(@IsOpenActiveCompatible, 0) = 1
		AND (SELECT COUNT(*) FROM FeedMapping 
			WHERE	FeedProviderId = @FeedProviderId 
					AND (ISNULL(@TableName, '') = ''
						OR TableName = @TableName)) = 0)
	BEGIN
		SELECT	id
				,ParentId
				,FeedProviderId
				,TableName
				,ColumnName
				,IsCustomFeedKey
				,FeedKey
				,FeedKeyPath
				,ActualFeedKeyPath
				,[Constraint]
				,ColumnDataType
		FROM	FeedMapping WITH (NOLOCK)
		WHERE	IsDeleted = 0
				AND FeedProviderId = 0 -- default feedprovider id
				AND (ISNULL(@TableName, '') = ''
					OR TableName = @TableName)
				AND IsDeleted = 0
		
	END
	ELSE
	BEGIN
		SELECT	id
				,ParentId
				,FeedProviderId
				,TableName
				,ColumnName
				,IsCustomFeedKey
				,FeedKey
				,FeedKeyPath
				,ActualFeedKeyPath
				,[Constraint]
				,ColumnDataType
		FROM	FeedMapping WITH (NOLOCK)
		WHERE	IsDeleted = 0
				AND FeedProviderId = @FeedProviderId
				AND (ISNULL(@TableName, '') = ''
					OR TableName = @TableName)
	END
END


GO
/****** Object:  StoredProcedure [dbo].[GetFeedMappingDetail]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetFeedMappingDetail]
	@Id BIGINT
AS
BEGIN	
	SELECT	id
			,ParentId
			,FeedProviderId
			,TableName
			,ColumnName
			,IsCustomFeedKey
			,FeedKey
			,FeedKeyPath
			,ActualFeedKeyPath
			,[Constraint]
	FROM	FeedMapping WITH (NOLOCK)
	WHERE	ID = @Id
END
GO
/****** Object:  StoredProcedure [dbo].[GetFeedMappingDetailByTableColumnName]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFeedMappingDetailByTableColumnName]
	@FeedProviderId INT = NULL,
	@TableName		NVARCHAR(50),
	@ColumnName		NVARCHAR(50)
AS
BEGIN	
	SELECT	
		id
		,ParentId
		,FeedProviderId
		,TableName
		,ColumnName
		,IsCustomFeedKey
		,FeedKey
		,FeedKeyPath
		,ActualFeedKeyPath
		,[Constraint]
		,Position
	FROM FeedMapping WITH (NOLOCK)
	WHERE TableName = @TableName
	AND ColumnName = @ColumnName
	AND FeedProviderId = ISNULL(@FeedProviderId, 0)
	AND ISNULL(IsDeleted,0) = 0	
END

GO
/****** Object:  StoredProcedure [dbo].[GetFeedProviderDetail]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFeedProviderDetail]
	@FeedProviderId INT
AS
BEGIN
	SELECT FP.[id]
			,FP.[Name]
			,FP.[Source]
			,FP.[IsIminConnector]
			,FP.[FeedDataTypeId]
			,FDT.[Name] AS FeedDataTypeName
			,FP.[IsUsesTimestamp]
			,FP.[IsUtcTimestamp]
			,FP.[IsUsesChangenumber]
			,FP.[IsUsesUrlSlug]
			,FP.[EndpointUp]
			,FP.[UsesPagingSpec]
			,FP.[IsOpenActiveCompatible]
			,FP.[IncludesCoordinates]
			,FP.[IsFeedMappingConfirmed]
			,FP.[JSONTreeFileName]
			,FP.[SampleJSONFIleName]
			,FP.[JsonTreeWithDisabledKeysFileName]
	FROM	[dbo].[FeedProvider] FP WITH (NOLOCK)
	INNER JOIN [dbo].[FeedDataType] FDT ON FP.[FeedDataTypeId] = FDT.[Id]
	WHERE FP.[ID] = @FeedProviderId
	AND	FP.[IsDeleted] = 0
END
GO
/****** Object:  StoredProcedure [dbo].[GetFeedProviderDetailBySource]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetFeedProviderDetailBySource]
	@Source NVARCHAR(MAX)
AS
BEGIN	
	SELECT	FP.id
			,FP.Name
			,Source
			,IsIminConnector
			,FP.FeedDataTypeId
			,FDT.Name AS FeedDataTypeName
			,EndpointUp
			,UsesPagingSpec
			,IsOpenActiveCompatible
			,IncludesCoordinates
			,IsFeedMappingConfirmed
	FROM	[FeedProvider] FP WITH (NOLOCK)
	INNER JOIN FeedDataType FDT ON FP.FeedDataTypeId = FDT.Id
	WHERE	Source = @Source
	AND		IsDeleted = 0
END
GO
/****** Object:  StoredProcedure [dbo].[GetFeedProviders]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFeedProviders]
	@Offset			INT = 0,
	@pageSize		INT = 10,
	@sortColumnNo	INT = 0,
	@sortDirection	NVARCHAR(50) = 'ASC',
	@searchText		NVARCHAR(50) = ''
AS
BEGIN
	
	DECLARE @SQL AS NVARCHAR(MAX) = ''

	DECLARE @SortColumn AS NVARCHAR(255) = ''

	IF @sortColumnNo = 0 
	BEGIN
		SET @SortColumn = 'FP.Name'
	END
	ELSE IF @sortColumnNo = 1
	BEGIN
		SET @SortColumn = 'Fdt.Name'
	END
	ELSE IF @sortColumnNo = 2
	BEGIN
		SET @SortColumn = 'HasFoundAllFieldMatches'
	END
	ELSE IF @sortColumnNo = 3
	BEGIN
		SET @SortColumn = 'IsSchedulerEnabled'
	END
	ELSE IF @sortColumnNo = 4
	BEGIN
		SET @SortColumn = 'E.TotalEvent'
	END
	ELSE
	BEGIN
		SET @SortColumn = 'FP.Name'
	END

	SET @SQL = 'SELECT	FP.id
						,FP.Name
						,Source
						,IsIminConnector
						,FP.FeedDataTypeId
						,FDT.Name AS FeedDataTypeName	
						,EndpointUp
						,UsesPagingSpec
						,IsOpenActiveCompatible
						,IncludesCoordinates
						,CASE 
							WHEN (SELECT COUNT(*) 
									FROM FeedMapping FM 
									WHERE FM.FeedProviderId = FP.id ) = 0 
								THEN 0
							WHEN (SELECT COUNT(*) 
									FROM FeedMapping FM 
									WHERE FM.FeedProviderId = FP.id 
									AND FeedKey IS NULL) > 0 
								THEN 0
							ELSE 1
						END AS HasFoundAllFieldMatches
						,CASE 
							WHEN (SELECT COUNT(*) 
									FROM SchedulerSettings SS
									WHERE SS.FeedProviderId = FP.id AND SS.IsEnabled = 1) > 0  
								THEN 1
							ELSE 0
						END AS IsSchedulerEnabled,
						ISNULL(E.TotalEvent,0) AS TotalEvent
				FROM	[FeedProvider] FP WITH (NOLOCK)
				INNER JOIN FeedDataType FDT ON FP.FeedDataTypeId = FDT.Id
				LEFT JOIN (SELECT E.FeedProviderId, ISNULL(COUNT(1),0) TotalEvent FROM Event E WITH (NOLOCK) WHERE E.FeedId IS NOT NULL GROUP BY E.FeedProviderId) E ON FP.id = E.FeedProviderId
				WHERE	FP.IsDeleted = 0
						AND (
								''' + @searchText + ''' = ''''
								OR FP.Name LIKE ''%' + @searchText + '%''
								OR FP.Source LIKE ''%' + @searchText + '%''
							)
				ORDER BY ' + @SortColumn + ' ' + @sortDirection + '
				OFFSET (' + CONVERT(NVARCHAR, @Offset) + ')*  ' + CONVERT(NVARCHAR, @pageSize) + ' ROWS
				FETCH NEXT ' + CONVERT(NVARCHAR, @pageSize) + ' ROWS ONLY '

	PRINT @SQL
	EXEC SP_EXECUTESQL @SQL

	SELECT	COUNT(1) 
	FROM	FeedProvider
	WHERE	IsDeleted = 0

	SELECT	COUNT(1) 
	FROM	FeedProvider FP WITH (NOLOCK)
	WHERE	FP.IsDeleted = 0
			AND (
					@searchText = ''
					OR FP.Name LIKE '%' + @searchText + '%'
					OR FP.Source LIKE '%' + @searchText + '%'
				)

END
GO
/****** Object:  StoredProcedure [dbo].[GetFilterCriteriaByFeedMappingId]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFilterCriteriaByFeedMappingId]	
	@FeedProviderId BIGINT = NULL,
	@FeedMappingId BIGINT = NULL	
AS
BEGIN
	SELECT 
		 FC.[FilterCriteriaId]
		,FC.[ParentId]
		,FC.[RuleId]
		,R.FeedProviderId
		,R.RuleName		
		,FC.[OperatorId]
		,O.[Name] AS  OperatorName
		,O.OperatorExpression
		,FC.[Value]
		,FC.[OperationId] AS RuleOperatorId
		,RO.[Name] AS RuleOperatorName
		,FC.[FieldMappingId]
		,FM.TableName
		,FM.ColumnName
		,FM.ColumnDataType
		,FM.FeedKey
		,FM.FeedKeyPath
		,FM.ActualFeedKeyPath		
	FROM FilterCriteria FC WITH (NOLOCK)
	INNER JOIN [FeedMapping] FM WITH (NOLOCK) ON FM.id = FC.FieldMappingId AND FM.IsDeleted = 0 
	INNER JOIN [Rule] R WITH (NOLOCK) ON R.Id = FC.RuleId AND R.IsEnabled = 1
	LEFT JOIN [Operator] O WITH(NOLOCK) ON O.Id =  FC.OperatorId
	LEFT JOIN [RuleOperator] RO WITH(NOLOCK) ON RO.Id =  FC.OperationId
	WHERE (ISNULL(@FeedMappingId,0) = 0 OR FC.FieldMappingId = @FeedMappingId)
	AND (ISNULL(@FeedProviderId,0) = 0 OR FM.FeedProviderId = @FeedProviderId)

	SELECT	DISTINCT
			O.OperationId
		   ,O.RuleId
		   ,ISNULL(O.FieldId,0) AS FieldId
		   ,O.[Value]
		   ,O.CurrentWord
		   ,O.NewWord
		   ,O.Sentance
		   ,O.FirstFieldId
		   ,O.SecondFieldId
		   ,O.OperationTypeId
		   ,OT.[Name] AS OperationTypeName
		   ,FM.TableName
		   ,FM.ColumnName
		   ,FM.ColumnDataType
		   ,FM.FeedKey
		   ,FM.FeedKeyPath
		   ,FM.ActualFeedKeyPath
		   ,TFM.TableName AS TempTableName
		   ,TFM.ColumnName AS TempColumnName
		   ,TFM.ColumnDataType AS TempColumnDataType
		   ,TFM.FeedKey AS TempFeedKey
		   ,TFM.FeedKeyPath AS TempFeedKeyPath
		   ,TFM.ActualFeedKeyPath AS TempActualFeedKeyPath
		   ,TFM.ColumnDataType AS TempColumnDataType
		   ,FRFM.TableName AS TempFRTableName
		   ,FRFM.FeedKey AS TempFRFeedKey
		   ,FRFM.FeedKeyPath AS TempFRFeedKeyPath
		   ,FRFM.ActualFeedKeyPath AS TempFRActualFeedKeyPath
		   ,FRFM.ColumnDataType AS TempFRColumnDataType
		   ,FRFM.ColumnName AS TempFRColumnName
		   ,FRFM.ColumnDataType AS TempFRColumnDataType
		   ,SCFM.TableName AS TempSCTableName
		   ,SCFM.FeedKey AS TempSCFeedKey
		   ,SCFM.FeedKeyPath AS TempSCFeedKeyPath
		   ,SCFM.ActualFeedKeyPath AS TempSCActualFeedKeyPath		
		   ,SCFM.ColumnDataType AS TempSCColumnDataType    
		   ,SCFM.ColumnName AS TempSCColumnName 
		   ,SCFM.ColumnDataType AS TempSCColumnDataType 
	FROM Operation O WITH (NOLOCK)
	INNER JOIN OperationType OT WITH (NOLOCK) ON O.OperationTypeId = OT.OperationTypeId
	LEFT JOIN FilterCriteria FC WITH (NOLOCK) ON FC.FieldMappingId = O.FieldId
	LEFT JOIN [FeedMapping] FM WITH (NOLOCK) ON FM.id = O.FieldId AND FM.IsDeleted = 0 
	INNER JOIN [Rule] R WITH (NOLOCK) ON R.Id = O.RuleId AND R.IsEnabled = 1
	LEFT JOIN [FeedMapping] TFM WITH (NOLOCK) ON ISNUMERIC(O.Value) = 1 AND TFM.FeedProviderId = @FeedProviderId AND CAST(TFM.id AS NVARCHAR) = O.Value 
	LEFT JOIN [FeedMapping] FRFM WITH (NOLOCK) ON ISNUMERIC(O.FirstFieldId) = 1  AND FRFM.FeedProviderId = @FeedProviderId AND CAST(FRFM.id AS NVARCHAR) =O.FirstFieldId  
	LEFT JOIN [FeedMapping] SCFM WITH (NOLOCK) ON ISNUMERIC(O.SecondFieldId) = 1 AND FRFM.FeedProviderId = @FeedProviderId AND CAST(SCFM.id AS NVARCHAR) =  O.SecondFieldId
	WHERE (ISNULL(@FeedProviderId,0) = 0 OR R.FeedProviderId = @FeedProviderId)
	AND 
	(
		(ISNULL(@FeedMappingId,0) = 0 OR O.FieldId = @FeedMappingId)
		OR
		o.RuleId = r.Id
	)
	
END
GO
/****** Object:  StoredProcedure [dbo].[GetFilteredEvents]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[GetFilteredEvents] 51.5073509,-0.1277583,30,1,50,NULL,NULL,NULL,'start',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents] 51.5073509,-0.1277583,50,1,50,'Aqua Aerobics,Swimming Lessons',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents] 51.5073509,-0.1277583,50,1,50,'Aqua Aerobics,Swimming Lessons',NULL,'Female',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents] 51.5073509,-0.1277583,50,1,50,'Aqua Aerobics,Swimming Lessons',NULL,'Female',NULL,600,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents] 51.5073509,-0.1277583,50,1,50,'Aqua Aerobics,Swimming Lessons',NULL,NULL,NULL,600,1080,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents] 51.5073509,-0.1277583,50,1,50,'Aqua Aerobics,Swimming Lessons',NULL,'Female',NULL,600,1080,NULL,NULL,'1,2,3,4,5,6,7',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents] 51.5073509,-0.1277583,30,1,50,NULL,'Deaf',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents] 51.5326,-0.1154589,30,1,50



-- [dbo].[GetFilteredEvents] 51.5073509,-0.1277583,30,1,50,'Aqua Aerobics,Dance',NULL,'Mixed',NULL,600,1080,NULL,NULL,'1,2,3,4,5,6,7',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
CREATE PROCEDURE [dbo].[GetFilteredEvents]
	@lat				DECIMAL(10,8),
	@long				DECIMAL(10,8),
	@radius				DECIMAL(10,8),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,
	@activity			NVARCHAR(MAX) = NULL,	
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,
	@sortmode			NVARCHAR(50) = NULL,	
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,	
	@weekdays			NVARCHAR(50) = NULL,	
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,	
	@source				NVARCHAR(MAX) = NULL,
	@kind				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@agerange			NVARCHAR(MAX)	= NULL
AS
BEGIN
	DECLARE @SortColumn AS NVARCHAR(250)
	DECLARE @sql AS NVARCHAR(MAX);
	DECLARE @filtered_sql AS NVARCHAR(MAX); 
	DECLARE @total_counted_sql AS NVARCHAR(MAX); 

	DECLARE @min_Minute INT = DATEDIFF(minute, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(minute, '00:00:00', '23:59:59.999999');

	SET @SortColumn = IIF(@sortmode = 'start','StartDate','Distance');	

	--AND CONVERT(DECIMAL(10,8),p.Lat) >= ' + CONVERT(NVARCHAR,@lat) + '
	--AND CONVERT(DECIMAL(10,8),p.Long) >= ' + CONVERT(NVARCHAR,@long) + '

	DECLARE @activities NVARCHAR(MAX) = '';

	SET @sql = 'SELECT 
					e.id
					,e.FeedProviderId	
					,LOWER(REPLACE(fp.Name, '' '', '''' )) AS FeedName				
					,e.FeedId
					,e.State
					,e.ModifiedDate
					,dbo.UNIX_TIMESTAMP(ModifiedDate) as ModifiedDateTimestamp
					,e.Name
					,e.Description
					,e.Image
					,e.ImageThumbnail					
					,CAST(ISNULL(es.StartDate,e.StartDate) as datetime) + cast(ISNULL(es.StartTime,e.StartDate) as datetime) AS StartDate
					,CAST(ISNULL(es.EndDate,e.EndDate) as datetime) + cast(ISNULL(es.EndTime,e.EndDate) as datetime) AS EndDate
					,CAST(ISNULL(es.StartTime,e.StartDate) AS TIME) AS StartTime
					,CAST(ISNULL(es.EndTime,e.EndDate) AS TIME) AS EndTime
					,e.Duration
					,e.MaximumAttendeeCapacity
					,e.RemainingAttendeeCapacity
					,e.EventStatus
					,e.SuperEventId
					,e.Category					
					,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
					,e.AttendeeInstructions
					,e.AccessibilitySupport
					,e.AccessibilityInformation
					,e.IsCoached
					,e.Level
					,e.MeetingPoint
					,e.Identifier
					,e.URL
					,[dbo].[AgeRangeForGivenEvent](e.Id) AS AgeRange
					,dbo.LatLonRadiusDistance(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long)*1000 AS Distance
					,(SELECT DISTINCT STUFF((SELECT '','' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('''')),1,1,'''') Activities) AS Activity
					,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
				FROM [dbo].[Event] e WITH (NOLOCK)
				INNER JOIN [dbo].[Place] p ON p.EventId = e.Id		
				INNER JOIN [dbo].[FeedProvider] fp ON fp.Id = e.FeedProviderId 			
				LEFT JOIN [dbo].[EventSchedule] es ON es.EventId = e.Id								
				WHERE e.state = ''updated'' 
				AND e.FeedId IS NOT NULL	
				AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL
				AND dbo.LatLonRadiusDistance(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + '	,p.Lat ,p.Long) <= ' + CONVERT(NVARCHAR,@radius) + '				
				AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
				AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '
				AND ((''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' OR ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''') OR [dbo].[IsAgeRangeEligibleForGivenAge_v1](e.Id,' + IIF(@minAge IS NULL, 'NULL', CONVERT(NVARCHAR,@minAge)) + ',' + IIF(@maxAge IS NULL, 'NULL', CONVERT(NVARCHAR,@maxAge)) + ') = 1)
				AND (''' + ISNULL(@gender,'') + ''' = '''' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = '''+ISNULL(@gender,'')+''' )
				AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
				AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
				AND (''' + ISNULL(@activity,'') + ''' = '''' 
						OR E.Id IN 
						(
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)		
							WHERE (PrefLabel IN (
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@activity,'') + ''', '','') 
								  ) AS tbl)
							)
						)					
				)
				AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
					OR (
							SELECT COUNT(*) FROM (
								SELECT * FROM (
									SELECT DISTINCT ITEM FROM (
										SELECT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
										FROM dbo.SplitString(e.AccessibilitySupport, '','') 
									) AS AccessibilitySupport
								)AS AccessibilitySupportInTable
								WHERE AccessibilitySupportInTable.ITEM IN (
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@disabilitySupport,'') + ''', '','') 
									) AS DisabilitySupport
								)
							) AS TotalCount 									
					) > 0
				)
				AND (''' + ISNULL(@weekdays,'') + ''' = '''' 
					OR (
						[dbo].[IsEventEligibleForGivenWeekdays] (e.Id,''' + ISNULL(@weekdays,'') + ''') = 1
					)
				)
				AND (''' + ISNULL(@tag,'') + '''  = ''''					 
					OR (
							SELECT COUNT(*) FROM (
								SELECT * FROM (
									SELECT DISTINCT Item FROM (
										SELECT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
										FROM dbo.SplitString(e.Category, '','') 
									) AS Category
								)AS CategoryInTable
								WHERE CategoryInTable.Item IN (									
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','')
									) AS Tag
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = '''' 
										OR 
										Tag.Item NOT IN (
											SELECT DISTINCT Item FROM (
												SELECT Item AS Item
												FROM dbo.SplitString(''' + ISNULL(@excludeTag,'') + ''', '','')
											) AS ExcludedTag
									)
								)
							) AS TotalCount 									
					) > 0
				)
				AND (
							SELECT COUNT(*) FROM EventOccurrence WITH (NOLOCK) WHERE EventId = e.id AND StartDate >= GETDATE()							
					) > 0'

	SET @filtered_sql = @sql + ' ORDER BY ' + @SortColumn + ' ASC
								OFFSET ' + CONVERT(NVARCHAR,@limit) + ' * (' + CONVERT(NVARCHAR,@page) + ' - 1) ROWS
								FETCH NEXT ' + CONVERT(NVARCHAR,@limit) + ' ROWS ONLY'

	SET @total_counted_sql = 'SELECT COUNT(*) AS TotalCount FROM (' + @sql + ') AS Data'

	--PRINT (@sql)
	PRINT (@filtered_sql)
	--PRINT (@total_counted_sql)

	EXEC SP_EXECUTESQL @filtered_sql
	EXEC SP_EXECUTESQL @total_counted_sql
END

GO
/****** Object:  StoredProcedure [dbo].[GetFilteredEvents_old]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[GetFilteredEvents] NULL,1,50,51.5073509,-0.1277583,50,'Gym,Spinning',NULL,NULL,NULL,NULL,NULL,NULL,'start'

CREATE PROCEDURE [dbo].[GetFilteredEvents_old]
	@source NVARCHAR(50),
	@page BIGINT = 1,
	@limit BIGINT = 50,
	@lat DECIMAL(10,8),
	@long DECIMAL(10,8),
	@radius DECIMAL(10,8),
	@activity NVARCHAR(50),	
	@disabilitysupport NVARCHAR(50)= NULL,
	@mintime INT = NULL,
	@maxtime INT = NULL,
	@gender NVARCHAR(50) = NULL,
	@weekdays NVARCHAR(50) = NULL,
	@agerange NVARCHAR(50) = NULL,
	@sortmode NVARCHAR(50) = NULL
AS
BEGIN
	DECLARE @SortColumn AS NVARCHAR(250)
	DECLARE @sql AS NVARCHAR(MAX);
	DECLARE @filtered_sql AS NVARCHAR(MAX); 
	DECLARE @total_counted_sql AS NVARCHAR(MAX); 

	DECLARE @min_Minute INT = datediff(minute, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = datediff(minute, '00:00:00', '23:59:59.999999');

	SET @SortColumn = IIF(@sortmode = 'start','StartDate','Distance');	

	DECLARE @activities NVARCHAR(MAX) = '';

	SET @sql = 'SELECT 
					e.id
					,e.FeedProviderId
					,e.FeedId
					,e.State
					,e.ModifiedDate
					,dbo.UNIX_TIMESTAMP(ModifiedDate) as ModifiedDateTimestamp
					,e.Name
					,e.Description
					,e.Image
					,e.ImageThumbnail
					,e.StartDate
					,e.EndDate
					,e.Duration
					,e.MaximumAttendeeCapacity
					,e.RemainingAttendeeCapacity
					,e.EventStatus
					,e.SuperEventId
					,e.Category
					,e.AgeRange
					,e.GenderRestriction
					,e.AttendeeInstructions
					,e.AccessibilitySupport
					,e.AccessibilityInformation
					,e.IsCoached
					,e.Level
					,e.MeetingPoint
					,e.Identifier
					,e.URL
					,dbo.LatLonRadiusDistance(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',CONVERT(DECIMAL,p.Lat) ,CONVERT(DECIMAL,p.Long)) AS Distance
					,(Select distinct STUFF((SELECT '','' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WHERE EventId = e.Id FOR XML PATH('''')),1,1,'''') Activities) AS Activity
				FROM [dbo].[Event] e
				INNER JOIN [dbo].[Place] p ON p.EventId = e.Id									
				WHERE e.state = ''updated'' 
				AND e.FeedId IS NOT NULL				
				AND CONVERT(DECIMAL,p.Lat) >= ' + CONVERT(NVARCHAR,@lat) + '
				AND CONVERT(DECIMAL,p.Long) >= ' + CONVERT(NVARCHAR,@long) + '			
				AND dbo.LatLonRadiusDistance(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + '	,CONVERT(DECIMAL,p.Lat) ,CONVERT(DECIMAL,p.Long)) <= ' + CONVERT(NVARCHAR,@radius) + '				
				AND E.Id IN 
				(
					SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity]		
					WHERE (''' + ISNULL(@activity,'') + ''' = '''' 
							OR (PrefLabel IN (
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@activity,'') + ''', '','') 
									) AS tbl ) 
								)
						     )				
				)
				AND (						
						e.GenderRestriction = '''+ISNULL(@gender,'')+''' 
						OR DATEDIFF(minute, ''00:00:00'', CAST(e.StartDate AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
						OR DATEDIFF(minute, ''00:00:00'', CAST(e.EndDate AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '
						OR DATEPART(dw, e.StartDate) LIKE ''%' + ISNULL(@weekdays,'') + '%''
						OR e.AccessibilitySupport LIKE ''%' + ISNULL(@disabilitysupport,'') + '%''
						OR e.AgeRange LIKE ''%' + ISNULL(@agerange,'') + '%''
						OR ''||'' = ''||''
				)'

	SET @filtered_sql = @sql + ' ORDER BY ' + @SortColumn + ' ASC
								OFFSET ' + CONVERT(NVARCHAR,@limit) + ' * (' + CONVERT(NVARCHAR,@page) + ' - 1) ROWS
								FETCH NEXT ' + CONVERT(NVARCHAR,@limit) + ' ROWS ONLY'

	SET @total_counted_sql = 'SELECT COUNT(*) AS TotalCount FROM (' + @sql + ') AS Data'


	--PRINT (@filtered_sql)
	--PRINT (@total_counted_sql)

	EXEC SP_EXECUTESQL @filtered_sql
	EXEC SP_EXECUTESQL @total_counted_sql
	
	
	--(
	--	SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity]		
	--	WHERE (''' + ISNULL(@activity,'') + ''' = '''' OR PrefLabel = ''' + ISNULL(@activity,'') + ''')					
	--)



	--SET @sql1 = 'SELECT 
	--				E.id
	--				,E.FeedProviderId
	--				,E.FeedId
	--				,E.State
	--				,E.ModifiedDate
	--				,dbo.UNIX_TIMESTAMP(E.ModifiedDate) as ModifiedDateTimestamp
	--				,E.Name
	--				,E.Description
	--				,E.Image
	--				,E.ImageThumbnail
	--				,E.StartDate
	--				,E.EndDate
	--				,E.Duration
	--				,E.MaximumAttendeeCapacity
	--				,E.RemainingAttendeeCapacity
	--				,E.EventStatus
	--				,E.SuperEventId
	--				,E.Category
	--				,E.AgeRange
	--				,E.GenderRestriction
	--				,E.AttendeeInstructions
	--				,E.AccessibilitySupport
	--				,E.AccessibilityInformation
	--				,E.IsCoached
	--				,E.Level
	--				,E.MeetingPoint
	--				,E.Identifier
	--				,E.URL
	--			FROM [dbo].[Event] E				
	--			WHERE state = ''updated'' 
	--			AND E.FeedId IS NOT NULL
	--			AND E.Id IN 
	--			(
	--				Select pa.EventId From [dbo].[PhysicalActivity] pa
	--				INNER JOIN [dbo].[Place] p ON p.EventId = pa.EventId
	--				Where CONVERT(DECIMAL,p.Lat) >= CONVERT(DECIMAL,''' + @lat + ''')
	--				AND CONVERT(DECIMAL,p.Long) >= CONVERT(DECIMAL,''' + @long + ''')					
	--				AND dbo.LatLonRadiusDistance(CONVERT(DECIMAL,''' + @lat + '''),CONVERT(DECIMAL,''' + @long + '''),CONVERT(DECIMAL,p.Lat) ,CONVERT(DECIMAL,p.Long)) <= ' + CONVERT(NVARCHAR,@radius) + '
	--			)
	--			ORDER BY 1
	--			OFFSET ' + CONVERT(NVARCHAR,@recordperpage) + ' * (' + CONVERT(NVARCHAR,@page) + ' - 1) ROWS
	--			FETCH NEXT ' + CONVERT(NVARCHAR,@recordperpage) + ' ROWS ONLY'

	--PRINT (@Sql1)
	--EXEC SP_EXECUTESQL @Sql1	
END

GO
/****** Object:  StoredProcedure [dbo].[GetFilteredEvents_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- [dbo].[GetFilteredEvents_v1] 51.5073509,-0.1277583,30,1,50,'Aqua Aerobics,Dance',NULL,'Mixed',NULL,600,1080,15,90,'1,2,3,4,5,6,7',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents_v1] 51.5073509,-0.1277583,30,1,50,NULL,NULL,'male',NULL,600,1080,NULL,12,'1,2',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents_v1] 51.50735090,-0.12775830,30,1,50
-- [dbo].[GetFilteredEvents_v1] 51.61885000,-0.16261100,10,1,50
CREATE PROCEDURE [dbo].[GetFilteredEvents_v1]
	@lat				DECIMAL(17,14),
	@long				DECIMAL(17,14),
	@radius				DECIMAL(17,14),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,
	@activity			NVARCHAR(MAX) = NULL,	
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,
	@sortmode			NVARCHAR(50) = 'start',	
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,	
	@weekdays			NVARCHAR(50) = NULL,	
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,	
	@source				NVARCHAR(MAX) = NULL,
	@kind				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@agerange			NVARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL SERIALIZABLE

	DECLARE @SortColumn AS NVARCHAR(250)
	DECLARE @sql AS NVARCHAR(MAX);
	DECLARE @filtered_sql AS NVARCHAR(MAX); 
	DECLARE @total_counted_sql AS NVARCHAR(MAX); 	

	DECLARE @min_Minute INT = DATEDIFF(MINUTE, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(MINUTE, '00:00:00', '23:59:59.999999');
		
	SET @SortColumn = IIF(@sortmode = 'start','StartDate','Distance');	

	SET @weekdays = IIF(@weekdays='1,2,3,4,5,6,7','',@weekdays);
	
	SET @activity = LTRIM(RTRIM(REPLACE(ISNULL(@activity,''), '''','''''')));
	BEGIN TRY
		SET @sql = 'SELECT 
						e.id
						,e.FeedProviderId
						,LOWER(REPLACE(fp.Name, '' '', '''' )) AS FeedName				
						,e.FeedId
						,e.State
						,e.ModifiedDate
						,e.ModifiedDateTimestamp						
						,e.Name
						,e.Description
						,e.Image
						,e.ImageThumbnail					
						,CAST(ISNULL(es.StartDate,e.StartDate) as datetime) + cast(ISNULL(es.StartTime,e.StartDate) as datetime) AS StartDate
						,CAST(ISNULL(es.EndDate,e.EndDate) as datetime) + cast(ISNULL(es.EndTime,e.EndDate) as datetime) AS EndDate
						,CAST(ISNULL(es.StartTime,e.StartDate) AS TIME) AS StartTime
						,CAST(ISNULL(es.EndTime,e.EndDate) AS TIME) AS EndTime
						,REPLACE(e.Duration,''-'','''') Duration
						,e.MaximumAttendeeCapacity
						,e.RemainingAttendeeCapacity
						,e.EventStatus
						,e.SuperEventId
						,e.Category		
						,e.MinAge
						,e.MaxAge						
						,e.Gender AS GenderRestriction
						,e.AttendeeInstructions
						,e.AccessibilitySupport
						,e.AccessibilityInformation
						,e.IsCoached
						,e.Level
						,e.MeetingPoint
						,e.Identifier
						,e.URL					
						,ROUND(C.Distance*1000,2) AS Distance
						,(SELECT DISTINCT STUFF((SELECT '','' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('''')),1,1,'''') Activities) AS Activity
						,(SELECT [dbo].[WeekDaysForGivenEvent_v1](e.Id)) AS WeekDays
					FROM [dbo].[Event] e WITH (NOLOCK)
					INNER JOIN [dbo].[Place] p WITH (NOLOCK)
									ON e.FeedId IS NOT NULL AND p.EventId = e.Id 
									AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
									AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL		
					INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = e.FeedProviderId AND fp.IsDeleted = 0			
					LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.EventId = e.Id
					CROSS APPLY dbo.CIRCLEDISTANCE(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long) AS C
					CROSS APPLY(SELECT COUNT(1) AS EventOccurrenceCount FROM EventOccurrence eo WITH (NOLOCK) WHERE eo.EventId=e.id AND  eo.StartDate >= GETDATE()) AS EOC
					WHERE CAST(ISNULL(es.StartDate,e.StartDate) AS DATETIME2)  < (DATEADD(WEEK,4,GETUTCDATE()))
					AND ( ISNULL(EOC.EventOccurrenceCount,0) ) > 0
					AND C.Distance <= ' + CONVERT(NVARCHAR,@radius) + '				
					AND DATEDIFF_BIG(MINUTE, ''00:00:00'', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
					AND DATEDIFF_BIG(MINUTE, ''00:00:00'', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '								
					AND (''' + ISNULL(@gender,'') + ''' = '''' OR e.Gender = '''+ISNULL(@gender,'')+''' )
					AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
					AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
					AND (''' + ISNULL(@activity,'') + ''' = ''''
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@activity,'') + ','' like ''%,''+PrefLabel+'',%''
						)					
					)
					AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
						OR (										
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.AccessibilitySupport, '','') 
								)AS AccessibilitySupports
								WHERE '',' + ISNULL(@disabilitySupport,'') + ','' LIKE ''%,'' + AccessibilitySupports.ITEM + '',%''
						) > 0
					)							
					AND (''' + ISNULL(@weekdays,'') + ''' = '''' 
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@weekdays,'') + ','' like ''%,'' + CONVERT(NVARCHAR,dbo.WeekDay(WeekName)) + '',%''
						)					
					)								
					AND (''' + ISNULL(@tag,'') + '''  = ''''
						OR (
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.Category, '','') 
								)AS Category
								WHERE Category.Item IN (
									SELECT DISTINCT Item
									FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','') 
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = ''''  
										OR '',' + ISNULL(@excludeTag,'') + ','' NOT LIKE ''%,'' + Item + '',%''
								)									
						) > 0
					)							
					AND 1 = (
						CASE
							WHEN (''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' 
									AND ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''')
							THEN 1
							WHEN (ISNULL(e.MinAge,0) BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + ' 
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ') 
								OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + '  
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ')
							THEN 1
							ELSE 0
						END
					)'

		SET @filtered_sql = @sql + ' ORDER BY ' + @SortColumn + ' ASC
							OFFSET ' + CONVERT(NVARCHAR,@limit) + ' * (' + CONVERT(NVARCHAR,@page) + ' - 1) ROWS
							FETCH NEXT ' + CONVERT(NVARCHAR,@limit) + ' ROWS ONLY'
		SET @total_counted_sql = 'SELECT COUNT(1) AS TotalCount FROM (' + @sql + ') AS Data'		

		EXEC SP_EXECUTESQL @filtered_sql
		EXEC SP_EXECUTESQL @total_counted_sql
		
		PRINT 'Record fetched Successfully'
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', page = '				+ CAST(@page AS NVARCHAR);
		SET @err_inner_exc += ', limit = '				+ CAST(@limit AS NVARCHAR);
		SET @err_inner_exc += ', sortmode = '			+ @SortColumn;
		SET @err_inner_exc += ', activity = '			+ ISNULL(@activity,'');
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetEventsDynamically - sp(GetFilteredEvents_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[GetFilteredEvents_v1_New]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFilteredEvents_v1_New]
	@lat				DECIMAL(17,14),
	@long				DECIMAL(17,14),
	@radius				DECIMAL(17,14),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,
	@activity			NVARCHAR(MAX) = NULL,	
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,
	@sortmode			NVARCHAR(50) = NULL,	
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,	
	@weekdays			NVARCHAR(50) = NULL,	
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,	
	@source				NVARCHAR(MAX) = NULL,
	@kind				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@agerange			NVARCHAR(MAX) = NULL
AS
BEGIN
	DECLARE @SortColumn AS NVARCHAR(250)
	DECLARE @sql AS NVARCHAR(MAX);
	DECLARE @filtered_sql AS NVARCHAR(MAX); 
	DECLARE @total_counted_sql AS NVARCHAR(MAX); 	

	DECLARE @min_Minute INT = DATEDIFF(MINUTE, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(MINUTE, '00:00:00', '23:59:59.999999');
		
	SET @SortColumn = IIF(@sortmode = 'start','StartDate','Distance');	

	SET @weekdays = IIF(@weekdays='1,2,3,4,5,6,7','',@weekdays);
	
	SET @activity = LTRIM(RTRIM(REPLACE(ISNULL(@activity,''), '''','''''')));
	BEGIN TRY

		SET @sql = 'SELECT 
						e.id
						,e.FeedProviderId
						,LOWER(REPLACE(fp.Name, '' '', '''' )) AS FeedName				
						,e.FeedId
						,e.State
						,e.ModifiedDate
						,e.ModifiedDateTimestamp								
						,e.Name
						,e.Description
						,e.Image
						,e.ImageThumbnail					
						,CAST(ISNULL(es.StartDate,e.StartDate) as datetime) + cast(ISNULL(es.StartTime,e.StartDate) as datetime) AS StartDate
						,CAST(ISNULL(es.EndDate,e.EndDate) as datetime) + cast(ISNULL(es.EndTime,e.EndDate) as datetime) AS EndDate
						,CAST(ISNULL(es.StartTime,e.StartDate) AS TIME) AS StartTime
						,CAST(ISNULL(es.EndTime,e.EndDate) AS TIME) AS EndTime
						,REPLACE(e.Duration,''-'','''') Duration
						,e.MaximumAttendeeCapacity
						,e.RemainingAttendeeCapacity
						,e.EventStatus
						,e.SuperEventId
						,e.Category		
						,e.MinAge
						,e.MaxAge						
						,e.Gender AS GenderRestriction
						,e.AttendeeInstructions
						,e.AccessibilitySupport
						,e.AccessibilityInformation
						,e.IsCoached
						,e.Level
						,e.MeetingPoint
						,e.Identifier
						,e.URL					
						,ROUND(C.Distance*1000,2) AS Distance
						,(SELECT DISTINCT STUFF((SELECT '','' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('''')),1,1,'''') Activities) AS Activity
						,(SELECT [dbo].[WeekDaysForGivenEvent_v1](e.Id)) AS WeekDays
					FROM [dbo].[Event] e WITH (NOLOCK)
					INNER JOIN [dbo].[Place] p WITH (NOLOCK)
									ON p.EventId = e.Id AND e.FeedId IS NOT NULL
									AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
									AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL		
					INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = e.FeedProviderId AND fp.IsDeleted = 0			
					LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.EventId = e.Id
					CROSS APPLY dbo.CIRCLEDISTANCE(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long) AS C
					OUTER APPLY(SELECT COUNT(1) AS EventOccurrenceCount FROM EventOccurrence eo WITH (NOLOCK) WHERE eo.EventId=e.id AND  eo.StartDate >= GETDATE()) AS EOC
					WHERE CAST(ISNULL(es.StartDate,e.StartDate) as datetime) + CAST(ISNULL(es.StartTime,e.StartDate) as datetime) < (DATEADD(WEEK,2,GETUTCDATE()))
					AND(ISNULL(EOC.EventOccurrenceCount,0)) > 0	
					AND C.Distance <= ' + CONVERT(NVARCHAR,@radius) + '				
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '								
					AND (''' + ISNULL(@gender,'') + ''' = '''' OR e.Gender = '''+ISNULL(@gender,'')+''' )
					AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
					AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
					AND (''' + ISNULL(@activity,'') + ''' = ''''
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@activity,'') + ','' like ''%,''+PrefLabel+'',%''
						)					
					)
					AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
						OR (										
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.AccessibilitySupport, '','') 
								)AS AccessibilitySupports
								WHERE '',' + ISNULL(@disabilitySupport,'') + ','' LIKE ''%,'' + AccessibilitySupports.ITEM + '',%''
						) > 0
					)							
					AND (''' + ISNULL(@weekdays,'') + ''' = '''' 
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@weekdays,'') + ','' like ''%,'' + CONVERT(NVARCHAR,dbo.WeekDay(WeekName)) + '',%''
						)					
					)								
					AND (''' + ISNULL(@tag,'') + '''  = ''''
						OR (
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.Category, '','') 
								)AS Category
								WHERE Category.Item IN (
									SELECT DISTINCT Item
									FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','') 
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = ''''  
										OR '',' + ISNULL(@excludeTag,'') + ','' NOT LIKE ''%,'' + Item + '',%''
								)									
						) > 0
					)							
					AND 1 = (
						CASE
							WHEN (''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' 
									AND ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''')
							THEN 1
							WHEN (ISNULL(e.MinAge,0) BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + ' 
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ') 
								OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + '  
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ')
							THEN 1
							ELSE 0
						END
					)'
		Print @sql
		SET @filtered_sql = @sql + ' ORDER BY ' + @SortColumn + ' ASC
							OFFSET ' + CONVERT(NVARCHAR,@limit) + ' * (' + CONVERT(NVARCHAR,@page) + ' - 1) ROWS
							FETCH NEXT ' + CONVERT(NVARCHAR,@limit) + ' ROWS ONLY'
		
		SET @total_counted_sql='SELECT COUNT(1) AS TotalCount						
					FROM [dbo].[Event] e WITH (NOLOCK)
					INNER JOIN [dbo].[Place] p WITH (NOLOCK)
									ON p.EventId = e.Id AND e.FeedId IS NOT NULL
									AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
									AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL		
					INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = e.FeedProviderId AND fp.IsDeleted = 0		
					LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.EventId = e.Id
					CROSS APPLY dbo.CIRCLEDISTANCE(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long) AS C
					OUTER APPLY(SELECT COUNT(1) AS EventOccurrenceCount FROM EventOccurrence eo WITH (NOLOCK) WHERE eo.EventId=e.id AND  eo.StartDate >= GETDATE() ) AS EOC
					WHERE CAST(ISNULL(es.StartDate,e.StartDate) as datetime) + CAST(ISNULL(es.StartTime,e.StartDate) as datetime) < (DATEADD(WEEK,2,GETUTCDATE()))
					AND(ISNULL(EOC.EventOccurrenceCount,0)) > 0
					AND C.Distance <= ' + CONVERT(NVARCHAR,@radius) + '				
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '								
					AND (''' + ISNULL(@gender,'') + ''' = '''' OR e.Gender = '''+ISNULL(@gender,'')+''' )
					AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
					AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
					AND (''' + ISNULL(@activity,'') + ''' = ''''
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@activity,'') + ','' like ''%,''+PrefLabel+'',%''
						)					
					)
					AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
						OR (										
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.AccessibilitySupport, '','') 
								)AS AccessibilitySupports
								WHERE '',' + ISNULL(@disabilitySupport,'') + ','' LIKE ''%,'' + AccessibilitySupports.ITEM + '',%''
						) > 0
					)							
					AND (''' + ISNULL(@weekdays,'') + ''' = '''' 
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@weekdays,'') + ','' like ''%,'' + CONVERT(NVARCHAR,dbo.WeekDay(WeekName)) + '',%''
						)					
					)								
					AND (''' + ISNULL(@tag,'') + '''  = ''''
						OR (
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.Category, '','') 
								)AS Category
								WHERE Category.Item IN (
									SELECT DISTINCT Item
									FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','') 
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = ''''  
										OR '',' + ISNULL(@excludeTag,'') + ','' NOT LIKE ''%,'' + Item + '',%''
								)									
						) > 0
					)								
					AND 1 = (
						CASE
							WHEN (''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' 
									AND ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''')
							THEN 1
							WHEN (ISNULL(e.MinAge,0) BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + ' 
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ') 
								OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + '  
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ')
							THEN 1
							ELSE 0
						END
					)';

		--PRINT (@sql)
		--PRINT (@filtered_sql)
		--PRINT (@total_counted_sql)

		EXEC SP_EXECUTESQL @filtered_sql;
		EXEC SP_EXECUTESQL @total_counted_sql;
		
		PRINT 'Record fetched Successfully';
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', page = '				+ CAST(@page AS NVARCHAR);
		SET @err_inner_exc += ', limit = '				+ CAST(@limit AS NVARCHAR);
		SET @err_inner_exc += ', sortmode = '			+ @SortColumn;
		SET @err_inner_exc += ', activity = '			+ ISNULL(@activity,'');
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetEventsDynamically - sp(GetFilteredEvents_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetFilteredEvents_v1_old]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[GetFilteredEvents_v2] 51.5073509,-0.1277583,30,1,50,'Aqua Aerobics,Dance',NULL,'Mixed',NULL,600,1080,15,90,'1,2,3,4,5,6,7',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents_v1] 51.5073509,-0.1277583,30,3,500,NULL,NULL,'Mixed',NULL,600,1080,NULL,12,'1,2',NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
-- [dbo].[GetFilteredEvents_v1] 51.50735090,-0.12775830,30,560,50
-- [dbo].[GetFilteredEvents_v2] 51.5073509,-0.1277583,30,5,50
CREATE PROCEDURE [dbo].[GetFilteredEvents_v1_old]
	@lat				DECIMAL(10,8),
	@long				DECIMAL(10,8),
	@radius				DECIMAL(10,8),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,
	@activity			NVARCHAR(MAX) = NULL,	
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,
	@sortmode			NVARCHAR(50) = NULL,	
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,	
	@weekdays			NVARCHAR(50) = NULL,	
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,	
	@source				NVARCHAR(MAX) = NULL,
	@kind				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@agerange			NVARCHAR(MAX) = NULL
AS
BEGIN
	DECLARE @SortColumn AS NVARCHAR(250)
	DECLARE @sql AS NVARCHAR(MAX);
	DECLARE @filtered_sql AS NVARCHAR(MAX); 
	DECLARE @total_counted_sql AS NVARCHAR(MAX);
	DECLARE @EventID AS NVARCHAR(MAX)='';
	DECLARE @Sql_For_Event AS NVARCHAR(MAX); 	

	DECLARE @min_Minute INT = DATEDIFF(minute, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(minute, '00:00:00', '23:59:59.999999');
		
	SET @SortColumn = IIF(@sortmode = 'start','StartDate','Distance');	

	SET @activity=REPLACE(ISNULL(@activity,''), '''', '');

	BEGIN TRY
		SET @sql = 'SELECT 
						e.id
						,e.FeedProviderId
						,LOWER(REPLACE(fp.Name, '' '', '''' )) AS FeedName				
						,e.FeedId
						,e.State
						,e.ModifiedDate
						,e.ModifiedDateTimestamp
						--,dbo.UNIX_TIMESTAMP(ModifiedDate) as ModifiedDateTimestamp					
						,e.Name
						,e.Description
						,e.Image
						,e.ImageThumbnail					
						,CAST(ISNULL(es.StartDate,e.StartDate) as datetime) + cast(ISNULL(es.StartTime,e.StartDate) as datetime) AS StartDate
						,CAST(ISNULL(es.EndDate,e.EndDate) as datetime) + cast(ISNULL(es.EndTime,e.EndDate) as datetime) AS EndDate
						,CAST(ISNULL(es.StartTime,e.StartDate) AS TIME) AS StartTime
						,CAST(ISNULL(es.EndTime,e.EndDate) AS TIME) AS EndTime
						,REPLACE(e.Duration,''-'','''') Duration
						,e.MaximumAttendeeCapacity
						,e.RemainingAttendeeCapacity
						,e.EventStatus
						,e.SuperEventId
						,e.Category		
						,e.MinAge
						,e.MaxAge
						--,CAST(ISNULL(e.MinAge,0) AS NVARCHAR(10)) + IIF(e.MaxAge IS NOT NULL,(''-'' + CAST(e.MaxAge AS NVARCHAR(10))),'''') AS AgeRange			
						,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
						,e.AttendeeInstructions
						,e.AccessibilitySupport
						,e.AccessibilityInformation
						,e.IsCoached
						,e.Level
						,e.MeetingPoint
						,e.Identifier
						,e.URL					
						,ROUND(C.Distance*1000,2) AS Distance
						,(SELECT DISTINCT STUFF((SELECT '','' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('''')),1,1,'''') Activities) AS Activity
						,(SELECT [dbo].[WeekDaysForGivenEvent_v1](e.Id)) AS WeekDays
						,ISNULL(EOC.EventOccurrenceCount,0) AS EventOccurrenceCount
					FROM [dbo].[Event] e WITH (NOLOCK)
					INNER JOIN [dbo].[Place] p WITH (NOLOCK)
									ON p.EventId = e.Id 
									AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
									AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL		
					INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = e.FeedProviderId AND fp.IsDeleted = 0		
					LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.EventId = e.Id
					CROSS APPLY dbo.CIRCLEDISTANCE(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long) AS C
					OUTER APPLY(SELECT COUNT(1) AS EventOccurrenceCount FROM EventOccurrence eo WITH (NOLOCK) WHERE eo.EventId=e.id AND  eo.StartDate >= GETDATE() ) AS EOC
					WHERE e.state = ''updated'' 
					AND e.FeedId IS NOT NULL
					AND C.Distance <= ' + CONVERT(NVARCHAR,@radius) + '				
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '								
					AND (''' + ISNULL(@gender,'') + ''' = '''' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = '''+ISNULL(@gender,'')+''' )
					AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
					AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
					AND (''' + ISNULL(@activity,'') + ''' = ''''
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@activity,'') + ','' like ''%,''+PrefLabel+'',%''
						)					
					)
					AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
						OR (										
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.AccessibilitySupport, '','') 
								)AS AccessibilitySupports
								WHERE '',' + ISNULL(@disabilitySupport,'') + ','' LIKE ''%,'' + AccessibilitySupports.ITEM + '',%''
						) > 0
					)							
					AND (''' + ISNULL(@weekdays,'') + ''' = '''' 
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@weekdays,'') + ','' like ''%,'' + CONVERT(NVARCHAR,dbo.WeekDay(WeekName)) + '',%''
						)					
					)								
					AND (''' + ISNULL(@tag,'') + '''  = ''''
						OR (
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.Category, '','') 
								)AS Category
								WHERE Category.Item IN (
									SELECT DISTINCT Item
									FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','') 
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = ''''  
										OR '',' + ISNULL(@excludeTag,'') + ','' NOT LIKE ''%,'' + Item + '',%''
								)									
						) > 0
					)				
					AND
					(
						ISNULL(EOC.EventOccurrenceCount,0) 							
					) > 0								
					AND 1 = (
						CASE
							WHEN (''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' 
									AND ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''')
							THEN 1
							WHEN (ISNULL(e.MinAge,0) BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + ' 
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ') 
								OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + '  
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ')
							THEN 1
							ELSE 0
						END
					)'

		SET @filtered_sql = @sql + ' ORDER BY ' + @SortColumn + ' ASC
							OFFSET ' + CONVERT(NVARCHAR,@limit) + ' * (' + CONVERT(NVARCHAR,@page) + ' - 1) ROWS
							FETCH NEXT ' + CONVERT(NVARCHAR,@limit) + ' ROWS ONLY'

		--SET @total_counted_sql = 'SELECT COUNT(1) AS TotalCount FROM (' + @sql + ') AS Data'
		SET @total_counted_sql='SELECT COUNT(1) AS TotalCount						
					FROM [dbo].[Event] e WITH (NOLOCK)
					INNER JOIN [dbo].[Place] p WITH (NOLOCK)
									ON p.EventId = e.Id 
									AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
									AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL		
					INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = e.FeedProviderId AND fp.IsDeleted = 0 			
					LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.EventId = e.Id
					CROSS APPLY dbo.CIRCLEDISTANCE(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long) AS C
					OUTER APPLY(SELECT COUNT(1) AS EventOccurrenceCount FROM EventOccurrence eo WITH (NOLOCK) WHERE eo.EventId=e.id AND  eo.StartDate >= GETDATE() ) AS EOC
					WHERE e.state = ''updated'' 
					AND e.FeedId IS NOT NULL
					AND C.Distance <= ' + CONVERT(NVARCHAR,@radius) + '				
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '								
					AND (''' + ISNULL(@gender,'') + ''' = '''' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = '''+ISNULL(@gender,'')+''' )
					AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
					AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
					AND (''' + ISNULL(@activity,'') + ''' = ''''
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@activity,'') + ','' like ''%,''+PrefLabel+'',%''
						)					
					)
					AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
						OR (										
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.AccessibilitySupport, '','') 
								)AS AccessibilitySupports
								WHERE '',' + ISNULL(@disabilitySupport,'') + ','' LIKE ''%,'' + AccessibilitySupports.ITEM + '',%''
						) > 0
					)							
					AND (''' + ISNULL(@weekdays,'') + ''' = '''' 
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@weekdays,'') + ','' like ''%,'' + CONVERT(NVARCHAR,dbo.WeekDay(WeekName)) + '',%''
						)					
					)								
					AND (''' + ISNULL(@tag,'') + '''  = ''''
						OR (
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.Category, '','') 
								)AS Category
								WHERE Category.Item IN (
									SELECT DISTINCT Item
									FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','') 
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = ''''  
										OR '',' + ISNULL(@excludeTag,'') + ','' NOT LIKE ''%,'' + Item + '',%''
								)									
						) > 0
					)				
					AND
					(
						ISNULL(EOC.EventOccurrenceCount,0) 
					) > 0								
					AND 1 = (
						CASE
							WHEN (''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' 
									AND ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''')
							THEN 1
							WHEN (ISNULL(e.MinAge,0) BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + ' 
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ') 
								OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + '  
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ')
							THEN 1
							ELSE 0
						END
					)';
	
		EXEC SP_EXECUTESQL @filtered_sql
		EXEC SP_EXECUTESQL @total_counted_sql

		PRINT 'Record fetched Successfully'
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', page = '				+ CAST(@page AS NVARCHAR);
		SET @err_inner_exc += ', limit = '				+ CAST(@limit AS NVARCHAR);
		SET @err_inner_exc += ', sortmode = '			+ @SortColumn;
		SET @err_inner_exc += ', activity = '			+ ISNULL(@activity,'');
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		--EXEC [dbo].[ErrorLog_Insert] 
		--		'[DataLaundryApi] FeedHelper(From SQL SP)'
		--	   ,'GetEventsDynamically - sp(GetFilteredEvents_v1)'
		--	   ,@err_msg			   
		--	   ,@err_inner_exc
		--	   ,NULL
		--	   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetFilteredEvents_v2]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <2019-05-14>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetFilteredEvents_v2]
	@lat				DECIMAL(17,14),
	@long				DECIMAL(17,14),
	@radius				DECIMAL(17,14),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,
	@activity			NVARCHAR(MAX) = NULL,	
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,
	@sortmode			NVARCHAR(50) = 'start',	
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,	
	@weekdays			NVARCHAR(50) = NULL,	
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,	
	@source				NVARCHAR(MAX) = NULL,
	@kind				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@agerange			NVARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON	
	BEGIN TRY	
	DECLARE @min_Minute INT = DATEDIFF(MINUTE, '00:00:00', '6:00:00.000000');
	DECLARE @max_Minute INT = DATEDIFF(MINUTE, '00:00:00', '23:59:59.999999');
	DECLARE @SortColumn AS NVARCHAR(250);

	SET @mintime = ISNULL(@mintime,@min_Minute);
	SET @maxtime = ISNULL(@maxtime,@max_Minute);
	SET @gender =  ISNULL(@gender,'');
	SET @from =    ISNULL(@from,'');
	SET @to =	   ISNULL(@to,'');
	SET @activity =ISNULL(@activity,'');
	SET @disabilitySupport = ISNULL(@disabilitySupport,'');
	SET @weekdays = ISNULL(@weekdays,'');
	SET @tag = ISNULL(@tag,'');
	SET @excludeTag = ISNULL(@excludeTag,'');
	SET @minAge = ISNULL(@minAge,0);
	SET @maxAge = ISNULL(@maxAge,0);
	
	SET @SortColumn = IIF(@sortmode = 'start','StartDate','Distance');	
	SET @weekdays = IIF(@weekdays='1,2,3,4,5,6,7','',@weekdays);	
	SET @activity = LTRIM(RTRIM(REPLACE(ISNULL(@activity,''), '''','''''')));
	
  SELECT
   e.[id]
  -- e.[FeedProviderId],
  --LOWER(REPLACE(fp.[Name], ' ', '' )) AS FeedName,   
  -- e.[FeedId],
  -- e.[State],
  -- e.[ModifiedDate],
  -- e.[ModifiedDateTimestamp],   
  -- e.[Name],
  -- e.[Description],
  -- e.[Image],
  -- e.[ImageThumbnail],
  -- CAST(ISNULL(es.[StartDate], e.[StartDate]) AS DATETIME) + CAST(ISNULL(es.[StartTime], e.[StartDate]) AS DATETIME) AS StartDate,
  -- CAST(ISNULL(es.[EndDate], e.[EndDate]) AS DATETIME) + CAST(ISNULL(es.[EndTime], e.[EndDate]) AS DATETIME) AS EndDate,
  -- CAST(ISNULL(es.[StartTime], e.[StartDate]) AS TIME) AS StartTime,
  -- CAST(ISNULL(es.[EndTime], e.[EndDate]) AS TIME) AS EndTime,
  -- REPLACE(e.[Duration], '-', '') Duration,
  -- e.[MaximumAttendeeCapacity],
  -- e.[RemainingAttendeeCapacity],
  -- e.[EventStatus],
  -- e.[SuperEventId],
  -- e.[Category],
  -- e.[MinAge],
  -- e.[MaxAge],  
  -- e.[Gender] AS GenderRestriction,
  -- e.[AttendeeInstructions],
  -- e.[AccessibilitySupport],
  -- e.[AccessibilityInformation],
  -- e.[IsCoached],
  -- e.[Level],
  -- e.[MeetingPoint],
  -- e.[Identifier],
  -- e.[URL],
  -- ROUND(C.[Distance]*1000, 2) AS Distance,
  -- (
  --    SELECT DISTINCT
  --       STUFF((
  --       SELECT
  --          ',' + CAST([PrefLabel] AS NVARCHAR(MAX)) [text()] 
  --       FROM
  --          [dbo].[PhysicalActivity] WITH (NOLOCK) 
  --       WHERE
  --          [EventId] = e.[Id] FOR XML PATH('')), 1, 1, '') Activities
  -- )
  -- AS Activity,
  -- (
  --    SELECT
  --       [dbo].[WeekDaysForGivenEvent_v1](e.[Id])
  -- )
  -- AS WeekDays  
FROM [dbo].[Event] e WITH (NOLOCK) 
INNER JOIN [dbo].[Place] p WITH (NOLOCK) ON e.[FeedId] IS NOT NULL 
      AND p.[EventId] = e.[Id] 
      AND TRY_CAST(p.[Lat] AS DECIMAL(10, 8)) IS NOT NULL 
      AND TRY_CAST(p.[Long] AS DECIMAL(10, 8)) IS NOT NULL 
INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.[Id] = e.[FeedProviderId] 
      AND fp.[IsDeleted] = 0 
LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK)    ON es.[EventId] = e.[Id] 
	  CROSS APPLY dbo.CIRCLEDISTANCE(@lat,@long, p.[Lat], p.[Long]) AS C 
	  OUTER APPLY(SELECT COUNT(1) AS EventOccurrenceCount FROM [dbo].[EventOccurrence] eo WITH (NOLOCK) WHERE eo.[EventId] = e.[id] AND eo.[StartDate] >= GETDATE()) AS EOC 
      WHERE CAST(ISNULL(es.[StartDate], e.[StartDate]) AS DATETIME2) < (DATEADD(WEEK, 4, GETUTCDATE())) 		
         --AND C.Distance <= @radius		 	
		 AND DATEDIFF_BIG(MINUTE, '00:00:00', CAST(ISNULL(es.[StartTime], e.[StartDate]) AS TIME)) >= @mintime
         AND DATEDIFF_BIG(MINUTE, '00:00:00', CAST(ISNULL(es.[EndTime], e.[EndDate]) AS TIME)) <= @maxtime 
         AND(@gender = '' OR e.[Gender] = @gender)
         AND(@from = ''  OR e.[StartDate] >= @from)
         AND(@to = '' OR e.[EndDate] >= @to)
         AND 
         (
            @activity = '' 
            OR E.[Id] IN 
            (
               SELECT DISTINCT
                  [EventId] 
               FROM
                  [dbo].[PhysicalActivity] WITH (NOLOCK) 
               WHERE
                  [PrefLabel] LIKE '%'+@activity+'%' 
            )
         )
         AND 
         (
            @disabilitySupport = '' 
            OR 
            (
               SELECT
                  COUNT(1) 
               FROM
                  (
                     SELECT DISTINCT
                        LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item 
                     FROM
                        dbo.SplitString(e.[AccessibilitySupport], ',') 
                  )
                  AS AccessibilitySupports 
               WHERE
                  ','+@disabilitySupport+',' LIKE '%,' + AccessibilitySupports.ITEM + ',%' 
            )
            > 0 
         )
         AND 
         (
            @weekdays = '' 
            OR E.[Id] IN 
            (
               SELECT DISTINCT
                  [EventId] 
               FROM
                  [dbo].[EventOccurrence] WITH (NOLOCK) 
               WHERE
                  ','+@weekdays+',' LIKE '%,' + CONVERT(NVARCHAR, dbo.WeekDay(WeekName)) + ',%' 
            )
         )
         AND 
         (
            @tag = '' 
            OR 
            (
               SELECT
                  COUNT(1) 
               FROM
                  (
                     SELECT DISTINCT
                        LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item 
                     FROM
                        dbo.SplitString(e.Category, ',') 
                  )
                  AS Category 
               WHERE
                  Category.Item IN 
                  (
                     SELECT DISTINCT
                        Item 
                     FROM
                        dbo.SplitString(@tag, ',') 
                     WHERE
                        @excludeTag = '' 
                        OR ','+@excludeTag+',' NOT LIKE '%,' + Item + ',%' 
                  )
            )
            > 0 
         )         
         AND 1 = 
         (
            CASE WHEN(@minAge = 0  AND @maxAge = 0) THEN 1 
                 WHEN(ISNULL(e.[MinAge], 0) BETWEEN @minAge AND @maxAge)
                  OR(e.[MaxAge] IS NOT NULL AND e.[MaxAge] BETWEEN @minAge AND @maxAge) THEN 1 
            ELSE 0 
            END
         )		 
    ORDER BY 
	CASE 
		WHEN @SortColumn = 'StartDate' THEN [dbo].[UNIX_TIMESTAMP](E.[StartDate])
		WHEN @SortColumn  = 'Distance' THEN [Distance]
		ELSE 
		[Distance]
	END	
	ASC 
	OFFSET @limit * (@page - 1) ROWS 
	FETCH NEXT @limit ROWS ONLY	

SELECT COUNT(1) AS TotalCount	
FROM [dbo].[Event] e WITH (NOLOCK) 
INNER JOIN [dbo].[Place] p WITH (NOLOCK) ON e.[FeedId] IS NOT NULL 
      AND p.[EventId] = e.[Id] 
      AND TRY_CAST(p.[Lat] AS DECIMAL(10, 8)) IS NOT NULL 
      AND TRY_CAST(p.[Long] AS DECIMAL(10, 8)) IS NOT NULL 
INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.[Id] = e.[FeedProviderId] 
      AND fp.[IsDeleted] = 0 
LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK)    ON es.[EventId] = e.[Id] 
	  CROSS APPLY dbo.CIRCLEDISTANCE(@lat,@long, p.[Lat], p.[Long]) AS C 
	  OUTER APPLY(SELECT COUNT(1) AS EventOccurrenceCount FROM [dbo].[EventOccurrence] eo WITH (NOLOCK) WHERE eo.[EventId] = e.[id] AND eo.[StartDate] >= GETDATE()) AS EOC 
      WHERE CAST(ISNULL(es.[StartDate], e.[StartDate]) AS DATETIME2) < (DATEADD(WEEK, 4, GETUTCDATE())) 
         AND C.Distance <= @radius
		 AND DATEDIFF_BIG(MINUTE, '00:00:00', CAST(ISNULL(es.[StartTime], e.[StartDate]) AS TIME)) >= @mintime
         AND DATEDIFF_BIG(MINUTE, '00:00:00', CAST(ISNULL(es.[EndTime], e.[EndDate]) AS TIME)) <= @maxtime 
         AND(@gender = '' OR e.[Gender] = @gender)
         AND(@from = ''  OR e.[StartDate] >= @from)
         AND(@to = '' OR e.[EndDate] >= @to)
         AND 
         (
            @activity = '' 
            OR E.[Id] IN 
            (
               SELECT DISTINCT
                  [EventId] 
               FROM
                  [dbo].[PhysicalActivity] WITH (NOLOCK) 
               WHERE
                  [PrefLabel] LIKE '%'+@activity+'%' 
            )
         )
         AND 
         (
            @disabilitySupport = '' 
            OR 
            (
               SELECT
                  COUNT(1) 
               FROM
                  (
                     SELECT DISTINCT
                        LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item 
                     FROM
                        dbo.SplitString(e.[AccessibilitySupport], ',') 
                  )
                  AS AccessibilitySupports 
               WHERE
                  ','+@disabilitySupport+',' LIKE '%,' + AccessibilitySupports.ITEM + ',%' 
            )
            > 0 
         )
         AND 
         (
            @weekdays = '' 
            OR E.[Id] IN 
            (
               SELECT DISTINCT
                  [EventId] 
               FROM
                  [dbo].[EventOccurrence] WITH (NOLOCK) 
               WHERE
                  ','+@weekdays+',' LIKE '%,' + CONVERT(NVARCHAR, dbo.WeekDay(WeekName)) + ',%' 
            )
         )
         AND 
         (
            @tag = '' 
            OR 
            (
               SELECT
                  COUNT(1) 
               FROM
                  (
                     SELECT DISTINCT
                        LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item 
                     FROM
                        dbo.SplitString(e.Category, ',') 
                  )
                  AS Category 
               WHERE
                  Category.Item IN 
                  (
                     SELECT DISTINCT
                        Item 
                     FROM
                        dbo.SplitString(@tag, ',') 
                     WHERE
                        @excludeTag = '' 
                        OR ','+@excludeTag+',' NOT LIKE '%,' + Item + ',%' 
                  )
            )
            > 0 
         )         
         AND 1 = 
         (
            CASE WHEN(@minAge = 0  AND @maxAge = 0) THEN 1 
                 WHEN(ISNULL(e.[MinAge], 0) BETWEEN @minAge AND @maxAge)
                  OR(e.[MaxAge] IS NOT NULL AND e.[MaxAge] BETWEEN @minAge AND @maxAge) THEN 1 
            ELSE 0 
            END
         )
		 END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', page = '				+ CAST(@page AS NVARCHAR);
		SET @err_inner_exc += ', limit = '				+ CAST(@limit AS NVARCHAR);
		SET @err_inner_exc += ', sortmode = '			+ @SortColumn;
		SET @err_inner_exc += ', activity = '			+ ISNULL(@activity,'');
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetEventsDynamically - sp(GetFilteredEvents_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END

GO
/****** Object:  StoredProcedure [dbo].[GetIntelligentMappingByTableName]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetIntelligentMappingByTableName]
	@FeedProviderId INT = NULL,
	@TableName		NVARCHAR(50) = NULL
AS
BEGIN
	
	SELECT	* 
	FROM
	(
		SELECT	IM.id
				,IM.ParentId
				,IM.TableName
				,IM.ColumnName
				,CASE 
					WHEN FM.FeedKey IS NOT NULL THEN FM.FeedKey
					ELSE IM.PossibleMatches
				END AS PossibleMatches
				,IM.PossibleHierarchies
				,IM.CustomCriteria 
				,FM.id as FeedMappingId
				,FM.ParentId as FeedMappingParentId
				,ISNULL(FM.IsCustomFeedKey, 0) AS IsCustomFeedKey
				,FM.ColumnDataType
				,FM.FeedKey
				,FM.FeedKeyPath
				,FM.ActualFeedKeyPath
				,ISNULL(FM.IsDeleted,0) AS IsDeleted				
				--,ISNULL(FM.Position,IM.Position) AS Position
				,IM.Position
		FROM	IntelligentMapping IM WITH (NOLOCK)
		LEFT JOIN FeedMapping FM ON IM.ColumnName = FM.ColumnName 
									AND IM.TableName = FM.TableName
									AND FM.FeedProviderId = ISNULL(@FeedProviderId, 0)
									AND FM.IsCustomFeedKey = 0
		WHERE	IM.IsDeleted = 0
				AND (@TableName IS NULL
					OR IM.TableName = @TableName)

		UNION ALL

		SELECT	FM.id
				,FM.ParentId
				,FM.TableName
				,FM.ColumnName
				,NULL as PossibleMatches
				,NULL as PossibleHierarchies
				,NULL as CustomCriteria 
				,FM.id as FeedMappingId
				,FM.ParentId as FeedMappingParentId
				,FM.IsCustomFeedKey
				,FM.ColumnDataType
				,FM.FeedKey
				,FM.FeedKeyPath
				,FM.ActualFeedKeyPath
				,0 as IsDeleted
				,FM.Position
		FROM	FeedMapping FM WITH (NOLOCK)
		WHERE	FM.FeedProviderId = ISNULL(@FeedProviderId, 0)
				AND FM.IsDeleted = 0
				AND FM.IsCustomFeedKey = 1
	) AS TBL
	ORDER BY ID

END

GO
/****** Object:  StoredProcedure [dbo].[GetIntelligentMappingByTableName_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- GetIntelligentMappingByTableName_v1 49
CREATE PROCEDURE [dbo].[GetIntelligentMappingByTableName_v1]
	@FeedProviderId INT = NULL,
	@TableName		NVARCHAR(50) = NULL
AS
BEGIN
	
	SELECT	* 
	FROM
	(
		SELECT	IM.id
				,IM.ParentId
				,IM.TableName
				,IM.ColumnName
				,CASE 
					WHEN FM.FeedKey IS NOT NULL THEN FM.FeedKey
					ELSE IM.PossibleMatches
				END AS PossibleMatches
				,IM.PossibleHierarchies
				,IM.CustomCriteria 
				,FM.id as FeedMappingId
				,FM.ParentId as FeedMappingParentId				
				,CASE 
					WHEN FM.id IS NOT NULL THEN ISNULL(FM.IsCustomFeedKey,0)
					ELSE ISNULL(IM.IsCustomFeedKey, 0)
				END AS IsCustomFeedKey
				,FM.ColumnDataType
				,FM.FeedKey
				,FM.FeedKeyPath
				,FM.ActualFeedKeyPath
				,ISNULL(FM.IsDeleted,0) AS IsDeleted
				,IM.Position
				--,ISNULL(FM.IsCustomKeyActive,0) AS IsCustomKeyActive
		FROM	IntelligentMapping IM WITH (NOLOCK)
		LEFT JOIN FeedMapping FM ON IM.ColumnName = FM.ColumnName 
									AND IM.TableName = FM.TableName
									AND FM.FeedProviderId = ISNULL(@FeedProviderId, 0)
									AND ((FM.IsCustomFeedKey IS NULL OR FM.IsCustomFeedKey = 0) 
											OR 
										(FM.IsCustomFeedKey = 1 AND FM.IsDeleted = 0))
									--AND FM.IsDeleted = 0
									--AND FM.IsCustomFeedKey = 0
		WHERE	IM.IsDeleted = 0
				AND (@TableName IS NULL
					OR IM.TableName = @TableName)

		--UNION ALL

		--SELECT	FM.id
		--		,FM.ParentId
		--		,FM.TableName
		--		,FM.ColumnName
		--		,NULL as PossibleMatches
		--		,NULL as PossibleHierarchies
		--		,NULL as CustomCriteria 
		--		,FM.id as FeedMappingId
		--		,FM.ParentId as FeedMappingParentId
		--		,FM.IsCustomFeedKey
		--		,FM.ColumnDataType
		--		,FM.FeedKey
		--		,FM.FeedKeyPath
		--		,FM.ActualFeedKeyPath
		--		,ISNULL(FM.IsDeleted,0) AS IsDeleted
		--		,FM.Position
		--FROM	FeedMapping FM
		--WHERE	FM.FeedProviderId = ISNULL(@FeedProviderId, 0)
		--		AND FM.IsDeleted = 0
		--		AND FM.IsCustomFeedKey = 1
	) AS TBL
	ORDER BY ID

END


GO
/****** Object:  StoredProcedure [dbo].[GetIntelligentMappingDetail]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetIntelligentMappingDetail]
	@Id				BIGINT,
	@FeedProviderId INT = NULL
AS
BEGIN
	
	SELECT	IM.id
			,IM.ParentId
			,IM.TableName
			,IM.ColumnName
			,IM.IsCustomFeedKey
			,IM.PossibleMatches
			,FM.FeedKey
			,FM.FeedKeyPath
			,FM.ActualFeedKeyPath
			,IM.PossibleHierarchies
			,IM.CustomCriteria 
	FROM	IntelligentMapping IM WITH (NOLOCK)
	LEFT JOIN FeedMapping FM ON IM.ColumnName = FM.ColumnName 
								AND IM.TableName = FM.TableName
								AND FM.FeedProviderId = ISNULL(@FeedProviderId, 0)
	WHERE	IM.Id = @Id

END
GO
/****** Object:  StoredProcedure [dbo].[GetIntelligentMappingForAnalyze]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetIntelligentMappingForAnalyze]	
	@TableName NVARCHAR(50) = NULL
AS
BEGIN	
	SELECT	* 
	FROM
	(
		SELECT	IM.id
				,IM.ParentId
				,IM.TableName
				,IM.ColumnName
				,IM.PossibleMatches
				,IM.PossibleHierarchies
				,IM.CustomCriteria 
				,NULL as FeedMappingId
				,NULL as FeedMappingParentId				
				,IM.IsCustomFeedKey
				,IM.ColumnDataType
				,NULL as FeedKey
				,NULL as FeedKeyPath
				,NULL as ActualFeedKeyPath
				,ISNULL(IM.IsDeleted,0) AS IsDeleted
				,IM.Position				
		FROM	IntelligentMapping IM WITH (NOLOCK)		
		WHERE	IM.IsDeleted = 0
				AND (@TableName IS NULL
					OR IM.TableName = @TableName)
	) AS TBL
	ORDER BY ID

END

GO
/****** Object:  StoredProcedure [dbo].[GetLocations]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[GetLocations] 51.5073509,-0.1277583,50,1,50,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
CREATE PROCEDURE [dbo].[GetLocations]
	@lat				DECIMAL(10,8),
	@long				DECIMAL(10,8),
	@radius				DECIMAL(10,8),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,
	@source				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@activity			NVARCHAR(MAX) = NULL,	
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@weekdays			NVARCHAR(50) = NULL,	
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@gender				NVARCHAR(50) = NULL,	
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL
AS
BEGIN
	DECLARE @sql AS NVARCHAR(MAX),@total_counted_sql AS NVARCHAR(MAX); 

	DECLARE @activities NVARCHAR(MAX) = '';

	DECLARE @min_Minute INT = datediff(minute, '00:00:00', '6:00:00.000000')
	DECLARE @max_Minute INT = datediff(minute, '00:00:00', '23:59:59.999999')

	SET @sql = 'SELECT 
					p.Lat
					,p.Long
					,COUNT(e.id) AS TotalEvents 
					,COUNT(o.id) AS TotalOrganisations					
					,dbo.LatLonRadiusDistance(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat ,p.Long)*1000 AS Distance					
				FROM [dbo].[Event] e WITH (NOLOCK)
				INNER JOIN [dbo].[Place] p ON p.EventId = e.Id	
				LEFT JOIN Organization o ON o.EventId = e.id
				LEFT JOIN [dbo].[EventSchedule] es ON es.EventId = e.Id								
				WHERE e.FeedId IS NOT NULL	
				AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL					
				AND CONVERT(DECIMAL(10,8),p.Lat) >= ' + CONVERT(NVARCHAR,@lat) + '
				AND CONVERT(DECIMAL(10,8),p.Long) >= ' + CONVERT(NVARCHAR,@long) + '			
				AND dbo.LatLonRadiusDistance(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + '	,p.Lat ,p.Long) <= ' + CONVERT(NVARCHAR,@radius) + '				
				AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
				AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '
				AND [dbo].[IsAgeRangeEligibleForGivenAge](e.Id,' + CONVERT(NVARCHAR,ISNULL(@minAge,'0')) + ',' + CONVERT(NVARCHAR,ISNULL(@maxAge,'100')) + ') = 1
				AND (''' + ISNULL(@gender,'') + ''' = '''' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = '''+ISNULL(@gender,'')+''' )
				AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
				AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
				AND (''' + ISNULL(@activity,'') + ''' = '''' 
						OR E.Id IN 
						(
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)		
							WHERE (PrefLabel IN (
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@activity,'') + ''', '','') 
								  ) AS tbl)
							)
						)					
				)
				AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
					OR (
							SELECT COUNT(*) FROM (
								SELECT * FROM (
									SELECT DISTINCT ITEM FROM (
										SELECT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
										FROM dbo.SplitString(e.AccessibilitySupport, '','') 
									) AS AccessibilitySupport
								)AS AccessibilitySupportInTable
								WHERE AccessibilitySupportInTable.ITEM IN (
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@disabilitySupport,'') + ''', '','') 
									) AS DisabilitySupport
								)
							) AS TotalCount 									
					) > 0
				)
				AND (''' + ISNULL(@weekdays,'') + ''' = '''' 
					OR (
						[dbo].[IsEventEligibleForGivenWeekdays] (e.Id,''' + ISNULL(@weekdays,'') + ''') = 1
					)
				)
				AND (''' + ISNULL(@tag,'') + '''  = ''''					 
					OR (
							SELECT COUNT(*) FROM (
								SELECT * FROM (
									SELECT DISTINCT Item FROM (
										SELECT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
										FROM dbo.SplitString(e.Category, '','') 
									) AS Category
								)AS CategoryInTable
								WHERE CategoryInTable.Item IN (									
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','')
									) AS Tag
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = '''' 
										OR 
										Tag.Item NOT IN (
											SELECT DISTINCT Item FROM (
												SELECT Item AS Item
												FROM dbo.SplitString(''' + ISNULL(@excludeTag,'') + ''', '','')
											) AS ExcludedTag
									)
								)
							) AS TotalCount 									
					) > 0
				)
				GROUP BY p.Lat,p.Long'

	SET @total_counted_sql = 'SELECT COUNT(*) AS TotalCount FROM (' + @sql + ') AS Data'

	--PRINT (@sql)
	--PRINT (@total_counted_sql)

	EXEC SP_EXECUTESQL @sql
	EXEC SP_EXECUTESQL @total_counted_sql




	--SELECT 
	--	p.Lat
	--	,p.Long
	--	,COUNT(e.id) AS TotalEvents 
	--	,COUNT(o.id) AS TotalOrganisations
	--	,dbo.LatLonRadiusDistance(@lat,@long,p.Lat ,p.Long) AS Distance
	--FROM place p 
	--INNER JOIN Event e ON e.id = p.eventid
	--LEFT JOIN Organization o ON o.EventId = e.id
	--LEFT JOIN [dbo].[EventSchedule] es ON es.EventId = e.Id
	--WHERE e.state = 'updated' 
	--AND e.FeedId IS NOT NULL				
	--AND CONVERT(DECIMAL,p.Lat) >= @lat
	--AND CONVERT(DECIMAL,p.Long) >= @long		
	--AND dbo.LatLonRadiusDistance(@lat,@long	,p.Lat ,p.Long) <= @radius				
	--AND DATEDIFF(minute, '00:00:00', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ISNULL(@mintime,@min_Minute)
	--AND DATEDIFF(minute, '00:00:00', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ISNULL(@maxtime,@max_Minute)
	--AND [dbo].[IsAgeRangeEligibleForGivenAge](e.Id,ISNULL(@minAge,'0'),ISNULL(@maxAge,'100')) = 1
	--AND (ISNULL(@gender,'') = '' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = @gender)
	--AND (ISNULL(@from,'') = '' OR e.StartDate >= @from)
	--AND (ISNULL(@to,'') = '' OR e.EndDate >= @to)	
	--AND (ISNULL(@activity,'') = '' 
	--		OR E.Id IN 
	--		(
	--			SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity]		
	--			WHERE (PrefLabel IN (
	--					SELECT DISTINCT Item FROM (
	--						SELECT Item AS Item
	--						FROM dbo.SplitString(ISNULL(@activity,''), ',') 
	--					) AS tbl)
	--			)
	--		)					
	--)
	--AND (ISNULL(@disabilitySupport,'')  = '' 
	--	OR (
	--			SELECT COUNT(*) FROM (
	--				SELECT * FROM (
	--					SELECT DISTINCT ITEM FROM (
	--						SELECT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
	--						FROM dbo.SplitString(e.AccessibilitySupport, ',') 
	--					) AS AccessibilitySupport
	--				)AS AccessibilitySupportInTable
	--				WHERE AccessibilitySupportInTable.ITEM IN (
	--					SELECT DISTINCT Item FROM (
	--						SELECT Item AS Item
	--						FROM dbo.SplitString(@disabilitySupport, ',') 
	--					) AS DisabilitySupport
	--				)
	--			) AS TotalCount 									
	--	) > 0
	--)
	--AND (ISNULL(@weekdays,'') = '' 
	--	OR (
	--		[dbo].[IsEventEligibleForGivenWeekdays] (e.Id,@weekdays) = 1
	--	)
	--)
	--AND (ISNULL(@tag,'')  = ''					 
	--	OR (
	--			SELECT COUNT(*) FROM (
	--				SELECT * FROM (
	--					SELECT DISTINCT Item FROM (
	--						SELECT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
	--						FROM dbo.SplitString(e.Category, ',') 
	--					) AS Category
	--				)AS CategoryInTable
	--				WHERE CategoryInTable.Item IN (									
	--					SELECT DISTINCT Item FROM (
	--						SELECT Item AS Item
	--						FROM dbo.SplitString(@tag, ',')
	--					) AS Tag
	--					WHERE ISNULL(@excludeTag,'') = ''
	--						OR 
	--						Tag.Item NOT IN (
	--							SELECT DISTINCT Item FROM (
	--								SELECT Item AS Item
	--								FROM dbo.SplitString(@excludeTag, ',')
	--							) AS ExcludedTag
	--					)
	--				)
	--			) AS TotalCount 									
	--	) > 0
	--)
	--GROUP BY p.Lat,p.Long
END

GO
/****** Object:  StoredProcedure [dbo].[GetLocations_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[GetLocations_v1] 51.5073509,-0.1277583,50,1,50,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL,NULL
CREATE PROCEDURE [dbo].[GetLocations_v1]
	@lat				DECIMAL(17, 14),
	@long				DECIMAL(17, 14),
	@radius				DECIMAL(17, 14),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,
	@source				NVARCHAR(MAX) = NULL,
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL,
	@activity			NVARCHAR(MAX) = NULL,	
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@weekdays			NVARCHAR(50) = NULL,	
	@minCost			DECIMAL(10,8) = NULL,	
	@maxCost			DECIMAL(10,8) = NULL,
	@gender				NVARCHAR(50) = NULL,	
	@mintime			BIGINT = NULL,
	@maxtime			BIGINT = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL
AS
BEGIN
	DECLARE @sql AS NVARCHAR(MAX),@total_counted_sql AS NVARCHAR(MAX); 
	
	DECLARE @min_Minute INT = DATEDIFF(minute, '00:00:00', '6:00:00.000000')
	DECLARE @max_Minute INT = DATEDIFF(minute, '00:00:00', '23:59:59.999999')
	SET @activity=REPLACE(ISNULL(@activity,''), '''', '');
	BEGIN TRY
		SET @sql = 'SELECT 
						p.Lat
						,p.Long
						,COUNT(e.id) AS TotalEvents 
						,COUNT(ISNULL(o.id,0)) AS TotalOrganisations					
						,ROUND(C.Distance*1000,2) AS Distance
					FROM [dbo].[Event] e WITH (NOLOCK)
					INNER JOIN [dbo].[Place] p WITH (NOLOCK)
									ON p.EventId = e.Id AND e.FeedId IS NOT NULL
									AND TRY_CAST(p.Lat as DECIMAL(10, 8)) IS NOT NULL 
									AND TRY_CAST(p.Long as DECIMAL(10, 8)) IS NOT NULL		
					LEFT JOIN Organization o WITH (NOLOCK) ON o.EventId = e.id
					LEFT JOIN [dbo].[EventSchedule] es WITH (NOLOCK) ON es.EventId = e.Id								
					CROSS APPLY dbo.CIRCLEDISTANCE(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long) AS C
					WHERE C.Distance <= ' + CONVERT(NVARCHAR,@radius) + '
					AND CAST(ISNULL(es.StartDate,e.StartDate) AS DATETIME2)  < (DATEADD(WEEK,4,GETUTCDATE()))
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.StartTime,e.StartDate) AS TIME)) >= ' + CONVERT(NVARCHAR,ISNULL(@mintime,@min_Minute)) + '
					AND DATEDIFF(minute, ''00:00:00'', CAST(ISNULL(es.EndTime,e.EndDate) AS TIME)) <= ' + CONVERT(NVARCHAR,ISNULL(@maxtime,@max_Minute)) + '
					AND (''' + ISNULL(@gender,'') + ''' = '''' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = '''+ISNULL(@gender,'')+''' )
					AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
					AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
					AND (''' + ISNULL(@activity,'') + ''' = '''' 
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@activity,'') + ','' like ''%,''+PrefLabel+'',%''
						)				
					)
					AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
						OR (										
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.AccessibilitySupport, '','') 
								)AS AccessibilitySupports
								WHERE '',' + ISNULL(@disabilitySupport,'') + ','' LIKE ''%,'' + AccessibilitySupports.ITEM + '',%''
						) > 0
					)
					AND (''' + ISNULL(@weekdays,'') + ''' = '''' 
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[EventOccurrence] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@weekdays,'') + ','' like ''%,'' + CONVERT(NVARCHAR,dbo.WeekDay(WeekName)) + '',%''
						)
					)
					AND (''' + ISNULL(@tag,'') + '''  = ''''					 
						OR (
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.Category, '','') 
								)AS Category
								WHERE Category.Item IN (
									SELECT DISTINCT Item
									FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','') 
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = ''''  
										OR '',' + ISNULL(@excludeTag,'') + ','' NOT LIKE ''%,'' + Item + '',%''
								)									
						) > 0
					)
					AND 1 = (
						CASE
							WHEN (''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' 
									AND ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''')
							THEN 1
							WHEN (ISNULL(e.MinAge,0) BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + ' 
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ') 
								OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + '  
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ')
							THEN 1
							ELSE 0
						END
					)
					GROUP BY p.Lat,p.Long,C.Distance'
					

		SET @total_counted_sql = 'SELECT COUNT(1) AS TotalCount FROM (' + @sql + ') AS Data'

		--PRINT (@sql)
		--PRINT (@total_counted_sql)

		EXEC SP_EXECUTESQL @sql
		EXEC SP_EXECUTESQL @total_counted_sql
	PRINT 'Record fetched Successfully'
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');	
		SET @err_inner_exc += ', mintime = '			+ CAST(ISNULL(@mintime,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxtime = '			+ CAST(ISNULL(@maxtime,'') AS NVARCHAR);
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', weekdays = '			+ ISNULL(@weekdays,'');
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetLocations - sp(GetLocations_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetMultiEventData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <28-01-2019>
-- Description:	<Avoid single call data for getting all data>
-- =============================================
CREATE PROCEDURE [dbo].[GetMultiEventData]
	@EventIDs NVARCHAR(MAX)
AS
BEGIN
	-- get place 0
	SELECT	P.id
			,P.EventId
			,P.ParentId
			,P.PlaceTypeId
			,PT.Name AS PlaceTypeName
			,P.Name
			,P.Description
			,P.Image
			,ISNULL(P.Address,' ') AS Address
			,P.Lat
			,P.Long
			,P.Telephone
			,P.FaxNumber
			,P.URL 
	FROM	[dbo].[Place] P WITH (NOLOCK)
	LEFT JOIN [dbo].[PlaceType] PT WITH (NOLOCK) ON P.PlaceTypeId = PT.Id
	WHERE	EventId IN(SELECT item FROM [dbo].SplitStringByComma(@EventIDs))
	AND p.Lat IS NOT NULL AND p.Long IS NOT NULL

	-- get organization 1
	SELECT  id
			,EventId
			,Name
			,Description
			,Email
			,Image
			,URL
			,Telephone 
	FROM	[dbo].[Organization] WITH (NOLOCK)
	WHERE	EventId IN(SELECT item FROM [dbo].SplitStringByComma(@EventIDs))

	-- get subevents 2
	SELECT	id
			,FeedProviderId
			,FeedId
			,State
			,ModifiedDate
			,dbo.UNIX_TIMESTAMP(ModifiedDate) as ModifiedDateTimestamp
			,Name
			,Description
			,Image
			,ImageThumbnail
			,StartDate
			,EndDate
			,Duration
			,MaximumAttendeeCapacity
			,RemainingAttendeeCapacity
			,EventStatus
			,SuperEventId
			,Category
			,AgeRange
			,GenderRestriction
			,AttendeeInstructions
			,AccessibilitySupport
			,AccessibilityInformation
			,IsCoached
			,Level
			,MeetingPoint
			,Identifier
			,URL 
	FROM	[dbo].[Event] WITH (NOLOCK)
	WHERE	SuperEventId IN(SELECT item FROM [dbo].SplitStringByComma(@EventIDs))
			AND FeedId IS NULL

-- get event schedule 3
	SELECT  id
			,EventId
			,StartDate
			,EndDate
			,StartTime
			,EndTime
			,Frequency
			,ByDay
			,ByMonth
			,ByMonthDay
			,RepeatCount
			,RepeatFrequency
	FROM	[dbo].[EventSchedule] WITH (NOLOCK)
	WHERE	EventId IN(SELECT item FROM [dbo].SplitStringByComma(@EventIDs))
	
	-- get occurrence 4
	SELECT  id
			,[EventId]
			,[SubEventId]
			,[StartDate]
			,[EndDate]			
	FROM	[dbo].[EventOccurrence] WITH (NOLOCK)
	WHERE	EventId IN(SELECT item FROM [dbo].SplitStringByComma(@EventIDs))
	AND StartDate >= GETDATE()

	-- get offer 5
	SELECT [Id]
		  ,[EventId]
		  ,ISNULL([Identifier],'') AS Identifier
		  ,ISNULL([Name],'') AS [Name]
		  ,ISNULL([Price],'') AS Price
		  ,ISNULL([PriceCurrency],'') AS PriceCurrency
		  ,ISNULL([Description],'') AS [Description]	
	FROM [dbo].[Offer] WITH(NOLOCK)
	WHERE [EventId] IN(SELECT Item FROM [dbo].[SplitStringByComma](@EventIDs))
END
GO
/****** Object:  StoredProcedure [dbo].[GetOrganisationById]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetOrganisationById]
	@organisationId BIGINT
AS
BEGIN
	SELECT 
		o.id
		,o.Name
		,o.Description
		,o.Email
		,o.Image
		,o.URL
		,o.Telephone
		,e.Category		
		--,e.id AS EventId
		--,e.FeedProviderId
		,e.FeedId
		--,e.State
		--,e.ModifiedDate
		--,dbo.UNIX_TIMESTAMP(ModifiedDate) as ModifiedDateTimestamp
		--,e.Name
		--,e.Description
		--,e.Image
		--,e.ImageThumbnail		
		,REPLACE(e.Duration,'-','') Duration
		,e.MaximumAttendeeCapacity
		,e.RemainingAttendeeCapacity
		,e.EventStatus
		--,e.SuperEventId
		,e.Category					
		,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
		,e.AttendeeInstructions
		,e.AccessibilitySupport
		,e.AccessibilityInformation
		,e.IsCoached
		,e.Level
		,e.MeetingPoint
		,e.Identifier
		--,e.URL
		,[dbo].[AgeRangeForGivenEvent](e.Id) AS AgeRange
		,NULL AS Distance
		,(SELECT DISTINCT STUFF((SELECT ',' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('')),1,1,'') Activities) AS Activity
		,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
		,p.Lat
		,p.Long
		,p.Address
	FROM [dbo].[Organization] o WITH (NOLOCK)
	LEFT JOIN [dbo].[Event] e WITH (NOLOCK) ON o.EventId = e.id
	INNER JOIN [dbo].[Place] p WITH (NOLOCK) ON p.EventId = e.Id
	WHERE e.state = 'updated' 
	AND e.FeedId IS NOT NULL	
	AND o.id = @organisationId
END
GO
/****** Object:  StoredProcedure [dbo].[GetOrganisations]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- [dbo].[GetOrganisations] 51.5073509,-0.1277583,50,1,50
CREATE PROCEDURE [dbo].[GetOrganisations]	
	@lat				DECIMAL(10,8),
	@long				DECIMAL(10,8),
	@radius				DECIMAL(10,8),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,	
	@activity			NVARCHAR(MAX) = NULL,
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,
	@to					NVARCHAR(MAX) = NULL,
	@source				NVARCHAR(MAX) = NULL,	
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL	
AS
BEGIN
	DECLARE @sql AS NVARCHAR(MAX),@total_counted_sql AS NVARCHAR(MAX)
	
	DECLARE @activities NVARCHAR(MAX) = '';

	SET @sql = 'SELECT 
					o.id
					,o.Name
					,o.Description
					,o.Email
					,o.Image
					,o.URL
					,o.Telephone
					,e.Category
					,e.FeedId
					,e.Duration
					,e.MaximumAttendeeCapacity
					,e.RemainingAttendeeCapacity
					,e.EventStatus							
					,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
					,e.AttendeeInstructions
					,e.AccessibilitySupport
					,e.AccessibilityInformation
					,e.IsCoached
					,e.Level
					,e.MeetingPoint
					,e.Identifier
					,[dbo].[AgeRangeForGivenEvent](e.Id) AS AgeRange
					,dbo.LatLonRadiusDistance(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat ,p.Long)*1000 AS Distance
					,(SELECT DISTINCT STUFF((SELECT '','' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('''')),1,1,'''') Activities) AS Activity
					,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
					,p.Lat
					,p.Long
					,p.Address
				FROM [dbo].[Organization] o WITH (NOLOCK)
				LEFT JOIN [dbo].[Event] e ON o.EventId = e.id
				INNER JOIN [dbo].[Place] p ON p.EventId = e.Id						
				WHERE e.state = ''updated'' 
				AND e.FeedId IS NOT NULL
				AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL				
				AND CONVERT(DECIMAL(10,8),p.Lat) >= ' + CONVERT(NVARCHAR,@lat) + '
				AND CONVERT(DECIMAL(10,8),p.Long) >= ' + CONVERT(NVARCHAR,@long) + '			
				AND dbo.LatLonRadiusDistance(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + '	,p.Lat ,p.Long) <= ' + CONVERT(NVARCHAR,@radius) + '				
				AND [dbo].[IsAgeRangeEligibleForGivenAge](e.Id,' + CONVERT(NVARCHAR,ISNULL(@minAge,'0')) + ',' + CONVERT(NVARCHAR,ISNULL(@maxAge,'100')) + ') = 1
				AND (''' + ISNULL(@gender,'') + ''' = '''' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = '''+ISNULL(@gender,'')+''' )
				AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
				AND (''' + ISNULL(@activity,'') + ''' = '''' 
						OR E.Id IN 
						(
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)		
							WHERE (PrefLabel IN (
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@activity,'') + ''', '','') 
								  ) AS tbl)
							)
						)					
				)
				AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
					OR (
							SELECT COUNT(*) FROM (
								SELECT * FROM (
									SELECT DISTINCT ITEM FROM (
										SELECT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
										FROM dbo.SplitString(e.AccessibilitySupport, '','') 
									) AS AccessibilitySupport
								)AS AccessibilitySupportInTable
								WHERE AccessibilitySupportInTable.ITEM IN (
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@disabilitySupport,'') + ''', '','') 
									) AS DisabilitySupport
								)
							) AS TotalCount 									
					) > 0
				)				
				AND (''' + ISNULL(@tag,'') + '''  = ''''					 
					OR (
							SELECT COUNT(*) FROM (
								SELECT * FROM (
									SELECT DISTINCT Item FROM (
										SELECT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
										FROM dbo.SplitString(e.Category, '','') 
									) AS Category
								)AS CategoryInTable
								WHERE CategoryInTable.Item IN (									
									SELECT DISTINCT Item FROM (
										SELECT Item AS Item
										FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','')
									) AS Tag
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = '''' 
										OR 
										Tag.Item NOT IN (
											SELECT DISTINCT Item FROM (
												SELECT Item AS Item
												FROM dbo.SplitString(''' + ISNULL(@excludeTag,'') + ''', '','')
											) AS ExcludedTag
									)
								)
							) AS TotalCount 									
					) > 0
				)'	

	SET @total_counted_sql = 'SELECT COUNT(*) AS TotalCount FROM (' + @sql + ') AS Data'	

	EXEC SP_EXECUTESQL @sql
	EXEC SP_EXECUTESQL @total_counted_sql




	--SELECT 
	--	o.id
	--	,o.Name
	--	,o.Description
	--	,o.Email
	--	,o.Image
	--	,o.URL
	--	,o.Telephone
	--	,e.Category
	--	,e.FeedId
	--	,e.Duration
	--	,e.MaximumAttendeeCapacity
	--	,e.RemainingAttendeeCapacity
	--	,e.EventStatus
	--	,e.Category					
	--	,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
	--	,e.AttendeeInstructions
	--	,e.AccessibilitySupport
	--	,e.AccessibilityInformation
	--	,e.IsCoached
	--	,e.Level
	--	,e.MeetingPoint
	--	,e.Identifier
	--	,[dbo].[AgeRangeForGivenEvent](e.Id) AS AgeRange
	--	,dbo.LatLonRadiusDistance(@lat,@long,p.Lat ,p.Long)*1000 AS Distance
	--	,(SELECT DISTINCT STUFF((SELECT ',' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WHERE EventId = e.Id FOR XML PATH('')),1,1,'') Activities) AS Activity
	--	,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
	--	,p.Lat
	--	,p.Long
	--	,p.Address
	--FROM [dbo].[Organization] o
	--LEFT JOIN [dbo].[Event] e ON o.EventId = e.id
	--INNER JOIN [dbo].[Place] p ON p.EventId = e.Id
	--WHERE e.state = 'updated' 
	--AND e.FeedId IS NOT NULL				
	--AND CONVERT(DECIMAL,p.Lat) >= @lat
	--AND CONVERT(DECIMAL,p.Long) >= @long			
	--AND dbo.LatLonRadiusDistance(@lat,@long,convert(float,p.Lat) ,convert(float,p.Long)) <= @radius	
	--AND [dbo].[IsAgeRangeEligibleForGivenAge](e.Id,@minAge,@maxAge) = 1
	--AND (ISNULL(@gender,'') = '' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = @gender)		
	--AND (ISNULL(@to,'') = '' OR e.EndDate >= @to)			
	--AND (ISNULL(@disabilitySupport,'')  = '' 
	--	OR (
	--			SELECT COUNT(*) FROM (
	--				SELECT * FROM (
	--					SELECT DISTINCT ITEM FROM (
	--						SELECT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
	--						FROM dbo.SplitString(e.AccessibilitySupport, ',') 
	--					) AS AccessibilitySupport
	--				)AS AccessibilitySupportInTable
	--				WHERE AccessibilitySupportInTable.ITEM IN (
	--					SELECT DISTINCT Item FROM (
	--						SELECT Item AS Item
	--						FROM dbo.SplitString(@disabilitySupport, ',') 
	--					) AS DisabilitySupport
	--				)
	--			) AS TotalCOunt 									
	--	) > 0
	--)		
	--AND (ISNULL(@tag,'')  = ''					 
	--	OR (
	--			SELECT COUNT(*) FROM (
	--				SELECT * FROM (
	--					SELECT DISTINCT Item FROM (
	--						SELECT LTRIM(REPLACE (REPLACE (Item, '[', ''), ']', '')) as Item
	--						FROM dbo.SplitString(e.Category, ',') 
	--					) AS Category
	--				)AS CategoryInTable
	--				WHERE CategoryInTable.Item IN (									
	--					SELECT DISTINCT Item FROM (
	--						SELECT Item AS Item
	--						FROM dbo.SplitString(@tag, ',')
	--					) AS Tag
	--					WHERE ISNULL(@excludeTag,'') = ''
	--						OR 
	--						Tag.Item NOT IN (
	--							SELECT DISTINCT Item FROM (
	--								SELECT Item AS Item
	--								FROM dbo.SplitString(@excludeTag, ',')
	--							) AS ExcludedTag
	--					)
	--				)
	--			) AS TotalCount 									
	--	) > 0
	--)
END

GO
/****** Object:  StoredProcedure [dbo].[GetOrganisations__test_v2]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[GetOrganisations__test_v2] 51.5073509,-0.1277583,50,1,50
CREATE PROCEDURE [dbo].[GetOrganisations__test_v2]	
	@lat				DECIMAL(10,8),
	@long				DECIMAL(10,8),
	@radius				DECIMAL(10,8),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,	
	@activity			NVARCHAR(MAX) = NULL,
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,
	@source				NVARCHAR(MAX) = NULL,	
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL	
AS
BEGIN
	DECLARE @sql AS NVARCHAR(MAX),@total_counted_sql AS NVARCHAR(MAX)

	BEGIN TRY
		SET @sql = 'SELECT 
						o.id
						,o.Name
						,o.Description
						,o.Email
						,o.Image
						,o.URL
						,o.Telephone
						,e.Category
						,e.FeedId
						,REPLACE(e.Duration,''-'','''') Duration
						,e.MaximumAttendeeCapacity
						,e.RemainingAttendeeCapacity
						,e.EventStatus							
						,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
						,e.AttendeeInstructions
						,e.AccessibilitySupport
						,e.AccessibilityInformation
						,e.IsCoached
						,e.Level
						,e.MeetingPoint
						,e.Identifier
						,CAST(ISNULL(e.MinAge,0) AS NVARCHAR(10)) + IIF(e.MaxAge IS NOT NULL,(''-'' + CAST(e.MaxAge AS NVARCHAR(10))),'''') AS AgeRange
						,ROUND(C.Distance*1000,2) AS Distance
						,(SELECT DISTINCT STUFF((SELECT '','' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('''')),1,1,'''') Activities) AS Activity
						,(SELECT [dbo].[WeekDaysForGivenEvent_v1](e.Id)) AS WeekDays
						,p.Lat
						,p.Long
						,p.Address
					FROM [dbo].[Organization] o WITH (NOLOCK)
					LEFT JOIN [dbo].[Event] e  WITH (NOLOCK) ON o.EventId = e.id
					INNER JOIN [dbo].[Place] p  WITH (NOLOCK)
									ON p.EventId = e.Id 
									AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
									AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL	
					CROSS APPLY dbo.CIRCLEDISTANCE(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long) AS C					
					WHERE e.state = ''updated'' 
					AND e.FeedId IS NOT NULL
					AND C.Distance <= ' + CONVERT(NVARCHAR,@radius) + '
					AND (''' + ISNULL(@gender,'') + ''' = '''' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = '''+ISNULL(@gender,'')+''' )
					AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
					AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
					AND (''' + ISNULL(@activity,'') + ''' = ''''
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@activity,'') + ','' like ''%,''+PrefLabel+'',%''
						)					
					)
					AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
						OR (										
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.AccessibilitySupport, '','') 
								)AS AccessibilitySupports
								WHERE '',' + ISNULL(@disabilitySupport,'') + ','' LIKE ''%,'' + AccessibilitySupports.ITEM + '',%''
						) > 0
					)				
					AND (''' + ISNULL(@tag,'') + '''  = ''''
						OR (
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.Category, '','') 
								)AS Category
								WHERE Category.Item IN (
									SELECT DISTINCT Item
									FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','') 
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = ''''  
										OR '',' + ISNULL(@excludeTag,'') + ','' NOT LIKE ''%,'' + Item + '',%''
								)									
						) > 0
					)
					AND 1 = (
						CASE
							WHEN (''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' 
									AND ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''')
							THEN 1
							WHEN (ISNULL(e.MinAge,0) BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + ' 
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ') 
								OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + '  
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ')
							THEN 1
							ELSE 0
						END
					)
					ORDER BY o.id	
					OFFSET ('+CONVERT(nvarchar,@limit) +' * ('+CONVERT(nvarchar,@page)+'-1))  ROWS
					FETCH NEXT ('+CONVERT(nvarchar,@limit) +')  ROWS ONLY
					'
					
		print @sql	
		--set @sql= @sql+' OFFSET ('+CONVERT(nvarchar,@limit) +' * ('+CONVERT(nvarchar,@page)+'-1))  ROWS
  --     FETCH NEXT ('+CONVERT(nvarchar,@limit) +')  ROWS ONLY'
       print @Sql
   


		SET @total_counted_sql = 'SELECT COUNT(*) AS TotalCount FROM (' + @sql + ') AS Data'	

		EXEC SP_EXECUTESQL @sql
		EXEC SP_EXECUTESQL @total_counted_sql
	PRINT 'Record fetched Successfully'
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetOrganisations - sp(GetOrganisations_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetOrganisations_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[GetOrganisations_v1] 51.5073509,-0.1277583,50,1,50
CREATE PROCEDURE [dbo].[GetOrganisations_v1]	
	@lat				DECIMAL(17, 14),
	@long				DECIMAL(17, 14),
	@radius				DECIMAL(17, 14),
	@page				BIGINT = 1,
	@limit				BIGINT = 50,	
	@activity			NVARCHAR(MAX) = NULL,
	@disabilitySupport	NVARCHAR(MAX) = NULL,
	@gender				NVARCHAR(50) = NULL,	
	@minAge				BIGINT = NULL,
	@maxAge				BIGINT = NULL,
	@from				NVARCHAR(MAX) = NULL,
	@to					NVARCHAR(MAX) = NULL,
	@source				NVARCHAR(MAX) = NULL,	
	@tag				NVARCHAR(MAX) = NULL,
	@excludeTag			NVARCHAR(MAX) = NULL	
AS
BEGIN
	DECLARE @sql AS NVARCHAR(MAX),@total_counted_sql AS NVARCHAR(MAX),@Filter_Record AS NVARCHAR(MAX)
	SET @activity=REPLACE(ISNULL(@activity,''), '''', '');
	BEGIN TRY
		SET @sql = 'SELECT 
						o.id
						,o.Name
						,o.Description
						,o.Email
						,o.Image
						,o.URL
						,o.Telephone
						,e.Category
						,e.FeedId
						,REPLACE(e.Duration,''-'','''') Duration
						,e.MaximumAttendeeCapacity
						,e.RemainingAttendeeCapacity
						,e.EventStatus							
						,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
						,e.AttendeeInstructions
						,e.AccessibilitySupport
						,e.AccessibilityInformation
						,e.IsCoached
						,e.Level
						,e.MeetingPoint
						,e.Identifier
						,CAST(ISNULL(e.MinAge,0) AS NVARCHAR(10)) + IIF(e.MaxAge IS NOT NULL,(''-'' + CAST(e.MaxAge AS NVARCHAR(10))),'''') AS AgeRange
						,ROUND(C.Distance*1000,2) AS Distance
						,(SELECT DISTINCT STUFF((SELECT '','' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('''')),1,1,'''') Activities) AS Activity
						,(SELECT [dbo].[WeekDaysForGivenEvent_v1](e.Id)) AS WeekDays
						,p.Lat
						,p.Long
						,p.Address
					FROM [dbo].[Organization] o WITH (NOLOCK)
					LEFT JOIN [dbo].[Event] e  WITH (NOLOCK) ON o.EventId = e.id
					INNER JOIN [dbo].[Place] p  WITH (NOLOCK)
									ON p.EventId = e.Id 
									AND TRY_CAST(p.Lat as DECIMAL(10,8)) IS NOT NULL 
									AND TRY_CAST(p.Long as DECIMAL(10,8)) IS NOT NULL	
					CROSS APPLY dbo.CIRCLEDISTANCE(' + CONVERT(NVARCHAR,@lat) + ',' + CONVERT(NVARCHAR,@long) + ',p.Lat,p.Long) AS C					
					WHERE e.state = ''updated'' 
					AND e.FeedId IS NOT NULL
					AND C.Distance <= ' + CONVERT(NVARCHAR,@radius) + '
					AND (''' + ISNULL(@gender,'') + ''' = '''' OR [dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) = '''+ISNULL(@gender,'')+''' )
					AND (''' + ISNULL(@from,'') + ''' = '''' OR e.StartDate >= ''' + ISNULL(@from,'') + ''')
					AND (''' + ISNULL(@to,'') + ''' = '''' OR e.EndDate >= ''' + ISNULL(@to,'') + ''')	
					AND (''' + ISNULL(@activity,'') + ''' = ''''
						OR E.Id IN 
						(							
							SELECT DISTINCT EventId FROM [dbo].[PhysicalActivity] WITH (NOLOCK)	
							WHERE '',' + ISNULL(@activity,'') + ','' like ''%,''+PrefLabel+'',%''
						)					
					)
					AND (''' + ISNULL(@disabilitySupport,'') + '''  = '''' 
						OR (										
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.AccessibilitySupport, '','') 
								)AS AccessibilitySupports
								WHERE '',' + ISNULL(@disabilitySupport,'') + ','' LIKE ''%,'' + AccessibilitySupports.ITEM + '',%''
						) > 0
					)				
					AND (''' + ISNULL(@tag,'') + '''  = ''''
						OR (
								SELECT COUNT(1) FROM (
									SELECT DISTINCT LTRIM(REPLACE (REPLACE (Item, ''['', ''''), '']'', '''')) as Item
									FROM dbo.SplitString(e.Category, '','') 
								)AS Category
								WHERE Category.Item IN (
									SELECT DISTINCT Item
									FROM dbo.SplitString(''' + ISNULL(@tag,'') + ''', '','') 
									WHERE ''' + ISNULL(@excludeTag,'') + ''' = ''''  
										OR '',' + ISNULL(@excludeTag,'') + ','' NOT LIKE ''%,'' + Item + '',%''
								)									
						) > 0
					)
					AND 1 = (
						CASE
							WHEN (''' + ISNULL(CONVERT(NVARCHAR,@minAge),'') + ''' = '''' 
									AND ''' + ISNULL(CONVERT(NVARCHAR,@maxAge),'') + ''' = '''')
							THEN 1
							WHEN (ISNULL(e.MinAge,0) BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + ' 
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ') 
								OR (e.MaxAge IS NOT NULL AND e.MaxAge BETWEEN ' + CONVERT(NVARCHAR,ISNULL(@minAge,0)) + '  
										AND ' + CONVERT(NVARCHAR,ISNULL(@maxAge,200)) + ')
							THEN 1
							ELSE 0
						END
					)						
					'	
		SET @Filter_Record = @sql + 'ORDER BY o.id 
			OFFSET (' + CONVERT(NVARCHAR, @limit) + ') *  (' + CONVERT(NVARCHAR, @page) + ' - 1) ROWS
					FETCH NEXT ' + CONVERT(NVARCHAR, @limit) + ' ROWS ONLY'

		SET @total_counted_sql = 'SELECT COUNT(*) AS TotalCount FROM (' + @sql + ') AS Data'	

		EXEC SP_EXECUTESQL @Filter_Record
		EXEC SP_EXECUTESQL @total_counted_sql
	PRINT 'Record fetched Successfully'
	PRINT @Filter_Record
	END TRY
	BEGIN CATCH	
		DECLARE @err_msg AS NVARCHAR(MAX), @err_inner_exc AS NVARCHAR(MAX);

		SELECT @err_msg = ISNULL(ERROR_MESSAGE(),'Error Occurred');

		SET @err_inner_exc = 'Error occurred with following parameters i.e ';
		SET @err_inner_exc += ' lat= '					+ CAST(@lat AS NVARCHAR);
		SET @err_inner_exc += ', long = '				+ CAST(@long AS NVARCHAR);
		SET @err_inner_exc += ', radius = '				+ CAST(@radius AS NVARCHAR);
		SET @err_inner_exc += ', disabilitySupport = '	+ ISNULL(@disabilitySupport,'');
		SET @err_inner_exc += ', gender = '				+ ISNULL(@gender,'');
		SET @err_inner_exc += ', minAge = '				+ CAST(ISNULL(@minAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', maxAge = '				+ CAST(ISNULL(@maxAge,'') AS NVARCHAR);
		SET @err_inner_exc += ', from = '				+ CAST(ISNULL(@from,'') AS NVARCHAR);
		SET @err_inner_exc += ', to = '					+ CAST(ISNULL(@to,'') AS NVARCHAR);
		SET @err_inner_exc += ', tag = '				+ ISNULL(@tag,'');
		SET @err_inner_exc += ', excludeTag = '			+ ISNULL(@excludeTag,'');

		EXEC [dbo].[ErrorLog_Insert] 
				'[DataLaundryApi] FeedHelper(From SQL SP)'
			   ,'GetOrganisations - sp(GetOrganisations_v1)'
			   ,@err_msg			   
			   ,@err_inner_exc
			   ,NULL
			   ,NULL;

		PRINT 'Sorry, Error occurred !!';
		THROW;
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[GetRuleDetailByID]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <01-01-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[GetRuleDetailByID]
	@Id INT
AS
BEGIN
	SELECT R.Id,
			R.RuleName,
			R.IsEnabled,
			R.FeedProviderId
	FROM [Rule] R WITH (NOLOCK)
	WHERE R.IsDeleted = 0 AND
			R.Id = @Id

	SELECT FC.FilterCriteriaId
			,ISNULL(FC.ParentId,0) AS ParentId
			,FC.FieldMappingId
			,FC.OperationId
			,FC.OperatorId
			,FC.Value
			,FM.TableName
			,FM.ColumnName
			,FM.ColumnDataType
			,FM.FeedKey
			,FM.FeedKeyPath
			,FM.ActualFeedKeyPath
	FROM FilterCriteria FC WITH (NOLOCK)
	INNER JOIN [Rule] R WITH (NOLOCK) ON FC.RuleId = R.Id AND R.Id = @Id AND R.IsDeleted = 0
	INNER JOIN [FeedMapping] FM WITH (NOLOCK) ON FM.id = FC.FieldMappingId
 
	SELECT DISTINCT
			O.OperationId,
			ISNULL(O.FieldId,0) AS FieldId,
			ISNULL(O.Value,'') AS Value,
			ISNULL(O.CurrentWord,'') AS CurrentWord,
			ISNULL(O.NewWord,'') AS NewWord,
			ISNULL(O.Sentance,'') AS Sentance,
			ISNULL(O.FirstFieldId,0) AS FirstFieldId,
			ISNULL(O.SecondFieldId,0) AS SecondFieldId,
			O.OperationTypeId,
			OT.Name AS OperationTypeName,			
			FM.TableName,
			FM.ColumnName,
			FM.ColumnDataType,
			FM.FeedKey,
			FM.FeedKeyPath,
			FM.ActualFeedKeyPath,
			TFM.FeedKey AS TempFeedKey,
			TFM.FeedKeyPath AS TempFeedKeyPath,
			TFM.ActualFeedKeyPath AS TempActualFeedKeyPath,
			TFM.ColumnName AS TempColumnName,
			TFM.TableName AS TempTableName,
			FRFM.FeedKey AS TempFRFeedKey,			
			FRFM.FeedKeyPath AS TempFRFeedKeyPath,
			FRFM.ColumnName AS TempFRColumnName,
			FRFM.ActualFeedKeyPath AS TempFRActualFeedKeyPath,
			FRFM.TableName AS TempFRTableName,
			SCFM.FeedKey AS TempSCFeedKey,
			SCFM.FeedKeyPath AS TempSCFeedKeyPath,
			SCFM.ActualFeedKeyPath AS TempSCActualFeedKeyPath,	
			SCFM.ColumnName AS TempSCColumnName,
			SCFM.TableName AS TempSCTableName		
	FROM Operation O WITH (NOLOCK) 
	INNER JOIN [Rule] R WITH (NOLOCK) ON O.RuleId = R.Id AND R.Id = @Id AND R.IsDeleted = 0
	INNER JOIN OperationType OT WITH (NOLOCK) ON O.OperationTypeId = OT.OperationTypeId
	LEFT JOIN [FeedMapping] FM WITH (NOLOCK) ON FM.id = O.FieldId
	LEFT JOIN [FeedMapping] TFM WITH (NOLOCK) ON ISNUMERIC(O.Value) = 1 AND  CAST(TFM.id AS NVARCHAR) = O.Value 
	LEFT JOIN [FeedMapping] FRFM WITH (NOLOCK) ON ISNUMERIC(O.FirstFieldId) = 1  AND CAST(FRFM.id AS NVARCHAR) =O.FirstFieldId  
	LEFT JOIN [FeedMapping] SCFM WITH (NOLOCK) ON ISNUMERIC(O.SecondFieldId) = 1  AND CAST(SCFM.id AS NVARCHAR) =  O.SecondFieldId
	

	
END
GO
/****** Object:  StoredProcedure [dbo].[GetScheduledFeedProviders]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- GetScheduledFeedProviders 0
CREATE PROCEDURE [dbo].[GetScheduledFeedProviders]
	@Offset		INT,
	@PageSize	INT = 10
AS
BEGIN

	SELECT
			SS.[id]
			,SS.[FeedProviderId]
			,SS.[StartDateTime]
			,ISNULL(dbo.UNIX_TIMESTAMP(SS.StartDateTime), 0) AS StartDateTimeStamp
			,SS.[ExpiryDateTime]
			,ISNULL(dbo.UNIX_TIMESTAMP(SS.ExpiryDateTime), 0) AS ExpiryDateTimeStamp
			,ISNULL(dbo.UNIX_TIMESTAMP(DATEADD(DAY, -5, GETUTCDATE())), 0) AS CurrentUtcTimestamp
			,SS.[LastExecutionDateTime]
			,SS.[NextPageUrlAfterExecution]
			,SS.[NextPageNumberAfterExecution]
			,SS.[IsEnabled]
			,SS.[SchedulerFrequencyId]
			,SS.[RecurranceInterval]
			,SS.[RecurranceDaysInWeek]
			,SS.[RecurranceMonths]
			,SS.[RecurranceDatesInMonth]
			,SS.[RecurranceWeekNos]
			,SS.[RecurranceDaysInWeekForMonth]			
			,IIF(SS.[IsTerminated] = 0, SS.[SchedulerLastStartTime], NULL) SchedulerLastStartTime
			,IIF(SS.[IsTerminated] = 0, SS.[SchedulerLastStartTimeStamp], NULL) SchedulerLastStartTimeStamp
			,ISNULL(SS.[IsStarted],0) IsStarted
			,ISNULL(SS.[IsCompleted],0) IsCompleted
			,ISNULL(SS.[IsTerminated],0) IsTerminated
			,FP.Name
			,FP.Name
			,FP.Source
			,FP.IsIminConnector
			,FP.FeedDataTypeId
			,FDT.Name AS FeedDataTypeName
			,FP.IsUsesTimestamp
			,FP.IsUtcTimestamp
			,FP.IsUsesChangenumber
			,FP.IsUsesUrlSlug
			,FP.EndpointUp
			,FP.UsesPagingSpec
			,FP.IsOpenActiveCompatible
			,FP.IncludesCoordinates
			,FP.IsFeedMappingConfirmed
	FROM	SchedulerSettings SS WITH (NOLOCK)
	INNER JOIN FeedProvider FP ON SS.FeedProviderId = FP.id AND FP.IsDeleted = 0 AND FP.IsFeedMappingConfirmed = 1
	INNER JOIN FeedDataType FDT ON FP.FeedDataTypeId = FDT.Id
	WHERE	SS.IsDeleted = 0
			AND GETUTCDATE() BETWEEN StartDateTime AND ISNULL(ExpiryDateTime, GETUTCDATE())
			AND IsEnabled = 1
			AND dbo.IsValidRecurranceForSchedulerFrequency(SchedulerFrequencyId
														,LastExecutionDateTime
														,RecurranceInterval
														,RecurranceDaysInWeek
														,RecurranceMonths
														,RecurranceDatesInMonth
														,RecurranceWeekNos
														,RecurranceDaysInWeekForMonth) = 1
			AND ISNULL(SS.IsStarted,0) = 0 
			--AND (ISNULL(SS.IsCompleted,0) = 1  OR ISNULL(SS.IsTerminated,0) = 1)
	ORDER BY SS.StartDateTime ASC	
END


GO
/****** Object:  StoredProcedure [dbo].[GetScheduledJobLogForFeedProvider]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetScheduledJobLogForFeedProvider]
	@Offset			INT = 0,
	@pageSize		INT = 10,
	@sortColumnNo	INT = 0,
	@sortDirection	NVARCHAR(50) = 'ASC',
	@searchText		NVARCHAR(50) = '',
	@feedProviderId BIGINT
AS
BEGIN
	
	DECLARE @SQL AS NVARCHAR(MAX) = ''

	DECLARE @SortColumn AS NVARCHAR(255) = ''

	IF @sortColumnNo = 0
	BEGIN
		SET @SortColumn = 'StartDate'		
	END	
	ELSE IF @sortColumnNo = 1
	BEGIN
		SET @SortColumn = 'EndDate'
	END	
	ELSE
	BEGIN
		SET @SortColumn = 'Id'
	END

	SET @SQL = 'SELECT	
						SJ.id
						,SJ.FeedProviderId
						,StartDate
						,EndDate
						,Status
						,AffectedEvents
				FROM	[SchedulerLog] SJ WITH (NOLOCK)
				INNER JOIN FeedProvider FP ON FP.id = SJ.FeedProviderId AND FP.IsDeleted = 0
				WHERE	SJ.FeedProviderId = ' + CONVERT(NVARCHAR, @feedProviderId) + '
				AND		StartDate IS NOT NULL
				AND		(
							''' + @searchText + ''' = ''''
							OR SJ.StartDate LIKE ''%' + @searchText + '%''
							OR SJ.EndDate LIKE ''%' + @searchText + '%''
							OR SJ.Status LIKE ''%' + @searchText + '%''
						)
				ORDER BY ' + @SortColumn + ' ' + @sortDirection + '
				OFFSET (' + CONVERT(NVARCHAR, @Offset) + ')*  ' + CONVERT(NVARCHAR, @pageSize) + ' ROWS
				FETCH NEXT ' + CONVERT(NVARCHAR, @pageSize) + ' ROWS ONLY '

	PRINT @SQL
	EXEC SP_EXECUTESQL @SQL

	SELECT	COUNT(1) 
	FROM	SchedulerLog S WITH(NOLOCK) 
	INNER JOIN [dbo].[FeedProvider] F WITH(NOLOCK) ON F.id = S.FeedProviderId AND F.IsDeleted = 0 
	WHERE	FeedProviderId = @feedProviderId

	SELECT	COUNT(1) 
	FROM	SchedulerLog SJ WITH (NOLOCK)
	INNER JOIN [dbo].[FeedProvider] F WITH(NOLOCK) ON F.id = SJ.FeedProviderId AND F.IsDeleted = 0 
	WHERE	FeedProviderId = @feedProviderId
			AND (
					@searchText = ''
					OR StartDate LIKE '%' + @searchText + '%'
					OR EndDate LIKE '%' + @searchText + '%'
					OR Status LIKE '%' + @searchText + '%'
				)

END
GO
/****** Object:  StoredProcedure [dbo].[GetSchedulerFrequencies]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetSchedulerFrequencies]
	
AS
BEGIN
	
	SELECT	Id
			,Name
	FROM	SchedulerFrequency

END

GO
/****** Object:  StoredProcedure [dbo].[GetSchedulerSettingsByFeedProvider]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSchedulerSettingsByFeedProvider]
	@FeedProviderId INT
AS
BEGIN
	
	SELECT	SS.[id]
			,SS.[StartDateTime]
			,ISNULL(dbo.UNIX_TIMESTAMP(SS.StartDateTime), 0) AS StartDateTimeStamp
			,SS.[ExpiryDateTime]
			,ISNULL(dbo.UNIX_TIMESTAMP(SS.ExpiryDateTime), 0) AS ExpiryDateTimeStamp
			,ISNULL(dbo.UNIX_TIMESTAMP(DATEADD(DAY, -5,GETUTCDATE())), 0) AS CurrentUtcTimestamp
			,SS.[LastExecutionDateTime]
			,SS.[NextPageUrlAfterExecution]
			,SS.[NextPageNumberAfterExecution]
			,SS.[IsEnabled]
			,SS.[SchedulerFrequencyId]
			,SS.[RecurranceInterval]
			,SS.[RecurranceDaysInWeek]
			,SS.[RecurranceMonths]
			,SS.[RecurranceDatesInMonth]
			,SS.[RecurranceWeekNos]
			,SS.[RecurranceDaysInWeekForMonth]
			--,SS.[SchedulerLastStartTime]
			--,SS.[SchedulerLastStartTimeStamp]
			,IIF(SS.[IsTerminated] = 0, SS.[SchedulerLastStartTime], NULL) SchedulerLastStartTime
			,IIF(SS.[IsTerminated] = 0, SS.[SchedulerLastStartTimeStamp], NULL) SchedulerLastStartTimeStamp
			,ISNULL(SS.[IsStarted],0) IsStarted
			,ISNULL(SS.[IsCompleted],0) IsCompleted
			,ISNULL(SS.[IsTerminated],0) IsTerminated
			,FP.id AS FeedProviderId
			,FP.Name
			,FP.Source
			,FP.IsIminConnector
			,FP.FeedDataTypeId
			,FDT.Name AS FeedDataTypeName
			,FP.IsUsesTimestamp
			,FP.IsUtcTimestamp
			,FP.IsUsesChangenumber
			,FP.IsUsesUrlSlug
			,FP.EndpointUp
			,FP.UsesPagingSpec
			,FP.IsOpenActiveCompatible
			,FP.IncludesCoordinates
			,FP.IsFeedMappingConfirmed
	FROM	FeedProvider FP WITH (NOLOCK)
	INNER JOIN FeedDataType FDT ON FP.FeedDataTypeId = FDT.Id
	LEFT JOIN [dbo].[SchedulerSettings] SS ON SS.FeedProviderId = FP.id AND SS.IsDeleted = 0
	WHERE	FP.Id = @FeedProviderId
	AND		FP.IsDeleted = 0

END
GO
/****** Object:  StoredProcedure [dbo].[GetSubEventByEventId]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSubEventByEventId]
	@eventId	BIGINT,
	@startDate	NVARCHAR(MAX) = NULL
AS
BEGIN
	IF EXISTS(SELECT 1 FROM EventOccurrence WITH (NOLOCK) WHERE EventId = @eventId AND SubEventId IS NOT NULL)
	BEGIN
		SELECT 
			super_e.id
			,super_e.FeedProviderId
			,LOWER(REPLACE(fp.Name, ' ', '' )) AS FeedName
			,super_e.FeedId
			,super_e.State 
			,ISNULL(e.ModifiedDate,super_e.ModifiedDate) AS ModifiedDate
			,ISNULL(e.ModifiedDateTimestamp,super_e.ModifiedDateTimestamp) AS ModifiedDateTimestamp
			--,dbo.UNIX_TIMESTAMP(ISNULL(e.ModifiedDate,super_e.ModifiedDate)) as ModifiedDateTimestamp
			,ISNULL(e.Name,super_e.Name) AS Name
			,ISNULL(e.Description,super_e.Description) AS Description
			,e.Image
			,e.ImageThumbnail
			,e_o.StartDate
			,e_o.EndDate
			,REPLACE(ISNULL(e.Duration,super_e.Duration),'-','') AS Duration
			,ISNULL(e.MaximumAttendeeCapacity,super_e.MaximumAttendeeCapacity) AS MaximumAttendeeCapacity
			,ISNULL(e.RemainingAttendeeCapacity,super_e.RemainingAttendeeCapacity) AS RemainingAttendeeCapacity
			,ISNULL(e.EventStatus,super_e.EventStatus) AS EventStatus
			,e.SuperEventId
			,ISNULL(e.Category,super_e.Category) AS Category
			,ISNULL(e.MinAge,super_e.MinAge) AS MinAge
			,ISNULL(e.MaxAge,super_e.MaxAge) AS MaxAge
			--,[dbo].[AgeRangeForGivenEvent](e.Id) AS AgeRange
			,[dbo].[GenderForGivenEvent] (e.Id,ISNULL(e.GenderRestriction,super_e.GenderRestriction)) AS GenderRestriction
			,ISNULL(e.AttendeeInstructions,super_e.AttendeeInstructions) AS AttendeeInstructions
			,ISNULL(e.AccessibilitySupport,super_e.AccessibilitySupport) AS AccessibilitySupport
			,ISNULL(e.AccessibilityInformation,super_e.AccessibilityInformation) AS AccessibilityInformation
			,ISNULL(e.IsCoached,super_e.IsCoached) AS IsCoached
			,ISNULL(e.Level,super_e.Level) AS Level
			,ISNULL(e.MeetingPoint,super_e.MeetingPoint) AS MeetingPoint
			,ISNULL(e.Identifier,super_e.Identifier) AS Identifier
			,ISNULL(e.URL,super_e.URL) AS URL
			,(SELECT DISTINCT STUFF((SELECT ',' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('')),1,1,'') Activities) AS Activity
			,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
			, NULL AS Distance
		FROM [dbo].[Event] e WITH (NOLOCK)
		INNER JOIN [dbo].[Event] super_e WITH (NOLOCK) ON super_e.id = e.SuperEventId
		INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = super_e.FeedProviderId AND fp.IsDeleted = 0
		INNER JOIN [dbo].[EventOccurrence]  e_o WITH (NOLOCK) ON e_o.SubEventId = e.id AND e_o.EventId = e.SuperEventId
		WHERE e.SuperEventId = @eventId
		AND (@startDate IS NULL OR e_o.StartDate = CONVERT(datetime, @startDate))
		AND super_e.state = 'updated' 
		AND super_e.FeedId IS NOT NULL	
	END
	ELSE IF EXISTS(SELECT 1 FROM EventOccurrence WITH (NOLOCK) WHERE EventId = @eventId AND SubEventId IS NULL)
	BEGIN
		SELECT 
			e.id
			,e.FeedProviderId
			,LOWER(REPLACE(fp.Name, ' ', '' )) AS FeedName
			,e.FeedId
			,e.State 
			,e.ModifiedDate
			,e.ModifiedDateTimestamp
			--,dbo.UNIX_TIMESTAMP(e.ModifiedDate) as ModifiedDateTimestamp
			,e.Name
			,e.Description
			,e.Image
			,e.ImageThumbnail
			,e_o.StartDate
			,e_o.EndDate
			,REPLACE(e.Duration,'-','') Duration
			,e.MaximumAttendeeCapacity
			,e.RemainingAttendeeCapacity
			,e.EventStatus
			,e.SuperEventId
			,e.Category
			,e.MinAge
			,e.MaxAge
			--,[dbo].[AgeRangeForGivenEvent](e.Id) AS AgeRange
			,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
			,e.AttendeeInstructions
			,e.AccessibilitySupport
			,e.AccessibilityInformation
			,e.IsCoached
			,e.Level
			,e.MeetingPoint
			,e.Identifier
			,e.URL			
			,(SELECT DISTINCT STUFF((SELECT ',' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('')),1,1,'') Activities) AS Activity
			,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
			, NULL AS Distance
		FROM [dbo].[Event] e WITH (NOLOCK)	 					
		INNER JOIN [dbo].[EventOccurrence] e_o WITH (NOLOCK) ON e_o.EventId = e.id
		INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = e.FeedProviderId AND fp.IsDeleted = 0
		WHERE e.id = @eventId
		AND (@startDate IS NULL OR e_o.StartDate = CONVERT(DATETIME, @startDate))
		AND e.state = 'updated' 
		AND e.FeedId IS NOT NULL	
	END
END

GO
/****** Object:  StoredProcedure [dbo].[GetSubEventBySessionId]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- EXEC [dbo].[GetSubEventBySessionId] '2579','opensessions','2018-11-05T20:00:00Z'
CREATE PROCEDURE [dbo].[GetSubEventBySessionId]
	@sessionId	NVARCHAR(50)
	,@FeedName	NVARCHAR(100)
	,@startDate NVARCHAR(MAX)
AS
BEGIN
	DECLARE @eventId BIGINT = (SELECT e.Id FROM event e WITH (NOLOCK)
								INNER JOIN FeedProvider fp WITH (NOLOCK) ON fp.id = e.FeedProviderId AND fp.IsDeleted = 0
								WHERE FeedId = @sessionId AND LOWER(REPLACE(fp.Name, ' ', '' )) = @FeedName)

	IF EXISTS(SELECT 1 FROM EventOccurrence WITH (NOLOCK) WHERE EventId = @eventId AND SubEventId IS NOT NULL)
	BEGIN
		SELECT 
			super_e.id
			,super_e.FeedProviderId
			,LOWER(REPLACE(fp.Name, ' ', '' )) AS FeedName
			,super_e.FeedId
			,super_e.State 
			,ISNULL(e.ModifiedDate,super_e.ModifiedDate) AS ModifiedDate
			,ISNULL(e.ModifiedDateTimestamp,super_e.ModifiedDateTimestamp) AS ModifiedDateTimestamp		
			--,dbo.UNIX_TIMESTAMP(ISNULL(e.ModifiedDate,super_e.ModifiedDate)) as ModifiedDateTimestamp				
			,ISNULL(e.Name,super_e.Name) AS Name
			,ISNULL(e.Description,super_e.Description) AS Description
			,e.Image
			,e.ImageThumbnail			
			,e_o.StartDate
			,e_o.EndDate
			,REPLACE(ISNULL(e.Duration,super_e.Duration),'-','') AS Duration
			,ISNULL(e.MaximumAttendeeCapacity,super_e.MaximumAttendeeCapacity) AS MaximumAttendeeCapacity
			,ISNULL(e.RemainingAttendeeCapacity,super_e.RemainingAttendeeCapacity) AS RemainingAttendeeCapacity
			,ISNULL(e.EventStatus,super_e.EventStatus) AS EventStatus
			,e.SuperEventId
			,ISNULL(e.Category,super_e.Category) AS Category
			,ISNULL(e.MinAge,super_e.MinAge) AS MinAge
			,ISNULL(e.MaxAge,super_e.MaxAge) AS MaxAge
			--,[dbo].[AgeRangeForGivenEvent](e.Id) AS AgeRange
			,[dbo].[GenderForGivenEvent] (e.Id,ISNULL(e.GenderRestriction,super_e.GenderRestriction)) AS GenderRestriction
			,ISNULL(e.AttendeeInstructions,super_e.AttendeeInstructions) AS AttendeeInstructions
			,ISNULL(e.AccessibilitySupport,super_e.AccessibilitySupport) AS AccessibilitySupport
			,ISNULL(e.AccessibilityInformation,super_e.AccessibilityInformation) AS AccessibilityInformation
			,ISNULL(e.IsCoached,super_e.IsCoached) AS IsCoached
			,ISNULL(e.Level,super_e.Level) AS Level
			,ISNULL(e.MeetingPoint,super_e.MeetingPoint) AS MeetingPoint
			,ISNULL(e.Identifier,super_e.Identifier) AS Identifier
			,ISNULL(e.URL,super_e.URL) AS URL
			,(SELECT DISTINCT STUFF((SELECT ',' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('')),1,1,'') Activities) AS Activity
			,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
			, NULL AS Distance
		FROM [dbo].[Event] e WITH (NOLOCK)
		INNER JOIN [dbo].[Event] super_e WITH (NOLOCK) ON super_e.id = e.SuperEventId
		INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = super_e.FeedProviderId AND fp.IsDeleted = 0
		INNER JOIN [dbo].[EventOccurrence]  e_o WITH (NOLOCK) ON e_o.SubEventId = e.id AND e_o.EventId = e.SuperEventId
		WHERE e.SuperEventId = @eventId		
		AND (@startDate IS NULL OR e_o.StartDate = CONVERT(datetime, @startDate))
		AND super_e.state = 'updated' 
		AND super_e.FeedId IS NOT NULL
		AND LOWER(REPLACE(fp.Name, ' ', '' )) = @FeedName	
	END
	ELSE IF EXISTS(SELECT 1 FROM EventOccurrence WITH (NOLOCK) WHERE EventId = @eventId AND SubEventId IS NULL)
	BEGIN
		SELECT 
			e.id
			,e.FeedProviderId
			,LOWER(REPLACE(fp.Name, ' ', '' )) AS FeedName
			,e.FeedId
			,e.State 
			,e.ModifiedDate
			,e.ModifiedDateTimestamp
			--,dbo.UNIX_TIMESTAMP(e.ModifiedDate) as ModifiedDateTimestamp
			,e.Name
			,e.Description
			,e.Image
			,e.ImageThumbnail			
			,e_o.StartDate
			,e_o.EndDate
			,REPLACE(e.Duration,'-','') Duration
			,e.MaximumAttendeeCapacity
			,e.RemainingAttendeeCapacity
			,e.EventStatus
			,e.SuperEventId
			,e.Category
			,e.MinAge
			,e.MaxAge
			--,[dbo].[AgeRangeForGivenEvent](e.Id) AS AgeRange
			,[dbo].[GenderForGivenEvent] (e.Id,e.GenderRestriction) AS GenderRestriction
			,e.AttendeeInstructions
			,e.AccessibilitySupport
			,e.AccessibilityInformation
			,e.IsCoached
			,e.Level
			,e.MeetingPoint
			,e.Identifier
			,e.URL			
			,(SELECT DISTINCT STUFF((SELECT ',' + CAST(PrefLabel AS nvarchar(MAX)) [text()] FROM PhysicalActivity WITH (NOLOCK) WHERE EventId = e.Id FOR XML PATH('')),1,1,'') Activities) AS Activity
			,(SELECT [dbo].[WeekDaysForGivenEvent](e.Id)) AS WeekDays
			, NULL AS Distance
		FROM [dbo].[Event] e WITH (NOLOCK)						
		INNER JOIN [dbo].[EventOccurrence] e_o WITH (NOLOCK) ON e_o.EventId = e.id
		INNER JOIN [dbo].[FeedProvider] fp WITH (NOLOCK) ON fp.Id = e.FeedProviderId AND fp.IsDeleted = 0
		WHERE e.id = @eventId		
		AND (@startDate IS NULL OR e_o.StartDate = CONVERT(DATETIME, @startDate))
		AND e.state = 'updated' 
		AND e.FeedId IS NOT NULL	
		AND LOWER(REPLACE(fp.Name, ' ', '' )) = @FeedName
	END
END
GO
/****** Object:  StoredProcedure [dbo].[IntelligentMapping_InsertUpdate]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[IntelligentMapping_InsertUpdate]
	@ParentId				BIGINT,
	@TableName				NVARCHAR(50),
	@ColumnName				NVARCHAR(50),
	@PossibleMatches		NVARCHAR(MAX),
	@PossibleHierarchies	NVARCHAR(MAX),
	@CustomCriteria			NVARCHAR(MAX)
AS
BEGIN
	
	IF(@TableName <> 'Custom')
	BEGIN
		-- update columnName based on parent's column name
		--IF EXISTS(SELECT	id 
		--			FROM	IntelligentMapping 
		--			WHERE	Id = ISNULL(@ParentId, 0))
		--BEGIN
		--	SET @ColumnName = (SELECT	ColumnName 
		--						FROM	IntelligentMapping 
		--						WHERE	id = @ParentId) + '_' + ISNULL(@ColumnName, '')
		--END

		-- insert feed key mapping
		IF NOT EXISTS(SELECT	id 
						FROM	IntelligentMapping WITH (NOLOCK) 
						WHERE	ISNULL(ParentId, 0) = ISNULL(@ParentId, 0)
								AND TableName = @TableName 
								AND ColumnName = @ColumnName)
		BEGIN
			INSERT INTO [dbo].[IntelligentMapping]
						([ParentId]
						,[TableName]
						,[ColumnName]
						,[PossibleMatches]
						,[PossibleHierarchies]
						,[CustomCriteria])
					VALUES
						(@ParentId
						,@TableName
						,@ColumnName
						,@PossibleMatches
						,@PossibleHierarchies
						,@CustomCriteria)
		END
		ELSE
		BEGIN
			-- update feed key mapping
			UPDATE	IntelligentMapping
			SET		PossibleMatches = CASE 
										WHEN ISNULL(PossibleMatches, '') <> '' 
											THEN (CASE WHEN CHARINDEX(@PossibleMatches COLLATE SQL_Latin1_General_CP1_CS_AS, PossibleMatches) <= 0
														THEN PossibleMatches + ',' + @PossibleMatches
													ELSE PossibleMatches
													END
													)
										ELSE @PossibleMatches
										END
					,PossibleHierarchies = CASE 
										WHEN ISNULL(PossibleHierarchies, '') <> '' 
											THEN (CASE WHEN CHARINDEX(@PossibleHierarchies COLLATE SQL_Latin1_General_CP1_CS_AS, PossibleHierarchies) <= 0
														THEN PossibleHierarchies + ',' + @PossibleHierarchies
													ELSE PossibleHierarchies
													END
													)
										ELSE @PossibleHierarchies
										END
			WHERE	ISNULL(ParentId, 0) = ISNULL(@ParentId, 0)
					AND TableName = @TableName
					AND ColumnName = @ColumnName
		END
	END
	

END

GO
/****** Object:  StoredProcedure [dbo].[IntelligentMapping_InsertUpdate_v1]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[IntelligentMapping_InsertUpdate_v1]
	@ParentId				BIGINT,
	@TableName				NVARCHAR(50),
	@ColumnName				NVARCHAR(50),
	@PossibleMatches		NVARCHAR(MAX),
	@PossibleHierarchies	NVARCHAR(MAX),
	@CustomCriteria			NVARCHAR(MAX),
	@IsCustomFeedKey		BIT,
	@RemoveFeedKey			BIT = 0
AS
BEGIN
	IF NOT EXISTS(SELECT id FROM IntelligentMapping WITH (NOLOCK)
			WHERE	ISNULL(ParentId, 0) = ISNULL(@ParentId, 0)
					AND TableName = @TableName 
					AND ColumnName = @ColumnName
					AND IsDeleted = 0)
	BEGIN
		DECLARE @Position INT
		SET @Position = (SELECT TOP 1 Position FROM IntelligentMapping WITH (NOLOCK) WHERE ParentId IS NULL ORDER BY 1 DESC)
		INSERT INTO [dbo].[IntelligentMapping]
					([ParentId]
					,[TableName]
					,[ColumnName]
					,[PossibleMatches]
					,[PossibleHierarchies]
					,[CustomCriteria]
					,[IsCustomFeedKey]
					,[Position])
				VALUES
					(@ParentId
					,@TableName
					,@ColumnName
					,@PossibleMatches
					,@PossibleHierarchies
					,@CustomCriteria
					,@IsCustomFeedKey
					,@Position + 1)
	END
	ELSE
	BEGIN		
		UPDATE	IntelligentMapping
		SET		PossibleMatches = CASE									
									WHEN ISNULL(PossibleMatches, '') <> '' 
									THEN (
										CASE 
											WHEN ISNULL(@RemoveFeedKey, '') = 0
											THEN (												
												CASE WHEN dbo.CheckStringExistanceFromDelimeterSeperatedString(PossibleMatches, @PossibleMatches, ',') = 0
													THEN PossibleMatches + ',' + @PossibleMatches
												ELSE PossibleMatches
												END
												)
											WHEN ISNULL(@RemoveFeedKey, '') = 1 AND RTRIM(LTRIM(PossibleMatches)) <> RTRIM(LTRIM(ISNULL(PossibleMatchesByManually,''))) COLLATE SQL_Latin1_General_CP1_CS_AS
											THEN (												
												dbo.ReplaceStringFromDelimeterSeperatedString(PossibleMatches, @PossibleMatches, ',')
												)
											ELSE PossibleMatches
										END
									)										
									ELSE @PossibleMatches
									END
				,PossibleHierarchies = CASE
									WHEN ISNULL(PossibleHierarchies, '') <> '' 
										THEN (
											CASE 
												WHEN ISNULL(@RemoveFeedKey, '') = 0
												THEN (												
													CASE WHEN dbo.CheckStringExistanceFromDelimeterSeperatedString(PossibleHierarchies, @PossibleHierarchies, ',') = 0
														THEN PossibleHierarchies + ',' + @PossibleHierarchies
													ELSE PossibleHierarchies
													END
												)
												WHEN ISNULL(@RemoveFeedKey, '') = 1 AND RTRIM(LTRIM(PossibleHierarchies)) <> RTRIM(LTRIM(ISNULL(PossibleHierarchiesByManually,''))) COLLATE SQL_Latin1_General_CP1_CS_AS
												THEN (
													dbo.ReplaceStringFromDelimeterSeperatedString(PossibleHierarchies, @PossibleHierarchies, ',')												
													)
												ELSE PossibleHierarchies
											END
										)
									ELSE @PossibleHierarchies
									END
		WHERE	ISNULL(ParentId, 0) = ISNULL(@ParentId, 0)
				AND TableName = @TableName
				AND ColumnName = @ColumnName
	END
END


GO
/****** Object:  StoredProcedure [dbo].[Offer_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <18-03-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Offer_Delete]
	@EventId BIGINT
AS
BEGIN
	DELETE FROM [dbo].[Offer]
	WHERE [EventId] = @EventId
END
GO
/****** Object:  StoredProcedure [dbo].[Offer_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <18-03-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Offer_Insert]
	@EventId BIGINT,
	@Identifier NVARCHAR(100),
	@Name NVARCHAR(MAX),
	@Price NVARCHAR(50),
	@PriceCurrency NVARCHAR(50),
	@Description NVARCHAR(MAX),
    @SlotId BIGINT = NULL,
	@OfferId BIGINT OUT
AS
BEGIN
	IF NOT EXISTS(SELECT [id] FROM [dbo].[Offer] WITH(NOLOCK) WHERE [EventId] = @EventId AND ISNULL(Price,'') = ISNULL(@Price,'') AND ISNULL(PriceCurrency,'') = ISNULL(@PriceCurrency,''))
	BEGIN
		INSERT INTO [dbo].[Offer]
		(
			[EventId],
            [SlotId],
			[Identifier],
			[Name],
			[Price],
			[PriceCurrency],
			[Description]
		)VALUES
		(
			@EventId,
            @SlotId,
			@Identifier,
			@Name,
			@Price,
			@PriceCurrency,
			@Description
		)
		SET @OfferId = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		SELECT @OfferId = id 
		FROM [dbo].[Offer] WITH (NOLOCK)
		WHERE [EventId] = @EventId 
		AND [Price] = @Price 
		AND [PriceCurrency] = @PriceCurrency
		
		UPDATE [dbo].[Offer]
		SET [Identifier] = @Identifier,
            [SlotId] = @SlotId,
			[Name] = @Name,
			[Price] = @Price,
			[PriceCurrency] = @PriceCurrency 
			WHERE [id] = @OfferId 
			AND [EventId] = @EventId
	END
END
GO
/****** Object:  StoredProcedure [dbo].[Organization_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Organization_Delete]
	@EventId BIGINT
AS
BEGIN
	DELETE	FROM Organization
	WHERE	EventId = @EventId
END


GO
/****** Object:  StoredProcedure [dbo].[Organization_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Organization_Insert]
	@EventId		BIGINT,
	@Name			NVARCHAR(50),
	@Description	NVARCHAR(MAX),
	@Email			NVARCHAR(100),
	@Image			NVARCHAR(MAX),
	@URL			NVARCHAR(MAX),
	@Telephone		NVARCHAR(50),
	@OrganizationId BIGINT OUT
AS
BEGIN
	
	IF NOT EXISTS(SELECT [id] 
				FROM [dbo].[Organization] WITH (NOLOCK)
				WHERE  [EventId] = @EventId
						AND [Name] = @Name
						AND ISNULL([Email], '') = ISNULL(@Email, '')
						AND ISNULL([Telephone], '') = ISNULL(@Telephone, '')
						AND ISNULL([Image], '') = ISNULL(@Image, '')
						AND ISNULL([URL], '') = ISNULL(@URL, ''))
	BEGIN
		INSERT INTO [dbo].[Organization]
				   ([EventId]
				   ,[Name]
				   ,[Description]
				   ,[Email]
				   ,[Image]
				   ,[URL]
				   ,[Telephone])
			 VALUES
				   (@EventId
				   ,@Name
				   ,@Description
				   ,@Email
				   ,@Image
				   ,@URL
				   ,@Telephone)

		SET @OrganizationId = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		SELECT	@OrganizationId = [id]
		FROM	[dbo].[Organization] WITH (NOLOCK)
		WHERE	[EventId] = @EventId
				AND [Name] = @Name
				AND ISNULL([Email], '') = ISNULL(@Email, '')
				AND ISNULL([Telephone], '') = ISNULL(@Telephone, '')
				AND ISNULL([Image], '') = ISNULL(@Image, '')
				AND ISNULL([URL], '') = ISNULL(@URL, '')

		UPDATE [dbo].[Organization]
		SET [Name] = @Name,
			[Description] = @Description,
			[Email] = @Email,
			[Image] = @Image,
			[URL] = @URL,
			[Telephone] = @Telephone
		WHERE [id] = @OrganizationId 
		AND EventId = @EventId
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[Person_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Person_Delete]
	@EventId BIGINT
AS
BEGIN
	DELETE	FROM Person
	WHERE	EventId = @EventId
END


GO
/****** Object:  StoredProcedure [dbo].[Person_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Person_Insert]
	@EventId		BIGINT,
	@IsLeader		BIT,
	@Name			NVARCHAR(50),
	@Description	NVARCHAR(MAX),
	@Email			NVARCHAR(100),
	@Image			NVARCHAR(MAX),
	@URL			NVARCHAR(MAX),
	@Telephone		NVARCHAR(50),
	@PersonId		BIGINT OUT
AS
BEGIN	
	IF NOT EXISTS(SELECT [id] 
					FROM [dbo].[Person] WITH (NOLOCK)
					WHERE [EventId] = @EventId
							AND [Name] = @Name
							AND ISNULL([IsLeader], 0) = ISNULL(@IsLeader, 0)
							AND ISNULL([Email], '') = ISNULL(@Email, '')
							AND ISNULL([Telephone], '') = ISNULL(@Telephone, '')
							AND ISNULL([Image], '') = ISNULL(@Image, '')
							AND ISNULL([URL], '') = ISNULL(@URL, ''))
	BEGIN

		INSERT INTO [dbo].[Person]
					([EventId]
					,[IsLeader]
					,[Name]
					,[Description]
					,[Email]
					,[Image]
					,[URL]
					,[Telephone])
				VALUES
					(@EventId
					,@IsLeader
					,@Name
					,@Description
					,@Email
					,@Image
					,@URL
					,@Telephone)

		set @PersonId = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		SELECT	@PersonId = [id]
		FROM	[dbo].[Person] WITH (NOLOCK)
		WHERE	[EventId] = @EventId
				AND [Name] = @Name
				AND ISNULL([IsLeader], 0) = ISNULL(@IsLeader, 0)
				AND ISNULL([Email], '') = ISNULL(@Email, '')
				AND ISNULL([Telephone], '') = ISNULL(@Telephone, '')
				AND ISNULL([Image], '') = ISNULL(@Image, '')
				AND ISNULL([URL], '') = ISNULL(@URL, '')

		UPDATE [dbo].[Person] SET
		[IsLeader] = @IsLeader,
		[Name] = @Name,
		[Description] = @Description,
		[Email] = @Email,
		[Image] = @Image,
		[URL] = @URL,
		[Telephone] = @Telephone
		WHERE [id] = @PersonId 
		AND [EventId] = @EventId
	END
END
GO
/****** Object:  StoredProcedure [dbo].[PhysicalActivity_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[PhysicalActivity_Delete]
	@EventId BIGINT
AS
BEGIN

	DELETE FROM PhysicalActivity
	WHERE EventId = @EventId

END
GO
/****** Object:  StoredProcedure [dbo].[PhysicalActivity_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[PhysicalActivity_Insert]
	@EventId			BIGINT,
	@PrefLabel			NVARCHAR(50),
	@AltLabel			NVARCHAR(50),
	@InScheme			NVARCHAR(50),
	@Notation			NVARCHAR(50),
	@BroaderId			BIGINT,
	@NarrowerId			BIGINT,
    @Image              NVARCHAR(255)=NULL,
    @Description        NVARCHAR(MAX)=NULL,
	@PhysicalActivityId BIGINT OUT
AS
BEGIN
	
	IF NOT EXISTS (SELECT [id] FROM [dbo].[PhysicalActivity] WITH (NOLOCK) WHERE [EventId] = @EventId AND [PrefLabel] = @PrefLabel)
	BEGIN
		INSERT INTO [dbo].[PhysicalActivity]
				   ([EventId]
				   ,[PrefLabel]
				   ,[AltLabel]
				   ,[InScheme]
				   ,[Notation]
				   ,[BroaderId]
				   ,[NarrowerId]
                   ,[Image]
                   ,[Description]
				   )
			 VALUES
				   (@EventId
				   ,@PrefLabel
				   ,@AltLabel
				   ,@InScheme
				   ,@Notation
				   ,@BroaderId
				   ,@NarrowerId
                   ,@Image
                   ,@Description
				   )

		SET @PhysicalActivityId = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		SELECT @PhysicalActivityId = [id] 
		FROM [dbo].[PhysicalActivity] WITH (NOLOCK)
		WHERE [EventId] = @EventId AND [PrefLabel] = @PrefLabel

		UPDATE [dbo].[PhysicalActivity]
		   SET [AltLabel] = @AltLabel
			  ,[InScheme] = @InScheme
			  ,[Notation] = @Notation
			  ,[BroaderId] = @BroaderId
			  ,[NarrowerId] = @NarrowerId
              ,[Image] = @Image
              ,[Description] = @Description
		 WHERE [id] = @PhysicalActivityId 
				AND [EventId] = @EventId
	END
END

GO
/****** Object:  StoredProcedure [dbo].[Place_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Place_Delete]
	@EventId BIGINT
AS
BEGIN
	DELETE	FROM Place
	WHERE	EventId = @EventId
END


GO
/****** Object:  StoredProcedure [dbo].[Place_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Place_Insert]
	@EventId		BIGINT,
	@ParentId		BIGINT,
	@PlaceTypeId	INT,
	@Name			NVARCHAR(50),
	@Description	NVARCHAR(MAX),
	@Image			NVARCHAR(MAX),
	@Address		NVARCHAR(MAX),
	@Lat			NVARCHAR(50),
	@Long			NVARCHAR(50),
	@Telephone		NVARCHAR(12),
	@FaxNumber		NVARCHAR(20),
	@Url			NVARCHAR(MAX),
	@StreetAddress  NVARCHAR(MAX)=NULL,
	@AddressLocality NVARCHAR(MAX)=NULL,
	@PostalCode     NVARCHAR(255)=NULL,
	@Region         NVARCHAR(255)=NULL,
    @OpeningHoursSpecification NVARCHAR(MAX) = NULL,
	@PlaceId		BIGINT OUT
AS
BEGIN
	
	IF(ISNULL(@Name, '') <> '' 
		OR ISNULL(@Description, '') <> ''
		OR ISNULL(@Image, '') <> ''
		OR ISNULL(@Address, '') <> ''
		OR ISNULL(@Lat, '') <> ''
		OR ISNULL(@Long, '') <> ''
		OR ISNULL(@Telephone, '') <> ''
		OR ISNULL(@FaxNumber, '') <> ''
		OR ISNULL(@URL, '') <> ''
		)
	BEGIN
		IF NOT EXISTS(SELECT [id] 
						FROM [dbo].[Place] WITH (NOLOCK)
						WHERE  [EventId] = @EventId
								AND ISNULL([Name], '') = ISNULL(@Name, '')
								AND ISNULL([Description], '') = ISNULL(@Description, '')
								AND ISNULL([Image], '') = ISNULL(@Image, '')
								AND ISNULL([Address], '') = ISNULL(@Address, '')
								AND ISNULL([Lat], '') = ISNULL(@Lat, '')
								AND ISNULL([Long], '') = ISNULL(@Long, '') )
		BEGIN
			-- insert for the same
			INSERT INTO [dbo].[Place]
					   ([EventId]
					   ,[ParentId]
					   ,[PlaceTypeId]
					   ,[Name]
					   ,[Description]
					   ,[Image]
					   ,[Address]
					   ,[Lat]
					   ,[Long]
					   ,[Telephone]
					   ,[FaxNumber]
					   ,[URL]
					   ,[StreetAddress]
					   ,[AddressLocality]
					   ,[PostalCode]
					   ,[Region]
                       ,[OpeningHoursSpecification]
					   )
				 VALUES
					   (@EventId
					   ,@ParentId
					   ,@PlaceTypeId
					   ,@Name
					   ,@Description
					   ,@Image
					   ,@Address
					   ,@Lat
					   ,@Long
					   ,@Telephone
					   ,@FaxNumber
					   ,@URL
					   ,@StreetAddress
					   ,@AddressLocality
					   ,@PostalCode
					   ,@Region
                       ,@OpeningHoursSpecification)

			set @PlaceId = SCOPE_IDENTITY()
		END
		ELSE
		BEGIN
			SELECT	@PlaceId = [id] 
			FROM	[dbo].[Place] WITH (NOLOCK)
			WHERE	[EventId] = @EventId
					AND ISNULL([Name], '') = ISNULL(@Name, '')
					AND ISNULL([Description], '') = ISNULL(@Description, '')
					AND ISNULL([Image], '') = ISNULL(@Image, '')
					AND ISNULL([Address], '') = ISNULL(@Address, '')
					AND ISNULL([Lat], '') = ISNULL(@Lat, '')
					AND ISNULL([Long], '') = ISNULL(@Long, '')

			UPDATE [dbo].[Place] SET
			[ParentId] = @ParentId,
			[PlaceTypeId] = @PlaceTypeId,
			[Name] = @Name,
			[Description] = @Description,
			[Image] = @Image,
			[Address] = @Address,
			[Lat] = @Lat,
 			[Long] = @Long,
			[Telephone] = @Telephone,
			[FaxNumber] = @FaxNumber,
			[URL] = @Url,
			[StreetAddress] = @StreetAddress,
			[AddressLocality] = @AddressLocality,
			[PostalCode] = @PostalCode,
			[Region] = @Region,
            [OpeningHoursSpecification]=@OpeningHoursSpecification
			WHERE [id] = @PlaceId 
			AND [EventId] = @EventId
			
		END
	END
	
END
GO
/****** Object:  StoredProcedure [dbo].[Programme_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Programme_Delete]
	@EventId BIGINT
AS
BEGIN
	DELETE	FROM Programme
	WHERE	EventId = @EventId
END


GO
/****** Object:  StoredProcedure [dbo].[Programme_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[Programme_Insert]
	@EventId		BIGINT,
	@Name			NVARCHAR(50),
	@Description	NVARCHAR(MAX),
	@Url			NVARCHAR(MAX),
	@Image			NVARCHAR(MAX),
	@ProgrammeId	BIGINT OUT
AS
BEGIN
	
	IF NOT EXISTS(SELECT [id] 
					FROM [dbo].[Programme] WITH (NOLOCK)
					WHERE [EventId] = @EventId AND [Name] = @Name)
	BEGIN
		INSERT INTO [dbo].[Programme]
				   ([EventId]
				   ,[Name]
				   ,[Description]
				   ,[Image]
				   ,[URL])
			 VALUES
				   (@EventId
				   ,@Name
				   ,@Description
				   ,@Image
				   ,@URL)

		SET @ProgrammeId = SCOPE_IDENTITY()
	END
	ELSE
	BEGIN
		SELECT	@ProgrammeId = [id]
		FROM	[dbo].[Programme] WITH (NOLOCK)
		WHERE	[EventId] = @EventId
				AND [Name] = @Name

		UPDATE [dbo].[Programme] SET
			[Name] = @Name,
			[Description] = @Description,
			[Image] = @Image,
			[URL] = @Url
		WHERE [id] = @ProgrammeId
		AND	[EventId] = @EventId
	END

END
GO
/****** Object:  StoredProcedure [dbo].[Rule_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <26-12-2018>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Rule_Delete]
	@id INT
AS
BEGIN
	UPDATE [Rule]
	SET IsDeleted = 1, IsEnabled = 0,UpdatedOn=GETUTCDATE()
	WHERE Id = @id AND IsDeleted = 0

	DELETE FROM FilterCriteria 
	WHERE RuleId = @id

	DELETE FROM Operation 
	WHERE RuleId = @id
END
GO
/****** Object:  StoredProcedure [dbo].[Rule_Update]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <11-01-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Rule_Update]
	@Id INT,
	@IsEnable BIT
AS
BEGIN
	UPDATE [Rule] SET IsEnabled = @IsEnable,UpdatedOn=GETUTCDATE()
	WHERE Id = @Id
END
GO
/****** Object:  StoredProcedure [dbo].[Rules_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <31-12-2018>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Rules_Insert]
	@Id INT=0,
	@FeedProviderId INT,
	@RuleName NVARCHAR(255),
	@IsEnabled BIT=1,
	@OperationTypeId INT NULL,
	@TTFilterCriteria  AS TTFilterCriteria  READONLY,
	@TTOperation AS TTOperation READONLY	
AS
BEGIN
	BEGIN TRY
		BEGIN TRANSACTION;

		INSERT INTO [Rule]
		(
			FeedProviderId,
			RuleName,
			IsEnabled,
			IsDeleted,
			CreatedOn
		)VALUES
		(
			@FeedProviderId,
			@RuleName,
			@IsEnabled,
			0,
			GETDATE()
		)
		SET @Id = SCOPE_IDENTITY();

		DECLARE @FilterCriteriaId INT,
				@RuleId INT,
				@FieldMappingId INT,
				@FieldId NVARCHAR(250),
				@OperatorId INT,
				@Value NVARCHAR(MAX),
				@OperationId INT,
				@ParentId INT,
				@ParentFilterCriteriaId INT;
		DECLARE cursor_filtercriteria CURSOR STATIC FOR 
		SELECT FilterCriteriaId,RuleId,FieldMappingId,FieldId,OperatorId,Value,OperationId,ParentId FROM @TTFilterCriteria
		OPEN cursor_filtercriteria
		IF @@CURSOR_ROWS > 0
			BEGIN 
			FETCH NEXT FROM cursor_filtercriteria INTO @FilterCriteriaId,@RuleId,@FieldMappingId,@FieldId,@OperatorId,@Value,@OperationId,@ParentId
			WHILE @@Fetch_status = 0
			BEGIN
			
				--SELECT @FieldMappingId = id FROM FeedMapping 
				--WHERE FeedKeyPath = @FieldId 
				--		AND FeedProviderId = @FeedProviderId		
				IF 	ISNULL(@ParentId,0) = 0
				BEGIN
					SET @ParentFilterCriteriaId = NULL;
					INSERT INTO FilterCriteria
					(
						RuleId,
						FieldMappingId,
						OperatorId,
						Value,
						OperationId
					)
					VALUES
					(
						@Id,
						@FieldId,
						@OperatorId,
						@Value,
						@OperationId
					)
				SET	@ParentFilterCriteriaId = SCOPE_IDENTITY();
				END
				ELSE
				BEGIN
					INSERT INTO FilterCriteria
					(
						RuleId,
						FieldMappingId,
						OperatorId,
						Value,
						OperationId,
						ParentId
					)
					VALUES
					(
						@Id,
						@FieldId,
						@OperatorId,
						@Value,
						@OperationId,
						@ParentFilterCriteriaId
					)			
				END				
				
				SET @FilterCriteriaId = NULL;
				SET @RuleId = NULL;
				SET @FieldMappingId = NULL;
				SET @FieldId = NULL;
				SET @OperatorId = NULL;
				SET @Value = NULL;
				SET @OperationId = NULL;
				SET @ParentId = NULL;
			FETCH NEXT FROM cursor_filtercriteria INTO @FilterCriteriaId,@RuleId,@FieldMappingId,@FieldId,@OperatorId,@Value,@OperationId,@ParentId
			END
		END
		CLOSE cursor_filtercriteria
		DEALLOCATE cursor_filtercriteria

		INSERT INTO Operation
		(
			RuleId,
			OperationTypeId,
			FieldId,
			Value,
			CurrentWord,
			NewWord,
			Sentance,
			FirstFieldId,
			SecondFieldId
		)
		SELECT @Id,
			   @OperationTypeId,
		 TTO.FieldId,
		 TTO.Value,
		 TTO.CurrentWord,
		 TTO.NewWord,
		 TTO.Sentance,
		 TTO.FirstFieldId,
		 TTO.SecondFieldId
		FROM @TTOperation TTO

		COMMIT;
	END TRY
	BEGIN CATCH
	ROLLBACK;
	THROW
	END CATCH
END
GO
/****** Object:  StoredProcedure [dbo].[SchedulerLog_InsertUpdate]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SchedulerLog_InsertUpdate]
	@Id				INT,
	@FeedProviderId INT,
	@StartDate		DATETIME2(7),
	@EndDate		DATETIME2(7),
	@Status			NVARCHAR(50),
	@Note			NVARCHAR(500) = NULL
AS
BEGIN
	
	IF(EXISTS (SELECT 1 FROM SchedulerLog WITH (NOLOCK) WHERE Id = @Id))
	BEGIN
		UPDATE	SchedulerLog
		SET		EndDate = @EndDate,
				Status = @Status,
				Note = @Note,
				AffectedEvents = (SELECT COUNT(1) FROM event WITH (NOLOCK) WHERE FeedProviderId = SchedulerLog.FeedProviderId
									AND FeedId IS NOT NULL
									AND InsertedDateTime >= SchedulerLog.StartDate OR UpdatedDateTime >= SchedulerLog.StartDate)
		WHERE	Id = @Id		
	END
	ELSE
	BEGIN
		INSERT INTO [dbo].[SchedulerLog]
				   ([FeedProviderId]
				   ,[StartDate]
				   ,[EndDate]
				   ,[Status]
				   ,[Note])
			 VALUES
				   (@FeedProviderId
				   ,@StartDate
				   ,@EndDate
				   ,@Status
				   ,@Note)
		SET  @Id = SCOPE_IDENTITY();
	END
	SELECT @Id;
END
GO
/****** Object:  StoredProcedure [dbo].[SchedulerSettings_InsertUpdate]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SchedulerSettings_InsertUpdate]
	@Id								INT,
	@FeedProviderId					INT,
	@StartDateTime					DATETIME,
	@ExpiryDateTime					DATETIME,
	@IsEnabled						BIT,
	@SchedulerFrequencyId			INT,
	@RecurranceInterval				INT,
	@RecurranceDaysInWeek			NVARCHAR(200),
	@RecurranceMonths				NVARCHAR(200),
	@RecurranceDatesInMonth			NVARCHAR(200),
	@RecurranceWeekNos				NVARCHAR(50),
	@RecurranceDaysInWeekForMonth	NVARCHAR(200)
AS
BEGIN
	
	IF NOT EXISTS (SELECT ID 
					FROM SchedulerSettings WITH (NOLOCK)
					WHERE ID = @Id)
	BEGIN
		INSERT INTO [dbo].[SchedulerSettings]
				   ([FeedProviderId]
				   ,[StartDateTime]
				   ,[ExpiryDateTime]
				   ,[IsEnabled]
				   ,[SchedulerFrequencyId]
				   ,[RecurranceInterval]
				   ,[RecurranceDaysInWeek]
				   ,[RecurranceMonths]
				   ,[RecurranceDatesInMonth]
				   ,[RecurranceWeekNos]
				   ,[RecurranceDaysInWeekForMonth])
			 VALUES
				   (@FeedProviderId
				   ,@StartDateTime
				   ,@ExpiryDateTime
				   ,@IsEnabled
				   ,@SchedulerFrequencyId
				   ,@RecurranceInterval
				   ,@RecurranceDaysInWeek
				   ,@RecurranceMonths
				   ,@RecurranceDatesInMonth
				   ,@RecurranceWeekNos
				   ,@RecurranceDaysInWeekForMonth)
	END
	ELSE
	BEGIN
		UPDATE [dbo].[SchedulerSettings]
		   SET [FeedProviderId] = @FeedProviderId
			  ,[StartDateTime] = @StartDateTime
			  ,[ExpiryDateTime] = @ExpiryDateTime
			  ,[IsEnabled] = @IsEnabled
			  ,[SchedulerFrequencyId] = @SchedulerFrequencyId
			  ,[RecurranceInterval] = @RecurranceInterval
			  ,[RecurranceDaysInWeek] = @RecurranceDaysInWeek
			  ,[RecurranceMonths] = @RecurranceMonths
			  ,[RecurranceDatesInMonth] = @RecurranceDatesInMonth
			  ,[RecurranceWeekNos] = @RecurranceWeekNos
			  ,[RecurranceDaysInWeekForMonth] = @RecurranceDaysInWeekForMonth
			  ,[LastExecutionDateTime] = IIF(@StartDateTime = StartDateTime,LastExecutionDateTime, NULL)
			  ,[UpdatedDatetime] = GETDATE()
		 WHERE id = @Id
	END
END
GO
/****** Object:  StoredProcedure [dbo].[SchedulerSettings_UpdateLastExecutionDetails]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[SchedulerSettings_UpdateLastExecutionDetails]
	@Id								INT,
	@NextPageUrlAfterExecution		NVARCHAR(MAX),
	@NextPageNumberAfterExecution	INT
AS
BEGIN	
	UPDATE	SchedulerSettings
	SET		NextPageUrlAfterExecution = @NextPageUrlAfterExecution
			,NextPageNumberAfterExecution = @NextPageNumberAfterExecution
			,LastExecutionDateTime = GETUTCDATE()
			,UpdatedDatetime = GETDATE()
	WHERE	id = @Id

	----Insert into LastExecution Log
	----IF(EXISTS(SELECT 1 FROM SchedulerLastExecutionLog LE
	----			INNER JOIN SchedulerSettings SS ON LE.SchedulerSettingsId = SS.Id
	----			WHERE LE.SchedulerSettingsId = @Id 
	----			AND LE.LastExecutionDateTime = SS.LastExecutionDateTime))
	----BEGIN
	--	INSERT INTO [dbo].[SchedulerLastExecutionLog]
	--			   ([FeedProviderId]
	--			   ,[SchedulerSettingsId]
	--			   ,[LastExecutionDateTime])
	--		 SELECT
	--			   FeedProviderId
	--			   ,Id
	--			   ,LastExecutionDateTime
	--		 FROM SchedulerSettings
	--		 WHERE Id = @Id
	----END
END
GO
/****** Object:  StoredProcedure [dbo].[SchedulerSettings_UpdateStatus]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SchedulerSettings_UpdateStatus]
	@Id				INT,
	@IsStarted		BIT = 0,
	@IsCompleted	BIT = 0,
	@IsTerminated	BIT = 0
AS
BEGIN
	DECLARE @IsStartedVal AS BIT = (SELECT ISNULL(IsStarted,0) FROM SchedulerSettings WITH(NOLOCK) WHERE id = @Id);
		
	IF @IsStartedVal <> @IsStarted
	BEGIN
		UPDATE	SchedulerSettings
		SET		IsStarted = IIF((@IsCompleted = 1 OR @IsTerminated = 1), 0,@IsStarted)			
				,IsCompleted = IIF((@IsStarted = 1 OR @IsTerminated = 1), 0,@IsCompleted)
				,IsTerminated = IIF((@IsStarted = 1 OR @IsCompleted = 1), 0,@IsTerminated)	
				--IsStarted = @IsStarted
				--,IsCompleted = @IsCompleted	
				--,IsTerminated = @IsTerminated	
				,SchedulerLastStartTime = IIF(@IsStarted = 1, GETDATE(),SchedulerLastStartTime)
				,SchedulerLastStartTimeStamp = IIF(@IsStarted = 1, dbo.UNIX_TIMESTAMP(GETDATE()),SchedulerLastStartTimeStamp)
				,UpdatedDatetime = GETDATE()
		WHERE	id = @Id

		--Insert into LastExecution Log
		--IF(EXISTS(SELECT 1 FROM SchedulerLastExecutionLog LE
		--			INNER JOIN SchedulerSettings SS ON LE.SchedulerSettingsId = SS.Id
		--			WHERE LE.SchedulerSettingsId = @Id 
		--			AND LE.LastExecutionDateTime = SS.LastExecutionDateTime))
		--BEGIN
			
		--IF(@IsCompleted = 1 OR @IsTerminated = 1)
		--BEGIN
			INSERT INTO [dbo].[SchedulerLastExecutionLog]
					   ([FeedProviderId]
					   ,[SchedulerSettingsId]
					   ,[LastExecutionDateTime])
				 SELECT
					   FeedProviderId
					   ,Id
					   ,LastExecutionDateTime
				 FROM SchedulerSettings WITH (NOLOCK)
				 WHERE Id = @Id
		--END
		--END
	END	
END
GO
/****** Object:  StoredProcedure [dbo].[SchedulerSettingsDisable]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Olson Fernandes>
-- Create date: <20-09-2019>
-- Description:	<Description>
-- =============================================
Create PROCEDURE [dbo].[SchedulerSettingsDisable]
	@FeedProviderId INT,
	@IsEnable BIT = 0
AS
BEGIN
	UPDATE [SchedulerSettings] SET IsEnabled = @IsEnable
	WHERE FeedProviderId = @FeedProviderId
END

GO
/****** Object:  StoredProcedure [dbo].[Slot_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Slot_Delete]
(
    @EventId BIGINT
) AS
BEGIN
    DELETE FROM [dbo].[Slot]
    WHERE [EventId] = @EventId
END
GO
/****** Object:  StoredProcedure [dbo].[Slot_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <18-03-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[Slot_Insert]
	@EventId BIGINT,
    @Identifier NVARCHAR(50) = NULL,
	@StartDate DATETIME = NULL,
    @EndDate DATETIME = NULL,
    @Duration NVARCHAR(20) = NULL,
    @OfferID BIGINT = NULL,
    @RemainingUses NVARCHAR(5) = NULL,
    @MaximumUses NVARCHAR(5) = NULL,
	@slotId BIGINT OUT
AS
BEGIN
	IF NOT EXISTS(SELECT [id] FROM [dbo].[Slot] WITH(NOLOCK) WHERE [EventId] = @EventId AND [StartDate] = @StartDate AND [RemainingUses] = @RemainingUses AND [MaximumUses] = @MaximumUses)
	BEGIN
		INSERT INTO [dbo].[Slot]
        (
             [EventId]
            ,[Identifier]
            ,[StartDate]
            ,[EndDate]
            ,[Duration]
            ,[OfferID]
            ,[RemainingUses]
            ,[MaximumUses]
        )
        VALUES
        (
             @EventId
            ,@Identifier
            ,@StartDate
            ,@EndDate
            ,@Duration
            ,@OfferID
            ,@RemainingUses
            ,@MaximumUses
        )
        SET @slotId = SCOPE_IDENTITY();
	END	
END
GO
/****** Object:  StoredProcedure [dbo].[spr_AutoFlushEvent_Delete]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <04-02-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_AutoFlushEvent_Delete]
	@EventID NVARCHAR(MAX)
AS
BEGIN	
	DELETE 
	FROM [dbo].[Event] 
	WHERE SuperEventId IN(SELECT * FROM [dbo].[SplitStringByComma](@EventID))
	DELETE
	FROM [dbo].[Event] 
	WHERE id IN(SELECT * FROM [dbo].[SplitStringByComma](@EventID))	
END
GO
/****** Object:  StoredProcedure [dbo].[spr_AutoFlushEvent_Delete_27-03-2019]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <04-02-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_AutoFlushEvent_Delete_27-03-2019]
	@EventID BIGINT
AS
BEGIN	
	DELETE FROM [dbo].[Event] WHERE SuperEventId = @EventID
	DELETE FROM [dbo].[Event] WHERE id = @EventID
END
GO
/****** Object:  StoredProcedure [dbo].[spr_CheckPlaceData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <28-02-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_CheckPlaceData]
	@FeedProviderId INT
AS
BEGIN
	/*First We Check in table if data exists for same postcode then update Place table*/
	UPDATE [dbo].[Place] 
	SET Lat = PC.Lat,
		Long = PC.Long,
		IsUpdate = 1
	FROM [dbo].[UKPostalCode] PC WITH(NOLOCK)
	INNER JOIN [dbo].[Event] E WITH(NOLOCK) ON E.FeedProviderId = @FeedProviderId AND E.FeedId IS NOT NULL
	INNER JOIN [dbo].[Place] P WITH(NOLOCK) ON P.EventId = E.id AND P.PostalCode = PC.PostalCode
	WHERE P.Lat IS NULL AND 
		  P.Long IS NULL AND 
		  P.PostalCode IS NOT NULL

	SELECT DISTINCT P.PostalCode 
	FROM [dbo].[Place] P WITH(NOLOCK)
	INNER JOIN [dbo].[Event] E WITH(NOLOCK) ON 
	E.FeedProviderId = @FeedProviderId 
	AND E.FeedId IS NOT NULL
	AND	E.id = P.EventId
	WHERE P.PostalCode IS NOT NULL 
	AND P.Lat IS NULL 
	AND p.Long IS NULL
END
GO
/****** Object:  StoredProcedure [dbo].[spr_CountTableRow]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <20-02-2019>
-- Description:	<Count Every table data >
-- =============================================
CREATE PROCEDURE [dbo].[spr_CountTableRow]
AS
BEGIN
	SELECT
    ST.name AS Table_Name,
    SUM(DMS.row_count) AS NUMBER_OF_ROWS
	FROM
		SYS.TABLES AS ST
		INNER JOIN SYS.DM_DB_PARTITION_STATS AS DMS ON ST.object_id = DMS.object_id
	WHERE
		DMS.index_id in (0,1)
	GROUP BY ST.name
	ORDER BY NUMBER_OF_ROWS DESC

	SELECT  
        ObjectName = OBJECT_NAME(sc.object_id)
        ,ColumnName = sc.name
        ,DataType   = TYPE_NAME (sc.system_type_id) 
        ,MaxValue   = sc.last_value		
   FROM sys.identity_columns sc
  WHERE OBJECTPROPERTYEX(sc.object_id,'IsTable') = 1 
  ORDER BY MaxValue DESC, ObjectName

END
GO
/****** Object:  StoredProcedure [dbo].[spr_DeleteUnWantedData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <19-02-2019>
-- Description:	<Delete unwanted data if already exists but event not exists>
-- =============================================
CREATE PROCEDURE [dbo].[spr_DeleteUnWantedData]
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED

	INSERT INTO [dbo].[ServiceLog]
	(
		MethodName,
		Model,
		CreatedOn
	)VALUES
	(
		'[DataLaundryApp] : EventJob',
		'spr_DeleteUnWantedData - Request',
		GETUTCDATE()
	)
    /*Delete Organization data*/
	DELETE  FROM [dbo].[Organization]  
	WHERE [EventId] NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	--DELETE FROM [dbo].[Organization]
	--WHERE NOT EXISTS 
	--(
	--	SELECT NULL
 --       FROM [dbo].[Event] E WITH(NOLOCK)
 --       WHERE E.id = EventId 
	--)
	--MERGE [dbo].[Organization] O 
	--USING [dbo].[Event] E
	--ON E.Id = O.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;
	
	
	/*Delete Person data*/
	DELETE  FROM [dbo].[Person]  WHERE [EventId] NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	) 
	--DELETE  FROM [dbo].[Person]
	--WHERE NOT EXISTS
	--(
	--	SELECT NULL
	--  FROM [dbo].[Event] E WITH(NOLOCK)
	--  WHERE E.id = EventId 
	--)
	--MERGE [dbo].[Person] P 
	--USING [dbo].[Event] E
	--ON E.Id = P.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;

	/*Delete Programme data*/
	DELETE FROM [dbo].[Programme] WHERE EventId NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	--DELETE FROM [dbo].[Programme]
	--WHERE NOT EXISTS
	--(
	--	SELECT NULL
 --       FROM [dbo].[Event] E WITH(NOLOCK)
 --       WHERE E.id = EventId 
	--)
	--MERGE [dbo].[Programme] PM 
	--USING [dbo].[Event] E
	--ON E.Id = PM.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;

	 /*Delete EventSchedule data*/
	DELETE FROM [dbo].[EventSchedule] WHERE EventId NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	--DELETE FROM [dbo].[EventSchedule]
	--WHERE NOT EXISTS
	--(
	--	SELECT NULL
 --       FROM [dbo].[Event] E WITH(NOLOCK)
 --       WHERE E.id = EventId 
	--)
	--MERGE [dbo].[EventSchedule] ES 
	--USING [dbo].[Event] E
	--ON E.Id = ES.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;

	/*Delete Place data*/
	DELETE FROM [dbo].[Place] WHERE EventId NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	--DELETE FROM [dbo].[Place]
	--WHERE NOT EXISTS
	--(
	--	SELECT NULL
 --       FROM [dbo].[Event] E WITH(NOLOCK)
 --       WHERE E.id = EventId 
	--)
	--MERGE [dbo].[Place] P 
	--USING [dbo].[Event] E
	--ON E.Id = P.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;

	/*Delete AmenityFeature data*/
	DELETE FROM [dbo].[AmenityFeature] WHERE EventId NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	 --DELETE FROM [dbo].[AmenityFeature]
	 --WHERE NOT EXISTS
	 --(
		--SELECT NULL
  --      FROM [dbo].[Event] E WITH(NOLOCK)
  --      WHERE E.id = EventId 
	 --)
	--MERGE [dbo].[AmenityFeature] AF 
	--USING [dbo].[Event] E
	--ON E.Id = AF.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;



	 /*Delete PhysicalActivity data*/
	DELETE FROM [dbo].[PhysicalActivity] WHERE EventId NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	 --DELETE FROM [dbo].[PhysicalActivity]
	 --WHERE NOT EXISTS
	 --(
		--SELECT NULL
		--FROM [dbo].[Event] E WITH(NOLOCK) 
		--WHERE E.Id = EventId
	 --)
	--MERGE [dbo].[PhysicalActivity] PA 
	--USING [dbo].[Event] E
	--ON E.Id = PA.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;
	 
	 /*Delete CustomFeedData data*/
	DELETE FROM [dbo].[CustomFeedData] WHERE EventId NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	--DELETE FROM [dbo].[CustomFeedData]
	--WHERE NOT EXISTS
	--(
	--	SELECT NULL
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	WHERE E.id = EventId
	--)
	--MERGE [dbo].[CustomFeedData] CD 
	--USING [dbo].[Event] E
	--ON E.Id = CD.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;
	
	/*Delete EventOccurrence data*/
	DELETE FROM  [dbo].[EventOccurrence] WHERE EventId NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	--DELETE FROM [dbo].[EventOccurrence] 
	--WHERE NOT EXISTS
	--(
	--	SELECT NULL
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	WHERE E.Id = EventId
	--)
	--MERGE [dbo].[EventOccurrence] EO 
	--USING [dbo].[Event] E
	--ON E.Id = EO.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;
	
	/*Delete Offer data*/
	DELETE FROM  [dbo].[Offer] WHERE EventId NOT IN
	(
		SELECT [ID] FROM [dbo].[Event] WITH(NOLOCK)
		WHERE [FeedId] IS NOT NULL
	)
	--MERGE [dbo].[Offer] OFFER
	--USING [dbo].[Event] E
	--ON E.Id = OFFER.EventId    
	--WHEN NOT MATCHED BY SOURCE
	--THEN DELETE;
	
	INSERT INTO [dbo].[ServiceLog]
	(
		MethodName,
		Model,
		CreatedOn
	)VALUES
	(
		'[DataLaundryApp] : EventJob',
		'spr_DeleteUnWantedData - Response',
		GETUTCDATE()
	)
END
GO
/****** Object:  StoredProcedure [dbo].[spr_Event_Update]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <04-02-2019>
-- Description:	<Description,,>
-- =============================================
--EXEC spr_Event_Update 186,'GenderRestriction','http://openactive.io/ns#NewNone'
CREATE PROCEDURE [dbo].[spr_Event_Update]
	@EventID BIGINT,
	@ColumnName NVARCHAR(255),
	@Value NVARCHAR(MAX)
AS
BEGIN
	DECLARE @SQL NVARCHAR(MAX);
	SET @SQL='UPDATE [dbo].[Event] SET '+CAST(@ColumnName AS NVARCHAR(MAX))+'='''+CAST(@Value AS NVARCHAR(MAX))+''' WHERE ID='+CAST(@EventID AS NVARCHAR(MAX));
	EXEC SP_EXECUTESQL @SQL;
END
GO
/****** Object:  StoredProcedure [dbo].[spr_GetEventByFeedProviderID]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <04-02-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_GetEventByFeedProviderID]
	@FeedProviderID INT,
	@Limit BIGINT = NULL,
	@Page INT = NULL
AS
BEGIN

	SELECT E.id,
		   E.FeedProviderId,
		   E.FeedId,
		   E.State,
		   E.ModifiedDate,
		   E.Name,
		   E.Description,
		   E.Image,
		   E.ImageThumbnail,
		   E.StartDate,
		   E.EndDate,
		   E.Duration,
		   E.MaximumAttendeeCapacity,
		   E.RemainingAttendeeCapacity,
		   E.EventStatus,
		   E.SuperEventId,
		   E.Category,
		   E.AgeRange,
		   E.MinAge,
		   E.MaxAge,
		   E.GenderRestriction,
		   E.Gender,
		   E.AttendeeInstructions,
		   E.AccessibilitySupport,
		   E.AccessibilityInformation,
		   E.IsCoached,
		   E.Level,
		   E.MeetingPoint,
		   E.Identifier,
		   E.URL,
		   E.ModifiedDateTimestamp		   
	FROM Event E WITH(NOLOCK) 	
	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId IS NOT NULL
	
	--/*Organization*/
	--SELECT O.* FROM [dbo].[Organization] O WITH(NOLOCK)
	--OUTER APPLY
	--(
	--	SELECT E.id
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	--	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId IS NOT NULL
	--) AS E
	--WHERE O.EventId IN(E.id)

	--/*Contributor As Person*/
	--SELECT P.* FROM [dbo].[Person] P WITH(NOLOCK)
	--OUTER APPLY(
	--	SELECT E.id
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	--	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId IS NOT NULL
	--) AS E
	--WHERE P.EventId IN(E.id)

	--/*Place As Location*/
	--SELECT P.* FROM [dbo].[Place] P WITH(NOLOCK)
	--OUTER APPLY
	--(
	--	SELECT E.id
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	--	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId IS NOT NULL
	--) AS E
	--WHERE P.EventId IN(E.id)

	--/*EventSchedule*/
	--SELECT ES.* FROM [dbo].[EventSchedule] ES WITH(NOLOCK)
	--OUTER APPLY
	--(
	--	SELECT E.id
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	--	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId IS NOT NULL
	--) AS E
	--WHERE ES.EventId IN(E.id)

	--/*Program*/
	--SELECT P.* FROM [dbo].[Programme] P WITH(NOLOCK)
	--OUTER APPLY
	--(
	--	SELECT E.id
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	--	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId IS NOT NULL
	--) AS E
	--WHERE P.EventId IN(E.id)

	--/*Physical Activity*/
	--SELECT PA.* FROM [dbo].[PhysicalActivity] PA WITH(NOLOCK)
	--OUTER APPLY
	--(
	--	SELECT E.id
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	--	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId IS NOT NULL
	--) AS E
	--WHERE PA.EventId IN(E.id)

	--/*SubEvent*/
	--SELECT SE.* FROM [dbo].[Event] SE WITH(NOLOCK)
	--OUTER APPLY
	--(
	--	SELECT E.Id
	--	FROM [dbo].[Event] E WITH(NOLOCK)
	--	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	--	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId IS NOT NULL
	--) AS E
	--WHERE SE.SuperEventId IN(E.id)

	--/*Occurrence*/
	--SELECT EO.* FROM [dbo].[EventOccurrence] EO WITH(NOLOCK)
	--OUTER APPLY
	--(
	--	SELECT E.Id
	--	FROM [dbo].[Event] E WITH (NOLOCK)
	--	INNER JOIN FeedProvider FP WITH(NOLOCK) ON FP.id = @FeedProviderID AND FP.IsDeleted = 0
	--	WHERE E.FeedProviderId = @FeedProviderID AND E.FeedId  IS NOT NULL
	--) AS E
	--WHERE EO.EventId IN(E.id)


END
GO
/****** Object:  StoredProcedure [dbo].[spr_GetSampleFeedData]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <11-02-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_GetSampleFeedData]
	@FeedProviderID INT
AS
BEGIN
	SELECT ID,
		   ISNULL(FM.ActualFeedKeyPath,'') AS ActualFeedKeyPath,
		   ISNULL(FM.FeedKeyPath,'') AS FeedKeyPath
	FROM [dbo].[FeedMapping] FM WITH (NOLOCK)
	WHERE FM.FeedProviderId = @FeedProviderID AND FM.IsDeleted = 0
END
GO
/****** Object:  StoredProcedure [dbo].[spr_JsonImportData_GetByID]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <30-01-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_JsonImportData_GetByID]
	@FeedProviderID INT
AS
BEGIN
	SELECT J.JsonID,
		   J.EventID,
		   J.FeedID,
		   J.JsonData
	FROM JsonImportData J WITH(NOLOCK)
	WHERE J.FeedProviderID = @FeedProviderID
END
GO
/****** Object:  StoredProcedure [dbo].[spr_JsonImportData_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <29-01-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_JsonImportData_Insert]
	@EventID BIGINT,
	@FeedProviderID BIGINT,
	@FeedID NVARCHAR(50),
	@JsonData NVARCHAR(MAX)
AS
BEGIN
	DECLARE @JsonID BIGINT = 0;
	SELECT @JsonID = ISNULL(JsonID,0) 
	FROM [dbo].[JsonImportData] WITH(NOLOCK)
	WHERE EventID = @EventID AND 
		  FeedID = @FeedID AND 
		  FeedProviderID = @FeedProviderID
	
	IF @JsonID > 0
	BEGIN
		UPDATE JsonImportData
			SET  JsonData=@JsonData
			WHERE EventID = @EventID
	END
	ELSE
	BEGIN
		INSERT INTO JsonImportData
		(
			FeedProviderID,
			FeedID,
			JsonData,
			EventID
		)
		VALUES
		(
			@FeedProviderID,
			@FeedID,
			@JsonData,
			@EventID
		)
	END
END
GO
/****** Object:  StoredProcedure [dbo].[spr_Place_UpdateAddress]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <13-02-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_Place_UpdateAddress]
	@FeedProviderId INT
AS
BEGIN
	UPDATE [dbo].[Place] SET Address =  CONCAT(ISNULL(P.StreetAddress,''),' ',ISNULL(P.AddressLocality,''),' ',ISNULL(P.PostalCode,''),' ',ISNULL(P.Region,'')) 
		FROM [dbo].[Place] P WITH (NOLOCK)		
		INNER JOIN [dbo].[Event] E WITH (NOLOCK) ON E.id = P.EventId AND E.FeedProviderId = @FeedProviderId
		WHERE P.StreetAddress IS NOT NULL OR
			  P.AddressLocality IS NOT NULL OR
			  P.PostalCode IS NOT NULL OR
			  P.Region IS NOT NULL
END
GO
/****** Object:  StoredProcedure [dbo].[spr_PlaceUpdateLatLong]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		<Jishan Siddique>
-- Create date: <28-02-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_PlaceUpdateLatLong]
	@TTPlace  AS TTPlace  READONLY
AS
BEGIN
	UPDATE
    [dbo].[Place]
	SET
		Lat = TTP.Lat,
		Long = TTP.Long,
		IsUpdate = 1
	FROM
		@TTPlace AS TTP
	INNER JOIN [dbo].[FeedProvider] FP WITH(NOLOCK) ON FP.id = TTP.FeedProviderId AND FP.IsDeleted = 0
	INNER JOIN [dbo].[Event] E WITH(NOLOCK) ON E.FeedProviderId = FP.id
    INNER JOIN [dbo].[Place] P WITH(NOLOCK) ON P.EventId = E.id AND  TTP.PostalCode = P.PostalCode	

	/*Insert in Default UKPostalCode*/
	MERGE [dbo].[UKPostalCode] AS TARGET
	USING @TTPlace AS SOURCE 
	ON (TARGET.PostalCode = SOURCE.PostalCode) 
	WHEN NOT MATCHED BY TARGET THEN  
	INSERT (PostalCode,Lat,Long,FeedProviderID)
	VALUES (SOURCE.PostalCode,SOURCE.Lat,SOURCE.Long,SOURCE.FeedProviderID)
	;
END
GO
/****** Object:  StoredProcedure [dbo].[spr_SearchAllTables]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [dbo].[spr_SearchAllTables]
(
    @SearchStr NVARCHAR(MAX),
	@IsLike INT = NULL
)
AS
BEGIN

    CREATE TABLE #Results (ColumnName NVARCHAR(370), ColumnValue NVARCHAR(3630))

    SET NOCOUNT ON

    DECLARE @TableName nvarchar(256), @ColumnName nvarchar(128), @SearchStr2 nvarchar(110)
    SET  @TableName = ''

	IF @IsLike = 1 SET @SearchStr2 = QUOTENAME('%' + @SearchStr + '%','''') -- SEARCH IN BOTH SIDE
	ELSE IF @IsLike = 2 SET @SearchStr2 = QUOTENAME(@SearchStr + '%','''') -- SEARCH IN START WITH
	ELSE IF @IsLike = 3 SET @SearchStr2 = QUOTENAME('%' + @SearchStr ,'''') -- SEARCH IN END WITH
	ELSE IF @IsLike = 4 SET @SearchStr2 = QUOTENAME(@SearchStr ,'''') -- SEARCH WITH SAME VALUE
	ELSE SET @SearchStr2 = QUOTENAME('%' + @SearchStr + '%','''') -- SEARCH IN BOTH SIDE

   -- SET @SearchStr2 = QUOTENAME('%' + @SearchStr + '%','''')					

    WHILE @TableName IS NOT NULL

    BEGIN
        SET @ColumnName = ''
        SET @TableName = 
        (
            SELECT MIN(QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME))
            FROM     INFORMATION_SCHEMA.TABLES
            WHERE         TABLE_TYPE = 'BASE TABLE'
                AND    QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME) > @TableName
                AND    OBJECTPROPERTY(
                        OBJECT_ID(
                            QUOTENAME(TABLE_SCHEMA) + '.' + QUOTENAME(TABLE_NAME)
                             ), 'IsMSShipped'
                               ) = 0
        )

        WHILE (@TableName IS NOT NULL) AND (@ColumnName IS NOT NULL)

        BEGIN
            SET @ColumnName =
            (
                SELECT MIN(QUOTENAME(COLUMN_NAME))
                FROM     INFORMATION_SCHEMA.COLUMNS
                WHERE         TABLE_SCHEMA    = PARSENAME(@TableName, 2)
                    AND    TABLE_NAME    = PARSENAME(@TableName, 1)
                    AND    DATA_TYPE IN ('char', 'varchar', 'nchar', 'nvarchar', 'int', 'decimal')
                    AND    QUOTENAME(COLUMN_NAME) > @ColumnName
            )

            IF @ColumnName IS NOT NULL

            BEGIN				
                INSERT INTO #Results
                EXEC
                (
                    'SELECT ''' + @TableName + '.' + @ColumnName + ''', LEFT(' + @ColumnName + ', 3630) 
                    FROM ' + @TableName + ' (NOLOCK) ' +
                    ' WHERE ' + @ColumnName + ' LIKE ' + @SearchStr2
                )				
            END
        END    
    END

    SELECT ColumnName, ColumnValue FROM #Results
	DROP TABLE #Results
END
GO
/****** Object:  StoredProcedure [dbo].[spr_ServiceLog_Insert]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<JISHAN SIDDIQUE>
-- Create date: <21-01-2019>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[spr_ServiceLog_Insert]
	@MethodName NVARCHAR(MAX) = NULL,
	@Model NVARCHAR(MAX) = NULL,
	@RequestTime DATETIME = NULL,
    @ResponseTime DATETIME = NULL
AS
BEGIN
	INSERT INTO ServiceLog
	(
		MethodName,
		Model,
		RequestTime,
		ResponseTime,
		CreatedOn
	)VALUES
	(
		@MethodName,
		@Model,
		@RequestTime,
		@ResponseTime,
		GETUTCDATE()
	)
END
GO
/****** Object:  StoredProcedure [dbo].[ValidateAdminLogin]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ValidateAdminLogin]
	@Email		NVARCHAR(50),
	@Password	NVARCHAR(50)
AS
BEGIN
	
	SELECT	Id
			,Name
			,Email
			,Password
	FROM	[USER] WITH (NOLOCK)
	WHERE	Email = @Email
			AND Password = @Password
			AND IsDeleted = 0

END

GO
/****** Object:  StoredProcedure [dbo].[ValidateFeedMappingColumnName]    Script Date: 13-02-2020 18:14:40 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[ValidateFeedMappingColumnName]
	@FeedProviderId		INT,
	@ColumnName			NVARCHAR(50),
	@IsCustomFeedKey	BIT = 1,
	@Id					BIGINT = NULL
AS
BEGIN
	
	
	SELECT	COUNT(*) 
	FROM	FeedMapping WITH (NOLOCK)
	WHERE	FeedProviderId = @FeedProviderId
			AND IsCustomFeedKey = @IsCustomFeedKey
			AND IsDeleted = 0
			AND ColumnName = @ColumnName
			AND (@Id IS NULL 
				OR ISNULL(Id, 0) <> ISNULL(@Id, 0))
END

GO
USE [master]
GO
ALTER DATABASE [DataLaundry] SET  READ_WRITE 
GO
