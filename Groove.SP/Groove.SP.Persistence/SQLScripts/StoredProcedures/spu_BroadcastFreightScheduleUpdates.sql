SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_BroadcastFreightScheduleUpdates', 'P') IS NOT NULL
DROP PROC dbo.spu_BroadcastFreightScheduleUpdates
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 10 Jan 2022
-- Description:	Call to broadcast Schedule updates value (ETA/ETD) to related tabels (dbo.Shipments/ dbo.Consignments/ dbo.Itineraries/ dbo.Containers/ dbo.BillOfLadings)
-- =============================================
CREATE PROCEDURE [dbo].[spu_BroadcastFreightScheduleUpdates]
	
	@freightSchedulerIds NVARCHAR(512), --comma-separated
	@updatedBy NVARCHAR(512),
	@updatedFromKeyword NVARCHAR(512),
	@updatedViaUI BIT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	DECLARE @UpdatedSchedulersTbl TABLE (
		ScheduleId BIGINT PRIMARY KEY,
		ETDDate DATETIME2(7),
		ETADate DATETIME2(7),
		ModeOfTransport NVARCHAR(MAX),
		LocationToName NVARCHAR(MAX)
	)

	INSERT INTO @UpdatedSchedulersTbl (ScheduleId, ETDDate, ETADate, ModeOfTransport, LocationToName)
	SELECT 
		Id,
		ETDDate,
		ETADate,
		ModeOfTransport,
		LocationToName
	
	FROM FreightSchedulers
	WHERE Id IN (SELECT value FROM [dbo].[fn_SplitStringToTable](@freightSchedulerIds,','))

	-- Section #1: Update values for related tables
	-- Apply ONLY for ModeofTransport = SEA
	-- As updating data, run business to add/update values on columns ETA, ETD
	-- on 5 tables [dbo].[Itineraries], [dbo].[Consignments], [dbo].[Containers], [dbo].[Shipments], [dbo].[BillOfLadings]	

	DECLARE @UpdatedItinerariesTbl TABLE (
		ItineraryId BIGINT PRIMARY KEY,
		UpdateBy NVARCHAR(256),
		LocationToName NVARCHAR(MAX)
	)

	-- To contain Id of records that needed to update the earliest ETD and the latest ETA
	DECLARE @IdTbl TABLE (
		Id BIGINT PRIMARY KEY
	)

	-- 1. Add values to [dbo].[Itineraries]
	UPDATE [dbo].[Itineraries]
	SET
		ETDDate = UST.ETDDate,
		ETADate = UST.ETADate,
		UpdatedBy = IIF(@updatedBy IS NULL OR @updatedBy = '', UpdatedBy, @updatedBy),
		UpdatedDate = GETUTCDATE()
	OUTPUT inserted.Id, inserted.UpdatedBy, UST.LocationToName
	INTO @UpdatedItinerariesTbl
	FROM [dbo].[Itineraries] I
	INNER JOIN @UpdatedSchedulersTbl UST ON I.ScheduleId = UST.ScheduleId
	WHERE
	-- Apply for ModeofTransport = SEA or AIR
		UST.[ModeOfTransport] = 'Sea' OR (UST.[ModeOfTransport] = 'Air' AND @updatedViaUI = 1)

	-- *** Update related tables only if NOT called from API (UIT.IsCallFromAPI is false = 0) ***

	-- 2. Add values to [dbo].[Containers]
	DELETE FROM @IdTbl

	INSERT
	INTO @IdTbl
	SELECT CON.Id
	FROM [dbo].[Containers] CON
	INNER JOIN [dbo].[ContainerItineraries] CI ON CON.Id = CI.ContainerId
	INNER JOIN @UpdatedItinerariesTbl UIT ON CI.ItineraryId = UIT.ItineraryId
	WHERE
		(
				-- updated via UI
				@updatedViaUI = 1
			OR 
				-- update via Schedule Update API and ShipTo of Shipment / Consignment / Container / House BL = Freight Schedule.LocationToName
				(
					@updatedFromKeyword LIKE 'updatedviaFreightSchedulerAPI%'
					AND CON.ShipTo = UIT.LocationToName
				)
		)

	-- Correct the earliest ETD and the latest ETA
	UPDATE [dbo].[Containers]
	SET [ShipFromETDDate] = ( SELECT MIN(I.ETDDate)
								FROM Itineraries I
								INNER JOIN [dbo].[ContainerItineraries] CI ON I.Id = CI.ItineraryId WHERE CI.ContainerId = CON.Id),
		[ShipToETADate] = ( SELECT MAX(I.ETADate)
								FROM Itineraries I
								INNER JOIN [dbo].[ContainerItineraries] CI ON I.Id = CI.ItineraryId WHERE CI.ContainerId = CON.Id),
		UpdatedDate = GETUTCDATE()
	FROM [dbo].[Containers] CON
	WHERE CON.Id IN (SELECT Id FROM @IdTbl)

	-- 3. Add values to [dbo].[Consignments]
	DELETE FROM @IdTbl

	INSERT
	INTO @IdTbl
	SELECT CONS.Id
	FROM [dbo].[Consignments] CONS
	INNER JOIN [dbo].[ConsignmentItineraries] CONSI ON CONS.Id = CONSI.ConsignmentId
	INNER JOIN @UpdatedItinerariesTbl UIT ON CONSI.ItineraryId = UIT.ItineraryId 
	WHERE 
		(
				-- updated via UI
				@updatedViaUI = 1
			OR 
				-- update via Schedule Update API and ShipTo of Shipment / Consignment / Container / House BL = Freight Schedule.LocationToName
				(
					@updatedFromKeyword LIKE 'updatedviaFreightSchedulerAPI%'
					AND CONS.ShipTo = UIT.LocationToName
				)
		)

	-- Correct the earliest ETD and the latest ETA
	UPDATE [dbo].[Consignments]
	SET [ShipFromETDDate] = ( SELECT MIN(I.ETDDate)
								FROM Itineraries I
								INNER JOIN [dbo].[ConsignmentItineraries] CI ON I.Id = CI.ItineraryId WHERE CI.ConsignmentId = CONS.Id),
		[ShipToETADate] = ( SELECT MAX(I.ETADate)
								FROM Itineraries I
								INNER JOIN [dbo].[ConsignmentItineraries] CI ON I.Id = CI.ItineraryId WHERE CI.ConsignmentId = CONS.Id),
		UpdatedDate = GETUTCDATE()
	FROM [dbo].[Consignments] CONS
	WHERE CONS.Id IN (SELECT Id FROM @IdTbl)

	-- 4. Add values to [dbo].[Shipments]
	-- Shipment loads Itineraries on UI via ConsignmentItineraries
	DELETE FROM @IdTbl

	INSERT
	INTO @IdTbl
	SELECT SHI.Id
	FROM [dbo].[Shipments] SHI
	INNER JOIN [dbo].[ConsignmentItineraries] CI ON SHI.Id = CI.ShipmentId
	INNER JOIN @UpdatedItinerariesTbl UIT ON CI.ItineraryId = UIT.ItineraryId 
	WHERE 
		(
				-- updated via UI
				@updatedViaUI = 1
			OR 
				-- update via Schedule Update API and ShipTo of Shipment / Consignment / Container / House BL = Freight Schedule.LocationToName
				(
					@updatedFromKeyword LIKE 'updatedviaFreightSchedulerAPI%'
					AND SHI.ShipTo = UIT.LocationToName
				)
		)

	-- Correct the earliest ETD and the latest ETA
	UPDATE [dbo].[Shipments]
	SET [ShipFromETDDate] = ( SELECT MIN(I.ETDDate)
								FROM Itineraries I
								INNER JOIN [dbo].[ConsignmentItineraries] CI ON I.Id = CI.ItineraryId WHERE CI.ShipmentId = SHI.Id),
		[ShipToETADate] = ( SELECT MAX(I.ETADate)
								FROM Itineraries I
								INNER JOIN [dbo].[ConsignmentItineraries] CI ON I.Id = CI.ItineraryId WHERE CI.ShipmentId = SHI.Id),
		UpdatedDate = GETUTCDATE()
	FROM [dbo].[Shipments] SHI
	WHERE SHI.Id IN (SELECT Id FROM @IdTbl)

	-- 5. Add values to [dbo].[BillOfLadings]
	DELETE FROM @IdTbl

	INSERT
	INTO @IdTbl
	SELECT BL.Id
	FROM [dbo].[BillOfLadings] BL
	INNER JOIN [dbo].[BillOfLadingItineraries] BLI ON BL.Id = BLI.BillOfLadingId
	INNER JOIN @UpdatedItinerariesTbl UIT ON BLI.ItineraryId = UIT.ItineraryId
	WHERE 
		(
				-- updated via UI
				@updatedViaUI = 1
			OR 
				-- update via Schedule Update API and ShipTo of Shipment / Consignment / Container / House BL = Freight Schedule.LocationToName
				(
					@updatedFromKeyword LIKE 'updatedviaFreightSchedulerAPI%'
					AND BL.ShipTo = UIT.LocationToName
				)
		)

	-- Correct the earliest ETD and the latest ETA
	UPDATE [dbo].[BillOfLadings]
	SET [ShipFromETDDate] = ( SELECT MIN(I.ETDDate)
								FROM Itineraries I
								INNER JOIN [dbo].[BillOfLadingItineraries] CI ON I.Id = CI.ItineraryId WHERE CI.BillOfLadingId = BL.Id),
		[ShipToETADate] = ( SELECT MAX(I.ETADate)
								FROM Itineraries I
								INNER JOIN [dbo].[BillOfLadingItineraries] CI ON I.Id = CI.ItineraryId WHERE CI.BillOfLadingId = BL.Id),
		UpdatedDate = GETUTCDATE()
	FROM [dbo].[BillOfLadings] BL
	WHERE BL.Id IN (SELECT Id FROM @IdTbl)

END

