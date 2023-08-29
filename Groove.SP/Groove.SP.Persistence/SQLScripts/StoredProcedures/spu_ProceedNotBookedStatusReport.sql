SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_ProceedNotBookedStatusReport', 'P') IS NOT NULL
DROP PROC dbo.spu_ProceedNotBookedStatusReport
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 05 August 2020
-- Description:	Get data for Not Booked Satus Report
-- =============================================
CREATE PROCEDURE [dbo].[spu_ProceedNotBookedStatusReport]
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
		EquipmentType VARCHAR(MAX),
		[Ex-work Date] VARCHAR(MAX),
		PORequestDeliveryDate VARCHAR(MAX),
		POQty VARCHAR(MAX),
		Supplier VARCHAR(MAX),
		ShipFrom VARCHAR(MAX),
		ShipTo VARCHAR(MAX),
		Incoterm VARCHAR(MAX)
	)

 --   SET @JsonFilterSet =  N'{
 --   "SelectedCustomerId": 461,
 --   "PONoFrom": null,
 --   "PONoTo": null,
 --   "ShipFromPortIds":  "118,122,139,141,150,153,155,180",
 --   "IncludeDraftBooking": 1
 --   }'

	DECLARE @EquipmentTypeTable TABLE (Id INT NOT NULL, Code VARCHAR(10) NOT NULL)
	DECLARE @SelectedCustomerId BIGINT
	DECLARE @PONoFrom VARCHAR (512) 
	DECLARE @PONoTo VARCHAR (512) 
	DECLARE @ETDFrom DATETIME2 
	DECLARE @ETDTo DATETIME2 
	DECLARE @ShipFromPortIds [VARCHAR] (MAX) 
	DECLARE @ShipFromPortIdTable TABLE (Id BIGINT NOT NULL)
	DECLARE @Incoterm VARCHAR (3)
	DECLARE @IncludeDraftBooking BIT
	DECLARE @SupplierId BIGINT

	-- PO Type allowed in buyer compliance
	DECLARE @SelectedPO_POType INT

	INSERT INTO @EquipmentTypeTable
	VALUES	(3, '20DG'), 
			(5, '20FR'),
			(7, '20GH'),
			(10, '20GP'),
			(11, '20HC'),
			(12, '20HT'),
			(13, '20HW'),
			(14, '20NOR'),
			(15, '20OS'),
			(16, '20OT'),
			(20, '40GP'),
			(21, '40HC'),
			(22, '40HG'),
			(23, '40HNOR'),
			(24, '40HO'),
			(25, '40HQDG'),
			(26, '40HR'),
			(27, '40HW'),
			(28, '40NOR'),
			(29, '40OT'),
			(30, '20RF'),
			(31, '20TK'),
			(32, '20VH'),
			(33, '40DG'),
			(34, '40FQ'),
			(35, '40FR'),
			(36, '40GH'),
			(37, '40PS'),
			(40, '40RF'),
			(41, '40TK'),
			(51, '45GO'),
			(52, '45HC'),
			(54, '45HG'),
			(55, '45HT'),
			(56, '45HW'),
			(57, '45RF'),
			(58, '48HC'),
			(50, 'Air'),
			(60, 'LCL'),
			(70, 'Truck')


	SELECT	@SelectedCustomerId = SelectedCustomerId,
			@PONoFrom = PONoFrom,
			@PONoTo = PONoTo,
			@ETDFrom = ETDFrom,
			@ETDTo = ETDTo,
			@ShipFromPortIds = ShipFromPortIds,
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
		[ShipFromPortIds] [VARCHAR] (MAX) '$.ShipFromPortIds',
		[Incoterm] [VARCHAR] (3) '$.Incoterm',
		[IncludeDraftBooking] [BIT] '$.IncludeDraftBooking',
		[SupplierId] BIGINT '$.SupplierId'
	);

	-- Get PO Type allowed in buyer compliance
	SELECT	@SelectedPO_POType = BC.AllowToBookIn
	FROM BuyerCompliances BC
	WHERE BC.[Status] = 1 AND BC.OrganizationId = @SelectedCustomerId

	IF (@ShipFromPortIds IS NOT NULL  AND  @ShipFromPortIds != '')
	BEGIN
		INSERT INTO @ShipFromPortIdTable (Id)
		SELECT CAST([Value] AS BIGINT) FROM dbo.fn_SplitStringToTable (@ShipFromPortIds, ',')
	END

	;WITH TmpPOIdCTE
	AS
	(
		-- Not linking to any POFF
		SELECT PO.Id AS [POId]
		FROM PurchaseOrders PO WITH(NOLOCK)
			INNER JOIN PurchaseOrderContacts POC WITH(NOLOCK) ON PO.Id = POC.PurchaseOrderId AND POC.OrganizationRole = 'Principal' AND POC.OrganizationId = @SelectedCustomerId
		WHERE NOT EXISTS (SELECT PurchaseOrderId FROM POFulfillmentOrders WITH(NOLOCK) WHERE [Status] = 1 AND PurchaseOrderId = PO.Id )

		UNION ALL

		(
			-- Linked to Draft POFF
			SELECT PO.Id AS [POId]
			FROM PurchaseOrders PO WITH(NOLOCK)
				INNER JOIN PurchaseOrderContacts POC WITH(NOLOCK) ON @IncludeDraftBooking = 1 AND PO.Id = POC.PurchaseOrderId AND POC.OrganizationRole = 'Principal' AND POC.OrganizationId = @SelectedCustomerId AND PO.[Status] = 1
				INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON @IncludeDraftBooking = 1 AND POFFO.PurchaseOrderId = PO.Id AND POFFO.[Status] = 1 
				INNER JOIN POFulfillments POFF WITH(NOLOCK) ON @IncludeDraftBooking = 1 AND POFFO.POFulfillmentId = POFF.Id AND POFF.[Status] = 10 AND POFF.Stage = 10
			
			-- Except from linked to other POFF not Draft
			EXCEPT 
				SELECT POFFO.PurchaseOrderId
				FROM POFulfillmentOrders POFFO WITH(NOLOCK)
					INNER JOIN POFulfillments POFF WITH(NOLOCK) ON POFFO.POFulfillmentId = POFF.Id AND POFF.[Status] = 10 AND POFF.Stage != 10
				WHERE POFFO.[Status] = 1
		)

	)

	,TmpCTE
	AS
	(
		SELECT PO.Id AS [POId]

		FROM PurchaseOrders PO WITH(NOLOCK)
		INNER JOIN TmpPOIdCTE ON PO.Id = TmpPOIdCTE.POId
		WHERE 

		EXISTS (		
			SELECT 1 FROM PurchaseOrderContacts POC WITH(NOLOCK) WHERE PO.Id = POC.PurchaseOrderId AND POC.OrganizationRole = 'Principal' AND POC.OrganizationId = @SelectedCustomerId
		)

		-- Purchase order number
		AND (@PONoFrom IS NULL OR PO.PONumber >= @PONoFrom) AND (@PONoTo IS NULL OR PO.PONumber <= @PONoTo)

		-- ETD
		AND (@ETDFrom IS NULL OR PO.CargoReadyDate >= @ETDFrom) AND (@ETDTo IS NULL OR PO.CargoReadyDate <= @ETDTo)

		-- Ship From Port
		AND (
			@ShipFromPortIds IS NULL OR @ShipFromPortIds = '' 
			OR PO.ShipFromId IN (SELECT Id FROM @ShipFromPortIdTable)
		)

		--Incoterm
		AND (@Incoterm IS NULL OR PO.Incoterm = @Incoterm)

		-- Purchase order status is active
		AND PO.[Status] = 1		

		-- PO Type
		AND ( 
			PO.POType = 10 OR PO.POType = @SelectedPO_POType
		)
	)

	INSERT INTO @ResultTable
	SELECT 
		PO.PONumber,
		T4.PONumber AS [BlanketPO],
		T3.PromoCode,
		T.Code,
		(CASE 
			WHEN PO.CargoReadyDate IS NULL THEN ''
			ELSE FORMAT(PO.CargoReadyDate, 'yyyy-MM-dd')
		END ) AS [CargoReadyDate],

		(CASE 
			WHEN PO.ExpectedDeliveryDate IS NULL THEN ''
			ELSE FORMAT(PO.ExpectedDeliveryDate, 'yyyy-MM-dd')
		END ) AS [PORequestDeliveryDate],		
		FORMAT(T1.POQty,'#,0') AS [POQty],
		T2.Supplier,
		PO.ShipFrom,
		PO.ShipTo,
		PO.Incoterm

	FROM TmpCTE CTE
	INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CTE.POId = PO.Id
	LEFT JOIN @EquipmentTypeTable T ON T.Id = PO.ContainerType

	OUTER APPLY
	(
		SELECT SUM(OrderedUnitQty) AS [POQty] FROM POLineItems POL WITH(NOLOCK) WHERE POL.PurchaseOrderId = CTE.POId
	) T1		

	CROSS APPLY
	(
		SELECT CompanyName AS [Supplier] 
		FROM PurchaseOrderContacts POC WITH(NOLOCK) 
		WHERE POC.PurchaseOrderId = CTE.POId AND POC.OrganizationRole = 'Supplier' AND (@SupplierId IS NULL OR POC.OrganizationId = @SupplierId)
	) T2

	CROSS APPLY
	(
		SELECT 
			CASE WHEN EXISTS (SELECT Id FROM POLineItems POL WITH(NOLOCK) WHERE POL.PurchaseOrderId = CTE.POId AND POL.ProductRemark IS NOT NULL AND POL.ProductRemark != '')
				THEN 'Yes'
				ELSE 'No'
			END AS PromoCode
	) T3

	OUTER APPLY
	(
		SELECT BPO.PONumber
		FROM PurchaseOrders BPO WITH(NOLOCK)
		WHERE BPO.Id = PO.BlanketPOId
	) T4

	-- Check to show columns based on some business rules
	IF (@SelectedPO_POType != 30)
	BEGIN
		SELECT 
			PONumber,
			PromoCode,
			EquipmentType,
			[Ex-work Date],
			PORequestDeliveryDate,
			POQty,
			Supplier,
			ShipFrom,
			ShipTo,
			Incoterm
		FROM @ResultTable
	END
	ELSE
	BEGIN
		SELECT * FROM @ResultTable
	END

END
GO

