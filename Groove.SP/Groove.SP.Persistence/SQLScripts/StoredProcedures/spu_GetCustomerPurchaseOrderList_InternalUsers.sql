SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetCustomerPurchaseOrderList_InternalUsers', 'P') IS NOT NULL
DROP PROC dbo.spu_GetCustomerPurchaseOrderList_InternalUsers
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 06 August 2020
-- Description:	This method to get list of purchase order for tab Customer PO on Booking
-- It should work only for external users
-- Params: SearchType = C (Customer PO) or P (Product Code)
-- =============================================
CREATE PROCEDURE spu_GetCustomerPurchaseOrderList_InternalUsers
	@CustomerOrganizationId BIGINT,
	@CustomerOrganizationCode NVARCHAR(50),
	@SearchType CHAR(1) = 'C',
	@SearchTerm NVARCHAR(255) = NULL,
	@SupplierOrganizationId BIGINT,
	@SupplierCompanyName NVARCHAR(250) = '',
	@SelectedPOId BIGINT = 0,
	@SelectedPOType INT = 0,
	@Skip INT = 0,
	@Take INT = 1000

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	--SET @CustomerOrganizationId = 456;
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
	DECLARE  @SelectedPO_ExWorkDate DATETIME2;


	-- Full list of PO matched
	DECLARE @POIdTable TABLE (
		[Id] BIGINT
	)

	-- After applied paging
	DECLARE @POIdTablePagging TABLE (
		[Id] BIGINT
	)

	-- Store data for Container Type of PO
	DECLARE @EquipmentTypeTable TABLE (Id INT NOT NULL, Code VARCHAR(10) NOT NULL)
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
	DECLARE @IsCompulsory BIT;
	DECLARE @BuyerComplianceId BIGINT;

	SELECT TOP(1) @BuyerComplianceId = Id, @IsProgressCheckCRD = IsProgressCargoReadyDate, @IsCompulsory = IsCompulsory
	FROM BuyerCompliances (NOLOCK)
	WHERE OrganizationId = @CustomerOrganizationId AND [Status] = 1
	
	-- If there is selected PO, get more information on PurchaseOrderVerification then make comparisons POs vs selected PO
	IF(@SelectedPOId > 0)
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
			@SelectedPO_ShipperId = t4.ShipperId,
			@SelectedPO_ConsigneeId = t3.ConsigneeId,
			@SelectedPO_SupplierId = t2.SupplierId,
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
			PO.CargoReadyDate AS [ExWorkDate]
			FROM [PurchaseOrders] PO WITH (NOLOCK)
			WHERE PO.Id = @SelectedPOId
		) t1					
		CROSS APPLY
		(
			SELECT TOP(1) sc.OrganizationId AS SupplierId
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
			INNER JOIN BuyerCompliances BC ON VER.BuyerComplianceId = BC.Id AND BC.[Status] = 1 AND BC.OrganizationId = @CustomerOrganizationId

		-- Get value for variable that defines whether PO Ex-work date checking
		SELECT @CheckPOExWorkDate = 1
		FROM BookingTimelesses BT
		WHERE
			BT.BuyerComplianceId = @BuyerComplianceId
			AND BT.DateForComparison = 10
			AND EXISTS
				(	SELECT 1
					FROM BookingPolicies BP
					WHERE BT.BuyerComplianceId = BP.BuyerComplianceId AND BP.BookingTimeless != 0
				)


		-- Store ShippingCompliances into variables
		SELECT	@AllowMultiplePOPerBooking =  SPC.AllowMultiplePOPerFulfillment
		FROM ShippingCompliances SPC WITH (NOLOCK)
			INNER JOIN BuyerCompliances BC ON SPC.BuyerComplianceId = BC.Id AND BC.[Status] = 1 AND BC.OrganizationId = @CustomerOrganizationId

		INSERT INTO @POIdTable
		SELECT  t1.Id
			FROM
				(
					SELECT	PO.Id AS [Id],
							PO.POType AS [POType],
							COUNT(POL.Id) AS [ItemsCount]
					FROM [PurchaseOrders] PO WITH (NOLOCK)
					INNER JOIN [PurchaseOrderContacts] POCP WITH (NOLOCK) ON PO.Id = POCP.PurchaseOrderId AND POCP.OrganizationId = @CustomerOrganizationId AND POCP.OrganizationRole = 'Principal'
					CROSS APPLY (
								SELECT	POL1.Id,
										POL1.BalanceUnitQty,
										CASE
											WHEN POL1.[GridValue] IS NOT NULL AND POL1.[GridValue] <> '' THEN CONCAT(POL1.[ProductCode], ' - ', POL1.GridValue)
											ELSE POL1.[ProductCode]
										END AS [ProductCode]
								FROM POLineItems POL1 WITH (NOLOCK)
								WHERE PO.Id = POL1.PurchaseOrderId
					) POL
					WHERE PO.[Status] = 1
						AND (
							@SupplierOrganizationId = 0
							OR
							EXISTS(
								SELECT 1
								FROM [PurchaseOrderContacts] POC
								WHERE POC.PurchaseOrderId = PO.Id
									AND POC.OrganizationId = @SupplierOrganizationId
									AND (POC.OrganizationRole = 'Supplier' OR POC.OrganizationRole = 'Delegation')
							)
						)
						-- try to compare org name if id is missing
						AND (
							@SupplierCompanyName = ''
							OR
							@SupplierOrganizationId <> 0
							OR
							EXISTS(
								SELECT 1
								FROM [PurchaseOrderContacts] POC
								WHERE POC.PurchaseOrderId = PO.Id
									AND POC.CompanyName = @SupplierCompanyName
									AND (POC.OrganizationRole = 'Supplier' OR POC.OrganizationRole = 'Delegation')
							)
						)
						AND PO.Stage IN (20, 30, 40, 50)
						AND (@SearchTerm IS NULL OR @SearchTerm = '' 
							OR (@SearchType = 'C' AND PO.[PONumber] LIKE '%' + @SearchTerm + '%')
							OR (@SearchType = 'P' AND POL.[ProductCode] LIKE '%' + @SearchTerm + '%')
						)
						AND POL.BalanceUnitQty > 0
						-- If the selected customer has been enabled Progress Check in the Compliance Setting, system will display list of POs which has Production Started = Yes only
						AND (
						@IsProgressCheckCRD = 0
						OR NOT (@IsProgressCheckCRD = 1 AND @IsCompulsory = 1 AND PO.ProductionStarted = 0)
						)
						AND (
							@AllowMultiplePOPerBooking = 1 OR PO.Id = @SelectedPOId
						)
					GROUP BY PO.Id, PO.POType
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
				OUTER APPLY
				(
					SELECT TOP(1) sc.OrganizationId AS SupplierId
					FROM PurchaseOrderContacts sc WITH (NOLOCK)
					WHERE t1.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
					-- Must same supplier Id
					AND sc.OrganizationId = @SelectedPO_SupplierId
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
			WHERE
				-- Check on PO type
				t1.POType = @SelectedPOType
				AND
				-- Check buyer compliance settings on purchase order verifications
				(
					((@ExpectedShipDateVerification != @WillCompareIfValue) OR ( @SelectedPO_ExpectedShipDate = t2.ExpectedShipDate))
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
			
		SET @RowCount = @@ROWCOUNT

	END
	ELSE
	BEGIN

		INSERT INTO @POIdTable
		SELECT  t1.Id
		FROM
			(
				SELECT PO.Id AS [Id],
					COUNT(POL.Id) AS [ItemsCount]
				FROM [PurchaseOrders] PO WITH (NOLOCK)
				INNER JOIN [PurchaseOrderContacts] POCP WITH (NOLOCK) ON PO.Id = POCP.PurchaseOrderId AND POCP.OrganizationId = @CustomerOrganizationId AND POCP.OrganizationRole = 'Principal'
				CROSS APPLY (
								SELECT	POL1.Id,
										POL1.BalanceUnitQty,
										CASE
											WHEN POL1.[GridValue] IS NOT NULL AND POL1.[GridValue] <> '' THEN CONCAT(POL1.[ProductCode], ' - ', POL1.GridValue)
											ELSE POL1.[ProductCode]
										END AS [ProductCode]
								FROM POLineItems POL1 WITH (NOLOCK)
								WHERE PO.Id = POL1.PurchaseOrderId
							) POL
				WHERE PO.[Status] = 1
					AND (
						@SupplierOrganizationId = 0
						OR
						EXISTS(
							SELECT 1
							FROM [PurchaseOrderContacts] POC
							WHERE POC.PurchaseOrderId = PO.Id
								AND POC.OrganizationId = @SupplierOrganizationId
								AND (POC.OrganizationRole = 'Supplier' OR POC.OrganizationRole = 'Delegation')
						)
					)
					-- try to compare org name if id is missing
					AND (
						@SupplierCompanyName = ''
						OR
						@SupplierOrganizationId <> 0
						OR
						EXISTS(
							SELECT 1
							FROM [PurchaseOrderContacts] POC
							WHERE POC.PurchaseOrderId = PO.Id
								AND POC.CompanyName = @SupplierCompanyName
								AND (POC.OrganizationRole = 'Supplier' OR POC.OrganizationRole = 'Delegation')
							)
						)
					-- Check on PO type
					AND PO.POType = @SelectedPOType
					AND PO.Stage IN (20, 30, 40, 50)
					AND (@SearchTerm IS NULL OR @SearchTerm = '' 
							OR (@SearchType = 'C' AND PO.[PONumber] LIKE '%' + @SearchTerm + '%')
							OR (@SearchType = 'P' AND POL.[ProductCode] LIKE '%' + @SearchTerm + '%')
						)
					AND POL.BalanceUnitQty > 0
					-- If the selected customer has been enabled Progress Check in the Compliance Setting, system will display list of POs which has Production Started = Yes only
					AND (
						@IsProgressCheckCRD = 0 
						OR NOT (@IsProgressCheckCRD = 1 AND @IsCompulsory = 1 AND PO.ProductionStarted = 0)
						)
				GROUP BY PO.id
				HAVING COUNT(POL.id) > 0
			) t1	
		
		SET @RowCount = @@ROWCOUNT	
	END

	INSERT INTO @POIdTablePagging
		SELECT Id
		FROM @POIdTable
		ORDER BY Id
		OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY

		-- Return data here, there are 3 data sets returned

		-- 1. Purchase Order
		
		SELECT 
			T1.CargoReadyDate AS [CargoReadyDate],
			T1.CarrierCode AS [CarrierCode],
			T1.CarrierName AS [CarrierName],
			T1.ContainerType AS [ContainerType],
			T1.ExpectedDeliveryDate AS [ExpectedDeliveryDate],
			T1.ExpectedShipDate AS [ExpectedShipDate],
			T1.Id AS [Id],
			T1.Incoterm AS [Incoterm],
			T1.ModeOfTransport AS [ModeOfTransport],
			T1.PONumber AS [PONumber],
			T1.ShipFrom AS [ShipFrom],
			T1.ShipFromId AS [ShipFromId],
			T1.ShipTo AS [ShipTo],
			T1.ShipToId AS [ShipToId],
			T1.[Status] AS [Status],
			T1.POType AS [POType],
			@RowCount AS [RowCount]

		FROM
		(
			SELECT 
				PO.CargoReadyDate,
				PO.CarrierCode,
				PO.CarrierName,
				PO.ContainerType AS [ContainerType],
				PO.ExpectedDeliveryDate,
				PO.ExpectedShipDate,
				PO.Id,
				PO.Incoterm,
				PO.ModeOfTransport,
				PO.PONumber,
				PO.ShipFromId,
				PO.ShipFrom,
				PO.ShipToId,
				PO.ShipTo,
				PO.[Status],
				PO.POType

			FROM PurchaseOrders PO WITH (NOLOCK)
			INNER JOIN @POIdTablePagging POP ON PO.Id = POP.Id
			--LEFT JOIN @EquipmentTypeTable ET ON ET.Id = PO.ContainerType
		) T1

		-- 2. Purchase Order Contacts
		SELECT
				POC.Id AS [Id],
				POC.PurchaseOrderId AS [PurchaseOrderId],
				POC.OrganizationId AS [OrganizationId],
				POC.OrganizationCode AS [OrganizationCode],
				POC.OrganizationRole AS [OrganizationRole],				
				POC.CompanyName AS [CompanyName],
				POC.AddressLine1 AS [AddressLine1],
				POC.AddressLine2 AS [AddressLine2],
				POC.AddressLine3 AS [AddressLine3],
				POC.AddressLine4 AS [AddressLine4],
				POc.Department AS [Department],
				POC.ContactName AS [ContactName],
				POC.[Name] AS [Name],
				POC.ContactNumber AS [ContactNumber],
				POC.ContactEmail AS [ContactEmail],				
				POC.[References] AS [References]			
						
		FROM PurchaseOrderContacts POC WITH (NOLOCK)
		INNER JOIN @POIdTablePagging POP ON POC.PurchaseOrderId = POP.Id

		-- 3. Purchase Order Line items
		SELECT
				POL.Id AS [Id],
				POL.PurchaseOrderId AS [PurchaseOrderId],
				POL.BalanceUnitQty AS [BalanceUnitQty],
				POL.BookedUnitQty AS [BookedUnitQty],
				POL.Commodity AS [Commodity],
				POL.CountryCodeOfOrigin AS [CountryCodeOfOrigin],
				POL.CurrencyCode AS [CurrencyCode],
				POL.DescriptionOfGoods AS [DescriptionOfGoods],
				POL.HSCode AS [HSCode],
				NULL AS [ChineseDescription],
				POL.LineOrder AS [LineOrder],
				POL.OrderedUnitQty AS [OrderedUnitQty],
				POL.PackageUOM AS [PackageUOM],
				POL.ProductCode AS [ProductCode],
				POL.GridValue as [GridValue],
				POL.ProductName AS [ProductName],
				POL.UnitPrice AS [UnitPrice],
				POL.UnitUOM AS [UnitUOM],
				POL.ShippingMarks AS [ShippingMarks],

				AM.OuterDepth AS [OuterDepth], 
				AM.OuterHeight AS [OuterHeight], 
				AM.OuterWidth AS [OuterWidth], 
				AM.OuterQuantity AS [OuterQuantity],
				AM.InnerQuantity AS [InnerQuantity], 
				AM.OuterGrossWeight AS [OuterGrossWeight]

		FROM POLineItems POL WITH (NOLOCK)
		INNER JOIN @POIdTablePagging POP ON POL.PurchaseOrderId = POP.Id
		LEFT JOIN ArticleMaster AM WITH (NOLOCK) ON TRIM(AM.ItemNo) = POL.ProductCode AND AM.CompanyCode = @CustomerOrganizationCode

END
GO
