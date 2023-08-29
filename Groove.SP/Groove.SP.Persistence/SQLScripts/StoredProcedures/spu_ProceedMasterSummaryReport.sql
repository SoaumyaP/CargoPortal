SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_ProceedMasterSummaryReport', 'P') IS NOT NULL
DROP PROC dbo.spu_ProceedMasterSummaryReport
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 23 Feb 2021
-- Description:	Get data for Master summary report
-- =============================================
CREATE PROCEDURE [dbo].[spu_ProceedMasterSummaryReport]
	@JsonFilterSet NVARCHAR(MAX) = ''
AS
BEGIN

	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Please input searching criteria here
	-- Notes: SelectedCustomerId -> Principal organization id
	--SET @JsonFilterSet = N'{
	--	"SelectedCustomerId": 2,
	--	"ETDFrom" : "2021-01-01",
	--	"ETDTo": "2021-01-31",
	--	"ETAFrom" : null,
	--	"ETATo": null,
	--  "ATDFrom": null,
	--  "ATDTo": null,
	--	"BookingNo": null,
	--	"ShipmentNo": null,
	--	"ContainerNo": null,
	--	"HouseBLNo": null,
	--	"MasterBLNo": null,
	--	"CargoReadyDateFrom" : null,
	--	"CargoReadyDateTo": null,
	--	"ShipFromLocation": null,
	--	"ShipToLocation": null,
	--	"PONoFrom": null,
	--	"PONoTo": null,
	--	"POStage": null,
	--	"MovementType": null,
	--	"Supplier": null,
	--	"ReportType": "PO level",
	--	"PromotionCode": null
	--}';

	-- Declare variables
	DECLARE @ETDFrom DATETIME2 
	DECLARE @ETDTo DATETIME2
	DECLARE @ETAFrom DATETIME2 
	DECLARE @ETATo DATETIME2
	DECLARE @ATDFrom DATETIME2
	DECLARE @ATDTo DATETIME2
	DECLARE @CargoReadyDateFrom DATETIME2
	DECLARE @CargoReadyDateTo DATETIME2
	DECLARE @ShipFrom NVARCHAR(MAX)
	DECLARE @ShipTo NVARCHAR(MAX)
	DECLARE @PONoFrom NVARCHAR(MAX)
	DECLARE @PONoTo NVARCHAR(MAX)
	DECLARE @BookingNumber NVARCHAR(MAX)
	DECLARE @ShipmentNumber NVARCHAR(MAX)
	DECLARE @Supplier BIGINT
	DECLARE @ReportType NVARCHAR(20)
	DECLARE @PromotionCode NVARCHAR(MAX) -- Filter data against POLineItem.ProductRemark
	DECLARE @HouseBLNumber NVARCHAR(MAX) -- House bill of lading number (BillOfLadings table)
	DECLARE @MasterBLNumber NVARCHAR(MAX) -- Master bill of lading number (MasterBills table)
	DECLARE @POStage INT
	DECLARE @MovementType NVARCHAR(10)
	DECLARE @ContainerNumber NVARCHAR(MAX)
	DECLARE @SelectedCustomerId BIGINT
	DECLARE @EquipmentTypeTbl TABLE (Id INT NOT NULL, Code NVARCHAR(10), [Name] NVARCHAR(50) NOT NULL)
	DECLARE @FilteredPOLTbl TABLE (POId BIGINT, POLId BIGINT)
	DECLARE @CustomerRelationshipTbl TABLE (SupplierId BIGINT, CustomerRefId NVARCHAR(20))
	DECLARE @BookedPOFiltering BIT
	SET @BookedPOFiltering = 0


	-- Set values for variables
	INSERT INTO @EquipmentTypeTbl
	VALUES	(3, '20DG', '20'' Dangerous Container'), 
			(5, '20FR', '20'' Flat Rack'),
			(7, '20GH', '20'' GOH Container'),
			(10, '20GP', '20'' Container'),
			(11, '20HC', '20'' High Cube'),
			(12, '20HT', '20'' HT Container'),
			(13, '20HW', '20'' High Wide'),
			(14, '20NOR', '20'' Reefer Dry'),
			(15, '20OS', '20'' Both Full Side Door Opening Container'),
			(16, '20OT', '20'' Open Top Container'),
			(20, '40GP', '40'' Container'),
			(21, '40HC', '40'' High Cube'),
			(22, '40HG', '40'' HC GOH Container'),
			(23, '40HNOR', '40'' HC Reefer Dry Container'),
			(24, '40HO', '40'' HC Open Top Container'),
			(25, '40HQDG', '40'' HQ DG Container'),
			(26, '40HR', '40'' HC Reefer Container'),
			(27, '40HW', '40'' High Cube Pallet Wide'),
			(28, '40NOR', '40'' Reefer Dry'),
			(29, '40OT', '40'' Open Top Container'),
			(30, '20RF', '20'' Reefer'),
			(31, '20TK', '20'' Tank Container'),
			(32, '20VH', '20'' Ventilated Container'),
			(33, '40DG', '40'' Dangerous Container'),
			(34, '40FQ', '40'' High Cube Flat Rack'),
			(35, '40FR', '40'' Flat Rack'),
			(36, '40GH', '40'' GOH Container'),
			(37, '40PS', '40'' Plus'),
			(40, '40RF', '40'' Reefer'),
			(41, '40TK', '40'' Tank'),
			(51, '45GO', '45'' GOH'),
			(52, '45HC', '45'' High Cube'),
			(54, '45HG', '45'' HC GOH Container'),
			(55, '45HT', '45'' Hard Top Container'),
			(56, '45HW', '45'' HC Pallet Wide'),
			(57, '45RF', '45'' Reefer Container'),
			(58, '48HC', '48'' HC Container'),
			(50, 'Air', 'Air'),
			(60, 'LCL', 'LCL'),
			(70, 'Truck', 'Truck')

	-- Set filter criteria
	SELECT
		@SelectedCustomerId = SelectedCustomerId,		
		@ETDFrom = ETDFrom,
		@ETDTo = ETDTo,
		@ETAFrom = ETAFrom,
		@ETATo = ETATo,
		@ATDFrom = ATDFrom,
		@ATDTo = ATDTo,
		@BookingNumber = BookingNumber,
		@ShipmentNumber = ShipmentNumber,
		@ContainerNumber = ContainerNumber,
		@HouseBLNumber = HouseBLNumber,
		@MasterBLNumber = MasterBLNumber,

		@CargoReadyDateFrom = CargoReadyDateFrom,
		@CargoReadyDateTo = CargoReadyDateTo,
		@ShipFrom = ShipFrom,
		@ShipTo = ShipTo,
		@PONoFrom = PONoFrom,
		@PONoTo = PONoTo,
		@POStage = POStage,
		@MovementType = MovementType,
		@Supplier = Supplier,
		@ReportType = ReportType,
		@PromotionCode = PromotionCode	

	FROM OPENJSON(@JsonFilterSet)
	WITH (
		[SelectedCustomerId] BIGINT '$.SelectedCustomerId',
		[ETDFrom] DATETIME2(7) '$.ETDFrom',
		[ETDTo] DATETIME2(7) '$.ETDTo',
		[ETAFrom] DATETIME2(7) '$.ETAFrom',
		[ETATo] DATETIME2(7) '$.ETATo',
		[ATDFrom] DATETIME2(7) '$.ATDFrom',
		[ATDTo] DATETIME2(7) '$.ATDTo',
		[BookingNumber] NVARCHAR(MAX) '$.BookingNo',
		[ContainerNumber] NVARCHAR(MAX) '$.ContainerNo',
		[ShipmentNumber] NVARCHAR(MAX) '$.ShipmentNo',		
		[HouseBLNumber] NVARCHAR(MAX) '$.HouseBLNo',
		[MasterBLNumber] NVARCHAR(MAX) '$.MasterBLNo',		

		[CargoReadyDateFrom] DATETIME2(7) '$.CargoReadyDateFrom',
		[CargoReadyDateTo] DATETIME2(7) '$.CargoReadyDateTo',
		[ShipFrom] NVARCHAR(MAX) '$.ShipFromLocation',
		[ShipTo] NVARCHAR(MAX) '$.ShipToLocation',
		[PONoFrom] [VARCHAR] (512) '$.PONoFrom',
		[PONoTo] [VARCHAR] (512) '$.PONoTo',				
		[POStage] INT '$.POStage',
		[MovementType] NVARCHAR(10) '$.MovementType',
		[Supplier] BIGINT '$.Supplier',
		[ReportType] NVARCHAR(20) '$.ReportType',
		[PromotionCode] NVARCHAR(MAX) '$.PromotionCode'
	);

	IF(@ETAFrom IS NOT NULL OR @ETATo IS NOT NULL
		OR @ETDFrom IS NOT NULL OR @ETDTo IS NOT NULL
		OR @ATDFrom IS NOT NULL OR @ATDTo IS NOT NULL)
	BEGIN
		SET @BookedPOFiltering = 1

		INSERT INTO @FilteredPOLTbl
		SELECT CD.OrderId, CD.ItemId
		FROM Shipments SHI WITH (NOLOCK)
		INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
		OUTER APPLY (
			SELECT TOP 1 FS.ATDDate
			FROM ConsignmentItineraries CI (NOLOCK)
			INNER JOIN Itineraries IT (NOLOCK) ON CI.ItineraryId = IT.Id AND IT.ModeOfTransport IN ('Sea', 'Air')
			INNER JOIN FreightSchedulers FS (NOLOCK) ON FS.Id = IT.ScheduleId
			WHERE CI.ShipmentId = SHI.Id
			ORDER BY IT.[Sequence]
		) T

		WHERE
			 SHI.[Status] = 'active'
			-- filter on ETA/ETD
			AND (@ETAFrom IS NULL OR SHI.ShipToETADate >= @ETAFrom)	
			AND (@ETATo IS NULL OR SHI.ShipToETADate <= @ETATo)	
			AND (@ETDFrom IS NULL OR SHI.ShipFromETDDate >= @ETDFrom)	
			AND (@ETDTo IS NULL OR SHI.ShipFromETDDate <= @ETDTo)

			-- filter on shipment number
			AND (@ShipmentNumber IS NULL OR SHI.ShipmentNo = @ShipmentNumber)

			-- filter on ATD
			AND (@ATDFrom IS NULL OR (T.ATDDate IS NOT NULL AND T.ATDDate >= @ATDFrom))
			AND (@ATDTo IS NULL OR (T.ATDDate IS NOT NULL AND T.ATDDate <= @ATDTo))
	END

	-- Store into variables to improve performance
	INSERT INTO @CustomerRelationshipTbl
	SELECT CR.SupplierId, CR.CustomerRefId
	FROM CustomerRelationship CR
	WHERE CR.CustomerId = @SelectedCustomerId

	-- Filtering data section
	-- 1.Filter on Purchase order
	;
	WITH PurchaseOrderCTE AS
	(
		SELECT PO.*
			,POCC.OrganizationCode AS CustomerCode -- to link with Article Master (Column 24 + 25)
			,POCS.OrganizationId AS SupplierId -- to link with CustomerRelationship (Column 7)
			,POCS.CompanyName AS [Supplier Name] -- Column 8
		FROM PurchaseOrders PO WITH(NOLOCK)
		INNER JOIN PurchaseOrderContacts POCC WITH(NOLOCK) -- filter on Customer,  PO belongs to selected customer
				ON PO.Id = POCC.PurchaseOrderId 
				AND POCC.OrganizationRole = 'Principal' 
				AND POCC.OrganizationId = @SelectedCustomerId
			LEFT JOIN PurchaseOrderContacts POCS WITH(NOLOCK) -- Supplier
				ON PO.Id = POCS.PurchaseOrderId 
				AND POCS.OrganizationRole = 'Supplier'
		WHERE 
			-- PO is Active
			PO.[Status] = 1 
			
			AND (@BookedPOFiltering = 0 OR EXISTS (SELECT 1 FROM @FilteredPOLTbl TMP WHERE PO.Id = TMP.POId))			
			
			-- filter on Cargo ready date
			AND (@CargoReadyDateFrom IS NULL OR PO.CargoReadyDate >= @CargoReadyDateFrom)
			AND (@CargoReadyDateTo IS NULL OR PO.CargoReadyDate <= @CargoReadyDateTo)

			-- filter on Ship from / Ship to
			AND (@ShipFrom IS NULL OR PO.ShipFromId IN (SELECT [Value] FROM [dbo].[fn_SplitStringToTable](@ShipFrom, ',')))
			AND (@ShipTo IS NULL OR PO.ShipToId IN (SELECT [Value] FROM [dbo].[fn_SplitStringToTable](@ShipTo, ',')))

			-- filter on Purchase order number from - to
			AND (@PONoFrom IS NULL OR PO.PONumber >= @PONoFrom)
			AND (@PONoTo IS NULL OR PO.PONumber <= @PONoTo)
			
			-- filter on Purchase order container type (Movement type)
			/*
				If value=CFS, just select PO with LCL (code=60)
				If value=CY, select PO with all other container type (refer to EquipmentType)
			*/
			AND (
				(@MovementType IS NULL OR @MovementType = '' AND PO.ContainerType IN (10,11,14,30,20,21,28,40,52,60))
				OR (@MovementType = 'CFS' AND PO.ContainerType = 60) -- LCL)
				OR (@MovementType = 'CY' AND PO.ContainerType IN (10,11,14,30,20,21,28,40,52)) -- Others
			)

			-- filter on Purchase order stage
			AND (@POStage IS NULL OR PO.Stage = @POStage)

			-- filter on Supplier
			AND (
				@Supplier IS NULL OR @Supplier = ''
				OR POCS.OrganizationId = @Supplier
			)

	)

	--SELECT * FROM PurchaseOrderCTE

	-- 2. Filter on other tables (linking to Shipments...)
	,PurchaseOrder1CTE AS
	(
		SELECT			
			 PO.Id AS [POId] -- for further join, 
			,PO.PONumber AS [PO#] -- Column 3
			,PO.CustomerCode AS [CustomerCode] -- to link with Article Master (Column 24 + 25)
			,PO.SupplierId AS [SupplierId] -- to link with CustomerRelationship (Column 7)
			,PO.[Supplier Name] AS [Supplier Name] -- Column 8,
			,PO.ModeOfTransport AS [Ship Mode] -- Column 9
			,PO.Incoterm -- Column 10,
			,PO.ShipFrom AS [ShipFrom (POR)] -- Column 13
			,PO.ShipTo AS [ShipTo (PODel)] -- Column 14
			,T.POFFReceiptPort AS [ReceiptPort]
			,T.POFFDeliveryPort AS [DeliveryPort]
			,PO.ContainerType [POContainerType] -- to link with @EquipmentTypeTbl (Column 17)
			,PO.CargoReadyDate AS [Ex work Date (Latest)] -- Column 19
			,PO.ExpectedDeliveryDate AS [Exp Delivery Date] --Column 20
			,PO.Stage AS [POStage] -- to get data Column 67

			,POL.Id AS [POLineItemId] -- for further join
			,POL.ProductCode AS [Item#] --Column 4
			,POL.ProductRemark AS [Promo Code] --Column 5
			,POL.ReferenceNumber1 AS [Site Code] --Column 6
			,POL.CurrencyCode AS [Currency] --Column 11
			,POL.UnitPrice AS [Item Price] --Column 12
			,POL.DescriptionOfGoods AS [Item Description] --Column 15
			,POL.OrderedUnitQty AS [PO QTY] --Column 16

			-- Notice: PO Lineitem links to shipment by cargo details, poff must be null
			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFId
					 ELSE NULL 
				END) AS [POFFId] -- for further join
			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFNumber
					 ELSE NULL 
				END) AS [BookingRef#] -- Column 1
			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFExpectedShipDate
					 ELSE NULL 
				END) AS [Ex work Date (1st Archive)] -- Column 18
			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFCargoReadyDate
					 ELSE NULL 
				END) AS [Cargo Ready Date] -- Column 28
			,(
				CASE 
					WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) AND POFFMovementType = 1 THEN 'CY/CY'
					WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) AND POFFMovementType = 2 THEN 'CFS/CY'
					WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) AND POFFMovementType = 4 THEN 'CY/CFS'
					WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) AND POFFMovementType = 8 THEN 'CFS/CFS'
					ELSE NULL
				END
			) AS [Booked Load Type] -- Column 33
						
			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFOVolume
					 ELSE NULL 
				END) AS [CBM] -- Column 21
			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFOGrossWeight
					 ELSE NULL 
				END) AS [KGS]  -- Column 22
			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFOOrderedUnitQty
					 ELSE NULL 
				END) AS [Item Quantity]  -- Column 23

			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFOBookedPackage
					 ELSE NULL 
				END) AS [Booked QTY (Ctns)]  -- Column 29
			,(	CASE WHEN POFFId IS NOT NULL AND (SHI.Id IS NULL OR POFFId = SHI.POFulfillmentId) THEN POFFOFulfillmentUnitQty
					 ELSE NULL 
				END) AS [Booked QTY (Pieces)]  -- Column 30

			,SHI.Id AS [ShipmentId] -- for further join
			,SHI.CarrierContractNo AS [ShipmentCarrierContractNo] -- for futher join
			,SHI.ShipmentNo AS [SO#] -- Column 2
			,SHI.Movement AS [Actual Loading Type] -- Column 37
			,SHI.ModeOfTransport AS [Actual Ship mode] -- Column 38,
			,IIF(SHI.ShipToETADate IS NOT NULL, CONVERT(CHAR(10), SHI.ShipToETADate, 120), '') AS [ShipToETADate] -- Column 43
			,T.POFFBookingDate AS [Booking Date (Latest)] -- Column 27
			,CDSHI.ShipmentId AS [CDShipmentId] -- for further use, indicate if shipment has cargo details

		FROM PurchaseOrderCTE PO
		INNER JOIN POLineItems POL WITH(NOLOCK) ON PO.Id = POL.PurchaseOrderId  AND (@ReportType = 'PO Level' OR @PromotionCode IS NULL OR POL.ProductRemark = @PromotionCode) -- filter on promotion code
		-- to fix issue on show deactivated booking orders
		LEFT JOIN 
		(
			SELECT	POFF.Id AS [POFFId]
					,POFF.Number AS [POFFNumber]
					,POFF.ExpectedShipDate AS [POFFExpectedShipDate]
					,POFF.MovementType AS [POFFMovementType]
					,POFF.CargoReadyDate AS [POFFCargoReadyDate]
					,POFF.BookingDate AS [POFFBookingDate]
					,POFF.ReceiptPort AS [POFFReceiptPort]
					,POFF.DeliveryPort AS [POFFDeliveryPort]

					,POFFO.POLineItemId AS [POLineItemId]
					,POFFO.Volume AS [POFFOVolume]
					,POFFO.GrossWeight AS [POFFOGrossWeight]
					,POFFO.OrderedUnitQty AS [POFFOOrderedUnitQty]
					,POFFO.BookedPackage AS [POFFOBookedPackage]
					,POFFO.FulfillmentUnitQty AS [POFFOFulfillmentUnitQty]

			FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
			INNER JOIN POFulfillments POFF WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId AND POFF.[Status] = 10 AND POFF.Stage > 10
		) T ON POL.Id = T.POLineItemId -- POFF status be active and stage > draft
		LEFT JOIN POFulfillmentBookingRequests POFFBR WITH(NOLOCK) ON POFFBR.POFulfillmentId = POFFId AND POFFBR.[Status] = 10
		LEFT JOIN Shipments SHI WITH(NOLOCK) ON SHI.[Status] = 'active' 
												AND (
													SHI.POFulfillmentId = T.POFFId
													-- Link to shipment via booking POFF
													OR
													EXISTS (
															SELECT 1 
															FROM CargoDetails CD WITH(NOLOCK) 
															WHERE CD.ShipmentId = SHI.Id AND CD.OrderId = POL.PurchaseOrderId AND CD.ItemId = POL.Id
														) -- Link to shipment via cargo details 													
													)
		-- To indicate that shipment has cargo details
		OUTER APPLY
		(
			SELECT TOP(1) CD.ShipmentId
			FROM CargoDetails CD WITH(NOLOCK)
			WHERE CD.ShipmentId = SHI.Id AND CD.ItemId = POL.Id
		) CDSHI

		WHERE 
			
			(@BookedPOFiltering = 0 OR EXISTS (SELECT 1 FROM @FilteredPOLTbl TMP WHERE POL.PurchaseOrderId = TMP.POId AND POL.Id = TMP.POLId	))

			-- Booking number
			AND (@BookingNumber IS NULL OR [POFFNumber] = @BookingNumber)
			-- Shipment number
			AND (@ShipmentNumber IS NULL OR SHI.ShipmentNo = @ShipmentNumber)	
			
			-- filter on ETA/ETD
			AND (@ETAFrom IS NULL OR SHI.ShipToETADate >= @ETAFrom)	
			AND (@ETATo IS NULL OR SHI.ShipToETADate <= @ETATo)	
			AND (@ETDFrom IS NULL OR SHI.ShipFromETDDate >= @ETDFrom)	
			AND (@ETDTo IS NULL OR SHI.ShipFromETDDate <= @ETDTo)				
					
			-- filter on Container number
			AND 
				@ContainerNumber IS NULL 
				OR EXISTS (
					SELECT 1
					FROM Containers CON WITH(NOLOCK)
					INNER JOIN ShipmentLoadDetails SHLD WITH(NOLOCK) ON CON.Id = SHLD.ContainerId
					INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHLD.CargoDetailId = CD.Id 
					WHERE CD.ShipmentId = SHI.Id AND CD.OrderId = PO.Id AND CD.ItemId = POL.Id 
						AND (CON.ContainerNo = @ContainerNumber)
				)			
		
	)

	--SELECT * FROM PurchaseOrder1CTE
	
	
	-- 3.Filter more on the first and last Itinerary (leg) and other columns
	,PurchaseOrder2CTE AS (
		
		SELECT 
			CTE.* -- Main data columns
			,I1.* -- First leg
			,I2.* -- 2nd leg
			,I3.* -- 3rd leg
			,ILast.* -- Last leg
			,ShipmentAct2014.ActivityDate AS [Shipment2014ActDate] -- Column 61
			,COALESCE(COL26MINPOFFR.MINPOFFRequestBookedDate, COL26MINSHI.MINShipmentBookingDate) AS [Booking Date (1st Archive)] -- Column 26
			-- Master bill of lading
			,STUFF(
					(	
						SELECT ', ' + MB.MasterBillOfLadingNo
						FROM  MasterBills MB WITH(NOLOCK)
						WHERE	
								-- non-direct link MB -> Shipment via HBL
								EXISTS (
									SELECT 1
									FROM BillOfLadingShipmentLoads BLSL WITH(NOLOCK)
									INNER JOIN ShipmentLoads SHL WITH(NOLOCK) ON BLSL.ShipmentLoadId = SHL.Id
									WHERE SHL.ShipmentId = CTE.ShipmentId AND BLSL.MasterBillOfLadingId = MB.Id
									)
								-- direct link MB -> Shipment
								OR EXISTS (
									SELECT 1
									FROM Consignments CSM WITH(NOLOCK)
									WHERE CSM.ShipmentId = CTE.ShipmentId AND CSM.MasterBillId = MB.Id
									)
						FOR XMl PATH('') 
					), 1, 2, ''
			) AS MBL -- Column 39

			-- House bill of lading
			,STUFF(
					(	SELECT ', ' + BOL.BillOfLadingNo
						FROM BillOfLadings BOL WITH(NOLOCK)
						INNER JOIN ShipmentBillOfLadings SBL WITH(NOLOCK) ON SBL.BillOfLadingId = BOL.Id 
						WHERE SBL.ShipmentId = CTE.ShipmentId
						FOR XMl PATH('') 
					), 1, 2, ''
			) AS HBL -- Column 40

			-- Actual Shipped Container Number
			,STUFF(
					(	SELECT ', ' + ContainerNo
						FROM Containers CON WITH(NOLOCK)
						INNER JOIN ShipmentLoadDetails SHLD WITH(NOLOCK) ON CON.Id = SHLD.ContainerId
						INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHLD.CargoDetailId = CD.Id  
						WHERE CD.ShipmentId = CTE.ShipmentId AND CD.OrderId = CTE.POId AND CD.ItemId = CTE.POLineItemId
						FOR XMl PATH('') 
					), 1, 2, ''
				)  AS [Actual Shipped Container Number] -- Column 41

			-- Actual Shipped Container Size
			,STUFF(
						(	SELECT ', ' + ISNULL(CEQ.[Name], CON.ContainerType)
							FROM Containers CON WITH(NOLOCK)
								LEFT JOIN @EquipmentTypeTbl CEQ ON CON.ContainerType = CEQ.Code
								INNER JOIN ShipmentLoadDetails SHLD WITH(NOLOCK) ON CON.Id = SHLD.ContainerId
								INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHLD.CargoDetailId = CD.Id  
								WHERE CD.ShipmentId = CTE.ShipmentId AND CD.OrderId = CTE.POId AND CD.ItemId = CTE.POLineItemId
							FOR XMl PATH('') 
						), 1, 2, ''
				) AS [Actual Shipped Container Size] -- Column 42

			-- Supplier Code
			,CR.CustomerRefId AS [Supplier Code] -- Column 7	

			-- Purchase order dialogs
			,STUFF(
					(	SELECT ', ' + CONCAT(CONVERT(CHAR(10), N.CreatedDate, 120), ' - ', N.NoteText)
						FROM Notes N WITH(NOLOCK) WHERE N.GlobalObjectId = CONCAT('CPO_', CTE.POId)
						ORDER BY N.CreatedDate DESC
						FOR XMl PATH('') 
					), 1, 2, ''
			) AS [Dialog] -- Column 66

			,CM.ContractType AS [ShipmentContractType] -- Column 32
			,ShipmentAct1074.ActivityDate AS [SO Release Date] -- Column 60

			-- Gate-In Date
			,STUFF(
					(	SELECT ', ' + CONVERT(CHAR(10), GIA.ActivityDate, 120)
						FROM GlobalIdActivities GIA WITH(NOLOCK)
						INNER JOIN Activities ACT WITH(NOLOCK) ON GIA.ActivityId = ACT.Id
						WHERE ACT.ActivityCode = '3001' 
							AND GIA.GlobalId IN (
								SELECT CONCAT('CTN_', CON.Id)
								FROM Containers CON WITH(NOLOCK)
								INNER JOIN ShipmentLoadDetails SHLD WITH(NOLOCK) ON CON.Id = SHLD.ContainerId
								INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHLD.CargoDetailId = CD.Id  
								WHERE CD.ShipmentId = CTE.ShipmentId AND CD.OrderId = CTE.POId AND CD.ItemId = CTE.POLineItemId
							)
						FOR XMl PATH('') 
					), 1, 2, ''
				)  AS [Gate-In Date] -- Column 44

			-- CY/CFS Closing Date
			,STUFF(
					(	SELECT ', ' + CONVERT(CHAR(10), GIA.ActivityDate, 120)
						FROM GlobalIdActivities GIA WITH(NOLOCK)
						WHERE EXISTS (
								SELECT 1 
								FROM Containers CON WITH(NOLOCK)
								INNER JOIN ShipmentLoadDetails SHLD WITH(NOLOCK) ON CON.Id = SHLD.ContainerId
								INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHLD.CargoDetailId = CD.Id
								INNER JOIN Activities ACT WITH(NOLOCK) ON GIA.ActivityId = ACT.Id
								WHERE CD.ShipmentId = CTE.ShipmentId AND CD.OrderId = CTE.POId AND CD.ItemId = CTE.POLineItemId
									AND GIA.GlobalId = CONCAT('CTN_', CON.Id)
									AND (
										-- EventDate of event #3050 of CFS Actual Shipped Container 
										(CON.IsFCL = 0 AND ACT.ActivityCode = '3050')
										OR
										-- EventDate of event #3051 of CY Actual Shipped Container 
										(CON.IsFCL = 1 AND ACT.ActivityCode = '3051')
										)
							)
						FOR XMl PATH('') 
					), 1, 2, ''
				)  AS [CY/CFS Closing Date] -- Column 45
			

		FROM PurchaseOrder1CTE CTE
		LEFT JOIN @CustomerRelationshipTbl CR ON CR.SupplierId = CTE.SupplierId

		-- The first Itinerary
		OUTER APPLY (
				SELECT TOP(1) ITI.Id AS [FLegId], 
					ITI.ETDDate AS [FLegETD], 
					ITI.ETADate AS [FLegETA],
					-- ATD
					(
						SELECT TOP(1) ACT.ActivityDate
						FROM GlobalIdActivities GIA WITH(NOLOCK)
						INNER JOIN Activities ACT WITH(NOLOCK) ON GIA.ActivityId = ACT.Id
						WHERE GIA.GlobalId = CONCAT('FSC_', ITI.ScheduleId) AND ACT.ActivityCode IN ('7001', '7003') AND ACT.[Location] = ITI.LoadingPort AND ITI.ScheduleId IS NOT NULL
					) AS [FLegATD],
					ITI.VesselName AS [FLegVesselName], -- Further use if Shipment is Sea
					ITI.VesselFlight AS [FLegVesselFlight], -- Further use if Shipment is Air
					ITI.Voyage AS [FLegVoyage],
					ITI.LoadingPort AS [FLegLoadingPort],
					ITI.DischargePort AS [FLegDischargePort],
					ITI.CarrierName AS [FLegCarrier]

				FROM Itineraries ITI WITH(NOLOCK)
				INNER JOIN ConsignmentItineraries COI WITH(NOLOCK) ON COI.ItineraryId = ITI.Id
				WHERE COI.ShipmentId = CTE.ShipmentId
				ORDER BY ITI.[Sequence] ASC, ITI.[Id] ASC 
		) I1

		-- 2nd Itinerary
		OUTER APPLY (
				SELECT ITI.Id AS [2ndLegId], 
					ITI.ETDDate AS [2ndLegETD], 
					ITI.ETADate AS [2ndLegETA],
					-- ATD
					(
						SELECT TOP(1) ACT.ActivityDate
						FROM GlobalIdActivities GIA WITH(NOLOCK)
						INNER JOIN Activities ACT WITH(NOLOCK) ON GIA.ActivityId = ACT.Id
						WHERE GIA.GlobalId = CONCAT('FSC_', ITI.ScheduleId) AND ACT.ActivityCode IN ('7001', '7003') AND ACT.[Location] = ITI.LoadingPort AND ITI.ScheduleId IS NOT NULL
					) AS [2ndLegATD],
					ITI.VesselName AS [2ndLegVesselName], -- Further use if Shipment is Sea
					ITI.VesselFlight AS [2ndLegVesselFlight], -- Further use if Shipment is Air
					ITI.Voyage AS [2ndLegVoyage],
					ITI.LoadingPort AS [2ndLegLoadingPort],
					ITI.DischargePort AS [2ndLegDischargePort]

				FROM Itineraries ITI WITH(NOLOCK)
				INNER JOIN ConsignmentItineraries COI WITH(NOLOCK) ON COI.ItineraryId = ITI.Id
				WHERE COI.ShipmentId = CTE.ShipmentId
				ORDER BY ITI.[Sequence] ASC, ITI.[Id] ASC
				OFFSET 1 ROWS
				FETCH NEXT 1 ROWS ONLY

		) I2

		-- 3rd Itinerary
		OUTER APPLY (
				SELECT ITI.Id AS [3rdLegId], 
					ITI.ETDDate AS [3rdLegETD], 
					ITI.ETADate AS [3rdLegETA],
					-- ATD
					(
						SELECT TOP(1) ACT.ActivityDate
						FROM GlobalIdActivities GIA WITH(NOLOCK)
						INNER JOIN Activities ACT WITH(NOLOCK) ON GIA.ActivityId = ACT.Id
						WHERE GIA.GlobalId = CONCAT('FSC_', ITI.ScheduleId) AND ACT.ActivityCode IN ('7001', '7003') AND ACT.[Location] = ITI.LoadingPort AND ITI.ScheduleId IS NOT NULL
					) AS [3rdLegATD],
					ITI.VesselName AS [3rdLegVesselName], -- Further use if Shipment is Sea
					ITI.VesselFlight AS [3rdLegVesselFlight], -- Further use if Shipment is Air
					ITI.Voyage AS [3rdLegVoyage],
					ITI.LoadingPort AS [3rdLegLoadingPort],
					ITI.DischargePort AS [3rdLegDischargePort]

				FROM Itineraries ITI WITH(NOLOCK)
				INNER JOIN ConsignmentItineraries COI WITH(NOLOCK) ON COI.ItineraryId = ITI.Id
				WHERE COI.ShipmentId = CTE.ShipmentId
				ORDER BY ITI.[Sequence] ASC, ITI.[Id] ASC
				OFFSET 2 ROWS 
				FETCH NEXT 1 ROWS ONLY

		) I3

		-- The last Itinerary
		OUTER APPLY (
				SELECT TOP(1) ITI.Id AS [LLegId], ITI.ETDDate AS [LLegETD], ITI.ETADate AS [LLegETA],
					-- ATD
					(
						SELECT TOP(1) ACT.ActivityDate
						FROM GlobalIdActivities GIA WITH(NOLOCK)
						INNER JOIN Activities ACT WITH(NOLOCK) ON GIA.ActivityId = ACT.Id
						WHERE GIA.GlobalId = CONCAT('FSC_', ITI.ScheduleId) AND ACT.ActivityCode IN ('7001', '7003') AND ACT.[Location] = ITI.LoadingPort AND ITI.ScheduleId IS NOT NULL
					) AS [LLegATD],
				ITI.VesselName AS [LLegVesselName], -- Further use if Shipment is Sea
				ITI.VesselFlight AS [LLegVesselFlight], -- Further use if Shipment is Air
				ITI.Voyage AS [LLegVoyage],
				ITI.LoadingPort AS [LLegLoadingPort],
				ITI.DischargePort AS [LLegDischargePort]

				FROM Itineraries ITI WITH(NOLOCK)
				INNER JOIN ConsignmentItineraries COI WITH(NOLOCK) ON COI.ItineraryId = ITI.Id
				WHERE 
					ITI.Id != I1.FLegId -- Not the first leg in case there is only one leg
					AND COI.ShipmentId = CTE.ShipmentId
					AND ITI.Id != I2.[2ndLegId]
					AND ITI.Id != I3.[3rdLegId]
				ORDER BY ITI.[Sequence] DESC, ITI.[Id] DESC
		) ILast

		 -- Shipments EventDate of event #2014
		OUTER APPLY (
				SELECT TOP(1) ACT.ActivityDate
				FROM GlobalIdActivities GIA WITH(NOLOCK)
				INNER JOIN Activities ACT WITH(NOLOCK) ON GIA.ActivityId = ACT.Id
				WHERE GIA.GlobalId = CONCAT('SHI_', CTE.ShipmentId) AND ACT.ActivityCode = '2014'
				ORDER BY ACT.ActivityDate DESC
		) ShipmentAct2014

		-- Data for Column 26 Booking Date (1st Archive)
		OUTER APPLY (
			SELECT MIN(POFFR.BookedDate) AS [MINPOFFRequestBookedDate]
			FROM POFulfillmentBookingRequests POFFR WITH(NOLOCK)
			WHERE POFFR.POFulfillmentId = CTE.POFFId

		) COL26MINPOFFR

		-- Data for Column 26 Booking Date (1st Archive)
		OUTER APPLY (
			SELECT MIN(SHI.BookingDate) AS [MINShipmentBookingDate]
			FROM Shipments SHI WITH(NOLOCK) 
			WHERE	SHI.POFulfillmentId = CTE.POFFId -- Link to shipment via booking POFF
					OR
					EXISTS (
							SELECT 1 
							FROM CargoDetails CD WITH(NOLOCK) 
							WHERE CD.ShipmentId = SHI.Id AND CD.OrderId = CTE.POId AND CD.ItemId = CTE.POLineItemId
					) -- Link to shipment via cargo details 													
		) COL26MINSHI

		-- Data for Column 32 Contract Type
		OUTER APPLY (
			SELECT TOP(1) CM.ContractType
			FROM ContractMaster CM WITH(NOLOCK)
			WHERE CM.CarrierContractNo = CTE.ShipmentCarrierContractNo
		) CM

		-- Data for Column 60 SO Release Date
		OUTER APPLY (
			SELECT TOP(1) ACT.ActivityDate
				FROM GlobalIdActivities GIA WITH(NOLOCK)
				INNER JOIN Activities ACT WITH(NOLOCK) ON GIA.ActivityId = ACT.Id
				WHERE GIA.GlobalId = CONCAT('SHI_', CTE.ShipmentId) AND ACT.ActivityCode = '1074'
		) ShipmentAct1074


		WHERE
			-- filter House Bill of Lading				
			(@HouseBLNumber IS NULL 
				OR EXISTS (
					SELECT 1
					FROM ShipmentBillOfLadings SBL WITH(NOLOCK)
						LEFT JOIN BillOfLadings BOL WITH(NOLOCK) ON SBL.BillOfLadingId = BOL.Id
					WHERE SBL.ShipmentId = CTE.ShipmentId AND BOL.BillOfLadingNo = @HouseBLNumber
				))

			-- filter on Master Bill of Lading
			AND (
				@MasterBLNumber IS NULL				
				OR EXISTS (
						SELECT 1 
						FROM MasterBills MB WITH(NOLOCK)
						INNER JOIN BillOfLadingShipmentLoads BLSL WITH(NOLOCK) ON BLSL.MasterBillOfLadingId = MB.Id
						INNER JOIN ShipmentLoads SHL WITH(NOLOCK) ON BLSL.ShipmentLoadId = SHL.Id
						WHERE SHL.ShipmentId = CTE.ShipmentId AND MB.MasterBillOfLadingNo = @MasterBLNumber
						)
				)
	)

	--SELECT * FROM PurchaseOrder2CTE

	-- Data returned section
	-- Order of columns must be matched to report design
	, DataReturnCTE AS (
		SELECT
			PO.BookingRef# --Column 1
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.SO# END AS [SO#] --Column 2
			,PO.PO# --Column 3
			,PO.[Item#] --Column 4
			,PO.[Promo Code] --Column 5
			,PO.[Site Code] --Column 6
			,PO.[Supplier Code] --Column 7
			,PO.[Supplier Name] --Column 8
			,PO.[Ship Mode] --Column 9
			,PO.Incoterm --Column 10
			,[Currency] --Column 11
			,[Item Price] --Column 12
			,PO.[ShipFrom (POR)] --Column 13
			,PO.[ShipTo (PODel)] --Column 14
			,PO.[ReceiptPort]
			,PO.[DeliveryPort]
			,PO.[Item Description] --Column 15
			,PO.[PO QTY] --Column 16
			,POEQP.[Name] AS [PO Container] --Column 17
			,(CASE 
				WHEN PO.[Ex work Date (1st Archive)] IS NULL THEN NULL
				ELSE CONVERT(CHAR(10), PO.[Ex work Date (1st Archive)], 120)
			END ) AS [Ex work Date (1st Archive)] --Column 18
			,(CASE 
				WHEN PO.[Ex work Date (Latest)] IS NULL THEN NULL
				ELSE CONVERT(CHAR(10), PO.[Ex work Date (Latest)], 120)
			END ) AS [Ex work Date (Latest)] --Column 19
			,(CASE 
				WHEN PO.[Exp Delivery Date] IS NULL THEN NULL
				ELSE CONVERT(CHAR(10), PO.[Exp Delivery Date], 120)
			END ) AS [Exp Delivery Date]  --Column 20,
			,PO.CBM --Column 21
			,PO.KGS --Column 22
			,PO.[Item Quantity] --Column 23	
			,AM.InnerQuantity AS [Inner Quantity] --Column 24
			,AM.OuterQuantity AS [Outer Quantity] --Column 25
			,(CASE 
				WHEN PO.[Booking Date (1st Archive)] IS NULL THEN NULL
				ELSE CONVERT(CHAR(10), PO.[Booking Date (1st Archive)], 120)
			END ) AS [Booking Date (1st Archive)] --Column 26
			,(CASE 
				WHEN PO.[Booking Date (Latest)] IS NULL THEN NULL
				ELSE CONVERT(CHAR(10), PO.[Booking Date (Latest)], 120)
			END ) AS [Booking Date (Latest)] --Column 27			
			,(CASE 
				WHEN PO.[Cargo Ready Date] IS NULL THEN NULL
				ELSE CONVERT(CHAR(10), PO.[Cargo Ready Date], 120)
			END ) AS [Cargo Ready Date] --Column 28
			,PO.[Booked QTY (Ctns)] --Column 29
			,PO.[Booked QTY (Pieces)] --Column 30
			,FLegCarrier AS [Carrier] --Column 31
			,PO.ShipmentContractType AS [Contract Type] -- Column 32
			,PO.[Booked Load Type] --Column 33
			, STUFF (
				(	SELECT ', ' + POFFLEQP.[Name]
					FROM POFulfillmentLoads POFFL WITH(NOLOCK) 
					INNER JOIN @EquipmentTypeTbl POFFLEQP ON POFFL.POFulfillmentId = PO.POFFId AND POFFL.EquipmentType = POFFLEQP.Id
					FOR XMl PATH('') 
				), 1, 2, ''
			) AS [Booked Container Size] --Column 34
			,CASE	WHEN PO.CDShipmentId IS NULL THEN NULL
					WHEN PO.[Actual Ship mode] = 'Air' THEN PO.FLegVesselFlight
					ELSE FLegVesselName END AS [Booked Vessel] --Column 35
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.FLegVoyage END AS [Booked Voyage] --Column 36
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.[Actual Loading Type] END AS [Actual Loading Type] --Column 37
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.[Actual Ship mode] END AS [Actual Ship mode] --Column 38
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.MBL END AS MBL --Column 39
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.HBL END AS HBL --Column 40
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.[Actual Shipped Container Number] END AS [Actual Shipped Container Number] --Column 41
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.[Actual Shipped Container Size] END AS [Actual Shipped Container Size] --Column 42
			,PO.ShipToETADate AS [ShipToETADate] --Column 43
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.[Gate-In Date] END AS [Gate-In Date] -- Column 44
			,CASE WHEN PO.CDShipmentId IS NULL THEN NULL ELSE PO.[CY/CFS Closing Date] END AS [CY/CFS Closing Date] -- Column 45

			-- First leg
			,PO.FLegLoadingPort AS [1st Leg POL]  --Column 46
			,PO.FLegDischargePort AS [1st Leg POD]  --Column 47
			,CASE	WHEN PO.[Actual Ship mode] = 'Air' THEN PO.FLegVesselFlight
					ELSE PO.FLegVesselName END AS [1st Leg Vessel]  --Column 48
			,PO.FLegVoyage AS [1st Leg Voyage]  --Column 49
			,(CASE 
				WHEN PO.FLegETD IS NOT NULL THEN CONVERT(CHAR(10), PO.FLegETD, 120)
				ELSE NULL
			END ) AS [1st Leg ETD]  --Column 50
			,(CASE 
				WHEN PO.FLegATD IS NOT NULL THEN CONVERT(CHAR(10), PO.FLegATD, 120)
				ELSE NULL
			END ) AS [1st Leg ATD]  --Column 51
			,(CASE 
				WHEN PO.FLegETA IS NOT NULL THEN CONVERT(CHAR(10), PO.FLegETA, 120)
				ELSE NULL
			END ) AS [1st Vessel ETA]  --Column 52		

			-- 2nd leg
			,PO.[2ndLegLoadingPort] AS [2nd Leg POL]  --Column 53
			,PO.[2ndLegDischargePort] AS [2nd Leg POD]  --Column 54
			,CASE	WHEN PO.[Actual Ship mode] = 'Air' THEN PO.[2ndLegVesselFlight]
					ELSE PO.[2ndLegVesselName] END AS [2nd Leg Vessel]  --Column 55
			,PO.[2ndLegVoyage] AS [2nd Leg Voyage]  --Column 56
			,(CASE 
				WHEN PO.[2ndLegETD] IS NOT NULL THEN CONVERT(CHAR(10), PO.[2ndLegETD], 120)
				ELSE NULL
			END ) AS [2nd Leg ETD]  --Column 57
			,(CASE 
				WHEN PO.[2ndLegATD] IS NOT NULL THEN CONVERT(CHAR(10), PO.[2ndLegATD], 120)
				ELSE NULL
			END ) AS [2nd Leg ATD]  --Column 58
			,(CASE 
				WHEN PO.[2ndLegETA] IS NOT NULL THEN CONVERT(CHAR(10), PO.[2ndLegETA], 120)
				ELSE NULL
			END ) AS [2nd Vessel ETA]  --Column 59

				-- 3rd leg
			,PO.[3rdLegLoadingPort] AS [3rd Leg POL]  --Column 60
			,PO.[3rdLegDischargePort] AS [3rd Leg POD]  --Column 61
			,CASE	WHEN PO.[Actual Ship mode] = 'Air' THEN PO.[3rdLegVesselFlight]
					ELSE PO.[3rdLegVesselName] END AS [3rd Leg Vessel]  --Column 62
			,PO.[3rdLegVoyage] AS [3rd Leg Voyage]  --Column 63
			,(CASE 
				WHEN PO.[3rdLegETD] IS NOT NULL THEN CONVERT(CHAR(10), PO.[3rdLegETD], 120)
				ELSE NULL
			END ) AS [3rd Leg ETD]  --Column 64
			,(CASE 
				WHEN PO.[3rdLegATD] IS NOT NULL THEN CONVERT(CHAR(10), PO.[3rdLegATD], 120)
				ELSE NULL
			END ) AS [3rd Leg ATD]  --Column 65
			,(CASE 
				WHEN PO.[3rdLegETA] IS NOT NULL THEN CONVERT(CHAR(10), PO.[3rdLegETA], 120)
				ELSE NULL
			END ) AS [3rd Vessel ETA]  --Column 66

			-- Last leg
			,PO.[LLegLoadingPort] AS [Last Leg POL]  --Column 67
			,PO.[LLegDischargePort] AS [Last Leg POD]  --Column 68
			,CASE	WHEN PO.[Actual Ship mode] = 'Air' THEN PO.[LLegVesselFlight]
					ELSE PO.[LLegVesselName] END AS [Last Leg Vessel]  --Column 69
			,PO.[LLegVoyage] AS [Last Leg Voyage]  --Column 70
			,(CASE 
				WHEN PO.[LLegETD] IS NOT NULL THEN CONVERT(CHAR(10), PO.[LLegETD], 120)
				ELSE NULL
			END ) AS [Last Leg ETD]  --Column 71
			,(CASE 
				WHEN PO.[LLegATD] IS NOT NULL THEN CONVERT(CHAR(10), PO.[LLegATD], 120)
				ELSE NULL
			END ) AS [Last Leg ATD]  --Column 72
			,(CASE 
				WHEN PO.[LLegETA] IS NOT NULL THEN CONVERT(CHAR(10), PO.[LLegETA], 120)
				ELSE NULL
			END ) AS [Last Vessel ETA]  --Column 73

			,(CASE 
				WHEN PO.CDShipmentId IS NOT NULL AND PO.[SO Release Date] IS NOT NULL THEN CONVERT(CHAR(10), PO.[SO Release Date], 120)
				ELSE NULL
			END ) AS [SO Release Date] -- Column 74
			,(CASE 
				WHEN PO.CDShipmentId IS NOT NULL AND PO.[Shipment2014ActDate] IS NOT NULL THEN CONVERT(CHAR(10), PO.[Shipment2014ActDate], 120)
				ELSE NULL
			END ) AS [Handover at Origin (CargoReceive InDate)]  --Column 75
			,CASE	WHEN PO.CDShipmentId IS NULL THEN NULL
					WHEN PO.[Actual Ship mode] = 'Air' THEN CD.Unit
					ELSE SLD.Unit END AS [Shipped Qty]  --Column 76
			,CASE	WHEN PO.CDShipmentId IS NULL THEN NULL 
					WHEN PO.[Actual Ship mode] = 'Air' THEN CD.Package
					ELSE SLD.Package END AS [Shipped Carton]  --Column 77
			,CASE	WHEN PO.CDShipmentId IS NULL THEN NULL 
					WHEN PO.[Actual Ship mode] = 'Air' THEN CD.Volume
					ELSE SLD.Volume END AS [Shipped cbm]  --Column 78
			,CASE	WHEN PO.CDShipmentId IS NULL THEN NULL 
					WHEN PO.[Actual Ship mode] = 'Air' THEN CD.GrossWeight
					ELSE SLD.GrossWeight END AS [Shipped weight]  --Column 79
			,PO.Dialog --Column 80
			,(	CASE 
					WHEN PO.POStage = 10 THEN 'Draft'
					WHEN PO.POStage = 20 THEN 'Released'
					WHEN PO.POStage = 30 THEN 'Booked'
					WHEN PO.POStage = 40 THEN 'Booking Confirmed'
					WHEN PO.POStage = 45 THEN 'Cargo Received'
					WHEN PO.POStage = 50 THEN 'Shipment Dispatch'
					WHEN PO.POStage = 60 THEN 'Closed'
					WHEN PO.POStage = 70 THEN 'Completed'
					ELSE NULL
				END ) AS [PO Stage]  --Column 81
			   
		FROM PurchaseOrder2CTE PO
		LEFT JOIN ArticleMaster AM WITH(NOLOCK) ON PO.[Item#] = AM.ItemNo AND AM.CompanyCode = PO.CustomerCode
		LEFT JOIN @EquipmentTypeTbl POEQP ON PO.POContainerType = POEQP.Id
		LEFT JOIN CargoDetails CD WITH(NOLOCK) ON CD.ShipmentId = PO.ShipmentId AND CD.ItemId = PO.POLineItemId AND CD.OrderId = PO.POId
		LEFT JOIN ShipmentLoadDetails SLD WITH(NOLOCK) ON SLD.CargoDetailId = CD.Id
	) 

	-- To return data
	SELECT * 
	INTO #TmpMasterSummaryReportResult
	FROM DataReturnCTE

	IF(@ReportType = 'Item level')
	BEGIN

		SELECT * 
		FROM #TmpMasterSummaryReportResult
		ORDER BY PO# --Column 3
				,Item# --Column 4
		DROP TABLE #TmpMasterSummaryReportResult

	END
	ELSE 
	BEGIN

		   SELECT
		    NULL AS BookingRef# --Column 1
		    ,NULL AS SO# --Column 2
			,PO# --Column 3
			,NULL AS [Item#] --Column 4
			,NULL AS [Promo Code] --Column 5
			,NULL AS [Site Code] --Column 6
			,[Supplier Code] --Column 7
			,[Supplier Name] --Column 8
			,[Ship Mode] --Column 9
			,Incoterm --Column 10
			,NULL AS [Currency] --Column 11
			,NULL AS [Item Price] --Column 12
			,[ShipFrom (POR)] --Column 13
			,[ShipTo (PODel)] --Column 14
			,MAX([ReceiptPort]) AS [ReceiptPort]
			,MAX([DeliveryPort]) AS [DeliveryPort]
			,NULL AS [Item Description] --Column 15
			,SUM(ISNULL([PO QTY], 0)) AS [PO QTY] --Column 16
			,MAX([PO Container]) AS [PO Container] --Column 17
			,MAX([Ex work Date (1st Archive)]) AS [Ex work Date (1st Archive)] --Column 18
			,MAX([Ex work Date (Latest)]) AS [Ex work Date (Latest)] --Column 19
			,MAX([Exp Delivery Date]) AS [Exp Delivery Date] --Column 20,
			,SUM(ISNULL(CBM, 0)) AS [CBM] --Column 21
			,SUM(ISNULL(KGS, 0)) AS [KGS] --Column 22
			,SUM(ISNULL([Item Quantity], 0)) AS [Item Quantity] --Column 23	
			,NULL AS [Inner Quantity] --Column 24
			,NULL AS [Outer Quantity] --Column 25
			,MAX([Booking Date (1st Archive)]) AS [Booking Date (1st Archive)] --Column 26
			,MAX([Booking Date (Latest)]) AS [Booking Date (Latest)] --Column 27
			,MAX([Cargo Ready Date]) AS [Cargo Ready Date] --Column 28
			,SUM(ISNULL([Booked QTY (Ctns)], 0)) AS [Booked QTY (Ctns)] --Column 29
			,SUM(ISNULL([Booked QTY (Pieces)], 0)) AS [Booked QTY (Pieces)] --Column 30
			,MAX(Carrier) AS Carrier --Column 31
			,MAX([Contract Type]) AS [Contract Type] -- Column 32
			,MAX([Booked Load Type]) AS [Booked Load Type] --Column 33
			,MAX([Booked Container Size]) AS [Booked Container Size] --Column 34
			,MAX([Booked Vessel]) AS [Booked Vessel] --Column 35
			,MAX([Booked Voyage]) AS [Booked Voyage] --Column 36
			,MAX([Actual Loading Type]) AS [Actual Loading Type] --Column 37
			,MAX([Actual Ship mode]) AS [Actual Ship mode]  --Column 38
			,MAX(MBL) AS [MBL] --Column 39
			,HBL --Column 40
			,STUFF (
				(	SELECT DISTINCT ', ' + [Actual Shipped Container Number]
					FROM #TmpMasterSummaryReportResult TMP
					WHERE TMP.PO# = #TmpMasterSummaryReportResult.PO#
					FOR XMl PATH('') 
				), 1, 2, ''
			) AS [Actual Shipped Container Number] --Column 41
			,STUFF (
				(	SELECT DISTINCT ', ' + [Actual Shipped Container Size]
					FROM #TmpMasterSummaryReportResult TMP
					WHERE TMP.PO# = #TmpMasterSummaryReportResult.PO#
					FOR XMl PATH('') 
				), 1, 2, ''
			) AS [Actual Shipped Container Size] --Column 42
			,MAX(ShipToETADate) AS ShipToETADate --Column 43
			,MAX([Gate-In Date]) AS [Gate-In Date] -- Column 44
			,MAX([CY/CFS Closing Date]) AS [CY/CFS Closing Date] -- Column 45

			-- First leg
			,MAX([1st Leg POL]) AS [1st Leg POL] --Column 46
			,MAX([1st Leg POD]) AS [1st Leg POD]  --Column 47
			,MAX([1st Leg Vessel]) AS [1st Leg Vessel]  --Column 48
			,MAX([1st Leg Voyage]) AS [1st Leg Voyage]  --Column 49
			,MAX([1st Leg ETD]) AS [1st Leg ETD]  --Column 50
			,MAX([1st Leg ATD]) AS [1st Leg ATD]  --Column 51
			,MAX([1st Vessel ETA]) AS [1st Vessel ETA]  --Column 52

			-- 2nd leg
			,MAX([2nd Leg POL]) AS [2nd Leg POL] --Column 53
			,MAX([2nd Leg POD]) AS [2nd Leg POD]  --Column 54
			,MAX([2nd Leg Vessel]) AS [2nd Leg Vessel]  --Column 55
			,MAX([2nd Leg Voyage]) AS [2nd Leg Voyage]  --Column 56
			,MAX([2nd Leg ETD]) AS [2nd Leg ETD]  --Column 57
			,MAX([2nd Leg ATD]) AS [2nd Leg ATD]  --Column 58
			,MAX([2nd Vessel ETA]) AS [2nd Vessel ETA]  --Column 59
		
			-- 3rd leg
			,MAX([3rd Leg POL]) AS [3rd Leg POL] --Column 60
			,MAX([3rd Leg POD]) AS [3rd Leg POD]  --Column 61
			,MAX([3rd Leg Vessel]) AS [3rd Leg Vessel]  --Column 62
			,MAX([3rd Leg Voyage]) AS [3rd Leg Voyage]  --Column 63
			,MAX([3rd Leg ETD]) AS [3rd Leg ETD]  --Column 64
			,MAX([3rd Leg ATD]) AS [3rd Leg ATD]  --Column 65
			,MAX([3rd Vessel ETA]) AS [3rd Vessel ETA]  --Column 66

			-- Last leg
			,MAX([Last Leg POL]) AS [Last Leg POL] --Column 67
			,MAX([Last Leg POD]) AS [Last Leg POD] --Column 68
			,MAX([Last Leg Vessel]) AS [Last Leg Vessel] --Column 69
			,MAX([Last Leg Voyage]) AS [Last Leg Voyage] --Column 70
			,MAX([Last Leg ETD]) AS [Last Leg ETD] --Column 71
			,MAX([Last Leg ATD]) AS [Last Leg ATD] --Column 72
			,MAX([Last Vessel ETA]) AS [Last Vessel ETA] --Column 73

			,MAX([SO Release Date]) AS [SO Release Date] -- Column 74
			,MAX([Handover at Origin (CargoReceive InDate)]) AS [Handover at Origin (CargoReceive InDate)]  --Column 75
			,SUM(ISNULL([Shipped Qty], 0)) AS [Shipped Qty]  --Column 76
			,SUM(ISNULL([Shipped Carton], 0)) AS [Shipped Carton]  --Column 77
			,SUM(ISNULL([Shipped cbm], 0)) AS [Shipped cbm]  --Column 78
			,SUM(ISNULL([Shipped weight], 0)) AS [Shipped weight]  --Column 79
			,MAX(Dialog) AS Dialog --Column 80
			,MAX([PO Stage]) AS [PO Stage]  --Column 81		

		INTO #ReportResult
		FROM #TmpMasterSummaryReportResult
		GROUP BY 
			PO# --Column 3

			,[Supplier Code] --Column 7
			,[Supplier Name] --Column 8
			,[Ship Mode] --Column 9
			,Incoterm --Column 10
		
			,[ShipFrom (POR)] --Column 13
			,[ShipTo (PODel)] --Column 14
		
			
			,HBL --Column 40

		DELETE FROM #ReportResult WHERE [PO Stage] = 'Booking Confirmed' and [Shipped Qty] = 0 and rtrim([Actual Shipped Container number]) <> ''
		DELETE FROM #ReportResult WHERE [PO Stage] = 'Shipment Dispatch' and [Shipped Qty] = 0
		DELETE FROM #ReportResult WHERE [PO Stage] = 'Closed' and [Shipped Qty] = 0
		SELECT * FROM #ReportResult
		ORDER BY PO# --Column 3
		DROP TABLE #ReportResult
		DROP TABLE #TmpMasterSummaryReportResult
	END

END
GO

