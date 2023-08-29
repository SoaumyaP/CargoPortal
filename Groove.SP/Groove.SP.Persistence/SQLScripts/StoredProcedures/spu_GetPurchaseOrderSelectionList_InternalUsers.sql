SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetPurchaseOrdersByPrincipalId_InternalUsers', 'P') IS NOT NULL
DROP PROC dbo.spu_GetPurchaseOrdersByPrincipalId_InternalUsers
GO

IF OBJECT_ID('spu_GetPurchaseOrderSelectionList_InternalUsers', 'P') IS NOT NULL
DROP PROC dbo.spu_GetPurchaseOrderSelectionList_InternalUsers
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 26 June 2020
-- Description:	This method to get all POs selections belonging to selected Principal organization as multi-POs selections
-- It works for Internal users
-- =============================================
CREATE PROCEDURE spu_GetPurchaseOrderSelectionList_InternalUsers
	@PrincipalOrganizationId BIGINT,
	@SearchTerm NVARCHAR(255) = NULL,   
	@Skip INT,
	@Take INT,
	@SelectedPOId BIGINT = 0

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	--SET @PrincipalOrganizationId = 456
	--SET @SearchTerm = ''
	--SET @Skip = 40
	--SET @Take = 20
	--SET @SelectedPOId = 0

	-- Variables

	DECLARE @RowCount INT;
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
		[PONumber] VARCHAR(MAX), 
		[ExpectedShipDate] DATETIME2,
		[ExpectedDeliveryDate] DATETIME2,
		[ShipFromId] BIGINT,
		[ShipToId] BIGINT,
		[ModeOfTransport] VARCHAR(MAX),
		[Incoterm] VARCHAR(MAX),
		[CarrierCode] VARCHAR(MAX),
		[SupplierId] BIGINT,
		[ConsigneeId] BIGINT
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

	-- BuyerCompliances variables
	DECLARE @IsProgressCheckCRD BIT;
	DECLARE @IsAllowShowAdditionalInforPOListing BIT;
	DECLARE @IsCompulsory BIT;
	DECLARE @BuyerComplianceId BIGINT;

	SELECT TOP(1) @BuyerComplianceId = Id, @IsProgressCheckCRD = IsProgressCargoReadyDate, @IsCompulsory = IsCompulsory, @IsAllowShowAdditionalInforPOListing = IsAllowShowAdditionalInforPOListing
	FROM BuyerCompliances (NOLOCK)
	WHERE OrganizationId = @PrincipalOrganizationId AND [Status] = 1

	-- If there is selected PO, get more information on PurchaseOrderVerification then make comparisons POs vs selected PO
	IF( @SelectedPOId > 0)
	BEGIN

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
			@SelectedPO_SupplierId= t2.SupplierId,
			@SelectedPO_SupplierCompany= t2.CompanyName,
			@SelectedPO_ConsigneeId = t3.ConsigneeId,
			@SelectedPO_ShipperId = t4.ShipperId,
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
			INNER JOIN BuyerCompliances BC ON VER.BuyerComplianceId = BC.Id AND BC.[Status] = 1 AND BC.OrganizationId = @PrincipalOrganizationId

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

		-- Count available records
		-- It is just to count a number of records, not need to select all columns
		SELECT @RowCount = COUNT(t1.Id)
		FROM
			(
				SELECT PO.Id AS [Id],
					COUNT(POL.Id) AS [ItemsCount]
				FROM [PurchaseOrders] PO WITH (NOLOCK)
				INNER JOIN [PurchaseOrderContacts] POC WITH (NOLOCK) ON PO.Id = POC.PurchaseOrderId AND POC.OrganizationId = @PrincipalOrganizationId AND POC.OrganizationRole = 'Principal'
				INNER JOIN POLineItems POL WITH (NOLOCK) ON PO.id = POL.PurchaseOrderId
				-- Searching column
				CROSS APPLY (
							SELECT TOP(1)
								CASE 
									WHEN @IsAllowShowAdditionalInforPOListing = 1 AND POL1.GridValue IS NOT NULL AND POL1.GridValue <> '' THEN CONCAT(PO.[PONumber], ' - ', POL1.ProductCode, ' - ', POL1.GridValue)
									WHEN @IsAllowShowAdditionalInforPOListing = 1 AND (POL1.GridValue IS NULL OR POL1.GridValue = '') THEN CONCAT(PO.[PONumber], ' - ', POL1.ProductCode)
									ELSE PO.PONumber
								END AS PONumber
							FROM POLineItems POL1 WITH (NOLOCK)
							WHERE PO.Id = POL1.PurchaseOrderId
				) t
				WHERE PO.[Status] = 1
					AND ( @SearchTerm IS NULL OR @SearchTerm = '' OR t.[PONumber] LIKE '%' + @SearchTerm + '%') 
					AND POL.BalanceUnitQty > 0
					-- Check on PO type
					AND PO.POType = @SelectedPO_POType
					-- If the selected customer has been enabled Progress Check in the Compliance Setting, system will display list of POs which has Production Started = Yes only
					AND (
						@IsProgressCheckCRD = 0 
						OR NOT (@IsProgressCheckCRD = 1 AND @IsCompulsory = 1 AND PO.ProductionStarted = 0)
						)
					AND (
							@AllowMultiplePOPerBooking = 1 OR PO.Id = @SelectedPOId
						)
				GROUP BY PO.Id
				HAVING COUNT(POL.Id) > 0
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
					AND ((sc.OrganizationId = @SelectedPO_SupplierId AND @SelectedPO_SupplierId <> 0) OR (sc.CompanyName = @SelectedPO_SupplierCompany AND @SelectedPO_SupplierId = 0))
			) t3
			OUTER APPLY
			(
				SELECT TOP(1) OrganizationId AS ConsigneeId
				FROM PurchaseOrderContacts sc WITH (NOLOCK)
				WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Consignee'
			) t4
			OUTER APPLY
            (
			    SELECT TOP(1) OrganizationId AS ShipperId
			    FROM PurchaseOrderContacts sc WITH (NOLOCK)
			    WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Shipper'
            ) t5
		WHERE 			
			-- Check buyer compliance settings on purchase order verifications
			(
				@SelectedPOId != t1.Id
				AND	((@ExpectedShipDateVerification != @WillCompareIfValue) OR ( @SelectedPO_ExpectedShipDate = t2.ExpectedShipDate))
				AND ((@ExpectedDeliveryDateVerification != @WillCompareIfValue) OR ( @SelectedPO_ExpectedDeliveryDate = t2.ExpectedDeliveryDate))
				AND ((@ConsigneeVerification != @WillCompareIfValue) OR ( @SelectedPO_ConsigneeId = t4.ConsigneeId))
				AND ((@ShipperVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipperId = t5.ShipperId))
				AND ((@ShipFromLocationVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipFromId = t2.ShipFromId))
				AND ((@ShipToLocationVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipToId = t2.ShipToId))
				AND ((@ModeOfTransportVerification != @WillCompareIfValue) OR ( @SelectedPO_ModeOfTransport = t2.ModeOfTransport))
				AND ((@IncotermVerification != @WillCompareIfValue) OR ( @SelectedPO_Incoterm = t2.Incoterm))
				AND ((@PreferredCarrierVerification != @WillCompareIfValue) OR ( @SelectedPO_CarrierCode = t2.CarrierCode))
				AND ((@CheckPOExWorkDate = 0) OR (t2.ExWorkDate IS NULL) OR (@SelectedPO_ExWorkDate IS NULL) OR (CAST(@SelectedPO_ExWorkDate AS DATE) = CAST(t2.ExWorkDate AS DATE)))
			)

		-- Return data here
		-- PLEASE make sure order of column is matched to C# mapping
		SELECT	t1.[Id], t1.[ItemsCount], t4.[PONumber], CAST(@RowCount AS BIGINT) AS [RecordCount]
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
					AND PO.POType = @SelectedPO_POType
					-- If the selected customer has been enabled Progress Check in the Compliance Setting, system will display list of POs which has Production Started = Yes only
					AND (
						@IsProgressCheckCRD = 0 
						OR NOT (@IsProgressCheckCRD = 1 AND @IsCompulsory = 1 AND PO.ProductionStarted = 0) 
						)
					AND (
						@AllowMultiplePOPerBooking = 1 OR PO.Id = @SelectedPOId
					)
				GROUP BY PO.Id
				HAVING COUNT(POL.Id) > 0
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
					AND ((sc.OrganizationId = @SelectedPO_SupplierId AND @SelectedPO_SupplierId <> 0) OR (sc.CompanyName = @SelectedPO_SupplierCompany AND @SelectedPO_SupplierId = 0))
			) t3
			-- Searching column
				CROSS APPLY (
							SELECT TOP(1)
								CASE 
									WHEN @IsAllowShowAdditionalInforPOListing = 1 AND POL1.GridValue IS NOT NULL AND POL1.GridValue <> '' THEN CONCAT(t2.[PONumber], ' - ', POL1.ProductCode, ' - ', POL1.GridValue)
									WHEN @IsAllowShowAdditionalInforPOListing = 1 AND (POL1.GridValue IS NULL OR POL1.GridValue = '') THEN CONCAT(t2.[PONumber], ' - ', POL1.ProductCode)
									ELSE t2.PONumber
								END AS PONumber
							FROM POLineItems POL1 WITH (NOLOCK)
							WHERE t1.Id = POL1.PurchaseOrderId
				) t4
			OUTER APPLY
			(
				SELECT TOP(1) OrganizationId AS ConsigneeId
				FROM PurchaseOrderContacts sc WITH (NOLOCK)
				WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Consignee'
			) t5
			OUTER APPLY
            (
			    SELECT TOP(1) OrganizationId AS ShipperId
			    FROM PurchaseOrderContacts sc WITH (NOLOCK)
			    WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Shipper'
            ) t6
		WHERE 
			-- Check buyer compliance settings on purchase order verifications
			(
				@SelectedPOId != t1.Id
				AND	((@ExpectedShipDateVerification != @WillCompareIfValue) OR ( @SelectedPO_ExpectedShipDate = t2.ExpectedShipDate))
				AND ((@ExpectedDeliveryDateVerification != @WillCompareIfValue) OR ( @SelectedPO_ExpectedDeliveryDate = t2.ExpectedDeliveryDate))
				AND ((@ConsigneeVerification != @WillCompareIfValue) OR ( @SelectedPO_ConsigneeId = t5.ConsigneeId))
				AND ((@ShipperVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipperId = t6.ShipperId))
				AND ((@ShipFromLocationVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipFromId = t2.ShipFromId))
				AND ((@ShipToLocationVerification != @WillCompareIfValue) OR ( @SelectedPO_ShipToId = t2.ShipToId))
				AND ((@ModeOfTransportVerification != @WillCompareIfValue) OR ( @SelectedPO_ModeOfTransport = t2.ModeOfTransport))
				AND ((@IncotermVerification != @WillCompareIfValue) OR ( @SelectedPO_Incoterm = t2.Incoterm))
				AND ((@PreferredCarrierVerification != @WillCompareIfValue) OR ( @SelectedPO_CarrierCode = t2.CarrierCode))
				AND ((@CheckPOExWorkDate = 0) OR (t2.ExWorkDate IS NULL) OR (@SelectedPO_ExWorkDate IS NULL) OR (CAST(@SelectedPO_ExWorkDate AS DATE) = CAST(t2.ExWorkDate AS DATE)))
			)
			AND (@SearchTerm IS NULL OR @SearchTerm = '' OR t4.[PONumber] LIKE '%' + @SearchTerm + '%')
		ORDER BY Id
		OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

	END
	ELSE
	BEGIN

		-- Get PO Type allowed in buyer compliance
		SELECT	@SelectedPO_POType = BC.AllowToBookIn
		FROM BuyerCompliances BC
		WHERE BC.[Status] = 1 AND BC.OrganizationId = @PrincipalOrganizationId

		-- Count available records
		-- It is just to count a number of records, not need to select all columns
		SELECT @RowCount = COUNT(t1.Id)
				FROM
				(
					SELECT PO.Id AS [Id],
						COUNT(POL.Id) AS [ItemsCount]
					FROM [PurchaseOrders] PO WITH (NOLOCK)
					INNER JOIN [PurchaseOrderContacts] POC WITH (NOLOCK) ON PO.Id = POC.PurchaseOrderId AND POC.OrganizationId = @PrincipalOrganizationId AND POC.OrganizationRole = 'Principal'
					INNER JOIN POLineItems POL WITH (NOLOCK) ON PO.id = POL.PurchaseOrderId
					-- Searching column
					CROSS APPLY (
								SELECT TOP(1)
									CASE 
										WHEN @IsAllowShowAdditionalInforPOListing = 1 AND POL1.GridValue IS NOT NULL AND POL1.GridValue <> '' THEN CONCAT(PO.[PONumber], ' - ', POL1.ProductCode, ' - ', POL1.GridValue)
										WHEN @IsAllowShowAdditionalInforPOListing = 1 AND (POL1.GridValue IS NULL OR POL1.GridValue = '') THEN CONCAT(PO.[PONumber], ' - ', POL1.ProductCode)
										ELSE PO.PONumber
									END AS PONumber
								FROM POLineItems POL1 WITH (NOLOCK)
								WHERE PO.Id = POL1.PurchaseOrderId
					) t
					WHERE PO.[Status] = 1
						AND ( @SearchTerm IS NULL OR @SearchTerm = '' OR t.[PONumber] LIKE '%' + @SearchTerm + '%') 
						AND POL.BalanceUnitQty > 0
						-- Check PO Type, must be single or from buyer compliance
						AND (PO.POType = 10 OR PO.POType = @SelectedPO_POType)
						-- If the selected customer has been enabled Progress Check in the Compliance Setting, system will display list of POs which has Production Started = Yes only
						AND (
							@IsProgressCheckCRD = 0 
							OR NOT (@IsProgressCheckCRD = 1 AND @IsCompulsory = 1 AND PO.ProductionStarted = 0) 
							)
					GROUP BY PO.Id
					HAVING COUNT(POL.Id) > 0
				) t1
			
		-- Return data here
		-- PLEASE make sure order of column is matched to C# mapping
		SELECT t1.[Id], t1.[ItemsCount], t3.[PONumber], CAST(@RowCount AS BIGINT) AS [RecordCount]
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
				GROUP BY PO.Id
				HAVING COUNT(POL.Id) > 0
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
			-- Searching column
			CROSS APPLY (
						SELECT TOP(1)
							CASE 
								WHEN @IsAllowShowAdditionalInforPOListing = 1 AND POL1.GridValue IS NOT NULL AND POL1.GridValue <> '' THEN CONCAT(t2.[PONumber], ' - ', POL1.ProductCode, ' - ', POL1.GridValue)
								WHEN @IsAllowShowAdditionalInforPOListing = 1 AND (POL1.GridValue IS NULL OR POL1.GridValue = '') THEN CONCAT(t2.[PONumber], ' - ', POL1.ProductCode)
								ELSE t2.PONumber
							END AS PONumber
						FROM POLineItems POL1 WITH (NOLOCK)
						WHERE t1.Id = POL1.PurchaseOrderId
			) t3
		WHERE (@SearchTerm IS NULL OR @SearchTerm = '' OR t3.[PONumber] LIKE '%' + @SearchTerm + '%')
		ORDER BY Id
		OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY
	END
END
GO
