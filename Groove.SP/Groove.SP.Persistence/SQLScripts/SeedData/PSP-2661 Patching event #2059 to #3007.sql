-- =============================================
-- Author:			Hau Nguyen
-- Created date:	17 Sep 2021
-- Description:		[PSP-2661] - 
--		Retrieve data of Shipment.Event = #2059 AND Event.Remark = ContainerNumber
--		Update it to Container.Event = #3007 for the Container linked with the Shipment and Container# = Remark
-- =============================================
SET NOCOUNT ON;
	
BEGIN TRANSACTION

DECLARE @TInsertedActivity TABLE (
	Id BIGINT,
	[Location] NVARCHAR(MAX),
	[Remark] NVARCHAR(MAX),
	[ActivityDate] DATETIME2(7),
	[CreatedDate] DATETIME2(7),
	[CreatedBy] NVARCHAR(256),
	[UpdatedDate] DATETIME2(7),
	[UpdatedBy] NVARCHAR(256),
	[ContainerId] BIGINT
)
-- Variables
DECLARE @ActivityCode NVARCHAR(10) = '3007'
DECLARE @ActivityType NVARCHAR(10) = 'CA'
DECLARE @ActivityDescription NVARCHAR(512) = 'Container - Empty Return'
DECLARE @ContainerEntityType NVARCHAR(10) = 'CTN'

;with cte as (
	SELECT
		ROW_NUMBER() OVER(PARTITION BY ctn.Id ORDER BY a.Id ASC) AS row_number,
		a.Id as [ActivityId],
		a.[Location] as [Location],
		a.Remark as [Remark],
		a.ActivityDate as [ActivityDate],
		a.CreatedDate as [CreatedDate],
		a.CreatedBy as [CreatedBy],
		a.UpdatedDate as [UpdatedDate],
		a.UpdatedBy as [UpdatedBy],
		ctn.Id as [ContainerId]
	FROM Activities a
	INNER JOIN GlobalIdActivities gida ON a.Id = gida.ActivityId
	INNER JOIN GlobalIds gid ON gida.GlobalId = gid.Id
	INNER JOIN ShipmentLoads sml ON sml.ShipmentId = gid.EntityId
	INNER JOIN Containers ctn ON sml.ContainerId = ctn.Id AND ctn.ContainerNo = a.Remark
	INNER JOIN GlobalIds ctngid ON CONCAT(@ContainerEntityType, '_', ctn.Id) = ctngid.Id
	WHERE a.ActivityCode = '2059'
	AND gid.EntityType = 'Shipment'
)
--==== INSERT into dbo.[Activities] ====
INSERT INTO Activities
(
	[ActivityCode],
	[ActivityType],
	[ActivityDescription],
	[ActivityDate],
	[Location],
	[Remark],
	-- audit info
	[CreatedDate],
	[CreatedBy],
	[UpdatedDate],
	[UpdatedBy],
	[Resolution]
)
OUTPUT 
	inserted.Id,
	inserted.[Location],
	inserted.Remark,
	inserted.ActivityDate,
	inserted.CreatedDate,
	inserted.CreatedBy,
	inserted.UpdatedDate,
	inserted.UpdatedBy,
	CAST(inserted.Resolution as BIGINT)
INTO @TInsertedActivity
SELECT
	@ActivityCode,
	@ActivityType,
	@ActivityDescription,
	t.[ActivityDate],
	t.[Location],
	t.[Remark],
	-- audit info
	t.CreatedDate,
	t.CreatedBy,
	t.UpdatedDate,
	t.UpdatedBy,
	t.ContainerId
FROM cte t
WHERE row_number = 1

--==== INSERT into dbo.[GlobalIdActivities] ====
INSERT INTO GlobalIdActivities
(
	[GlobalId],
	[ActivityId],
	[Location],
	[Remark],
	[ActivityDate],
	[CreatedDate],
	[CreatedBy],
	[UpdatedDate],
	[UpdatedBy]
)
SELECT
	CONCAT(@ContainerEntityType, '_', ins.ContainerId) as [GlobalId],
	ins.Id as [ActivityId],
	ins.[Location],
	ins.[Remark],
	ins.[ActivityDate],
	ins.CreatedDate,
	ins.CreatedBy,
	ins.UpdatedDate,
	ins.UpdatedBy
FROM @TInsertedActivity ins

UPDATE Activities
SET Resolution = NULL
WHERE Id IN (
	SELECT Id FROM @TInsertedActivity
)

COMMIT TRANSACTION