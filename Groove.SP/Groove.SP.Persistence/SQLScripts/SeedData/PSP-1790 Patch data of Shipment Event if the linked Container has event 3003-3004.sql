-- =============================================
-- Author:		Phuoc Le
-- Created date: 29 July 2020
-- Description:	PSP-1790 Patch data of Shipment Event if the linked Container has event 3003/3004
-- =============================================


DECLARE @TNewActivities TABLE  
(  
	ActivityId BIGINT NOT NULL,
	GlobalId VARCHAR(50),
	Location NVARCHAR(512),
	ActivityDate DATETIME2(7)
)

-----------------------------------------------------------------------
-- Process event 3003 (CM - Container - Actual Port Departure)
------------------------------------------------------------------------
	PRINT N'Processing event 3003 (CM - Container - Actual Port Departure)';  
	DECLARE @T3003dActivities TABLE  
	(  
		ContainerId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	DECLARE @TShipment2029 TABLE  
	(  
		ShipmentId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	INSERT INTO @T3003dActivities
	SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS BIGINT) AS ContainerId, a.Location, a.ActivityDate
	FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
	WHERE a.ActivityCode = '3003'

	INSERT INTO @TShipment2029
	SELECT sl.ShipmentId AS ShipmentId, a.Location, a.ActivityDate
	FROM Containers c JOIN ShipmentLoads sl ON c.Id = sl.ContainerId
	JOIN @T3003dActivities a ON c.Id = a.ContainerId
	WHERE c.IsFCL = 1
		AND sl.ShipmentId NOT IN (
			SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS bigint) AS ShipmentId
			FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
			WHERE a.ActivityCode = '2029'
		)


	INSERT INTO @TShipment2029
	SELECT sl.ShipmentId AS ShipmentId, a.Location, a.ActivityDate
	FROM Containers c 
		JOIN Consolidations cs ON c.Id = cs.ContainerId
		JOIN ShipmentLoads sl ON cs.Id = sl.ConsolidationId 
		JOIN @T3003dActivities a ON cs.ContainerId = a.ContainerId
	WHERE c.IsFCL = 0
		AND sl.ShipmentId NOT IN (
			SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS bigint) AS ShipmentId
			FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
			WHERE a.ActivityCode = '2029'
		)

	-- Insert event 2029 (SM - Shipment actual departure from origin port)
	DELETE FROM @TNewActivities
	INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark, Location)
		OUTPUT INSERTED.Id, INSERTED.Remark, INSERTED.Location, INSERTED.ActivityDate
			INTO @TNewActivities
	SELECT '2029', 'Shipment actual departure from origin port', 'SM', ActivityDate, 'System', GETUTCDATE(), 'SHI_' + CONVERT(VARCHAR, ShipmentId), Location
	FROM @TShipment2029

	INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, Location, CreatedDate, CreatedBy)
	SELECT a.GlobalId, a.ActivityId, a.ActivityDate, a.Location, GETUTCDATE(), 'System'
	FROM @TNewActivities a

	UPDATE a
	SET Remark = NULL
	FROM Activities a JOIN @TNewActivities na ON na.ActivityId = a.Id


-----------------------------------------------------------------------
-- Process event 3004 (CM - Container - Actual Port Arrival)
------------------------------------------------------------------------
	PRINT N'Processing event 3004 (CM - Container - Actual Port Arrival)';  

	DECLARE @T3004dActivities TABLE  
	(  
		ContainerId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	DECLARE @TShipment2039 TABLE  
	(  
		ShipmentId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)


	INSERT INTO @T3004dActivities
	SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS BIGINT) AS ContainerId, a.Location, a.ActivityDate
	FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
	WHERE a.ActivityCode = '3004'

	INSERT INTO @TShipment2039
	SELECT sl.ShipmentId AS ShipmentId, a.Location, a.ActivityDate
	FROM Containers c JOIN ShipmentLoads sl ON c.Id = sl.ContainerId
	JOIN @T3004dActivities a ON c.Id = a.ContainerId
	WHERE c.IsFCL = 1
		AND sl.ShipmentId NOT IN (
			SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS bigint) AS ShipmentId
			FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
			WHERE a.ActivityCode = '2039'
		)

	INSERT INTO @TShipment2039
	SELECT sl.ShipmentId AS ShipmentId, a.Location, a.ActivityDate
	FROM Containers c 
		JOIN Consolidations cs ON c.Id = cs.ContainerId
		JOIN ShipmentLoads sl ON cs.Id = sl.ConsolidationId 
		JOIN @T3004dActivities a ON cs.ContainerId = a.ContainerId
	WHERE c.IsFCL = 0
		AND sl.ShipmentId NOT IN (
			SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS bigint) AS ShipmentId
			FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
			WHERE a.ActivityCode = '2039'
		)

	-- Insert event 2039 (SM - Shipment actual arrival at discharge port) into linked Shipment(s)
	DELETE FROM @TNewActivities
	INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark, Location)
		OUTPUT INSERTED.Id, INSERTED.Remark, INSERTED.Location, INSERTED.ActivityDate
			INTO @TNewActivities
	SELECT '2039', 'Shipment actual arrival at discharge port', 'SM', ActivityDate, 'System', GETUTCDATE(), 'SHI_' + CONVERT(VARCHAR, ShipmentId), Location
	FROM @TShipment2039

	INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, Location, CreatedDate, CreatedBy)
	SELECT a.GlobalId, a.ActivityId, a.ActivityDate, a.Location, GETUTCDATE(), 'System'
	FROM @TNewActivities a

	UPDATE a
	SET Remark = NULL
	FROM Activities a JOIN @TNewActivities na ON na.ActivityId = a.Id



