-- =============================================
-- Author:			Hau Nguyen
-- Created date:	16 Sep 2021
-- Description:		[PSP-2661] - Replace arrival activities (event #2039, 3004, 2030, 3008) by Vessel activity (event #7002)
-- =============================================
SET NOCOUNT ON;
	
BEGIN TRANSACTION

DECLARE @TActivity TABLE
(
	[Id] BIGINT,
	[EntityId] BIGINT,
	[EntityType] NVARCHAR(64),
	[ActivityCode] NVARCHAR(10),
	[Location] NVARCHAR(512),
	[Remark] NVARCHAR(MAX),
	[ActivityDate] DATETIME2(7),
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256)
)

DECLARE @TShipmentActivity TABLE
(
	[Id] BIGINT,
	[ActivityId] BIGINT,
	[Location] NVARCHAR(512),
	[Remark] NVARCHAR(MAX),
	[EventDate] DATETIME2(7),
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256)
)

DECLARE @TContainerActivity TABLE
(
	[Id] BIGINT,
	[ActivityId] BIGINT,
	[Location] NVARCHAR(512),
	[Remark] NVARCHAR(MAX),
	[EventDate] DATETIME2(7),
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256)
)

DECLARE @TMatchingScheduleActivity TABLE
(
	[ScheduleId] BIGINT,
	[ActivityId] BIGINT,
	[Location] NVARCHAR(512),
	[Remark] NVARCHAR(MAX),
	[EventDate] DATETIME2(7),
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256),
	[ShipmentId] BIGINT NULL,
	[ContainerId] BIGINT NULL
)

DECLARE @TInsertedActivity TABLE (
	Id BIGINT,
	[Location] NVARCHAR(MAX),
	[Remark] NVARCHAR(MAX),
	[ActivityDate] DATETIME2(7),
	[ScheduleId] BIGINT,
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256)
)

INSERT INTO @TActivity
SELECT
	a.Id,
	gid.EntityId,
	gid.EntityType,
	a.ActivityCode,
	a.[Location],
	a.Remark,
	CAST(a.ActivityDate AS DATE),
	a.CreatedDate,
	a.CreatedBy
FROM [dbo].[Activities] a
	INNER JOIN [dbo].[GlobalIdActivities] gida ON a.Id = gida.ActivityId
	INNER JOIN GlobalIds gid ON gida.GlobalId = gid.Id
WHERE ActivityCode IN (
	'2039', '3004', '2030', '3008'
)

INSERT INTO @TShipmentActivity
SELECT DISTINCT
	[EntityId] as [Id],
	[Id] as [ActivityId],
	[Location],
	[Remark],
	[ActivityDate],
	[CreatedDate],
	[CreatedBy]
FROM @TActivity
WHERE [EntityType] = 'Shipment'

INSERT INTO @TContainerActivity
SELECT DISTINCT
	[EntityId] as [Id],
	[Id] as [ActivityId],
	[Location],
	[Remark],
	[ActivityDate],
	[CreatedDate],
	[CreatedBy]
FROM @TActivity
WHERE [EntityType] = 'Container'

INSERT INTO @TMatchingScheduleActivity
(
	[ScheduleId],
	[ActivityId],
	[ShipmentId],
	[Location],
	[Remark],
	[EventDate],
	[CreatedDate],
	[CreatedBy]
)
SELECT
	i.ScheduleId,
	sa.ActivityId,
	sa.Id as ShipmentId,
	sa.[Location],
	sa.Remark,
	sa.EventDate,
	sa.CreatedDate,
	sa.CreatedBy
FROM ConsignmentItineraries csmi
	INNER JOIN Itineraries i ON csmi.ItineraryId = i.Id
	INNER JOIN FreightSchedulers fs ON fs.Id = i.ScheduleId
	INNER JOIN Shipments s ON csmi.ShipmentId = s.Id AND s.[Status] = 'Active' AND s.ModeOfTransport = 'Sea'
	INNER JOIN @TShipmentActivity sa ON csmi.ShipmentId = sa.Id AND sa.[Location] = fs.LocationToName

