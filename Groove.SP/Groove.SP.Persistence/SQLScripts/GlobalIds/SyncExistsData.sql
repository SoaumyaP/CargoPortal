DELETE FROM GlobalIds

--sync shipments to global id tables
INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
SELECT 'SHI_' + CAST(s.Id AS varchar), 'Shipment', s.Id, GETUTCDATE()
FROM Shipments as s;

--sync containers to global id tables
INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
SELECT 'CTN_' + CAST(c.Id AS varchar), 'Container', c.Id, GETUTCDATE()
FROM Containers as c;

--sync billOfLadings to global id tables
INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
SELECT 'BOL_' + CAST(b.Id AS varchar), 'BillOfLading', b.Id, GETUTCDATE()
FROM BillOfLadings as b;

--sync masterBill to global id tables
INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
SELECT 'MBL_' + CAST(m.Id AS varchar), 'MasterBill', m.Id, GETUTCDATE()
FROM MasterBills as m;

--sync consignments to global id tables
INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
SELECT 'CSM_' + CAST(s.Id AS varchar), 'Consignment', s.Id, GETUTCDATE()
FROM Consignments as s;

--sync FreightSchedulers to GlobalIds tables
INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
SELECT 'FSC_' + CAST(f.Id AS varchar), 'FreightScheduler', f.Id, GETUTCDATE()
FROM FreightSchedulers as f;

--sync activities to global activities tables
DELETE FROM GlobalIdActivities
INSERT INTO GlobalIdActivities(GlobalId, ActivityId, CreatedDate)
SELECT 
	CASE 
		WHEN a.ShipmentId IS NOT NULL THEN 'SHI_' + CAST(a.ShipmentId AS varchar)
		WHEN a.ContainerId IS NOT NULL THEN 'CTN_' + CAST(a.ContainerId AS varchar)
		WHEN a.ConsignmentId IS NOT NULL THEN 'CSM_' + CAST(a.ConsignmentId AS varchar)
	END
	,a.Id, GETUTCDATE()
FROM Activities as a;

--sync activities to global attachment tables
DELETE FROM GlobalIdAttachments
INSERT INTO GlobalIdAttachments(GlobalId, AttachemntId, CreatedDate)
SELECT 
	CASE 
		WHEN a.ShipmentId IS NOT NULL THEN 'SHI_' + CAST(a.ShipmentId AS varchar)
		WHEN a.ContainerId IS NOT NULL THEN 'CTN_' + CAST(a.ContainerId AS varchar)
		WHEN a.BillOfLadingId IS NOT NULL THEN 'BOL_' + CAST(a.BillOfLadingId AS varchar)
		WHEN a.MasterBillOfLadingId IS NOT NULL THEN 'MBL_' + CAST(a.MasterBillOfLadingId AS varchar)
	END AS GlobalId,
	a.Id, GETUTCDATE()
FROM Attachments as a