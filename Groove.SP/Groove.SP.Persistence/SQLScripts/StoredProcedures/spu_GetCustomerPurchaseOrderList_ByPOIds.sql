SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetCustomerPurchaseOrderList_ByPOIds', 'P') IS NOT NULL
DROP PROC dbo.spu_GetCustomerPurchaseOrderList_ByPOIds
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 16 November 2020
-- Description:	This method to get list of purchase order for tab Customer PO on Booking by purchase order ids
-- =============================================
CREATE PROCEDURE spu_GetCustomerPurchaseOrderList_ByPOIds
	@purchaseOrderIds NVARCHAR(MAX),
	@customerOrganizationCode NVARCHAR(35),
	@preferredOrganizationId BIGINT = -1

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	-- SET @purchaseOrderIds = N'11039,11040';
	-- SET @customerOrganizationCode = N'MLS-PRIN01';

	-- Variables
	DECLARE @purchaseOrderIdTable TABLE (Id BIGINT NOT NULL)

	-- Store valid purchase order ids
	INSERT INTO @purchaseOrderIdTable
	SELECT DISTINCT [Value] 
	FROM PurchaseOrders PO WITH(NOLOCK)
	INNER JOIN (SELECT DISTINCT CAST([Value] AS BIGINT) AS [Value] FROM fn_SplitStringToTable(@purchaseOrderIds, ',')) TMP ON PO.Id = TMP.[Value]
	WHERE PO.[Status] = 1 AND (PO.[Stage] = 20 OR PO.[Stage] = 30 OR PO.[Stage] = 40 OR PO.[Stage] = 50)

	-- Fetch information for purchase order
	SELECT PO.[Id], 
		PO.[CargoReadyDate], 
		PO.[CarrierCode], 
		PO.[CarrierName], 
		PO.[ContainerType], 
		PO.[ExpectedDeliveryDate], 
		PO.[ExpectedShipDate], 
		PO.[Incoterm], 
		PO.[ModeOfTransport], 
		PO.[PONumber], 
		PO.[POType], 
		PO.[ShipFrom], 
		PO.[ShipFromId], 
		PO.[ShipTo], 
		PO.[ShipToId], 
		PO.[Status]
	FROM [PurchaseOrders] AS PO WITH(NOLOCK)
	WHERE PO.[Id] IN (SELECT Id FROM @purchaseOrderIdTable)
	ORDER BY PO.Id
	
	-- Fetch information for purchase order contacts
	SELECT POC.[Id], 
		POC.[OrganizationId], 
		POC.[OrganizationRole],
		POC.[OrganizationCode],
		POC.CompanyName,
		POC.ContactName,
		POC.ContactNumber,
		POC.ContactEmail,
		POC.AddressLine1,
		POC.AddressLine2,
		POC.AddressLine3,
		POC.AddressLine4,
		POC.[PurchaseOrderId]
	FROM [PurchaseOrderContacts] POC WITH(NOLOCK)
	WHERE POC.PurchaseOrderId IN (SELECT Id FROM @purchaseOrderIdTable)
	ORDER BY POC.PurchaseOrderId, POC.Id

	-- Fetch information for purchase order line items
	SELECT POL.[Id], 
		POL.[BalanceUnitQty], 
		POL.[BookedUnitQty], 
		POL.[Commodity], 
		POL.[CountryCodeOfOrigin],  
		POL.[CurrencyCode], 
		POL.[DescriptionOfGoods], 
		ISNULL(OP.[HSCode], POL.[HSCode]) AS [HSCode],
		OP.[ChineseDescription] AS [ChineseDescription],
		POL.[LineOrder], 
		POL.[OrderedUnitQty], 
		POL.[PackageUOM],
		POL.[ProductCode], 
		POL.[ProductName], 
		POL.[PurchaseOrderId],
		POL.[ShippingMarks], 
		POL.[UnitPrice], 
		POL.[UnitUOM],
		POL.[GridValue],
		AM.[OuterDepth],
		AM.[OuterHeight],
		AM.[OuterWidth], 
		AM.[OuterQuantity], 
		AM.[InnerQuantity], 
		AM.[OuterGrossWeight]
	FROM [POLineItems] POL WITH(NOLOCK)
	LEFT JOIN ArticleMaster AM WITH(NOLOCK) ON POL.ProductCode = TRIM(AM.ItemNo) AND AM.CompanyCode = @customerOrganizationCode
	LEFT JOIN OrganizationPreferences OP WITH(NOLOCK) ON OP.OrganizationId = @preferredOrganizationId AND OP.ProductCode = POL.ProductCode
	WHERE POL.PurchaseOrderId IN (SELECT Id FROM @purchaseOrderIdTable)
	ORDER BY POL.PurchaseOrderId, POL.Id

END
GO

