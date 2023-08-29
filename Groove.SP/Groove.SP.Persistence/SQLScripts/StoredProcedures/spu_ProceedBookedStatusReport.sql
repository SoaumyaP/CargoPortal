SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_ProceedBookedStatusReport', 'P') IS NOT NULL
DROP PROC dbo.spu_ProceedBookedStatusReport
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 05 August 2020
-- Description:	Get data for Booked Satus Report
-- =============================================
CREATE PROCEDURE [dbo].[spu_ProceedBookedStatusReport]
	@JsonFilterSet NVARCHAR(MAX) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @ResultTable TABLE
	(
		PONumber VARCHAR(MAX),
		BlanketPO VARCHAR(MAX),
		PromoCode VARCHAR(MAX),
		POStage VARCHAR(MAX),
		[Ex-work Date] VARCHAR(MAX),
		PORequestDeliveryDate VARCHAR(MAX),
		MovementType VARCHAR(MAX),
		POQty VARCHAR(MAX),
		BookedQty VARCHAR(MAX),
		Supplier VARCHAR(MAX),
		ShipFrom VARCHAR(MAX),
		ShipTo VARCHAR(MAX),
		ExpectedShipDate VARCHAR(MAX),
		ExpectedDeliveryDate VARCHAR(MAX),
		BookingNumber VARCHAR(MAX),
		BookingStage VARCHAR(MAX),
		BookingDate VARCHAR(MAX),
		BookingItemWeight VARCHAR(MAX),
		BookingItemVolume VARCHAR(MAX),
		Incoterm VARCHAR(MAX),
		Carrier VARCHAR(MAX),
		ETDOrigin VARCHAR(MAX),
		FinalETADestination VARCHAR(MAX),
		Vessel VARCHAR(MAX),
		Voyage VARCHAR(MAX)
	)

 --   SET @JsonFilterSet = N'{
 --   "SelectedCustomerId": 461,
 --   "PONoFrom": null,
 --   "PONoTo": null,
 --   "ETDFrom" : null,
 --   "ETDTo": null,
 --   "POStage": "20",
 --   "BookingStage" : "20",
 --   "ShipFromPortIds":  "118,122,139,141,150,153,155,180",
 --   "IncludeDraftBooking": 1
 --}';

	DECLARE @SelectedCustomerId BIGINT
	DECLARE @PONoFrom VARCHAR (512) 
	DECLARE @PONoTo VARCHAR (512) 
	DECLARE @ETDFrom DATETIME2 
	DECLARE @ETDTo DATETIME2 
	DECLARE @ShipFromPortIds [VARCHAR] (MAX) 
	DECLARE @ShipFromPortIdTable TABLE (Id BIGINT NOT NULL)
	DECLARE @POStage BIGINT 
	DECLARE @BookingStage BIGINT
	DECLARE @Incoterm VARCHAR (3)
	DECLARE @IncludeDraftBooking BIT
	DECLARE @SupplierId BIGINT

	-- PO Type allowed in buyer compliance
	DECLARE @SelectedPO_POType INT

	SELECT	@SelectedCustomerId = SelectedCustomerId,
			@PONoFrom = PONoFrom,
			@PONoTo = PONoTo,
			@ETDFrom = ETDFrom,
			@ETDTo = ETDTo,
			@ShipFromPortIds = ShipFromPortIds,
			@POStage = POStage,
			@BookingStage = BookingStage,
			@Incoterm = Incoterm,
			@IncludeDraftBooking = IncludeDraftBooking,
			@SupplierId = SupplierId

	FROM OPENJSON(@JsonFilterSet)
	WITH (
		[SelectedCustomerId] BIGINT '$.SelectedCustomerId',
		[PONoFrom] [VARCHAR] (512) '$.PONoFrom',
		[PONoTo] [VARCHAR] (512) '$.PONoTo',
		[ETDFrom] [DATETIME2] '$.ETDFrom',
		[ETDTo] [DATETIME2] '$.ETDTo',
		[POStage] [BIGINT] '$.POStage',
		[BookingStage] [BIGINT] '$.BookingStage',
		[ShipFromPortIds] [VARCHAR] (MAX) '$.ShipFromPortIds',
		[Incoterm] [VARCHAR] (3) '$.Incoterm',
		[IncludeDraftBooking] [BIT] '$.IncludeDraftBooking',
		[SupplierId] BIGINT '$.SupplierId'
	);

	-- Get PO Type allowed in buyer compliance
	SELECT	@SelectedPO_POType = BC.AllowToBookIn
	FROM BuyerCompliances BC
	WHERE BC.[Status] = 1 AND BC.OrganizationId = @SelectedCustomerId

	-- In case Booking Stage = Draft but IncludeDraftBooking = False, no data return
	IF (@BookingStage = 10 AND @IncludeDraftBooking = 0)
	BEGIN
		SELECT * FROM @ResultTable
	END
	ELSE
	BEGIN
		IF (@ShipFromPortIds IS NOT NULL AND @ShipFromPortIds != '')
		BEGIN
			INSERT INTO @ShipFromPortIdTable (Id)
			SELECT CAST([Value] AS BIGINT) FROM dbo.fn_SplitStringToTable (@ShipFromPortIds, ',')
		END

		;WITH TmpCTE
		AS
		(
			SELECT	DISTINCT
					PO.Id AS [POId],
					POFF.Id AS [POFFId]

			FROM PurchaseOrders PO WITH(NOLOCK)
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFFO.PurchaseOrderId = PO.Id AND POFFO.[Status] = 1 AND PO.[Status] = 1
			INNER JOIN POFulfillments POFF WITH(NOLOCK) ON POFFO.POFulfillmentId = POFF.Id AND POFF.[Status] = 10
			WHERE 

			-- Purchase order status is active
			PO.[Status] = 1
			
			AND EXISTS (
				SELECT 1 FROM  PurchaseOrderContacts POC WITH(NOLOCK) WHERE PO.Id = POC.PurchaseOrderId AND POC.OrganizationRole = 'Principal' AND POC.OrganizationId = @SelectedCustomerId
			)

			-- Purchase order number
			AND (@PONoFrom IS NULL OR PO.PONumber >= @PONoFrom) AND (@PONoTo IS NULL OR PO.PONumber <= @PONoTo)

			-- ETD
			AND (@ETDFrom IS NULL OR PO.CargoReadyDate >= @ETDFrom) AND (@ETDTo IS NULL OR PO.CargoReadyDate <= @ETDTo)


			-- PO Stage
			AND (@POStage IS NULL OR Po.Stage = @POStage)

			-- Booking Stage
			AND (
				(@BookingStage IS NULL OR POFF.Stage = @BookingStage) OR

				(@IncludeDraftBooking = 1 AND POFF.Stage = 10)
			)

			-- Incoterm
			AND (@Incoterm IS NULL OR PO.Incoterm = @Incoterm)

			-- Include Draft Booking
			AND (
				(@IncludeDraftBooking = 0 AND POFF.Stage != 10) OR

				(@IncludeDraftBooking = 1 AND (POFF.Stage = 10 OR @BookingStage IS NULL OR POFF.Stage = @BookingStage))				
			)

			-- Ship From Port
			AND (
				@ShipFromPortIds IS NULL OR @ShipFromPortIds = '' 
				OR PO.ShipFromId IN (SELECT Id FROM @ShipFromPortIdTable)
			)
			
			-- PO Type
			AND ( 
				PO.POType = 10 OR PO.POType = @SelectedPO_POType
			)
		)

		INSERT INTO @ResultTable
		SELECT
			PO.PONumber AS [PONumber],
			T8.PONumber AS [BlanketPO],
			T7.PromoCode,
			(CASE 
				WHEN PO.Stage = 10 THEN 'Draft'
				WHEN PO.Stage = 20 THEN 'Released'
				WHEN PO.Stage = 30 THEN 'Booked'
				WHEN PO.Stage = 40 THEN 'Booking Confirmed'
				WHEN PO.Stage = 45 THEN 'Cargo Received'
				WHEN PO.Stage = 50 THEN 'Shipment Dispatch'
				WHEN PO.Stage = 60 THEN 'Closed'
				WHEN PO.Stage = 70 THEN 'Completed'
				ELSE ''
			END ) AS [POStage],

			(CASE 
				WHEN PO.CargoReadyDate IS NULL THEN ''
				ELSE FORMAT(PO.CargoReadyDate, 'yyyy-MM-dd')
			END ) AS [CargoReadyDate],

			(CASE 
				WHEN PO.ExpectedDeliveryDate IS NULL THEN ''
				ELSE FORMAT(PO.ExpectedDeliveryDate, 'yyyy-MM-dd')
			END ) AS [PORequestDeliveryDate],

			(CASE 
				WHEN POFF.MovementType = 1 THEN 'CY'
				WHEN POFF.MovementType = 2 THEN 'CFS'
				WHEN POFF.MovementType = 4 THEN 'CY'
				WHEN POFF.MovementType = 8 THEN 'CFS'
				ELSE ''
			END ) AS [MovementType],

			FORMAT(T1.POQty,'#,0') AS [POQty],
			FORMAT(T2.BookedQty,'#,0') AS [BookedQty],
			T3.Supplier,
			PO.ShipFrom AS [ShipFrom],
			PO.ShipTo AS [ShipTo],

			(CASE 
				WHEN POFF.ExpectedShipDate IS NULL THEN ''
				ELSE FORMAT(POFF.ExpectedShipDate, 'yyyy-MM-dd')
			END ) AS [ExpectedShipDate],

			(CASE 
				WHEN POFF.ExpectedDeliveryDate IS NULL THEN ''
				ELSE FORMAT(POFF.ExpectedDeliveryDate, 'yyyy-MM-dd')
			END ) AS [ExpectedDeliveryDate],

			POFF.Number AS [BookingNumber],

			(CASE 
				WHEN POFF.Stage = 10 THEN 'Draft'
				WHEN POFF.Stage = 20 THEN 'Booked'
				WHEN POFF.Stage = 30 THEN 'Booking Confirmed'
				WHEN POFF.Stage = 35 THEN 'Cargo Received'
				WHEN POFF.Stage = 40 THEN 'Shipment Dispatch'
				WHEN POFF.Stage = 50 THEN 'Closed'
				ELSE ''
			END ) AS [BookingStage],
				
			(CASE 
				WHEN POFF.BookingDate IS NULL THEN ''
				ELSE FORMAT(POFF.BookingDate, 'yyyy-MM-dd')
			END ) AS [BookingDate],

			FORMAT(T4.BookingItemWeight,'#,0.00') AS [BookingItemWeight],
			FORMAT(T5.BookingItemVolume,'#,0.000') AS [BookingItemVolume],
			PO.Incoterm AS [Incoterm],
			T6.CarrierName AS [Carrier],
			FORMAT(T6.ETDDate, 'yyyy-MM-dd') AS [ETDOrigin],
			FORMAT(T6.ETADate, 'yyyy-MM-dd') AS [FinalETADestination],
			T6.VesselName AS [Vessel],
			T6.Voyage AS [Voyage]

		FROM TmpCTE CTE
		INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CTE.POId = PO.Id
		INNER JOIN POFulfillments POFF WITH(NOLOCK) ON CTE.POFFId = POFF.Id

		OUTER APPLY
		(
			SELECT SUM(OrderedUnitQty) AS [POQty] FROM POLineItems POL WITH(NOLOCK) WHERE POL.PurchaseOrderId = CTE.POId
		) T1

		OUTER APPLY
		(
			SELECT SUM(FulfillmentUnitQty) AS [BookedQty] FROM POFulfillmentOrders POFFO WITH(NOLOCK) WHERE POFFO.POFulfillmentId = CTE.POFFId
		) T2

		CROSS APPLY
		(
			SELECT CompanyName AS [Supplier]
			FROM PurchaseOrderContacts POC WITH(NOLOCK) 
			WHERE POC.PurchaseOrderId = CTE.POId AND POC.OrganizationRole = 'Supplier' AND (@SupplierId IS NULL OR POC.OrganizationId = @SupplierId)
		) T3

		OUTER APPLY
		(
			SELECT SUM(COALESCE(GrossWeight,0)) AS [BookingItemWeight] FROM POFulfillmentOrders POFFO WITH(NOLOCK) WHERE POFFO.POFulfillmentId = CTE.POFFId
		) T4

		OUTER APPLY
		(
			SELECT SUM(COALESCE(Volume,0)) AS [BookingItemVolume] FROM POFulfillmentOrders POFFO WITH(NOLOCK) WHERE POFFO.POFulfillmentId = CTE.POFFId
		) T5

		OUTER APPLY
		(
			SELECT TOP 1 I.CarrierName, I.ETDDate, I.ETADate, I.VesselName, I.Voyage
			FROM Shipments SM WITH(NOLOCK) JOIN ConsignmentItineraries CI WITH(NOLOCK) ON CI.ShipmentId = SM.Id JOIN Itineraries I WITH(NOLOCK) ON I.Id = CI.ItineraryId
			WHERE SM.POFulfillmentId = POFF.Id AND SM.Status = 'Active'
			ORDER BY I.[Sequence] DESC
		) T6

		CROSS APPLY
		(
			SELECT 
				CASE WHEN EXISTS (SELECT Id FROM POLineItems POL WITH(NOLOCK) WHERE POL.PurchaseOrderId = CTE.POId AND POL.ProductRemark IS NOT NULL AND POL.ProductRemark != '')
					THEN 'Yes'
					ELSE 'No'
				END AS PromoCode
		) T7

		OUTER APPLY
		(
			SELECT BPO.PONumber
			FROM PurchaseOrders BPO WITH(NOLOCK)
			WHERE BPO.Id = PO.BlanketPOId
		) T8

		-- Check to show columns based on some business rules
		IF (@SelectedPO_POType != 30)
		BEGIN
			SELECT 
				PONumber,
				PromoCode,
				POStage,
				[Ex-work Date],
				PORequestDeliveryDate,
				MovementType,
				POQty,
				BookedQty,
				Supplier,
				ShipFrom,
				ShipTo,
				ExpectedShipDate,
				ExpectedDeliveryDate,
				BookingNumber,
				BookingStage,
				BookingDate,
				BookingItemWeight,
				BookingItemVolume,
				Incoterm,
				Carrier,
				ETDOrigin,
				FinalETADestination,
				Vessel,
				Voyage
			FROM @ResultTable
		END
		ELSE
		BEGIN
			SELECT * FROM @ResultTable
		END

	END

END
GO

