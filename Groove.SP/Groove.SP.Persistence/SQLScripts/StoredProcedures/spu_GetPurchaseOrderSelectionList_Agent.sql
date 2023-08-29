SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetPurchaseOrdersByPrincipalId_Agent', 'P') IS NOT NULL
DROP PROC dbo.spu_GetPurchaseOrdersByPrincipalId_Agent
GO

IF OBJECT_ID('spu_GetPurchaseOrderSelectionList_Agent', 'P') IS NOT NULL
DROP PROC dbo.spu_GetPurchaseOrderSelectionList_Agent
GO

-- =============================================
-- Author:		Dong Tran
-- Create date: 10 Jan 2022
-- Description:	This method to get all POs selections belonging to selected Principal organization as multi-POs selections
-- It works for Agent users
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetPurchaseOrderSelectionList_Agent]
	@PrincipalOrganizationId BIGINT,
	@SearchTerm NVARCHAR(255) = NULL,
	@Skip INT,
	@Take INT,
	@SelectedPOId BIGINT = 0,
	@Affiliates NVARCHAR(512) = ''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	--SET @PrincipalOrganizationId = 456;
	--	SET @SearchTerm = NULL;
	--	SET @SupplierOrganizationId = 462;
	--	SET @SupplierCustomerRelationships = ';';
	--	SET @Skip = 0;
	--	SET @Take = 20;
	--	SET @SelectedPOId = 0;	

	-- Variables

	DECLARE @RowCount BIGINT;
	DECLARE @WillCompareIfValue INT;
	SET @WillCompareIfValue = 10;

	DECLARE  @SelectedPO_PONumber VARCHAR(MAX); 	
	DECLARE  @SelectedPO_ExpectedShipDate DATETIME2;
	DECLARE  @SelectedPO_ExpectedDeliveryDate DATETIME2;
	DECLARE  @SelectedPO_ShipFromId BIGINT;
	DECLARE  @SelectedPO_ShipToId BIGINT
	DECLARE  @SelectedPO_ModeOfTransport VARCHAR(MAX);
	DECLARE  @SelectedPO_Incoterm VARCHAR(MAX);
	DECLARE  @SelectedPO_CarrierCode VARCHAR(MAX);
	DECLARE  @SelectedPO_ShipperId BIGINT;
	DECLARE  @SelectedPO_ConsigneeId BIGINT;
	DECLARE  @SelectedPO_SupplierId BIGINT;
	DECLARE  @SelectedPO_SupplierCompany NVARCHAR(512);
	DECLARE  @SelectedPO_POType INT;
	DECLARE  @SelectedPO_ExWorkDate DATETIME2;

	DECLARE @POTableResult TABLE (
		[Id] BIGINT,
		[ItemsCount] INT,
		[PONumber] VARCHAR(MAX)
	)
	
	DECLARE @CustomerIdTable TABLE (
		[Id] BIGINT
	)


	DECLARE @ExpectedShipDateVerification INT;
	DECLARE @ExpectedDeliveryDateVerification INT;
	DECLARE @ConsigneeVerification INT;
	DECLARE @ShipperVerification INT;
	DECLARE @ShipFromLocationVerification INT;
	DECLARE @ShipToLocationVerification INT;
	DECLARE @ModeOfTransportVerification INT;
	DECLARE @IncotermVerification INT;
	DECLARE @PreferredCarrierVerification INT;
	DECLARE @CheckPOExWorkDate BIT;
    SET @CheckPOExWorkDate = 0;

	DECLARE @AllowMultiplePOPerBooking BIT;

	-- From dbo.BuyerCompliances
	DECLARE @IsProgressCheckCRD BIT;
	DECLARE @IsAllowShowAdditionalInforPOListing BIT;
	DECLARE @IsCompulsory BIT;
	DECLARE @BuyerComplianceId BIGINT;

	-- If there is selected PO, get more information on PurchaseOrderVerification then make comparisons POs vs selected PO
	IF( @SelectedPOId > 0)
	BEGIN

		SELECT	@BuyerComplianceId = Id,
				@IsProgressCheckCRD = IsProgressCargoReadyDate,
				@IsCompulsory = IsCompulsory,
				@IsAllowShowAdditionalInforPOListing = IsAllowShowAdditionalInforPOListing
		FROM BuyerCompliances (NOLOCK)
		WHERE OrganizationId = @PrincipalOrganizationId AND [Status] = 1

		-- Store current selected PO to variables
		SELECT 
			@SelectedPO_PONumber = t1.PONumber,
			@SelectedPO_ExpectedShipDate = t1.ExpectedShipDate,
			@SelectedPO_ExpectedDeliveryDate = t1.ExpectedDeliveryDate,
			@SelectedPO_ShipFromId = t1.ShipFromId,
			@SelectedPO_ShipToId = t1.ShipToId,
			@SelectedPO_ModeOfTransport = t1.ModeOfTransport,
			@SelectedPO_Incoterm = t1.Incoterm,
			@SelectedPO_CarrierCode = t1.CarrierCode,
			@SelectedPO_ShipperId = t4.ShipperId,
			@SelectedPO_ConsigneeId = t3.ConsigneeId,
			@SelectedPO_SupplierId = t2.SupplierId,
			@SelectedPO_SupplierCompany = t2.CompanyName,
			@SelectedPO_POType = t1.POType,
			@SelectedPO_ExWorkDate = t1.ExWorkDate

		FROM
		(
			SELECT
			PO.Id AS [Id],
			PO.PONumber AS [PONumber], 	
			PO.ExpectedShipDate AS [ExpectedShipDate],
			PO.ExpectedDeliveryDate AS [ExpectedDeliveryDate],
			PO.ShipFromId AS [ShipFromId],
			PO.ShipToId AS [ShipToId],
			PO.ModeOfTransport AS [ModeOfTransport],
			PO.Incoterm AS [Incoterm],
			PO.CarrierCode AS [CarrierCode],
			PO.POType AS [POType],
			PO.CargoReadyDate AS [ExWorkDate]
			FROM [PurchaseOrders] PO WITH (NOLOCK)
			WHERE PO.Id = @SelectedPOId
		) t1					
		CROSS APPLY
		(
			SELECT TOP(1) sc.OrganizationId AS SupplierId,sc.CompanyName
			FROM PurchaseOrderContacts sc WITH (NOLOCK)
			WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
		) t2
		OUTER APPLY
		(
			SELECT TOP(1) OrganizationId AS ConsigneeId
			FROM PurchaseOrderContacts sc WITH (NOLOCK)
			WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Consignee'
		) t3
		OUTER APPLY
		(
			SELECT TOP(1) OrganizationId AS ShipperId
			FROM PurchaseOrderContacts sc WITH (NOLOCK)
			WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Shipper'
		) t4

		-- Store POVerifications into variables
		SELECT	@ExpectedShipDateVerification =  VER.ExpectedShipDateVerification,
				@ExpectedDeliveryDateVerification = VER.ExpectedDeliveryDateVerification,
				@ConsigneeVerification = VER.ConsigneeVerification,
				@ShipperVerification = VER.ShipperVerification,
				@ShipFromLocationVerification = VER.ShipFromLocationVerification,
				@ShipToLocationVerification = VER.ShipToLocationVerification,
				@ModeOfTransportVerification = VER.ModeOfTransportVerification,
				@IncotermVerification = VER.IncotermVerification,
				@PreferredCarrierVerification = VER.PreferredCarrierVerification
		FROM PurchaseOrderVerificationSettings VER WITH (NOLOCK)
		WHERE VER.BuyerComplianceId = @BuyerComplianceId

		-- Get value for variable that defines whether PO Ex-work date checking
        SELECT @CheckPOExWorkDate = 1
        FROM BookingTimelesses BT
        WHERE
			BT.BuyerComplianceId = @BuyerComplianceId
			AND BT.DateForComparison = 10
            AND EXISTS
                (    SELECT 1
                    FROM BookingPolicies BP
                    WHERE BT.BuyerComplianceId = BP.BuyerComplianceId AND BP.BookingTimeless != 0
                )

		-- Store ShippingCompliances into variables
		SELECT	@AllowMultiplePOPerBooking =  SPC.AllowMultiplePOPerFulfillment
		FROM ShippingCompliances SPC WITH (NOLOCK)
			INNER JOIN BuyerCompliances BC ON SPC.BuyerComplianceId = BC.Id AND BC.[Status] = 1 AND BC.OrganizationId = @PrincipalOrganizationId

		-- PLEASE make sure order of column is matched to C# mapping
		INSERT INTO @POTableResult
		SELECT  t1.Id, t1.ItemsCount, t10.PONumber
			FROM
				(
					SELECT PO.Id AS [Id],
						COUNT(POL.Id) AS [ItemsCount]
					FROM [PurchaseOrders] PO WITH (NOLOCK)
					INNER JOIN [PurchaseOrderContacts] POC WITH (NOLOCK) ON PO.Id = POC.PurchaseOrderId AND POC.OrganizationId = @PrincipalOrganizationId AND POC.OrganizationRole = 'Principal'
					INNER JOIN POLineItems POL WITH (NOLOCK) ON PO.id = POL.PurchaseOrderId 
					WHERE PO.[Status] = 1  
						AND POL.BalanceUnitQty > 0
						-- Check on PO type
						AND (PO.POType = @SelectedPO_POType)
						-- If the selected customer has been enabled Progress Check in the Compliance Setting, system will display list of POs which has Production Started = Yes only
						AND (
						@IsProgressCheckCRD = 0 
						OR NOT (@IsProgressCheckCRD = 1 AND @IsCompulsory = 1 AND PO.ProductionStarted = 0)
						)
						AND (
							@AllowMultiplePOPerBooking = 1 OR PO.Id = @SelectedPOId
						)
					GROUP BY PO.id
					HAVING COUNT(POL.id) > 0
				) t1
				OUTER APPLY
				(
					SELECT
					PO.PONumber AS [PONumber], 	
					PO.ExpectedShipDate AS [ExpectedShipDate],
					PO.ExpectedDeliveryDate AS [ExpectedDeliveryDate],
					PO.ShipFromId AS [ShipFromId],
					PO.ShipToId AS [ShipToId],
					PO.ModeOfTransport AS [ModeOfTransport],
					PO.Incoterm AS [Incoterm],
					PO.CarrierCode AS [CarrierCode],
					PO.CargoReadyDate AS [ExWorkDate]
				FROM [PurchaseOrders] PO WITH (NOLOCK) WHERE PO.Id = t1.Id
				) t2
				CROSS APPLY
				(
					SELECT TOP(1) sc.OrganizationId AS SupplierId
					FROM PurchaseOrderContacts sc WITH (NOLOCK)
					WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
					-- Must same supplier Id
					AND ( 
					  (sc.OrganizationId = @SelectedPO_SupplierId AND @SelectedPO_SupplierId <> 0)
					  OR (sc.CompanyName = @SelectedPO_SupplierCompany AND @SelectedPO_SupplierId = 0)
					)
				) t3
				OUTER APPLY
				(
					SELECT TOP(1) OrganizationId AS ConsigneeId
					FROM PurchaseOrderContacts sc WITH (NOLOCK)
					WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Consignee'
				) t4
				OUTER APPLY
                (
			        SELECT TOP(1) OrganizationId AS CustomerId
			        FROM PurchaseOrderContacts sc WITH (NOLOCK)
			        WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
                ) t5
				OUTER APPLY
                (
			        SELECT TOP(1) OrganizationId AS ShipperId
			        FROM PurchaseOrderContacts sc WITH (NOLOCK)
			        WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Shipper'
                ) t6
					OUTER APPLY
				(
					SELECT TOP(1) OrganizationId AS OriginAgentId
					FROM PurchaseOrderContacts sc WITH (NOLOCK)
					WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Origin Agent'
				) t7
				OUTER APPLY
				(
					SELECT TOP(1) OrganizationId AS DestinationAgentId
					FROM PurchaseOrderContacts sc WITH (NOLOCK)
					WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Destination Agent'
				) t8
				OUTER APPLY
				(
					SELECT TOP(1) OrganizationId AS DelegationId
					FROM PurchaseOrderContacts sc WITH (NOLOCK)
					WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Delegation'
				) t9
				-- Searching column
				CROSS APPLY (
							SELECT TOP(1)
								CASE 
									WHEN @IsAllowShowAdditionalInforPOListing = 1 AND POL.GridValue IS NOT NULL AND POL.GridValue <> '' THEN CONCAT(t2.[PONumber], ' - ', POL.ProductCode, ' - ', POL.GridValue)
									WHEN @IsAllowShowAdditionalInforPOListing = 1 AND (POL.GridValue IS NULL OR POL.GridValue = '') THEN CONCAT(t2.[PONumber], ' - ', POL.ProductCode)
									ELSE t2.PONumber
								END AS PONumber
							FROM POLineItems POL WITH (NOLOCK)
							WHERE t1.Id = POL.PurchaseOrderId
				) t10
			WHERE 
				(@SearchTerm IS NULL OR @SearchTerm = '' OR t10.[PONumber] LIKE '%' + @SearchTerm + '%')
				AND
				-- Check buyer compliance settings on purchase order verifications
				(
					@SelectedPOId != t1.Id
					AND	((@ExpectedShipDateVerification != @WillCompareIfValue) OR ( @SelectedPO_ExpectedShipDate = t2.ExpectedShipDate))
					AND ((@ExpectedDeliveryDateVerification != @WillCompareIfValue) OR ( @SelectedPO_ExpectedDeliveryDate = t2.ExpectedDeliveryDate))
					AND ((@ConsigneeVerification != @WillCompareIfValue) OR ( @SelectedPO_ConsigneeId = t4.ConsigneeId))
					AND ((@ShipperVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipperId = t6.ShipperId))
					AND ((@ShipFromLocationVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipFromId = t2.ShipFromId))
					AND ((@ShipToLocationVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipToId = t2.ShipToId))
					AND ((@ModeOfTransportVerification != @WillCompareIfValue) OR ( @SelectedPO_ModeOfTransport = t2.ModeOfTransport))
					AND ((@IncotermVerification != @WillCompareIfValue) OR ( @SelectedPO_Incoterm = t2.Incoterm))
					AND ((@PreferredCarrierVerification != @WillCompareIfValue) OR ( @SelectedPO_CarrierCode = t2.CarrierCode))
					AND ((@CheckPOExWorkDate = 0) OR (t2.ExWorkDate IS NULL) OR (@SelectedPO_ExWorkDate IS NULL) OR (CAST(@SelectedPO_ExWorkDate AS DATE) = CAST(t2.ExWorkDate AS DATE)))
				)
				AND
				-- Other checks on permissions of Agent
					(
						t9.DelegationId IN (SELECT value FROM dbo.fn_SplitStringToTable(@Affiliates,','))
						OR t7.OriginAgentId IN (SELECT value FROM dbo.fn_SplitStringToTable(@Affiliates,','))
						OR t8.DestinationAgentId IN (SELECT value FROM dbo.fn_SplitStringToTable(@Affiliates,','))
					)
			
		SET @RowCount = @@ROWCOUNT

		SELECT *, @RowCount AS [RecordCount] FROM @POTableResult
		ORDER BY Id
		OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

	END

	ELSE
	BEGIN

		SELECT	@SelectedPO_POType = BC.AllowToBookIn, -- Get PO Type allowed in buyer compliance
				@IsProgressCheckCRD = BC.IsProgressCargoReadyDate,
				@IsCompulsory = BC.IsCompulsory,
				@IsAllowShowAdditionalInforPOListing = BC.IsAllowShowAdditionalInforPOListing
		FROM BuyerCompliances BC
		WHERE BC.[Status] = 1 AND BC.OrganizationId = @PrincipalOrganizationId

		-- PLEASE make sure order of column is matched to C# mapping
		INSERT INTO @POTableResult
		SELECT t1.Id, t1.ItemsCount, t4.PONumber
		FROM
			(
				SELECT PO.Id AS [Id],
					COUNT(POL.Id) AS [ItemsCount]
				FROM [PurchaseOrders] PO WITH (NOLOCK)
				INNER JOIN [PurchaseOrderContacts] POC WITH (NOLOCK) ON PO.Id = POC.PurchaseOrderId AND POC.OrganizationId = @PrincipalOrganizationId AND POC.OrganizationRole = 'Principal'
				INNER JOIN POLineItems POL WITH (NOLOCK) ON PO.id = POL.PurchaseOrderId 
				WHERE PO.[Status] = 1
					AND POL.BalanceUnitQty > 0
					-- Check PO Type, must be single or from buyer compliance
					AND (PO.POType = 10 OR PO.POType = @SelectedPO_POType)
					-- If the selected customer has been enabled Progress Check in the Compliance Setting, system will display list of POs which has Production Started = Yes only
					AND (
						@IsProgressCheckCRD = 0 
						OR NOT (@IsProgressCheckCRD = 1 AND @IsCompulsory = 1 AND PO.ProductionStarted = 0)
						)
				GROUP BY PO.id
				HAVING COUNT(POL.id) > 0
			) t1
			CROSS APPLY
			(
				SELECT
				PO.PONumber AS [PONumber], 	
				PO.ExpectedShipDate AS [ExpectedShipDate],
				PO.ExpectedDeliveryDate AS [ExpectedDeliveryDate],
				PO.ShipFromId AS [ShipFromId],
				PO.ShipToId AS [ShipToId],
				PO.ModeOfTransport AS [ModeOfTransport],
				PO.Incoterm AS [Incoterm],
				PO.CarrierCode AS [CarrierCode]
			FROM [PurchaseOrders] PO WITH (NOLOCK) WHERE PO.Id = t1.Id
			) t2
			CROSS APPLY
			(
				SELECT TOP(1) sc.OrganizationId AS SupplierId
				FROM PurchaseOrderContacts sc WITH (NOLOCK)
				WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
			) t3
			-- Searching column
			CROSS APPLY (
						SELECT TOP(1)
							CASE 
								WHEN @IsAllowShowAdditionalInforPOListing = 1 AND POL.GridValue IS NOT NULL AND POL.GridValue <> '' THEN CONCAT(t2.[PONumber], ' - ', POL.ProductCode, ' - ', POL.GridValue)
								WHEN @IsAllowShowAdditionalInforPOListing = 1 AND (POL.GridValue IS NULL OR POL.GridValue = '') THEN CONCAT(t2.[PONumber], ' - ', POL.ProductCode)
								ELSE t2.PONumber
							END AS PONumber
						FROM POLineItems POL WITH (NOLOCK)
						WHERE t1.Id = POL.PurchaseOrderId
			) t4
			CROSS APPLY
            (
			    SELECT TOP(1) OrganizationId AS CustomerId
			    FROM PurchaseOrderContacts sc WITH (NOLOCK)
			    WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
            ) t5
			OUTER APPLY
			(
				SELECT TOP(1) OrganizationId AS DelegationId
				FROM PurchaseOrderContacts sc WITH (NOLOCK)
				WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Delegation'
			) t6
			OUTER APPLY
			(
				SELECT TOP(1) OrganizationId AS OriginAgentId
				FROM PurchaseOrderContacts sc WITH (NOLOCK)
				WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Origin Agent'
			) t7
			OUTER APPLY
			(
				SELECT TOP(1) OrganizationId AS DestinationAgentId
				FROM PurchaseOrderContacts sc WITH (NOLOCK)
				WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Destination Agent'
			) t8

			WHERE
				(@SearchTerm IS NULL OR @SearchTerm = '' OR t4.[PONumber] LIKE '%' + @SearchTerm + '%')
				AND
				(
				t6.DelegationId IN (SELECT value FROM dbo.fn_SplitStringToTable(@Affiliates,','))
				OR t7.OriginAgentId IN (SELECT value FROM dbo.fn_SplitStringToTable(@Affiliates,','))
				OR t8.DestinationAgentId IN (SELECT value FROM dbo.fn_SplitStringToTable(@Affiliates,','))
				)
		
		SET @RowCount = @@ROWCOUNT

		SELECT *, @RowCount AS [RecordCount] FROM @POTableResult
		ORDER BY Id
		OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
	END
END
GO
