
BEGIN TRAN


-- Variables

DECLARE @fromPOId BIGINT = 0
DECLARE @toPOId BIGINT = 0

DECLARE @ActivitiesTbl TABLE 
(
	Id BIGINT
)
DECLARE @LastestPOTbl TABLE 
(
	OrderId BIGINT,
	Stage INT
)
DECLARE @POToChangeStageTbl TABLE
(
	OrderId BIGINT,
	ScheduleId BIGINT,
	ATDDate Datetime2(7),
	ATADate Datetime2(7),
	LocationToName Nvarchar(512),
	ServiceType Nvarchar(512),
	ShipTo Nvarchar(512),
	Status Nvarchar(512),
	ActivityCode Nvarchar(30)
)
DECLARE @CalculatedPO TABLE
(
	OrderId BIGINT,
	TotalBalanceQty DECIMAL
)
DECLARE @POTable TABLE
(
	OrderId BIGINT,
	ShipmentId BIGINT
)


-- Get POs to patching
-- Applying data range by Purchase order id

INSERT INTO @POTable
SELECT c.OrderId, s.Id as ShipmentId
FROM Shipments s
INNER JOIN CargoDetails c ON s.Id = c.ShipmentId
INNER JOIN PurchaseOrders po ON po.Id = c.OrderId AND po.Id >= @fromPOId AND po.Id < @toPOId
WHERE s.POFulfillmentId IS NULL AND (s.ModeOfTransport = 'sea' OR s.ModeOfTransport = 'air')
AND s.OrderType = 1 AND c.OrderId NOT IN (SELECT PurchaseOrderId FROM POFulfillmentOrders)
GROUP BY c.OrderId, s.Id

;WITH PONeedToUpdate AS
(
	SELECT PO.OrderId , S.Id AS ShipmentId
	FROM Shipments S
	INNER JOIN @POTable PO ON PO.ShipmentId = S.Id
		AND 
		(
			EXISTS 
			(
				SELECT 1
				FROM  ConsignmentItineraries CI 
				INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
				INNER JOIN FreightSchedulers FS ON FS.Id = I.ScheduleId AND (ATDDate IS NOT NULL OR ATADate IS NOT NULL)
				WHERE S.Id = CI.ShipmentId
			)
			OR EXISTS
			(
				SELECT 1
				FROM GlobalIdActivities GA
				INNER JOIN Activities A ON GA.ActivityId = A.Id AND A.ActivityCode = '2054'
				WHERE GA.GlobalId = CONCAT('SHI_', S.Id) 
				AND S.ServiceType IS NOT NULL AND S.ServiceType LIKE('%to-Door%') AND S.Status = 'Active'
			)
		)
)
, CargoDetailsCTE AS
(
	SELECT ItemId, SUM(Unit) AS BookedQty
	FROM CargoDetails CD
	INNER JOIN PONeedToUpdate P ON P.ShipmentId = CD.ShipmentId
	GROUP BY ItemId
)

-- Stage 1: Update quantity from POLineItems
UPDATE POLineItems
SET BalanceUnitQty = BalanceUnitQty - CTE.BookedQty, 
    BookedUnitQty = BookedUnitQty + CTE.BookedQty
FROM POLineItems POL
INNER JOIN CargoDetailsCTE CTE ON POL.Id = CTE.ItemId


INSERT INTO @POToChangeStageTbl
SELECT PO.OrderId, e.ScheduleId, e.ATDDate, e.ATADate, e.LocationToName,e.ServiceType,e.ShipTo, s.Status, e.ActivityCode
FROM Shipments S
INNER JOIN @POTable PO ON PO.ShipmentId = S.Id
CROSS APPLY
(
	SELECT FS.Id AS ScheduleId, FS.ATDDate, FS.ATADate, FS.LocationToName, S.ServiceType, S.ShipTo, null as ActivityCode
	FROM ConsignmentItineraries CI
	INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
	INNER JOIN FreightSchedulers FS ON FS.Id = I.ScheduleId AND 
	(ATDDate IS NOT NULL OR ATADate IS NOT NULL )
	WHERE S.Id = CI.ShipmentId
	UNION ALL
	SELECT 0,NULL,A.ActivityDate,A.Location,NULL,NULL,ActivityCode
	FROM GlobalIdActivities GA
	INNER JOIN Activities A ON GA.ActivityId = A.Id AND A.ActivityCode = '2054'
	WHERE GA.GlobalId = CONCAT('SHI_', S.Id) 
	AND S.ServiceType IS NOT NULL AND S.ServiceType LIKE('%to-Door%') AND S.Status = 'Active'
)e

