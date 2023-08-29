SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetQuickTrackOption', 'P') IS NOT NULL
DROP PROC dbo.spu_GetQuickTrackOption
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 12 April 2023
-- Description:	Based on the search term to determine the quick track option which the user is searching for
				-- The quick track options can be: 
				--1. Customer reference no
				--2. Agent reference no
				--3. Container no
				--4. HBL no
				--5. Item no
				--6. Purchase Order No
				--7. Booking No
				--8. Shipment No
-- =============================================
CREATE PROCEDURE spu_GetQuickTrackOption
	@searchTerm NVARCHAR(255) = NULL,
	@isInternal BIT = 1,
	@organizationId BIGINT = NULL,
	@affiliates NVARCHAR(MAX) = NULL,
	@supplierCustomerRelationships NVARCHAR(MAX) = NULL -- for shipper user
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	-- Declare table here

	DECLARE @result TABLE (
		[MatchedOption] NVARCHAR(MAX),

		--For further redirect to the detail page, 
		--leave it empty if the option will redirect to the listing page
		[MatchedValue] BIGINT
	)

	DECLARE @affiliateIds TABLE ([Id] BIGINT)

	IF (@affiliates IS NOT NULL AND @affiliates <> '')
	BEGIN
		INSERT INTO @affiliateIds
		SELECT [VALUE]
		FROM [dbo].[fn_SplitStringToTable] (@affiliates, ',')
	END

	--The result will be returned immediately
	--if there is a matched option found

	--Search option 1: Booking No

	INSERT INTO @result
	(
	[MatchedOption],
	[MatchedValue]
	)
	SELECT
		CASE POF.FulfillmentType
			WHEN 1 THEN IIF(
				POF.OrderFulfillmentPolicy = 10 AND POF.Stage = 10, 'missingPOFulfillmentNumber', 'poFulfillmentNumber')
			WHEN 2 THEN 'bulkFulfillmentNumber'
			WHEN 3 THEN 'warehouseFulfillmentNumber'
		ELSE 'poFulfillmentNumber'
		END AS [MatchedOption],
		POF.Id
	FROM POFulfillments POF WITH(NOLOCK)
	WHERE POF.Number = @searchTerm AND (@isInternal = 1 OR EXISTS (SELECT 1
								FROM POFulfillmentContacts POFC WITH(NOLOCK)
								WHERE POFC.POFulfillmentId = POF.Id AND POFC.OrganizationId IN (SELECT * FROM @affiliateIds)))

	--Search option 2: Shipment No.

	IF NOT EXISTS(SELECT * FROM @result)
	BEGIN

		INSERT INTO @result
		(
		[MatchedOption],
		[MatchedValue]
		)
		SELECT 'shipmentNo' AS [MatchedOption], SHI.Id
		FROM Shipments SHI WITH(NOLOCK)
		WHERE SHI.ShipmentNo = @searchTerm AND (@isInternal = 1 OR EXISTS (SELECT 1
									FROM ShipmentContacts SC WITH(NOLOCK)
									WHERE SC.ShipmentId = SHI.Id AND SC.OrganizationId IN (SELECT * FROM @affiliateIds)))
	END

	--Search option 3: Container No. 

	IF NOT EXISTS(SELECT * FROM @result)
	BEGIN

		INSERT INTO @result
		(
		[MatchedOption],
		[MatchedValue]
		)
		SELECT TOP(1) 'containerNo' AS [MatchedOption], CTN.Id
		FROM Containers CTN WITH(NOLOCK)
		WHERE CTN.ContainerNo = @searchTerm AND (
			@isInternal = 1
			OR (CTN.IsFCL = 1 AND EXISTS (	SELECT 1
											FROM ShipmentLoads SL WITH(NOLOCK) INNER JOIN Shipments SHI WITH(NOLOCK) ON SL.ShipmentId = SHI.Id INNER JOIN ShipmentContacts SC WITH(NOLOCK) ON SHI.Id = SC.ShipmentId
											WHERE SL.ContainerId = CTN.Id AND SC.OrganizationId IN (SELECT * FROM @affiliateIds))
			)
			OR (CTN.IsFCL = 0 AND EXISTS (	SELECT 1
											FROM Consolidations CONSL WITH(NOLOCK) INNER JOIN ShipmentLoads SL WITH(NOLOCK) ON CONSL.Id = SL.ConsolidationId
																INNER JOIN Shipments SHI WITH(NOLOCK) ON SL.ShipmentId = SHI.Id
																INNER JOIN ShipmentContacts SC WITH(NOLOCK) ON SHI.Id = SC.ShipmentId
											WHERE CONSL.ContainerId = CTN.Id AND SC.OrganizationId IN (SELECT * FROM @affiliateIds)
			)
		))

	END

	--Search option 4: House Bill No.

	IF NOT EXISTS(SELECT * FROM @result)
	BEGIN
		INSERT INTO @result
		(
		[MatchedOption],
		[MatchedValue]
		)
		SELECT TOP(1) 'houseBillNumber' AS [MatchedOption], BOL.Id
		FROM BillOfLadings BOL WITH(NOLOCK)
		WHERE BOL.BillOfLadingNo = @searchTerm AND (@isInternal = 1 OR EXISTS (SELECT 1
									FROM BillOfLadingContacts BOLC WITH(NOLOCK)
									WHERE BOLC.BillOfLadingId = BOL.Id AND BOLC.OrganizationId IN (SELECT * FROM @affiliateIds)))
	END

	--Search option 5: Purchase Order No OR Customer Reference No. OR Agent Reference No
	IF NOT EXISTS(SELECT * FROM @result)
	BEGIN
		IF (@isInternal = 1)
		BEGIN
					INSERT INTO @result
					(
					[MatchedOption]
					)
					SELECT TOP(1) 'ShipmentRefNo'
                    FROM Shipments SHI WITH(NOLOCK)
					OUTER APPLY
					(
								SELECT POD.CustomerPONumber
								FROM POFulfillments POF WITH(NOLOCK) INNER JOIN POFulfillmentOrders POD WITH(NOLOCK) ON POF.Id = POD.POFulfillmentId
								WHERE POF.Id = SHI.POFulfillmentId
								UNION
								SELECT OD.PONumber as CustomerPONumber
								FROM CargoDetails CD WITH(NOLOCK) INNER JOIN PurchaseOrders OD WITH(NOLOCK) ON CD.OrderId = OD.Id
								WHERE SHI.POFulfillmentId is NULL AND CD.ShipmentId = SHI.Id
					) t1
					WHERE SHI.CustomerReferenceNo like '%' + @searchTerm + '%'
						OR SHI.AgentReferenceNo like '%' + @searchTerm + '%'
						OR t1.CustomerPONumber like '%' + @searchTerm + '%'
		END
		ELSE -- External
		BEGIN
					INSERT INTO @result
					(
					[MatchedOption]
					)
					SELECT TOP(1) 'ShipmentRefNo'
                    FROM Shipments SHI WITH(NOLOCK)
					OUTER APPLY
					(
								SELECT POD.CustomerPONumber
								FROM POFulfillments POF WITH(NOLOCK) INNER JOIN POFulfillmentOrders POD WITH(NOLOCK) ON POF.Id = POD.POFulfillmentId
								WHERE POF.Id = SHI.POFulfillmentId
								UNION
								SELECT OD.PONumber as CustomerPONumber
								FROM CargoDetails CD WITH(NOLOCK) INNER JOIN PurchaseOrders OD WITH(NOLOCK) ON CD.OrderId = OD.Id
								WHERE SHI.POFulfillmentId is NULL AND CD.ShipmentId = SHI.Id
					) t1
                    WHERE (
						SHI.CustomerReferenceNo like '%' + @searchTerm + '%'
						OR SHI.AgentReferenceNo like '%' + @searchTerm + '%'
						OR t1.CustomerPONumber like '%' + @searchTerm + '%'
					)
					AND
					EXISTS (
			                SELECT 1 
			                FROM ShipmentContacts SC WITH(NOLOCK) 
                            WHERE SHI.Id = SC.ShipmentId AND SC.OrganizationId IN (SELECT * FROM @affiliateIds)
	                    )
		END
	END
	


	--Search option 6: Item No.

	IF NOT EXISTS(SELECT * FROM @result)
	BEGIN
		IF (@isInternal = 1)
		BEGIN

			INSERT into @result
			(
			[MatchedOption]
			)
			SELECT TOP(1) 'itemNo' AS [MatchedOption]
	        FROM PurchaseOrders po WITH (NOLOCK)
			WHERE EXISTS (SELECT 1
						  FROM POLineItems pol WITH(NOLOCK)
						  WHERE pol.PurchaseOrderId = po.Id AND pol.ProductCode like '%' + @searchTerm + '%')
		END
		ELSE IF (@supplierCustomerRelationships is null OR @supplierCustomerRelationships = '') -- agent/ principal
		BEGIN
		
			INSERT INTO @result
			(
			[MatchedOption]
			)
			SELECT TOP(1) 'itemNo' AS [MatchedOption]
			FROM PurchaseOrders po WITH (NOLOCK)
			WHERE po.Id IN (
				SELECT pc.PurchaseOrderId FROM PurchaseOrderContacts pc WITH(NOLOCK)
				WHERE po.Id = pc.PurchaseOrderId AND pc.OrganizationId IN (SELECT * FROM @affiliateIds))
			AND EXISTS (
				SELECT 1
				FROM POLineItems pol WITH(NOLOCK)
				WHERE pol.PurchaseOrderId = po.Id AND pol.ProductCode like '%' + @searchTerm + '%')
		END

		ELSE --Shipper
		BEGIN

			INSERT into @result
			(
			[MatchedOption]
			)
			SELECT TOP(1) 'itemNo' AS [MatchedOption]
            FROM
            (
	            SELECT
                    po.Id,
					pc.OrganizationRole
	            FROM PurchaseOrders po WITH (NOLOCK)
                INNER JOIN PurchaseOrderContacts pc WITH (NOLOCK) on po.Id = pc.PurchaseOrderId
                WHERE pc.OrganizationId = @organizationId
            ) PO
            CROSS APPLY
            (
			    SELECT TOP(1) sc.OrganizationId AS SupplierId
			    FROM PurchaseOrderContacts sc WITH (NOLOCK)
			    WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
            ) SUP
            CROSS APPLY
            (
                SELECT TOP(1) sc.OrganizationId AS CustomerId
                FROM PurchaseOrderContacts sc WITH (NOLOCK)
                WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
            ) PRIN
            CROSS APPLY
            (
		        SELECT TOP(1) BC.Id
		        FROM BuyerCompliances BC (NOLOCK)
		        WHERE BC.OrganizationId = PRIN.CustomerId AND BC.Stage = 1
            ) BCOM
            WHERE
	        (
				PO.OrganizationRole = 'Delegation'
				OR (
					PO.OrganizationRole = 'Supplier' 
					AND CAST(SUP.SupplierID AS NVARCHAR(20)) + ',' + CAST(PRIN.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] 
					FROM dbo.fn_SplitStringToTable(@supplierCustomerRelationships, ';') tmp )
				)
			)
			AND EXISTS (
				SELECT 1
				FROM POLineItems pol
				WHERE pol.PurchaseOrderId = po.Id AND pol.ProductCode like '%' + @searchTerm + '%'
			)
		END
	END

	SELECT * FROM @result
END

GO