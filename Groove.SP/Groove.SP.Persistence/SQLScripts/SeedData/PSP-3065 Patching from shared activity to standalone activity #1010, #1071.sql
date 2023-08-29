-- =============================================
-- Author:			Hau Nguyen
-- Created date:	27 Dec 2021
-- Description:		[PSP-3065]  Patching from shared activity to standalone activity #1010, #1071 then correct their event dates
-- =============================================
SET NOCOUNT ON;
	
BEGIN TRANSACTION

DECLARE @TInsertedActivity TABLE (
	[Id] BIGINT,
	[ActivityCode] NVARCHAR(MAX),
	[Location] NVARCHAR(MAX),
	[Remark] NVARCHAR(MAX),
	[ActivityDate] DATETIME2(7),
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256),
	[GlobalIdActivityId] BIGINT
)

DECLARE @TUpdatingActivity1071 TABLE (
	[Id] BIGINT,
	[ActivityCode] NVARCHAR(MAX),
	[Location] NVARCHAR(MAX),
	[Remark] NVARCHAR(MAX),
	[ActivityDate] DATETIME2(7),
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256),
	[GlobalIdActivityId] BIGINT,
	[VesselArrivalDate] DATETIME2(7)
)

DECLARE @TUpdatingActivity1010 TABLE (
	[Id] BIGINT,
	[ActivityCode] NVARCHAR(MAX),
	[Location] NVARCHAR(MAX),
	[Remark] NVARCHAR(MAX),
	[ActivityDate] DATETIME2(7),
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256),
	[GlobalIdActivityId] BIGINT,
	[VesselArrivalDate] DATETIME2(7)
)

;with CTE as (
	SELECT gba.Id as GlobalIdActivityId, act.*
	FROM GlobalIdActivities gba (NOLOCK) JOIN Activities act (NOLOCK) ON gba.ActivityId = act.Id
	WHERE act.ActivityCode IN ('1010', '1071')
	AND EXISTS (
		SELECT 1
		FROM GlobalIdActivities gba1 (NOLOCK)
		WHERE gba1.ActivityId = gba.ActivityId AND gba1.GlobalId <> gba.GlobalId
	)
)

--[dbo].Activities
--Insert standalone activities from shared activities
--
INSERT INTO [dbo].[Activities]
(
	[ActivityCode],
	[ActivityType],
	[ActivityDescription],
	[ActivityDate],
	[Location],
	[Remark],
	[Resolution],
	[CreatedDate],
	[CreatedBy],
	[UpdatedBy],
	[UpdatedDate]
)
OUTPUT inserted.Id, inserted.ActivityCode, inserted.[Location], inserted.Remark, inserted.ActivityDate, inserted.CreatedDate, inserted.CreatedBy, CAST(inserted.Resolution as BIGINT)
INTO @TInsertedActivity
SELECT
	ActivityCode,
	ActivityType,
	ActivityDescription,
	ActivityDate,
	[Location],
	Remark,
	GlobalIdActivityId,
	CreatedDate,
	CreatedBy,
	UpdatedBy,
	UpdatedDate
FROM CTE

--[dbo].GlobalIdActivity
--Update association between activity and entity
--
UPDATE Gba
SET 
    Gba.ActivityId = Act.Id
FROM [dbo].[GlobalIdActivities] Gba
INNER JOIN
@TInsertedActivity Act
ON Gba.Id = Act.GlobalIdActivityId

select * from @TInsertedActivity
--
--Update booking event #1071 by #7002/ #2054
--
INSERT INTO @TUpdatingActivity1071 (
	[Id],
	[ActivityCode],
	[Location],
	[Remark],
	[ActivityDate],
	[CreatedDate],
	[CreatedBy],
	[GlobalIdActivityId],
	[VesselArrivalDate]
)
SELECT
	ins.Id,
	ins.ActivityCode,
	ins.[Location],
	ins.Remark,
	ins.ActivityDate,
	ins.CreatedDate,
	ins.CreatedBy,
	ins.GlobalIdActivityId,
	t1.ActivityDate as [VesselArrivalDate]
