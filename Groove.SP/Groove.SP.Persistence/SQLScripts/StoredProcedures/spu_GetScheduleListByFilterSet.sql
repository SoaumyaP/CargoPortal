SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetScheduleListByFilterSet', 'P') IS NOT NULL
DROP PROC dbo.spu_GetScheduleListByFilterSet
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 30 December 2020
-- Description:	Get Schedule data by criteria
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetScheduleListByFilterSet]
	@JsonFilterSet NVARCHAR(MAX) = '',
	@Offset INT = 0,
	@Size INT = 1000
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ResultTable TABLE
	(
		Id BIGINT,
		VesselName VARCHAR(512),
		Voyage VARCHAR(512),
		FlightNumber NVARCHAR(512),
		VesselFlight NVARCHAR(512),
		MAWB NVARCHAR(512),
		CarrierName VARCHAR(512),
		ETDDate DATETIME2,
		ETADate DATETIME2
	)

	-- Criteria variables from JSON to filter
	DECLARE @selectedModeOfTransport VARCHAR(128)
	DECLARE @selectedLoadingPort VARCHAR (512) 
	DECLARE @selectedDischargePort VARCHAR (512) 
	DECLARE @selectedDeparture DATETIME2
	DECLARE @selectedCarrierCode NVARCHAR(128)

	SELECT	@selectedModeOfTransport = ModeOfTransport,
			@selectedLoadingPort = LoadingPort,
			@selectedDischargePort = DischargePort,
			@selectedDeparture = Departure,
			@selectedCarrierCode = CarrierCode
	FROM OPENJSON(@JsonFilterSet)
	WITH (
		[ModeOfTransport] [VARCHAR] (128) '$.ModeOfTransport',
		[LoadingPort] [VARCHAR] (512) '$.LoadingPort',
		[DischargePort] [VARCHAR] (512) '$.DischargePort',
		[Departure] [DATETIME2] '$.Departure',
		[CarrierCode] [NVARCHAR] (128) '$.CarrierCode'
	);

	INSERT INTO @ResultTable
	SELECT fs.Id
		,fs.VesselName
		,fs.Voyage
		,fs.FlightNumber
		,IIF(fs.ModeOfTransport = 'Air', fs.FlightNumber, concat(fs.VesselName, '/', fs.Voyage)) as VesselFlight
		,fs.MAWB
		,fs.CarrierName
		,fs.ETDDate
		,fs.ETADate
	FROM FreightSchedulers fs WITH(NOLOCK)
	WHERE fs.ModeOfTransport = @selectedModeOfTransport
		AND fs.LocationFromName = @selectedLoadingPort
		AND fs.LocationToName = @selectedDischargePort
		AND CAST(fs.ETDDate AS DATE) >= CAST(@selectedDeparture AS DATE)
		AND (@selectedCarrierCode IS NULL OR @selectedCarrierCode = '' OR @selectedCarrierCode = fs.CarrierCode)

	SELECT *
	FROM @ResultTable
	ORDER BY ETDDate
	OFFSET @Offset ROWS FETCH NEXT @Size ROWS ONLY;

END
GO