INSERT INTO @CalculatedPO
SELECT PurchaseOrderId, SUM(BalanceUnitQty) TotalBalanceQty
FROM POLineItems POI
WHERE EXISTS(
	SELECT 1
	FROM @POToChangeStageTbl PO
	WHERE PO.OrderId = POI.PurchaseOrderId
)
GROUP BY PurchaseOrderId

;with LastestPOTbl AS 
(
	SELECT PS.OrderId
		,CASE
			WHEN TotalBalanceQty <= 0 
			AND 
				(
					(
						ATADate IS NOT NULL 
						AND PS.ServiceType IS NOT NULL AND PS.Status = 'Active' 
						AND (PS.ServiceType LIKE ('%to-port%') OR PS.ServiceType LIKE ('%to-Airport%'))
						AND PS.LocationToName = PS.ShipTo
					)
					OR 
					(
						ActivityCode = '2054'
					)
				)
					
				THEN 60

			WHEN ATDDate IS NOT NULL THEN 50
		END Stage
	FROM @POToChangeStageTbl PS
	INNER JOIN @CalculatedPO CP ON PS.OrderId = CP.OrderId
)

INSERT INTO @LastestPOTbl
SELECT OrderId,MAX(Stage) AS Stage
FROM LastestPOTbl
GROUP BY OrderId

-- Stage 2: Update Stage from PurchaseOrders
UPDATE PurchaseOrders
SET Stage = LP.Stage
FROM PurchaseOrders PO 
INNER JOIN @LastestPOTbl LP ON PO.Id = LP.OrderId  AND LP.Stage IS NOT NULL

-- Stage 3: Insert Activity 1010 for closed POs
DECLARE @POId BIGINT
DECLARE @CreatedBy Nvarchar(100)
DECLARE @CreatedDate Datetime2(7)
DECLARE @ActivityCode Nvarchar(100)
DECLARE @ActivityId BIGINT
DECLARE @ActivityType Nvarchar(100)
DECLARE @ActivityDescription Nvarchar(100)
DECLARE @ActivityDate Datetime2(7)
DECLARE @LocationToName Nvarchar(512)

DECLARE PO_CURSOR CURSOR 
  LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR 
	-- Collect data
	SELECT 
		PO.OrderId,
		'System' AS CreatedBy,
		GETUTCDATE() AS CreatedDate,
		'1010' AS ActivityCode,
		'PM' AS ActivityType,
		'PO Closed' AS ActivityDescription,
		POToChangeStageTbl.ATADate AS ActivityDate,
		POToChangeStageTbl.LocationToName AS Location
	FROM @LastestPOTbl PO 
	CROSS APPLY 
	(
		select TOP 1 *
		from @POToChangeStageTbl PST
		WHERE PST.OrderId = PO.OrderId AND ATADate IS NOT NULL
		ORDER BY ATADate 	
	) POToChangeStageTbl
	WHERE Stage = 60 

OPEN PO_CURSOR
FETCH NEXT FROM PO_CURSOR
INTO @POId,@CreatedBy,@CreatedDate,@ActivityCode,@ActivityType,@ActivityDescription,@ActivityDate,@LocationToName
 
WHILE @@FETCH_STATUS = 0
BEGIN	
	-- Insert Activities
	INSERT INTO Activities (CreatedBy,CreatedDate,ActivityCode,ActivityType,ActivityDescription,ActivityDate,[Location])
	OUTPUT INSERTED.Id INTO @ActivitiesTbl(ID)
	SELECT 
	@CreatedBy,
	@CreatedDate,
	@ActivityCode,
	@ActivityType,
	@ActivityDescription,
	@ActivityDate,
	@LocationToName

	SELECT @ActivityId = MAX(Id) FROM @ActivitiesTbl

	-- Insert GlobalIdActivities
	INSERT INTO GlobalIdActivities(GlobalId,ActivityId,CreatedBy,CreatedDate,Location,ActivityDate)
	SELECT CONCAT('CPO_',@POId),@ActivityId, @CreatedBy, @CreatedDate,@LocationToName,@ActivityDate

   FETCH NEXT FROM PO_CURSOR 
   INTO @POId,@CreatedBy,@CreatedDate,@ActivityCode,@ActivityType,@ActivityDescription,@ActivityDate,@LocationToName
END
CLOSE PO_CURSOR
DEALLOCATE PO_CURSOR
GO
COMMIT TRAN
GO