-----------------------------------------------------------------------
-- Process event 2029-SM-Shipment actual departure from origin port
------------------------------------------------------------------------
	PRINT N'Processing event 2029-SM-Shipment actual departure from origin port';  

	DECLARE @T2029dActivities TABLE  
	(  
		ShipmentId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	DECLARE @TShipmentDispatchBooking TABLE  
	(  
		POFulfillmentId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	DECLARE @TShipmentDispatchPO TABLE  
	(  
		POId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	INSERT INTO @T2029dActivities
	SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS BIGINT) AS ShipmentId, a.Location, a.ActivityDate
	FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
	WHERE a.ActivityCode = '2029'


	-- Store SHIPMENT DISPATCH bookings 
	INSERT INTO @TShipmentDispatchBooking
	SELECT DISTINCT p.Id, a.Location, a.ActivityDate
	FROM POFulfillments p JOIN Shipments sm ON p.Id = sm.POFulfillmentId
	JOIN @T2029dActivities a ON a.ShipmentId = sm.Id
	WHERE p.Status = 10 AND p.Stage = 30

	-- Update Booking stage: FB CONFIRMED --> SHIPMENT DISPATCH
	UPDATE POFulfillments
	SET Stage = 40
	WHERE EXISTS (
		SELECT 1
		FROM @TShipmentDispatchBooking tmp
		WHERE tmp.POFulfillmentId = POFulfillments.Id 
	)

	-- Insert event 1068-FM-Goods Dispatch for SHIPMENT DISPATCH bookings
	DELETE FROM @TNewActivities
	INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark, Location)
		OUTPUT INSERTED.Id, INSERTED.Remark, INSERTED.Location, INSERTED.ActivityDate
			INTO @TNewActivities
	SELECT '1068', 'Goods Dispatch', 'FM', ActivityDate, 'System', GETUTCDATE(), 'POF_' + CONVERT(VARCHAR, POFulfillmentId), Location
	FROM @TShipmentDispatchBooking

	INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, Location, CreatedDate, CreatedBy)
	SELECT a.GlobalId, a.ActivityId, a.ActivityDate, a.Location, GETUTCDATE(), 'System'
	FROM @TNewActivities a

	UPDATE a
	SET Remark = NULL
	FROM Activities a JOIN @TNewActivities na ON na.ActivityId = a.Id

	-- Store SHIPMENT DISPATCH POs
	INSERT INTO @TShipmentDispatchPO
	SELECT DISTINCT po.Id, poff.Location, poff.ActivityDate
	FROM POFulfillmentOrders cpo 
		JOIN @TShipmentDispatchBooking poff ON cpo.POFulfillmentId = poff.POFulfillmentId
		JOIN PurchaseOrders po ON cpo.PurchaseOrderId = po.Id
	WHERE po.Stage = 40

	-- Update PO stage: FB CONFIRMED --> SHIPMENT DISPATCH
	UPDATE PurchaseOrders
	SET Stage = 50
	WHERE EXISTS (
		SELECT 1
		FROM @TShipmentDispatchPO po
		WHERE po.POId = PurchaseOrders.Id 
	)

	-- Insert event 1009-PM-Shipment Dispatch into all associated POs
	DELETE FROM @TNewActivities
	INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark, Location)
		OUTPUT INSERTED.Id, INSERTED.Remark, INSERTED.Location, INSERTED.ActivityDate
			INTO @TNewActivities
	SELECT '1009', 'Shipment Dispatch', 'PM', ActivityDate, 'System', GETUTCDATE(), 'CPO_' + CONVERT(VARCHAR, POId), Location
	FROM @TShipmentDispatchPO

	INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, Location, CreatedDate, CreatedBy)
	SELECT a.GlobalId, a.ActivityId, a.ActivityDate, a.Location, GETUTCDATE(), 'System'
	FROM @TNewActivities a

	UPDATE a
	SET Remark = NULL
	FROM Activities a JOIN @TNewActivities na ON na.ActivityId = a.Id