FROM @TInsertedActivity ins
INNER JOIN [dbo].[GlobalIdActivities] gba ON ins.GlobalIdActivityId = gba.Id
INNER JOIN [dbo].[GlobalIds] gid ON gba.GlobalId = gid.Id
CROSS APPLY (
	SELECT TOP 1 gba1.ActivityDate
	FROM
	(
		SELECT
			CONCAT('FSC_', i.ScheduleId) as GlobalId
		FROM ConsignmentItineraries csmi (NOLOCK) INNER JOIN Itineraries i (NOLOCK) ON csmi.ItineraryId = i.Id AND i.ModeOfTransport = 'sea' AND i.ScheduleId IS NOT NULL
		WHERE csmi.ShipmentId IN (
			SELECT
				s.Id
			FROM Shipments s (NOLOCK)
			WHERE s.[Status] = 'active' --active shipment
			AND s.ServiceType LIKE '%-to-Port'
			AND s.POFulfillmentId = gid.EntityId
		)
		UNION
		SELECT
			CONCAT('SHI_', s.Id) as GlobalId
		FROM Shipments s (NOLOCK)
		WHERE s.[Status] = 'active' --active shipment
		AND s.ServiceType LIKE '%-to-Door'
		AND s.POFulfillmentId = gid.EntityId

	) t INNER JOIN GlobalIdActivities gba1 ON gba1.GlobalId = t.GlobalId INNER JOIN Activities act1 ON act1.Id = gba1.ActivityId
	WHERE act1.ActivityCode IN ('7002', '2054') AND act1.[Location] = ins.[Location]
	ORDER BY gba1.CreatedDate DESC
) t1
WHERE ins.ActivityCode = '1071' AND CAST(t1.ActivityDate as DATE) <> CAST(ins.ActivityDate as DATE)

UPDATE Act
SET Act.ActivityDate = T.VesselArrivalDate
FROM [dbo].Activities Act INNER JOIN @TUpdatingActivity1071 T
ON Act.Id = T.Id

UPDATE Gba
SET Gba.ActivityDate = Act.VesselArrivalDate
FROM [dbo].[GlobalIdActivities] Gba INNER JOIN @TUpdatingActivity1071 Act
ON Gba.Id = Act.GlobalIdActivityId

select * from @TUpdatingActivity1071
--
--Update PO closed event #1010 by #1071
--
INSERT INTO @TUpdatingActivity1010 (
	[Id],
	[ActivityCode],
	[Location],
	[Remark],
	[ActivityDate],
	[CreatedDate],
	[CreatedBy],
	[GlobalIdActivityId],
	[VesselArrivalDate]
)
SELECT
	ins.Id,
	ins.ActivityCode,
	ins.[Location],
	ins.Remark,
	ins.ActivityDate,
	ins.CreatedDate,
	ins.CreatedBy,
	ins.GlobalIdActivityId,
	t1.ActivityDate as [VesselArrivalDate]
FROM @TInsertedActivity ins
INNER JOIN [dbo].[GlobalIdActivities] gba ON ins.GlobalIdActivityId = gba.Id
INNER JOIN [dbo].[GlobalIds] gid ON gba.GlobalId = gid.Id
CROSS APPLY (
	SELECT TOP 1 gba1.ActivityDate
	FROM
	(
		SELECT
			CONCAT('POF_', poff.id) as GlobalId
		FROM POFulfillments poff (NOLOCK)
		WHERE poff.[Status] = 10 --active
		AND EXISTS (
			SELECT 1
			FROM POFulfillmentOrders poffod (NOLOCK)
			WHERE poffod.POFulfillmentId = poff.Id AND poffod.PurchaseOrderId = gid.EntityId)
	) t INNER JOIN GlobalIdActivities gba1 ON gba1.GlobalId = t.GlobalId INNER JOIN Activities act1 ON act1.Id = gba1.ActivityId
	WHERE act1.ActivityCode = '1071' AND act1.[Location] = ins.[Location]
	ORDER BY gba1.CreatedDate DESC
) t1
WHERE ins.ActivityCode = '1010' AND CAST(t1.ActivityDate as DATE) <> CAST(ins.ActivityDate as DATE)

UPDATE Act
SET Act.ActivityDate = T.VesselArrivalDate
FROM [dbo].Activities Act INNER JOIN @TUpdatingActivity1010 T
ON Act.Id = T.Id

UPDATE Gba
SET Gba.ActivityDate = Act.VesselArrivalDate
FROM [dbo].[GlobalIdActivities] Gba INNER JOIN @TUpdatingActivity1010 Act
ON Gba.Id = Act.GlobalIdActivityId

select * from @TUpdatingActivity1010
--
-- Reset
--
UPDATE [dbo].Activities
SET Resolution = NULL
WHERE Id IN (SELECT Id FROM @TInsertedActivity)

COMMIT TRANSACTION