SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetActivityTimeLineCrossModule', 'P') IS NOT NULL
DROP PROC spu_GetActivityTimeLineCrossModule
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 7 September 2021
-- Description:	Get cross module activity list for timeline (filtering/ordering support)
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetActivityTimeLineCrossModule]
	@entityId BIGINT = 0,
	@entityType NVARCHAR(50),
	@filterFromDate DATETIME2(7) = '0001/1/1',
	@filterToDate DATETIME2(7) = '0001/1/1',
	@filterBy NVARCHAR(50) = '',
	@filterValue NVARCHAR(255) = '',
	@orderByDESC BIT = 1
	--@pageIndex INT = 0,
	--@pageSize INT = 5
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
		[POFulfillmentNos] NVARCHAR(MAX),
		[ShipmentNos] NVARCHAR(MAX),
		[ContainerNos] NVARCHAR(MAX),
		[VesselFlight] NVARCHAR(MAX),
		[Resolved] BIT,
		[Resolution] NVARCHAR(MAX),
		[ResolutionDate] DATETIME2(7),
		[CreatedBy] NVARCHAR(256),
		[CreatedDate] DATETIME2(7),
		[GlobalIdActivityId] BIGINT,
		[GlobalId] NVARCHAR(450),
		[GlobalIdLocation] NVARCHAR(MAX),
		[GlobalIdRemark] NVARCHAR(MAX),
		[GlobalIdActivityDate] DATETIME2(7),
		[GlobalIdCreatedDate] DATETIME2(7),
		[GlobalIdUpdatedDate] DATETIME2(7),
		[GlobalIdCreatedBy] NVARCHAR(256),
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

	
		-- Obtain globalId from related Shipments by using CargoDetails without booking
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
		WHERE s.[Status] = 'active' AND S.POFulfillmentId  IS NULL
		AND EXISTS (
			SELECT 1
			FROM CargoDetails CD 
			WHERE CD.ShipmentId = S.Id AND CD.OrderId = @entityId
		)

		-- Obtain globalId from related Containers

		;with tmp as (
			SELECT EntityId as ShipmentId FROM @globalIdActivitiesTbl WHERE EntityType = @shipmentEntityType
		)
		INSERT INTO @globalIdActivitiesTbl (
			GlobalId,
			EntityId,
			EntityType
		)
		SELECT DISTINCT
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
		SELECT DISTINCT
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
		SELECT DISTINCT
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
		[POFulfillmentNos],
		[ShipmentNos],
		[ContainerNos],
		[VesselFlight],
		[Resolved],
		[Resolution],
		[ResolutionDate],
		[CreatedBy],
		[CreatedDate],
		[GlobalIdActivityId],
		[GlobalId],
		[GlobalIdLocation],
		[GlobalIdRemark],
		[GlobalIdActivityDate],
		[GlobalIdCreatedDate],
		[GlobalIdUpdatedDate],
		[GlobalIdCreatedBy],
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
		t1.POFulfillmentNos,
		t2.ShipmentNos,
		t3.ContainerNos,
		t4.VesselFlight,
		a.Resolved,
		a.Resolution,
		a.ResolutionDate,
		a.CreatedBy,
		a.CreatedDate,
		ga.Id,
		ga.GlobalId,
		ga.[Location],
		ga.Remark,
		ga.ActivityDate,
		ga.CreatedDate,
		ga.UpdatedDate,
		ga.CreatedBy,
		evc.SortSequence
	FROM GlobalIdActivities ga (NOLOCK)
	INNER JOIN @globalIdActivitiesTbl t ON ga.GlobalId = t.GlobalId
	INNER JOIN Activities a (NOLOCK) ON ga.ActivityId = a.Id
	INNER JOIN EventCodes evc ON a.ActivityCode = evc.ActivityCode
	INNER JOIN EventTypes evt ON evc.ActivityTypeCode = evt.Code
	OUTER APPLY (
		SELECT STUFF(
			            (SELECT ';' + CONCAT(t.Number, '~', t.Id, '~', t.Stage, '~', t.OrderFulfillmentPolicy)
							FROM (
								SELECT tpoff.*
								FROM (

										SELECT Number, Id, Stage, OrderFulfillmentPolicy
										FROM POFulfillments
										WHERE Id = t.EntityId AND t.EntityType = @poffEntityType AND [Status] = 10 -- active

										UNION

										SELECT poff.Number, poff.Id, poff.Stage, poff.OrderFulfillmentPolicy
										FROM Shipments sm
											INNER JOIN POFulfillments poff ON sm.POFulfillmentId = poff.Id
										WHERE  sm.Id = t.EntityId AND t.EntityType = @shipmentEntityType AND poff.[Status] = 10 -- active

										UNION

										SELECT poff.Number, poff.Id, poff.Stage, poff.OrderFulfillmentPolicy
										FROM ShipmentLoads sl (NOLOCK)
											INNER JOIN Shipments sm ON sl.ShipmentId = sm.Id INNER JOIN POFulfillments poff ON poff.Id = sm.POFulfillmentId
										WHERE sl.ContainerId IS NOT NULL AND sl.ContainerId = t.EntityId AND t.EntityType = @containerEntityType
											AND sm.[Status] = 'active' AND poff.[Status] = 10 -- active

										UNION

										SELECT poff.Number, poff.Id, poff.Stage, poff.OrderFulfillmentPolicy
										FROM Shipments sm INNER JOIN POFulfillments poff ON sm.POFulfillmentId = poff.Id
										WHERE EXISTS (
											SELECT 1
											FROM Itineraries iti INNER JOIN ConsignmentItineraries ci ON iti.Id = ci.ItineraryId
											WHERE ci.ShipmentId = sm.Id AND iti.ScheduleId = t.EntityId AND t.EntityType =  @freightSchedulerEntityType)
											AND sm.[Status] = 'active' AND poff.[Status] = 10 -- active
								) tpoff

								-- To make sure that all bookings have been linked to the current entity Id
								WHERE
								(@entityType != @poffEntityType OR tpoff.Id = @entityId) AND
								(@entityType != @poEntityType OR EXISTS (
									SELECT 1 FROM POFulfillmentOrders WHERE POFulfillmentId = tpoff.Id AND PurchaseOrderId = @entityId
								))
						) t FOR Xml Path('')), 1, 1, ''
	    ) AS POFulfillmentNos
		
	) AS t1

	OUTER APPLY (
		SELECT STUFF(
			            (SELECT ';' + CONCAT(t.ShipmentNo, '~', t.Id)
							FROM (
								SELECT tshipment.*
								FROM (

										SELECT ShipmentNo, Id, POFulfillmentId
										FROM Shipments
										WHERE  Id = t.EntityId AND t.EntityType = @shipmentEntityType AND [Status] = 'active' -- active

										UNION

										SELECT sm.ShipmentNo, sm.Id, sm.POFulfillmentId
										FROM ShipmentLoads sl (NOLOCK)
											INNER JOIN Shipments sm ON sl.ShipmentId = sm.Id
										WHERE sl.ContainerId = t.EntityId AND t.EntityType = @containerEntityType AND sm.[Status] = 'active'

										UNION

										SELECT sm.ShipmentNo, sm.Id, sm.POFulfillmentId
										FROM Shipments sm
										WHERE EXISTS (
											SELECT 1
											FROM Itineraries iti INNER JOIN ConsignmentItineraries ci ON iti.Id = ci.ItineraryId
											WHERE ci.ShipmentId = sm.Id AND iti.ScheduleId = t.EntityId AND t.EntityType =  @freightSchedulerEntityType)
											AND sm.[Status] = 'active'
								) tshipment

								-- To make sure that all shipments have been linked to the current entity Id
								WHERE
								(@entityType != @poffEntityType OR EXISTS (
									SELECT 1 FROM Shipments WHERE Id = tshipment.Id AND POFulfillmentId = @entityId
								))
								AND
								(@entityType != @poEntityType 
									OR( 
											EXISTS (
											SELECT 1 FROM POFulfillmentOrders pod INNER JOIN Shipments sm ON pod.POFulfillmentId = sm.POFulfillmentId
											WHERE sm.Id = tshipment.Id AND pod.PurchaseOrderId = @entityId
											)
											OR EXISTS (
												SELECT 1
												FROM CargoDetails CD 
												WHERE CD.ShipmentId = tshipment.Id AND CD.OrderId = @entityId AND tshipment.POFulfillmentId IS NULL
											)
										)
								)

						) t FOR Xml Path('')), 1, 1, ''
	    ) AS ShipmentNos
		
	) AS t2

	OUTER APPLY (
		SELECT STUFF(
			            (SELECT ';' + CONCAT(t.ContainerNo, '~', t.Id)
							FROM (
								SELECT *
								FROM
								(

										SELECT ContainerNo, Id
										FROM Containers
										WHERE Id = t.EntityId AND t.EntityType = @containerEntityType

										UNION

										SELECT ctn.ContainerNo, ctn.Id
										FROM Shipments sm INNER JOIN ShipmentLoads sl ON sm.Id = sl.ShipmentId INNER JOIN Containers ctn ON ctn.Id = sl.ContainerId
										WHERE EXISTS (
											SELECT 1
											FROM Itineraries iti INNER JOIN ConsignmentItineraries ci ON iti.Id = ci.ItineraryId
											WHERE ci.ShipmentId = sm.Id AND iti.ScheduleId = t.EntityId AND t.EntityType =  @freightSchedulerEntityType)
											AND sm.[Status] = 'active'
								) tcontainer

								-- To make sure that all containers have been linked to the current entity Id
								WHERE
								(
									@entityType != @poffEntityType OR EXISTS (
									SELECT 1 FROM Shipments sm INNER JOIN ShipmentLoads sl ON sm.Id = sl.ShipmentId
									WHERE sl.ContainerId = tcontainer.Id AND sm.POFulfillmentId = @entityId
								))
								AND (
									@entityType != @poEntityType OR( 
									EXISTS (
										SELECT 1 FROM POFulfillmentOrders pod INNER JOIN Shipments sm ON pod.POFulfillmentId = sm.POFulfillmentId 
																			  INNER JOIN ShipmentLoads sl ON sm.Id = sl.ShipmentId
										WHERE sl.ContainerId = tcontainer.Id AND pod.PurchaseOrderId = @entityId
									)

									OR EXISTS (
											SELECT 1
											FROM CargoDetails CD
											INNER JOIN Shipments S ON CD.ShipmentId = S.Id AND S.POFulfillmentId IS NULL
											INNER JOIN ShipmentLoads sl ON S.Id = sl.ShipmentId AND sl.ContainerId = tcontainer.Id
											WHERE CD.OrderId = @entityId 
									)
								))
						) t FOR Xml Path('')), 1, 1, ''
	    ) AS ContainerNos
		
	) AS t3

	OUTER APPLY (
		SELECT STUFF(
			            (SELECT ';' + t.Number
							FROM (
								SELECT CONCAT(IIF(fs.ModeOfTransport = 'Sea', CONCAT(fs.VesselName, '/', Voyage), fs.FlightNumber), '~', fs.Id) as Number
								FROM FreightSchedulers fs
								WHERE fs.Id = t.EntityId AND t.EntityType =  @freightSchedulerEntityType
						) t FOR Xml Path('')), 1, 1, ''
	    ) AS VesselFlight
		
	) AS t4
	WHERE evt.EventLevel >= @fromEventLevel
	AND (CAST(@filterFromDate as DATE) = '0001/1/1' OR CAST(ga.ActivityDate as DATE) >= CAST(@filterFromDate as DATE))
	AND (CAST(@filterToDate as DATE) = '0001/1/1' OR CAST(ga.ActivityDate as DATE) <= CAST(@filterToDate as DATE))
	AND (@filterBy = '' OR @filterValue = '' OR (
		(@filterBy != 'BookingNo' OR t1.POFulfillmentNos LIKE '%' + @filterValue + '%') AND
		(@filterBy != 'ShipmentNo' OR t2.ShipmentNos LIKE '%' + @filterValue + '%') AND
		(@filterBy != 'ContainerNo' OR t3.ContainerNos LIKE '%' + @filterValue + '%') AND
		(@filterBy != 'VesselName' OR t4.VesselFlight LIKE '%' + @filterValue + '%')
	))

	-- Return
	SELECT COUNT(Id) AS Total FROM @resultTbl

	IF (@orderByDESC = 1)
	BEGIN
		SELECT * FROM @resultTbl
		ORDER BY CAST(CONVERT(CHAR(16), ActivityDate, 20) AS datetime) DESC, SortSequence DESC
		--OFFSET (@pageIndex * @pageSize) ROWS FETCH NEXT (@pageSize) ROWS ONLY
	END
	ELSE
	BEGIN
		SELECT * FROM @resultTbl
		ORDER BY CAST(CONVERT(CHAR(16), ActivityDate, 20) AS datetime) ASC, SortSequence DESC
		--OFFSET (@pageIndex * @pageSize) ROWS FETCH NEXT (@pageSize) ROWS ONLY
	END
END

GO