INSERT INTO @TMatchingScheduleActivity
(
	[ScheduleId],
	[ActivityId],
	[ContainerId],
	[Location],
	[Remark],
	[EventDate],
	[CreatedDate],
	[CreatedBy]
)
SELECT
	i.ScheduleId,
	ca.ActivityId,
	ca.Id as ContainerId,
	ca.[Location],
	ca.Remark,
	ca.EventDate,
	ca.CreatedDate,
	ca.CreatedBy
FROM ContainerItineraries ctni
	INNER JOIN Containers ctn ON ctn.Id = ctni.ContainerId AND ctn.ContainerNo != 'LCL'
	INNER JOIN Itineraries i ON ctni.ItineraryId = i.Id
	INNER JOIN FreightSchedulers fs ON fs.Id = i.ScheduleId
	INNER JOIN @TContainerActivity ca ON ctni.ContainerId = ca.Id AND ca.[Location] = fs.LocationToName

--==== INSERT into dbo.[Activities] ====
INSERT INTO Activities
(
	CreatedDate,
	CreatedBy,
	ActivityCode,
	ActivityType,
	ActivityDescription,
	ActivityDate,
	[Location],
	Remark,
	UpdatedBy
)
OUTPUT inserted.Id, inserted.[Location], inserted.Remark, inserted.ActivityDate, CAST(inserted.UpdatedBy as BIGINT), inserted.CreatedDate, inserted.CreatedBy
INTO @TInsertedActivity
SELECT
	t.CreatedDate,
	t.CreatedBy,
	'7002' as [ActivityCode],
	'VA' as [ActivityType],
	'Vessel Arrival' as [ActivityDescription],
	t.EventDate as [ActivityDate],
	t.[Location],
	t.Remark,
	fs.Id
FROM FreightSchedulers fs
CROSS APPLY
(
	SELECT TOP 1 *
	FROM @TMatchingScheduleActivity
	WHERE ScheduleId = fs.Id
	ORDER BY ActivityId DESC
) t

--==== INSERT into dbo.[GlobalIds] ====
INSERT INTO GlobalIds
(
	[Id],
	[EntityId],
	[EntityType],
	[CreatedDate]
)
SELECT
	CONCAT('FSC_', ScheduleId),
	ScheduleId,
	'FreightScheduler',
	GETDATE()
FROM @TInsertedActivity
WHERE NOT EXISTS (
	SELECT 1
	FROM GlobalIds
	WHERE Id = CONCAT('FSC_', ScheduleId)
)

--==== INSERT into dbo.[GlobalIdActivities] ====
INSERT INTO GlobalIdActivities
(
	[GlobalId],
	[ActivityId],
	[Location],
	[Remark],
	[ActivityDate],
	[CreatedDate],
	[CreatedBy]
)
SELECT
	CONCAT('FSC_', ScheduleId) as [GlobalId],
	Id as [ActivityId],
	[Location],
	[Remark],
	[ActivityDate],
	[CreatedDate],
	[CreatedBy]
FROM @TInsertedActivity

--==== UPDATE ATD = ActivityDate to dbo.[FreightSchedulers] ====
ALTER TABLE FreightSchedulers DISABLE TRIGGER trg_ModifyFreightSchedulers

UPDATE FreightSchedulers
SET ATADate = t.ActivityDate
FROM FreightSchedulers fs
INNER JOIN @TInsertedActivity t
ON fs.Id = t.ScheduleId

ALTER TABLE FreightSchedulers ENABLE TRIGGER trg_ModifyFreightSchedulers

UPDATE Activities
SET UpdatedBy = NULL
WHERE Id IN (SELECT Id FROM @TInsertedActivity)

--==== REMOVE corresponding #2029, 3003, 2031, 3011 after replaced by #7001 ====
DELETE Activities
WHERE Id IN (
	SELECT ActivityId FROM @TMatchingScheduleActivity
)

COMMIT TRANSACTION