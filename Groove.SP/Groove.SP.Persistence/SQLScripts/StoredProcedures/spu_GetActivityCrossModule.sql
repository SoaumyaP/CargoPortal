SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetActivityCrossModule', 'P') IS NOT NULL
DROP PROC spu_GetActivityCrossModule
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 3 September 2021
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetActivityCrossModule]
	@entityId BIGINT = 0,
	@entityType NVARCHAR(50) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;
	
	-- Global variable declarations
	DECLARE @poEntityType NVARCHAR(20) = 'CPO'
	DECLARE @poffEntityType NVARCHAR(20) = 'POF'
	DECLARE @shipmentEntityType NVARCHAR(20) = 'SHI'
	DECLARE @containerEntityType NVARCHAR(20) = 'CTN'
	DECLARE @freightSchedulerEntityType NVARCHAR(20) = 'FSC'
	-- Event level mapping
	DECLARE @poEventLevel INT = 1
	DECLARE @poffEventLevel INT = 2
	DECLARE @shipmentEventLevel INT = 4
	DECLARE @containerEventLevel INT = 5

	DECLARE @resultTbl TABLE (
		[Id] BIGINT,
		[ShipmentId] BIGINT,
		[ContainerId] BIGINT,
		[PurchaseOrderId] BIGINT,
		[POFulfillmentId] BIGINT,
		[FreightSchedulerId] BIGINT,
		[ActivityCode] NVARCHAR(10),
		[ActivityType] NVARCHAR(10),
		[ActivityLevel] NVARCHAR(100),
		[ActivityDate] DATETIME2(7),
		[ActivityDescription] NVARCHAR(512),
		[Location] NVARCHAR(512),
		[Remark] NVARCHAR(MAX),
		[Resolved] BIT,
		[Resolution] NVARCHAR(MAX),
		[ResolutionDate] DATETIME2(7),
		[CreatedBy] NVARCHAR(256),
		[CreatedDate] DATETIME2(7),
		[SortSequence] BIGINT
	)
	DECLARE @fromEventLevel INT
	DECLARE @globalIdActivitiesTbl TABLE (GlobalId NVARCHAR(450), EntityId BIGINT, EntityType NVARCHAR(50))

	INSERT INTO @globalIdActivitiesTbl
	SELECT CONCAT(@entityType, '_', @entityId), @entityId, @entityType

	IF (@entityType = @poEntityType)
	BEGIN
		SET @fromEventLevel = @poEventLevel

		-- Obtain globalId from related POFulfillments

		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@poffEntityType, '_', poff.Id),
			poff.Id,
			@poffEntityType
		FROM POFulfillments poff (NOLOCK)
		WHERE poff.[Status] = 10 --active
		AND EXISTS (
			SELECT 1
			FROM POFulfillmentOrders poffod (NOLOCK)
			WHERE poffod.POFulfillmentId = poff.Id AND poffod.PurchaseOrderId = @entityId)


		-- Obtain globalId from related Shipments

		;with tmp as (
			SELECT EntityId as POFulfillmentId FROM @globalIdActivitiesTbl WHERE EntityType = @poffEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@shipmentEntityType, '_', s.Id),
			s.Id,
			@shipmentEntityType
		FROM Shipments s (NOLOCK)
		WHERE s.[Status] = 'active' AND s.POFulfillmentId IN (
			SELECT POFulfillmentId FROM tmp)

		-- Obtain globalId from related Containers

		;with tmp as (
			SELECT EntityId as ShipmentId FROM @globalIdActivitiesTbl WHERE EntityType = @shipmentEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@containerEntityType, '_', sl.ContainerId),
			sl.ContainerId,
			@containerEntityType
		FROM ShipmentLoads sl (NOLOCK)
		WHERE sl.ContainerId IS NOT NULL AND sl.ShipmentId IN (
			SELECT ShipmentId FROM tmp)

		-- Obtain globalId from related Vessels

		;with tmp as (
			SELECT EntityId as ShipmentId FROM @globalIdActivitiesTbl WHERE EntityType = @shipmentEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@freightSchedulerEntityType, '_', i.ScheduleId),
			i.ScheduleId,
			@freightSchedulerEntityType
		FROM ConsignmentItineraries csmi (NOLOCK) INNER JOIN Itineraries i (NOLOCK) ON csmi.ItineraryId = i.Id AND (i.ModeOfTransport = 'sea' OR i.ModeOfTransport = 'air') AND i.ScheduleId IS NOT NULL
		WHERE csmi.ShipmentId IN (
			SELECT ShipmentId FROM tmp)
	END
	ELSE
	IF (@entityType = @poffEntityType)
	BEGIN
		SET @fromEventLevel = @poffEventLevel

		-- Obtain globalId from PurchaseOrders

		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT DISTINCT
			CONCAT(@poEntityType, '_', PurchaseOrderId),
			PurchaseOrderId,
			@poEntityType
		FROM POFulfillmentOrders (NOLOCK)
		WHERE POFulfillmentId = @entityId

		-- Obtain globalId from related Shipments

		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@shipmentEntityType, '_', s.Id),
			s.Id,
			@shipmentEntityType
		FROM Shipments s (NOLOCK)
		WHERE s.[Status] = 'active' AND s.POFulfillmentId = @entityId

		-- Obtain globalId from related Containers

		;with tmp as (
			SELECT EntityId as ShipmentId FROM @globalIdActivitiesTbl WHERE EntityType = @shipmentEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@containerEntityType, '_', sl.ContainerId),
			sl.ContainerId,
			@containerEntityType
		FROM ShipmentLoads sl (NOLOCK)
		WHERE sl.ContainerId IS NOT NULL AND sl.ShipmentId IN (
			SELECT ShipmentId FROM tmp)

		-- Obtain globalId from related Vessels

		;with tmp as (
			SELECT EntityId as ShipmentId FROM @globalIdActivitiesTbl WHERE EntityType = @shipmentEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@freightSchedulerEntityType, '_', i.ScheduleId),
			i.ScheduleId,
			@freightSchedulerEntityType
		FROM ConsignmentItineraries csmi (NOLOCK) INNER JOIN Itineraries i (NOLOCK) ON csmi.ItineraryId = i.Id AND (i.ModeOfTransport = 'sea' OR i.ModeOfTransport = 'air') AND i.ScheduleId IS NOT NULL
		WHERE csmi.ShipmentId IN (
			SELECT ShipmentId FROM tmp)
	END
	ELSE
	IF (@entityType = @shipmentEntityType)
	BEGIN
		SET @fromEventLevel = @shipmentEventLevel

		-- Obtain globalId from related POFulfillments

		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@poffEntityType, '_', s.POFulfillmentId),
			s.POFulfillmentId,
			@poffEntityType
		FROM Shipments s (NOLOCK)
		WHERE s.Id = @entityId AND s.POFulfillmentId IS NOT NULL

		-- Obtain globalId from related PurchaseOrders

		;with tmp as (
			SELECT EntityId as POFulfillmentId FROM @globalIdActivitiesTbl WHERE EntityType = @poffEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT DISTINCT
			CONCAT(@poEntityType, '_', PurchaseOrderId),
			PurchaseOrderId,
			@poEntityType
		FROM POFulfillmentOrders (NOLOCK)
		WHERE POFulfillmentId IN (
			SELECT POFulfillmentId FROM tmp)

		-- Obtain globalId from related Containers

		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@containerEntityType, '_', sl.ContainerId),
			sl.ContainerId,
			@containerEntityType
		FROM ShipmentLoads sl (NOLOCK)
		WHERE sl.ContainerId IS NOT NULL AND sl.ShipmentId = @entityId

		-- Obtain globalId from related Vessels

		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@freightSchedulerEntityType, '_', i.ScheduleId),
			i.ScheduleId,
			@freightSchedulerEntityType
		FROM ConsignmentItineraries csmi (NOLOCK) INNER JOIN Itineraries i (NOLOCK) ON csmi.ItineraryId = i.Id AND (i.ModeOfTransport = 'sea' OR i.ModeOfTransport = 'air') AND i.ScheduleId IS NOT NULL
		WHERE csmi.ShipmentId = @entityId
	END
	ELSE
	IF (@entityType = @containerEntityType)
	BEGIN
		SET @fromEventLevel = @containerEventLevel

		-- Obtain GlobalId from related Shipments

		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@shipmentEntityType, '_', sl.ShipmentId),
			sl.ShipmentId,
			@shipmentEntityType
		FROM ShipmentLoads sl (NOLOCK) JOIN Containers c (NOLOCK) ON sl.ContainerId = c.Id
		WHERE c.Id = @entityId

		-- Obtain globalId from related POFulfillments

		;with tmp as (
			SELECT EntityId as ShipmentId FROM @globalIdActivitiesTbl WHERE EntityType = @shipmentEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@poffEntityType, '_', s.POFulfillmentId),
			s.POFulfillmentId,
			@poffEntityType
		FROM Shipments s (NOLOCK)
		WHERE s.POFulfillmentId IS NOT NULL AND s.Id IN (
			SELECT ShipmentId FROM tmp)

		-- Obtain globalId from related PurchaseOrders

		;with tmp as (
			SELECT EntityId as POFulfillmentId FROM @globalIdActivitiesTbl WHERE EntityType = @poffEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT DISTINCT
			CONCAT(@poEntityType, '_', PurchaseOrderId),
			PurchaseOrderId,
			@poEntityType
		FROM POFulfillmentOrders (NOLOCK)
		WHERE POFulfillmentId IN (
			SELECT POFulfillmentId FROM tmp)

		-- Obtain globalId from related Vessels

		;with tmp as (
			SELECT EntityId as ShipmentId FROM @globalIdActivitiesTbl WHERE EntityType = @shipmentEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT 
			CONCAT(@freightSchedulerEntityType, '_', i.ScheduleId),
			i.ScheduleId,
			@freightSchedulerEntityType
		FROM ConsignmentItineraries csmi (NOLOCK) INNER JOIN Itineraries i (NOLOCK) ON csmi.ItineraryId = i.Id AND (i.ModeOfTransport = 'sea' OR i.ModeOfTransport = 'air') AND i.ScheduleId IS NOT NULL
		WHERE csmi.ShipmentId IN (
			SELECT ShipmentId FROM tmp)

	END

	INSERT INTO @resultTbl (
		[Id],
		[ShipmentId],
		[ContainerId],
		[PurchaseOrderId],
		[POFulfillmentId],
		[FreightSchedulerId],
		[ActivityCode],
		[ActivityType],
		[ActivityLevel],		
		[ActivityDate],
		[ActivityDescription],
		[Location],
		[Remark],
		[Resolved],
		[Resolution],
		[ResolutionDate],
		[CreatedBy],
		[CreatedDate],
		[SortSequence]
	)
	SELECT
		a.Id,
		(CASE
			WHEN t.EntityType = @shipmentEntityType THEN t.EntityId
			ELSE Null
		END) as [ShipmentId],
		(CASE
			WHEN t.EntityType = @containerEntityType THEN t.EntityId
			ELSE Null
		END) as [ContainerId],
		(CASE
			WHEN t.EntityType = @poEntityType THEN t.EntityId
			ELSE Null
		END) as [PurchaseOrderId],
		(CASE
			WHEN t.EntityType = @poffEntityType THEN t.EntityId
			ELSE Null
		END) as [POFulfillmentId],
		(CASE
			WHEN t.EntityType = @freightSchedulerEntityType THEN t.EntityId
			ELSE Null
		END) as [FreightSchedulerId],
		a.ActivityCode,
		a.ActivityType,
		evt.LevelDescription as [ActivityLevel],
		a.ActivityDate,
		a.ActivityDescription,
		a.[Location],
		a.Remark,
		a.Resolved,
		a.Resolution,
		a.ResolutionDate,
		a.CreatedBy,
		a.CreatedDate,
		evc.SortSequence
	FROM GlobalIdActivities ga (NOLOCK)
	INNER JOIN @globalIdActivitiesTbl t ON ga.GlobalId = t.GlobalId
	INNER JOIN Activities a (NOLOCK) ON ga.ActivityId = a.Id
	INNER JOIN EventCodes evc ON a.ActivityCode = evc.ActivityCode
	INNER JOIN EventTypes evt ON evc.ActivityTypeCode = evt.Code
	WHERE evt.EventLevel >= @fromEventLevel

	-- Return
	SELECT * FROM @resultTbl

END