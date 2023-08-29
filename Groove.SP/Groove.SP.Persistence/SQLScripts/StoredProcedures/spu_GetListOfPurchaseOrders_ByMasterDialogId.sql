SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetListOfPurchaseOrders_ByMasterDialogId', 'P') IS NOT NULL
DROP PROC dbo.spu_GetListOfPurchaseOrders_ByMasterDialogId
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 21 Mar 2021
-- Description:	This method to get data for List of PO tree in read-only mode and selected items will be on the top
-- =============================================
CREATE PROCEDURE spu_GetListOfPurchaseOrders_ByMasterDialogId
	@MasterDialogId BIGINT,
	@SearchTerm NVARCHAR(255) = NULL,
	@Skip INT = 0,
	@Take INT = 20

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	-- 'Purchase Orders' , 'Bookings', 'Shipments'
	--SET @MessageShownOn = 'Purchase Orders'
	-- 'Master BL No.', 'House BL No.', 'Container No.', 'Purchase Order No.', 'Booking No.', 'Shipment No.'
	--SET @FilterCriteria = 'Purchase Order No.'
	--SET @FilterValues = '123,123,123'
	--SET @SearchTerm = NULL;
	--SET @Skip = 0;
	--SET @Take = 20;

	-- Variables
	DECLARE @MessageShownOn NVARCHAR(64)
	DECLARE @FilterCriteria NVARCHAR(64)
	DECLARE @FilterValues NVARCHAR(250)
	DECLARE @SelectedItems NVARCHAR(MAX)
	DECLARE @SelectedItemsTbl TABLE ([ParentId] NVARCHAR(20))
	DECLARE @FilterValueTbl TABLE ([Value] NVARCHAR(255))
	
	-- Set values for variables from current master dialog id
	SELECT	@MessageShownOn = DisplayOn
			,@FilterCriteria = FilterCriteria
			,@FilterValues = FilterValue
			,@SelectedItems = SelectedItems
	FROM MasterDialogs MD WITH(NOLOCK)
	WHERE Id = @MasterDialogId

	-- Convert selected items into table
	INSERT INTO @SelectedItemsTbl
	SELECT DISTINCT (IIF([ParentId] IS NULL OR [ParentId] = '', [Value], [ParentId]))
	FROM OPENJSON(@SelectedItems)
	WITH (
		[Value] NVARCHAR(20) '$.value',
		[ParentId] NVARCHAR(20) '$.parentId'
	);

	INSERT INTO @FilterValueTbl ([Value])
	SELECT [Value]
	FROM [dbo].[fn_SplitStringToTable] (@FilterValues, ',')

	/* Filtering logic */
	IF (@MessageShownOn = 'Purchase Orders' AND @FilterCriteria = 'Master BL No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 				
				CONCAT('CPO_', PO.Id) AS [Value]
				,PO.PONumber AS [Text]
				-- further processing
				,PO.Id AS [POId]
				,PO.PONumber
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM PurchaseOrders PO WITH(NOLOCK)
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('CPO_', PO.Id)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM MasterBills MB WITH(NOLOCK)
						INNER JOIN BillOfLadingShipmentLoads BLSL WITH(NOLOCK) ON BLSL.MasterBillOfLadingId = MB.Id
						INNER JOIN ShipmentLoads SHL WITH(NOLOCK) ON BLSL.ShipmentLoadId = SHL.Id
						INNER JOIN Shipments SHI WITH(NOLOCK) ON SHI.Id = SHL.ShipmentId
						INNER JOIN CargoDetails CD WITH(NOLOCK) ON CD.ShipmentId = SHI.Id
						WHERE CD.OrderId = PO.Id AND MB.MasterBillOfLadingNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [Text] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,NULL AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT('CPO_', CTE.POId, '&ItemId_', POL.Id) AS [Value]
			,POL.ProductCode AS [Text]
			,CONCAT('CPO_', CTE.POId) AS [ParentId] -- link to parent
			,POL.ProductCode AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POLineItems POL WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON CTE.POId = POL.PurchaseOrderId
		

	END

	ELSE IF (@MessageShownOn = 'Purchase Orders' AND @FilterCriteria = 'House BL No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 
				CONCAT('CPO_', PO.Id) AS [Value]
				,PO.PONumber AS [Text]
				-- further processing
				,PO.Id AS [POId] 
				,PO.PONumber
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM PurchaseOrders PO WITH(NOLOCK)			
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('CPO_', PO.Id)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM BillOfLadings BOL WITH(NOLOCK)
						INNER JOIN ShipmentBillOfLadings SBL WITH(NOLOCK) ON SBL.BillOfLadingId = BOL.Id 
						INNER JOIN Shipments SHI WITH(NOLOCK) ON SHI.Id = SBL.ShipmentId
						INNER JOIN CargoDetails CD WITH(NOLOCK) ON CD.ShipmentId = SHI.Id
						WHERE CD.OrderId = PO.Id AND BOL.BillOfLadingNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [Text] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,NULL AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT('CPO_', CTE.POId, '&ItemId_', POL.Id) AS [Value]
			,POL.ProductCode AS [Text]
			,CONCAT('CPO_', CTE.POId) AS [ParentId] -- link to parent
			,POL.ProductCode AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POLineItems POL WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON CTE.POId = POL.PurchaseOrderId
		

	END

	ELSE IF (@MessageShownOn = 'Purchase Orders' AND @FilterCriteria = 'Container No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 
				CONCAT('CPO_', PO.Id) AS [Value]
				,PO.PONumber AS [Text]
				-- further processing
				,PO.Id AS [POId]
				,PO.PONumber
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM PurchaseOrders PO WITH(NOLOCK)
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('CPO_', PO.Id)
			WHERE  
				( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM Containers CON WITH(NOLOCK)
						INNER JOIN ShipmentLoadDetails SHLD WITH(NOLOCK) ON CON.Id = SHLD.ContainerId
						INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHLD.CargoDetailId = CD.Id 
						WHERE CD.OrderId = PO.Id AND CON.ContainerNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [Text] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,NULL AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT('CPO_', CTE.POId, '&ItemId_', POL.Id) AS [Value]
			,POL.ProductCode AS [Text]
			,CONCAT('CPO_', CTE.POId) AS [ParentId] -- link to parent
			,POL.ProductCode AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POLineItems POL WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON CTE.POId = POL.PurchaseOrderId
		

	END

	ELSE IF (@MessageShownOn = 'Purchase Orders' AND @FilterCriteria = 'Purchase Order No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 
				CONCAT('CPO_', PO.Id) AS [Value]
				,PO.PONumber AS [Text]
				-- further processing
				,PO.Id AS [POId] 
				,PO.PONumber
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM PurchaseOrders PO WITH(NOLOCK)
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('CPO_', PO.Id)
			WHERE PO.PONumber IN (SELECT [VALUE] FROM @FilterValueTbl)
				AND ( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [Text] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,NULL AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT('CPO_', CTE.POId, '&ItemId_', POL.Id) AS [Value]
			,POL.ProductCode AS [Text]
			,CONCAT('CPO_', CTE.POId) AS [ParentId] -- link to parent
			,POL.ProductCode AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POLineItems POL WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON CTE.POId = POL.PurchaseOrderId
		

	END

	ELSE IF (@MessageShownOn = 'Bookings' AND @FilterCriteria = 'Master BL No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId) AS [Value]
				,CONCAT(POFF.Number, ' - ', POFFO.CustomerPONumber) AS [Text]
				,POFFO.CustomerPONumber AS [DialogItemNumber]
				-- for further processing
				,POFF.Id AS POFFId
				,POFFO.PurchaseOrderId AS POId
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM POFulfillments POFF WITH(NOLOCK)			
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR POFFO.CustomerPONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM MasterBills MB WITH(NOLOCK)
						INNER JOIN BillOfLadingShipmentLoads BLSL WITH(NOLOCK) ON BLSL.MasterBillOfLadingId = MB.Id
						INNER JOIN ShipmentLoads SHL WITH(NOLOCK) ON BLSL.ShipmentLoadId = SHL.Id
						INNER JOIN Shipments SHI WITH(NOLOCK) ON SHI.Id = SHL.ShipmentId
						WHERE SHI.POFulfillmentId = POFF.Id AND MB.MasterBillOfLadingNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [DialogItemNumber] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT(CTE.[Value], '&ItemId_', POFFO.POLineItemId) AS [Value]
			,POFFO.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			,CONCAT(POFFO.CustomerPONumber, ' - ', POFFO.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON POFFO.POFulfillmentId = CTE.POFFId AND POFFO.PurchaseOrderId = CTE.POId
		

	END

	ELSE IF (@MessageShownOn = 'Bookings' AND @FilterCriteria = 'House BL No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId) AS [Value]
				,CONCAT(POFF.Number, ' - ', POFFO.CustomerPONumber) AS [Text]
				,POFFO.CustomerPONumber AS [DialogItemNumber]
				-- for further processing
				,POFF.Id AS POFFId
				,POFFO.PurchaseOrderId AS POId
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM POFulfillments POFF WITH(NOLOCK)
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR POFFO.CustomerPONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM BillOfLadings BOL WITH(NOLOCK)
						INNER JOIN ShipmentBillOfLadings SBL WITH(NOLOCK) ON SBL.BillOfLadingId = BOL.Id 
						INNER JOIN Shipments SHI WITH(NOLOCK) ON SHI.Id = SBL.ShipmentId
						WHERE SHI.POFulfillmentId = POFF.Id AND BOL.BillOfLadingNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [DialogItemNumber] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT(CTE.[Value], '&ItemId_', POFFO.POLineItemId) AS [Value]
			,POFFO.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			,CONCAT(POFFO.CustomerPONumber, ' - ', POFFO.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON POFFO.POFulfillmentId = CTE.POFFId AND POFFO.PurchaseOrderId = CTE.POId
		

	END	
	
	ELSE IF (@MessageShownOn = 'Bookings' AND @FilterCriteria = 'Container No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId) AS [Value]
				,CONCAT(POFF.Number, ' - ', POFFO.CustomerPONumber) AS [Text]
				,POFFO.CustomerPONumber AS [DialogItemNumber]
				-- for further processing
				,POFF.Id AS POFFId
				,POFFO.PurchaseOrderId AS POId
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM POFulfillments POFF WITH(NOLOCK)
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR POFFO.CustomerPONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM Containers CON WITH(NOLOCK)
						INNER JOIN ShipmentLoadDetails SHLD WITH(NOLOCK) ON CON.Id = SHLD.ContainerId
						INNER JOIN Shipments SHI WITH(NOLOCK) ON SHI.Id = SHLD.ShipmentId
						WHERE SHI.POFulfillmentId = POFF.Id AND CON.ContainerNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [DialogItemNumber] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT(CTE.[Value], '&ItemId_', POFFO.POLineItemId) AS [Value]
			,POFFO.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			,CONCAT(POFFO.CustomerPONumber, ' - ', POFFO.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON POFFO.POFulfillmentId = CTE.POFFId AND POFFO.PurchaseOrderId = CTE.POId
		

	END

	ELSE IF (@MessageShownOn = 'Bookings' AND @FilterCriteria = 'Booking No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId) AS [Value]
				,CONCAT(POFF.Number, ' - ', POFFO.CustomerPONumber) AS [Text]
				,POFFO.CustomerPONumber AS [DialogItemNumber]
				-- for further processing
				,POFF.Id AS POFFId
				,POFFO.PurchaseOrderId AS POId
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM POFulfillments POFF WITH(NOLOCK)
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId)
			WHERE POFF.Number IN (SELECT [VALUE] FROM @FilterValueTbl)
				AND ( @SearchTerm IS NULL OR @SearchTerm = '' OR POFFO.CustomerPONumber LIKE '%' + @SearchTerm + '%' )
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [DialogItemNumber] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT(CTE.[Value], '&ItemId_', POFFO.POLineItemId) AS [Value]
			,POFFO.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			,CONCAT(POFFO.CustomerPONumber, ' - ', POFFO.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON POFFO.POFulfillmentId = CTE.POFFId AND POFFO.PurchaseOrderId = CTE.POId
		

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Master BL No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('SHI_', SHI.Id, '&CPO_', PO.Id) AS [Value]
				,CONCAT(SHI.ShipmentNo, ' - ', PO.PONumber) AS [Text]
				,PO.PONumber AS [DialogItemNumber]
				-- for further processing
				,SHI.Id AS ShipmentId
				,PO.Id AS POId
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM Shipments SHI WITH(NOLOCK)
			INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
			INNER JOIN POLineItems POL WITH(NOLOCK) ON CD.ItemId = POL.Id
			INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CD.OrderId = PO.Id
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('SHI_', SHI.Id, '&CPO_', PO.Id)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM MasterBills MB WITH(NOLOCK)
						INNER JOIN BillOfLadingShipmentLoads BLSL WITH(NOLOCK) ON BLSL.MasterBillOfLadingId = MB.Id
						INNER JOIN ShipmentLoads SHL WITH(NOLOCK) ON BLSL.ShipmentLoadId = SHL.Id
						WHERE SHL.ShipmentId = SHI.Id AND MB.MasterBillOfLadingNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [DialogItemNumber] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT(CTE.[Value], '&ItemId_', POL.Id) AS [Value]
			,POL.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			,CONCAT(PO.PONumber, ' - ', POL.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POLineItems POL WITH(NOLOCK) 
		INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON PO.Id = POL.PurchaseOrderId
		INNER JOIN CargoDetails CD WITH(NOLOCK) ON PO.Id = CD.OrderId AND POL.Id = CD.ItemId AND CD.OrderType = 1 -- Freight Order
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON PO.Id = CTE.POId AND CD.ShipmentId = CTE.ShipmentId		
		

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'House BL No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('SHI_', SHI.Id, '&CPO_', PO.Id) AS [Value]
				,CONCAT(SHI.ShipmentNo, ' - ', PO.PONumber) AS [Text]
				,PO.PONumber AS [DialogItemNumber]
				-- for further processing
				,SHI.Id AS ShipmentId
				,PO.Id AS POId
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM Shipments SHI WITH(NOLOCK)
			INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
			INNER JOIN POLineItems POL WITH(NOLOCK) ON CD.ItemId = POL.Id
			INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CD.OrderId = PO.Id
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('SHI_', SHI.Id, '&CPO_', PO.Id)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM BillOfLadings BOL WITH(NOLOCK)
						INNER JOIN ShipmentBillOfLadings SBL WITH(NOLOCK) ON SBL.BillOfLadingId = BOL.Id 
						WHERE SBL.ShipmentId = SHI.Id AND BOL.BillOfLadingNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [DialogItemNumber] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT(CTE.[Value], '&ItemId_', POL.Id) AS [Value]
			,POL.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			,CONCAT(PO.PONumber, ' - ', POL.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POLineItems POL WITH(NOLOCK) 
		INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON PO.Id = POL.PurchaseOrderId
		INNER JOIN CargoDetails CD WITH(NOLOCK) ON PO.Id = CD.OrderId AND POL.Id = CD.ItemId AND CD.OrderType = 1 -- Freight Order
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON PO.Id = CTE.POId AND CD.ShipmentId = CTE.ShipmentId	
		

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Container No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('SHI_', SHI.Id, '&CPO_', PO.Id) AS [Value]
				,CONCAT(SHI.ShipmentNo, ' - ', PO.PONumber) AS [Text]
				,PO.PONumber AS [DialogItemNumber]
				-- for further processing
				,SHI.Id AS ShipmentId
				,PO.Id AS POId
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM Shipments SHI WITH(NOLOCK)
			INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
			INNER JOIN POLineItems POL WITH(NOLOCK) ON CD.ItemId = POL.Id
			INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CD.OrderId = PO.Id
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('SHI_', SHI.Id, '&CPO_', PO.Id)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
				AND EXISTS (
						SELECT 1
						FROM Containers CON WITH(NOLOCK)
						INNER JOIN ShipmentLoads SL WITH(NOLOCK) ON CON.Id = SL.ContainerId
						WHERE SL.ShipmentId = SHI.Id AND CON.ContainerNo IN (SELECT [VALUE] FROM @FilterValueTbl)
						)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [DialogItemNumber] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT(CTE.[Value], '&ItemId_', POL.Id) AS [Value]
			,POL.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			,CONCAT(PO.PONumber, ' - ', POL.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POLineItems POL WITH(NOLOCK) 
		INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON PO.Id = POL.PurchaseOrderId
		INNER JOIN CargoDetails CD WITH(NOLOCK) ON PO.Id = CD.OrderId AND POL.Id = CD.ItemId AND CD.OrderType = 1 -- Freight Order
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON PO.Id = CTE.POId AND CD.ShipmentId = CTE.ShipmentId
		

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Shipment No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('SHI_', SHI.Id, '&CPO_', PO.Id) AS [Value]
				,CONCAT(SHI.ShipmentNo, ' - ', PO.PONumber) AS [Text]
				,PO.PONumber AS [DialogItemNumber]
				-- for further processing
				,SHI.Id AS ShipmentId
				,PO.Id AS POId
				,IIF(TBL.ParentId IS NOT NULL, 1, 0) AS [IsSelected]
			FROM Shipments SHI WITH(NOLOCK)
			INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
			INNER JOIN POLineItems POL WITH(NOLOCK) ON CD.ItemId = POL.Id
			INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CD.OrderId = PO.Id
			LEFT JOIN @SelectedItemsTbl TBL ON TBL.ParentId = CONCAT('SHI_', SHI.Id, '&CPO_', PO.Id)
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
				AND SHI.ShipmentNo IN (SELECT [VALUE] FROM @FilterValueTbl)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [IsSelected] DESC, [DialogItemNumber] ASC
			OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2
			
		UNION ALL

		SELECT 
			CONCAT(CTE.[Value], '&ItemId_', POL.Id) AS [Value]
			,POL.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			,CONCAT(PO.PONumber, ' - ', POL.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POLineItems POL WITH(NOLOCK) 
		INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON PO.Id = POL.PurchaseOrderId
		INNER JOIN CargoDetails CD WITH(NOLOCK) ON PO.Id = CD.OrderId AND POL.Id = CD.ItemId AND CD.OrderType = 1 -- Freight Order
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON PO.Id = CTE.POId AND CD.ShipmentId = CTE.ShipmentId	
		

	END

END
GO
