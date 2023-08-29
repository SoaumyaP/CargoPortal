SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetPurchaseOrderList_MasterDialog', 'P') IS NOT NULL
DROP PROC dbo.spu_GetPurchaseOrderList_MasterDialog
GO

IF OBJECT_ID('spu_GetListOfPurchaseOrders_OnMasterDialog', 'P') IS NOT NULL
DROP PROC dbo.spu_GetListOfPurchaseOrders_OnMasterDialog
GO


-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 21 Mar 2021
-- Description:	This method to get data for List of PO tree
-- =============================================
CREATE PROCEDURE spu_GetListOfPurchaseOrders_OnMasterDialog
	@MessageShownOn NVARCHAR(255),
	@FilterCriteria NVARCHAR(255),
	@FilterValues NVARCHAR(MAX),
	@SearchTerm NVARCHAR(255) = NULL,
	@Skip INT = 0,
	@Take INT = 20,
	@ReturnType NVARCHAR(50) = 'Child level' 
	-- 'Parent level' for saving data
	-- 'Child level' to loading data for list of PO tree on UI

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
	DECLARE @FilterValueTbl TABLE ([Value] NVARCHAR(255))
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
			FROM PurchaseOrders PO WITH(NOLOCK)
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
			ORDER BY [Text] ASC
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
		WHERE @ReturnType = 'Child level'

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
			FROM PurchaseOrders PO WITH(NOLOCK)
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
			ORDER BY [Text] ASC
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
		WHERE @ReturnType = 'Child level'

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
			FROM PurchaseOrders PO WITH(NOLOCK)
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
			ORDER BY [Text] ASC
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
		WHERE @ReturnType = 'Child level'

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
			FROM PurchaseOrders PO WITH(NOLOCK)
			WHERE PO.PONumber IN (SELECT [VALUE] FROM @FilterValueTbl)
				AND ( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [Text] ASC
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
		WHERE @ReturnType = 'Child level'

	END

	ELSE IF (@MessageShownOn = 'Bookings' AND @FilterCriteria = 'Master BL No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId) AS [Value]
				-- If NVO booking, POFFO.CustomerPONumber = NULL
				,CONCAT(POFF.Number, (CASE WHEN POFFO.CustomerPONumber IS NULL OR POFFO.CustomerPONumber = '' THEN '' ELSE ' - ' + POFFO.CustomerPONumber END)) AS [Text]
				,POFFO.CustomerPONumber AS [DialogItemNumber]
				-- for further processing
				,POFF.Id AS POFFId
				,POFFO.PurchaseOrderId AS POId
			FROM POFulfillments POFF WITH(NOLOCK)
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId
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
			ORDER BY [DialogItemNumber] ASC
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
			-- If NVO booking, POFFO.POLineItemId = 0 -> must use POFFO.Id to make unique value
			CONCAT(CTE.[Value], '&ItemId_', CASE WHEN POFFO.POLineItemId = 0 THEN POFFO.Id ELSE POFFO.POLineItemId END) AS [Value]
			,POFFO.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			-- If NVO booking, POFFO.CustomerPONumber = NULL
			,CONCAT((CASE WHEN POFFO.CustomerPONumber IS NULL OR POFFO.CustomerPONumber = '' THEN '' ELSE POFFO.CustomerPONumber + ' - ' END) , POFFO.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON POFFO.POFulfillmentId = CTE.POFFId AND POFFO.PurchaseOrderId = CTE.POId
		WHERE @ReturnType = 'Child level'

	END

	ELSE IF (@MessageShownOn = 'Bookings' AND @FilterCriteria = 'House BL No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId) AS [Value]
				-- If NVO booking, POFFO.CustomerPONumber = NULL
				,CONCAT(POFF.Number, (CASE WHEN POFFO.CustomerPONumber IS NULL OR POFFO.CustomerPONumber = '' THEN '' ELSE ' - ' + POFFO.CustomerPONumber END)) AS [Text]
				,POFFO.CustomerPONumber AS [DialogItemNumber]
				-- for further processing
				,POFF.Id AS POFFId
				,POFFO.PurchaseOrderId AS POId
			FROM POFulfillments POFF WITH(NOLOCK)
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId
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
			ORDER BY [DialogItemNumber] ASC
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
			-- If NVO booking, POFFO.POLineItemId = 0 -> must use POFFO.Id to make unique value
			CONCAT(CTE.[Value], '&ItemId_', CASE WHEN POFFO.POLineItemId = 0 THEN POFFO.Id ELSE POFFO.POLineItemId END) AS [Value]
			,POFFO.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			-- If NVO booking, POFFO.CustomerPONumber = NULL
			,CONCAT((CASE WHEN POFFO.CustomerPONumber IS NULL OR POFFO.CustomerPONumber = '' THEN '' ELSE POFFO.CustomerPONumber + ' - ' END) , POFFO.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON POFFO.POFulfillmentId = CTE.POFFId AND POFFO.PurchaseOrderId = CTE.POId
		WHERE @ReturnType = 'Child level'

	END	
	
	ELSE IF (@MessageShownOn = 'Bookings' AND @FilterCriteria = 'Container No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId) AS [Value]
				-- If NVO booking, POFFO.CustomerPONumber = NULL
				,CONCAT(POFF.Number, (CASE WHEN POFFO.CustomerPONumber IS NULL OR POFFO.CustomerPONumber = '' THEN '' ELSE ' - ' + POFFO.CustomerPONumber END)) AS [Text]
				,POFFO.CustomerPONumber AS [DialogItemNumber]
				-- for further processing
				,POFF.Id AS POFFId
				,POFFO.PurchaseOrderId AS POId
			FROM POFulfillments POFF WITH(NOLOCK)
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId
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
			ORDER BY [DialogItemNumber] ASC
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
			-- If NVO booking, POFFO.POLineItemId = 0 -> must use POFFO.Id to make unique value
			CONCAT(CTE.[Value], '&ItemId_', CASE WHEN POFFO.POLineItemId = 0 THEN POFFO.Id ELSE POFFO.POLineItemId END) AS [Value]
			,POFFO.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			-- If NVO booking, POFFO.CustomerPONumber = NULL
			,CONCAT((CASE WHEN POFFO.CustomerPONumber IS NULL OR POFFO.CustomerPONumber = '' THEN '' ELSE POFFO.CustomerPONumber + ' - ' END) , POFFO.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON POFFO.POFulfillmentId = CTE.POFFId AND POFFO.PurchaseOrderId = CTE.POId
		WHERE @ReturnType = 'Child level'

	END

	ELSE IF (@MessageShownOn = 'Bookings' AND @FilterCriteria = 'Booking No.')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('POF_', POFF.Id, '&CPO_', POFFO.PurchaseOrderId) AS [Value]
				-- If NVO booking, POFFO.CustomerPONumber = NULL
				,CONCAT(POFF.Number, (CASE WHEN POFFO.CustomerPONumber IS NULL OR POFFO.CustomerPONumber = '' THEN '' ELSE ' - ' + POFFO.CustomerPONumber END)) AS [Text]
				,POFFO.CustomerPONumber AS [DialogItemNumber]
				-- for further processing
				,POFF.Id AS POFFId
				,POFFO.PurchaseOrderId AS POId
			FROM POFulfillments POFF WITH(NOLOCK)
			INNER JOIN POFulfillmentOrders POFFO WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId
			WHERE POFF.Number IN (SELECT [VALUE] FROM @FilterValueTbl)
				AND ( @SearchTerm IS NULL OR @SearchTerm = '' OR POFFO.CustomerPONumber LIKE '%' + @SearchTerm + '%' )
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [DialogItemNumber] ASC
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
			-- If NVO booking, POFFO.POLineItemId = 0 -> must use POFFO.Id to make unique value
			CONCAT(CTE.[Value], '&ItemId_', CASE WHEN POFFO.POLineItemId = 0 THEN POFFO.Id ELSE POFFO.POLineItemId END) AS [Value]
			,POFFO.ProductCode AS [Text]
			,CTE.[Value] AS [ParentId] -- link to parent
			-- If NVO booking, POFFO.CustomerPONumber = NULL
			,CONCAT((CASE WHEN POFFO.CustomerPONumber IS NULL OR POFFO.CustomerPONumber = '' THEN '' ELSE POFFO.CustomerPONumber + ' - ' END) , POFFO.ProductCode) AS [DialogItemNumber] -- value to display on Dialog grid
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM POFulfillmentOrders POFFO WITH(NOLOCK) 
		INNER JOIN CTE2 CTE WITH(NOLOCK) ON POFFO.POFulfillmentId = CTE.POFFId AND POFFO.PurchaseOrderId = CTE.POId
		WHERE @ReturnType = 'Child level'

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Master BL No.' AND @ReturnType = 'Parent level')
	BEGIN
		
		-- Parent level called as saving data for master dialog
		-- Also get data for shipment, not care about purchase orders available

		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('SHI_', SHI.Id) AS [Value]
				,SHI.ShipmentNo AS [Text]
				,NULL AS [DialogItemNumber]
			FROM Shipments SHI WITH(NOLOCK)
			WHERE EXISTS (
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
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2	

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Master BL No.' AND @ReturnType = 'Child level')
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
			FROM Shipments SHI WITH(NOLOCK)
			INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
			INNER JOIN POLineItems POL WITH(NOLOCK) ON CD.ItemId = POL.Id
			INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CD.OrderId = PO.Id
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
			ORDER BY [DialogItemNumber] ASC
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

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'House BL No.' AND @ReturnType = 'Parent level')
	BEGIN
		
		-- Parent level called as saving data for master dialog
		-- Also get data for shipment, not care about purchase orders available

		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('SHI_', SHI.Id) AS [Value]
				,SHI.ShipmentNo AS [Text]
				,NULL AS [DialogItemNumber]
			FROM Shipments SHI WITH(NOLOCK)
			WHERE EXISTS (
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
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'House BL No.' AND @ReturnType = 'Child level')
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
			FROM Shipments SHI WITH(NOLOCK)
			INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
			INNER JOIN POLineItems POL WITH(NOLOCK) ON CD.ItemId = POL.Id
			INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CD.OrderId = PO.Id
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
			ORDER BY [DialogItemNumber] ASC
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

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Container No.' AND @ReturnType = 'Parent level')
	BEGIN
		
		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('SHI_', SHI.Id) AS [Value]
				,SHI.ShipmentNo AS [Text]
				,NULL AS [DialogItemNumber]
			FROM Shipments SHI WITH(NOLOCK)
			WHERE EXISTS (
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
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Container No.' AND @ReturnType = 'Child level')
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
			FROM Shipments SHI WITH(NOLOCK)
			INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
			INNER JOIN POLineItems POL WITH(NOLOCK) ON CD.ItemId = POL.Id
			INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CD.OrderId = PO.Id
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
			ORDER BY [DialogItemNumber] ASC
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

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Shipment No.' AND @ReturnType = 'Parent level')
	BEGIN
		-- Parent level called as saving data for master dialog
		-- Also get data for shipment, not care about purchase orders available

		WITH CTE1 AS (  
			SELECT 	
				DISTINCT
				CONCAT('SHI_', SHI.Id) AS [Value]
				,SHI.ShipmentNo AS [Text]
				,NULL AS [DialogItemNumber]
			FROM Shipments SHI WITH(NOLOCK)
			WHERE SHI.ShipmentNo IN (SELECT [VALUE] FROM @FilterValueTbl)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
		)

		SELECT 
			[Value]
			,[Text]
			,NULL AS [ParentId]
			,DialogItemNumber AS [DialogItemNumber]
			,CAST(RecordCount AS BIGINT) -- Total number of record (parent level) for paging
		FROM CTE2

	END

	ELSE IF (@MessageShownOn = 'Shipments' AND @FilterCriteria = 'Shipment No.' AND @ReturnType = 'Child level')
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
			FROM Shipments SHI WITH(NOLOCK)
			INNER JOIN CargoDetails CD WITH(NOLOCK) ON SHI.Id = CD.ShipmentId
			INNER JOIN POLineItems POL WITH(NOLOCK) ON CD.ItemId = POL.Id
			INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON CD.OrderId = PO.Id
			WHERE 
				( @SearchTerm IS NULL OR @SearchTerm = '' OR PO.PONumber LIKE '%' + @SearchTerm + '%' )
				AND SHI.ShipmentNo IN (SELECT [VALUE] FROM @FilterValueTbl)
		)

		,CTE2 AS
		(
			SELECT *, (SELECT COUNT(1) FROM CTE1) AS [RecordCount]
			FROM CTE1
			ORDER BY [DialogItemNumber] ASC
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
