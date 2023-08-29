SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetFilterValue_ActivityTimeline', 'P') IS NOT NULL
DROP PROC spu_GetFilterValue_ActivityTimeline
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 19 September 2022
-- Description:	Get filter value datasource for activity timeline
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetFilterValue_ActivityTimeline]
	@entityId BIGINT = 0,
	@entityType NVARCHAR(50),
	@filterBy NVARCHAR(50)
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

	DECLARE @resultTbl TABLE (
		[Text] NVARCHAR(MAX)
	)

	IF (@filterBy = 'BookingNo')
	BEGIN
		INSERT INTO @resultTbl ([Text])
		SELECT poff.Number
		FROM POFulfillments poff INNER JOIN POFulfillmentOrders pod ON poff.Id = pod.POFulfillmentId
		WHERE @entityType = @poEntityType and pod.PurchaseOrderId = @entityId and poff.[Status] = 10 --active

		UNION

		SELECT Number
		FROM POFulfillments
		WHERE @entityType = @poffEntityType and Id = @entityId and [Status] = 10 --active

	END
	ELSE
	IF (@filterBy = 'ShipmentNo')
	BEGIN
		INSERT INTO @resultTbl ([Text])
		SELECT sm.ShipmentNo
		FROM Shipments sm 
		WHERE (EXISTS (
				SELECT 1
				FROM POFulfillmentOrders pod
				WHERE pod.POFulfillmentId = sm.POFulfillmentId and pod.PurchaseOrderId = @entityId and @entityType = @poEntityType
		)
		
		OR EXISTS (
			SELECT 1
			FROM CargoDetails CD 
			WHERE CD.ShipmentId = sm.Id AND CD.OrderId = @entityId AND sm.POFulfillmentId IS NULL
		)) 

		and sm.[Status] = 'active'
		

		UNION

		SELECT ShipmentNo
		FROM Shipments
		WHERE POFulfillmentId = @entityId and @entityType = @poffEntityType and [Status] = 'active'

	END
	ELSE
	IF (@filterBy = 'ContainerNo')
	BEGIN
		INSERT INTO @resultTbl ([Text])
		SELECT ctn.ContainerNo
		FROM Shipments sm INNER JOIN ShipmentLoads sl ON sm.Id = sl.ShipmentId INNER JOIN Containers ctn ON ctn.Id = sl.ContainerId
		WHERE (EXISTS (
				SELECT 1
				FROM POFulfillmentOrders pod
				WHERE pod.PurchaseOrderId = @entityId AND @entityType = @poEntityType AND sm.POFulfillmentId = pod.POFulfillmentId
				)
		OR EXISTS (
			SELECT 1
			FROM CargoDetails CD 
			WHERE CD.ShipmentId = sm.Id AND CD.OrderId = @entityId AND sm.POFulfillmentId IS NULL
		))
		
		AND sm.[Status] = 'active'
		
		UNION

		SELECT ctn.ContainerNo
		FROM Shipments sm INNER JOIN ShipmentLoads sl ON sm.Id = sl.ShipmentId INNER JOIN Containers ctn ON ctn.Id = sl.ContainerId
		WHERE sm.POFulfillmentId = @entityId AND @entityType = @poffEntityType AND sm.[Status] = 'active'
	END
	ELSE
	IF (@filterBy = 'VesselName')
	BEGIN
		INSERT INTO @resultTbl ([Text])
		SELECT IIF(fs.ModeOfTransport = 'Sea', CONCAT(fs.VesselName, '/', fs.Voyage), fs.FlightNumber) as [Text]
		FROM Itineraries iti INNER JOIN FreightSchedulers fs ON iti.ScheduleId = fs.Id
		WHERE EXISTS (
															SELECT 1
															FROM ConsignmentItineraries ci INNER JOIN Shipments sm ON ci.ShipmentId = sm.Id
																INNER JOIN POFulfillmentOrders pod ON pod.POFulfillmentId = sm.POFulfillmentId
															WHERE @entityType = @poEntityType and pod.PurchaseOrderId = @entityId and ci.ItineraryId = iti.Id
		)
		OR EXISTS (
			SELECT 1
			FROM ConsignmentItineraries ci 
			INNER JOIN Shipments sm ON ci.ShipmentId = sm.Id AND sm.POFulfillmentId IS NULL
			INNER JOIN CargoDetails CD ON CD.ShipmentId = sm.Id AND CD.OrderId = @entityId
			WHERE @entityType = @poEntityType and ci.ItineraryId = iti.Id
		)

		UNION

		SELECT IIF(fs.ModeOfTransport = 'Sea', CONCAT(fs.VesselName, '/', fs.Voyage), fs.FlightNumber) as [Text]
		FROM Itineraries iti INNER JOIN FreightSchedulers fs ON iti.ScheduleId = fs.Id
		WHERE EXISTS (
															SELECT 1
															FROM ConsignmentItineraries ci INNER JOIN Shipments sm ON ci.ShipmentId = sm.Id
															WHERE @entityType = @poffEntityType and sm.POFulfillmentId = @entityId and ci.ItineraryId = iti.Id
		)
	END

	SELECT * FROM @resultTbl ORDER BY [Text] asc
END

GO