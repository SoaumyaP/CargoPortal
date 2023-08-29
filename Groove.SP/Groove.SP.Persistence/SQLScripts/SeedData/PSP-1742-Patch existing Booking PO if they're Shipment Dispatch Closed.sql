-- =============================================
-- Author:		Phuoc Le
-- Created date: 3 July 2020
-- Description:	PSP-1742 [Shipment-Booking-PO] Patch existing Booking/PO if they're Shipment Dispatch/Closed
-- =============================================


DECLARE @TShipmentDispatchBooking TABLE  
(  
	POFulfillmentId BIGINT NOT NULL
)

DECLARE @TClosedBooking TABLE  
(  
	POFulfillmentId BIGINT NOT NULL
)

DECLARE @TNewActivities TABLE  
(  
	ActivityId BIGINT NOT NULL,
	GlobalPOFFId VARCHAR(50)
)

DECLARE @TShipmentDispatchPO TABLE  
(  
	POId BIGINT NOT NULL
)

DECLARE @TClosedPO TABLE  
(  
	POId BIGINT NOT NULL
)

-----------------------------------------------------------------------
-- Process event 2029-SM-Shipment actual departure from origin port
------------------------------------------------------------------------

-- Store SHIPMENT DISPATCH bookings 
INSERT INTO @TShipmentDispatchBooking
SELECT DISTINCT p.Id
FROM POFulfillments p JOIN Shipments sm ON p.Id = sm.POFulfillmentId
WHERE p.Status = 10 AND p.Stage = 30 AND sm.Id IN (
	SELECT DISTINCT CAST(SUBSTRING(ga.GlobalId, 5, 5) AS bigint) AS ShipmentId
	FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
	WHERE a.ActivityCode = '2029'
)

-- Update Booking stage: FB CONFIRMED --> SHIPMENT DISPATCH
UPDATE POFulfillments
SET Stage = 40
WHERE EXISTS (
	SELECT 1
	FROM @TShipmentDispatchBooking tmp
	WHERE tmp.POFulfillmentId = POFulfillments.Id 
)

-- Insert event 1068-FM-Goods Dispatch for SHIPMENT DISPATCH bookings
INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark)
	OUTPUT INSERTED.Id, INSERTED.Remark
        INTO @TNewActivities
SELECT '1068', 'Goods Dispatch', 'FM', GETUTCDATE(), 'System', GETUTCDATE(), 'POF_' + CONVERT(VARCHAR, POFulfillmentId)
FROM @TShipmentDispatchBooking

INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, CreatedDate, CreatedBy)
SELECT a.GlobalPOFFId, a.ActivityId, GETUTCDATE(), GETUTCDATE(), 'System'
FROM @TNewActivities a

-- Store SHIPMENT DISPATCH POs
INSERT INTO @TShipmentDispatchPO
SELECT DISTINCT po.Id
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
INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark)
	OUTPUT INSERTED.Id, INSERTED.Remark
        INTO @TNewActivities
SELECT '1009', 'Shipment Dispatch', 'PM', GETUTCDATE(), 'System', GETUTCDATE(), 'CPO_' + CONVERT(VARCHAR, POId)
FROM @TShipmentDispatchPO

INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, CreatedDate, CreatedBy)
SELECT a.GlobalPOFFId, a.ActivityId, GETUTCDATE(), GETUTCDATE(), 'System'
FROM @TNewActivities a




-----------------------------------------------------------------------
-- Process event 2039-SM-Shipment actual arrival at discharge port
-- and 2054-SM-Shipment handover to consignee
------------------------------------------------------------------------

-- Store CLOSED bookings 
INSERT INTO @TClosedBooking
SELECT DISTINCT p.Id
FROM POFulfillments p JOIN Shipments sm ON p.Id = sm.POFulfillmentId
WHERE p.Status = 10 AND EXISTS (
	SELECT 1
	FROM GlobalIdActivities ga JOIN Activities a ON ga.ActivityId = a.Id
	WHERE sm.Id = CAST(SUBSTRING(ga.GlobalId, 5, 5) AS bigint)
	AND ((a.ActivityCode = '2039' AND sm.ServiceType LIKE '%to-Port') OR (a.ActivityCode = '2054' AND sm.ServiceType LIKE '%to-Door')) 
)

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
INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark)
	OUTPUT INSERTED.Id, INSERTED.Remark
        INTO @TNewActivities
SELECT '1071', 'Booking Closed', 'FM', GETUTCDATE(), 'System', GETUTCDATE(), 'POF_' + CONVERT(VARCHAR, POFulfillmentId)
FROM @TClosedBooking

INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, CreatedDate, CreatedBy)
SELECT a.GlobalPOFFId, a.ActivityId, GETUTCDATE(), GETUTCDATE(), 'System'
FROM @TNewActivities a

-- Store Closed POs
INSERT INTO @TClosedPO
SELECT DISTINCT po.Id
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
INSERT INTO Activities(ActivityCode, ActivityDescription, ActivityType, ActivityDate, CreatedBy, CreatedDate, Remark)
	OUTPUT INSERTED.Id, INSERTED.Remark
        INTO @TNewActivities
SELECT '1010', 'PO Closed', 'PM', GETUTCDATE(), 'System', GETUTCDATE(), 'CPO_' + CONVERT(VARCHAR, POId)
FROM @TClosedPO

INSERT INTO GlobalIdActivities(GlobalId, ActivityId, ActivityDate, CreatedDate, CreatedBy)
SELECT a.GlobalPOFFId, a.ActivityId, GETUTCDATE(), GETUTCDATE(), 'System'
FROM @TNewActivities a