-----------------------------------------------------------------------
-- Process event 2039-SM-Shipment actual arrival at discharge port
------------------------------------------------------------------------
	PRINT N'Processing event 2039-SM-Shipment actual arrival at discharge port';  

	DECLARE @T2039dActivities TABLE  
	(  
		ShipmentId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	DECLARE @TClosedBooking TABLE  
	(  
		POFulfillmentId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	DECLARE @TClosedPO TABLE  
	(  
		POId BIGINT NOT NULL,
		Location NVARCHAR(512),
		ActivityDate DATETIME2(7)
	)

	INSERT INTO @T2039dActivities
	SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS BIGINT) AS ShipmentId, a.Location, a.ActivityDate
	FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
	WHERE a.ActivityCode = '2039'


	-- Store CLOSED bookings 
	INSERT INTO @TClosedBooking
	SELECT DISTINCT p.Id, a.Location, a.ActivityDate
	FROM POFulfillments p JOIN Shipments sm ON p.Id = sm.POFulfillmentId
	JOIN @T2039dActivities a ON a.ShipmentId = sm.Id
	WHERE p.Status = 10 AND p.Stage != 50 AND sm.ServiceType LIKE '%to-Port'

	-- Update Booking Stage to CLOSED
	UPDATE POFulfillments
	SET Stage = 50
	WHERE EXISTS (
		SELECT 1
		FROM @TClosedBooking tmp
		WHERE tmp.POFulfillmentId = POFulfillments.Id 
	) 

	-- Insert event 1071-FM-Booking Closed for CLOSED bookings
	DELETE FROM @TNewActivities
	INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark, Location)
		OUTPUT INSERTED.Id, INSERTED.Remark, INSERTED.Location, INSERTED.ActivityDate
			INTO @TNewActivities
	SELECT '1071', 'Booking Closed', 'FM', ActivityDate, 'System', GETUTCDATE(), 'POF_' + CONVERT(VARCHAR, POFulfillmentId), Location
	FROM @TClosedBooking

	INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, Location, CreatedDate, CreatedBy)
	SELECT a.GlobalId, a.ActivityId, a.ActivityDate, a.Location, GETUTCDATE(), 'System'
	FROM @TNewActivities a

	UPDATE a
	SET Remark = NULL
	FROM Activities a JOIN @TNewActivities na ON na.ActivityId = a.Id

	-- Store Closed POs
	INSERT INTO @TClosedPO
	SELECT DISTINCT po.Id, tpoff.Location, tpoff.ActivityDate
	FROM POFulfillmentOrders cpo 
		JOIN @TClosedBooking tpoff ON cpo.POFulfillmentId = tpoff.POFulfillmentId
		JOIN PurchaseOrders po ON cpo.PurchaseOrderId = po.Id
	WHERE NOT EXISTS (
		SELECT 1
		FROM POLineItems poi
		WHERE poi.PurchaseOrderId = po.Id AND poi.BalanceUnitQty != 0
	)
	AND NOT EXISTS (
		SELECT 1
		FROM POFulfillments poff
			JOIN POFulfillmentOrders cpo2 on cpo2.POFulfillmentId = poff.Id
		WHERE poff.Id != tpoff.POFulfillmentId AND poff.Status = 10 AND poff.Stage != 50 AND CPO2.PurchaseOrderId = po.Id
	)

	-- Update PO stage --> CLOSED
	UPDATE PurchaseOrders
	SET Stage = 60
	WHERE EXISTS (
		SELECT 1
		FROM @TClosedPO po
		WHERE po.POId = PurchaseOrders.Id 
	)

	-- Insert event 1010-PM-PO Closed event into all associated POs
	DELETE FROM @TNewActivities
	INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark, Location)
		OUTPUT INSERTED.Id, INSERTED.Remark, INSERTED.Location, INSERTED.ActivityDate
			INTO @TNewActivities
	SELECT '1010', 'PO Closed', 'PM', ActivityDate, 'System', GETUTCDATE(), 'CPO_' + CONVERT(VARCHAR, POId), Location
	FROM @TClosedPO

	INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, Location, CreatedDate, CreatedBy)
	SELECT a.GlobalId, a.ActivityId, a.ActivityDate, a.Location, GETUTCDATE(), 'System'
	FROM @TNewActivities a

	UPDATE a
	SET Remark = NULL
	FROM Activities a JOIN @TNewActivities na ON na.ActivityId = a.Id

