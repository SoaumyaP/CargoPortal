SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_ProceedPurchaseOrderAdhocChanges', 'P') IS NOT NULL
DROP PROC spu_ProceedPurchaseOrderAdhocChanges
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 01 July 2020
-- Description:	Mark some ad-hoc changes on PO which may impact to Booking
-- Notes: 
-- 1. With datatype nvarchar/varchar/char, we treat NULL == '' (blank/empty)
-- 2. [Booking] Notification: Removing/Adding PO items should trigger Case2 instead of Case1
-- 3. [PO]: not trigger Case2 for PO Issue Date
-- 4. [PO]: Case3 for [ContainerType]
-- =============================================
CREATE PROCEDURE [dbo].[spu_ProceedPurchaseOrderAdhocChanges]
	@PurchaseOrderId BIGINT,
	@BuyerComplianceId BIGINT = 0,
	@PrincipalOrganizationId BIGINT = 0,
	@JsonNewValue NVARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	--DECLARE @PurchaseOrderId BIGINT;
	--DECLARE @BuyerComplianceId BIGINT;
	--DECLARE @PrincipalOrganizationId BIGINT;
	--DECLARE @JsonNewValue NVARCHAR(MAX);


	--SET @PurchaseOrderId = 242;
	--SET @BuyerComplianceId = 6;
	--SET @JsonNewValue = N'
	--{"Id":242,"POKey":"MLS-PRIN01CDD-70101","PONumber":"CDD-701014","CarrierCode":"","EarliestDeliveryDate":"2020-06-01T00:00:00","EarliestShipDate":"2020-05-01T00:00:00","ExpectedDeliveryDate":"2020-06-01T00:00:00","ExpectedShipDate":"2020-05-01T00:00:00","GatewayCode":"","Incoterm":"EXW","LatestDeliveryDate":"2020-06-01T00:00:00","LatestShipDate":"2020-05-01T00:00:00","ModeOfTransport":"Sea","NumberOfLineItems":4,"POIssueDate":"2020-04-24T00:00:00","CustomerReferences":"Cus. Ref. 1","Department":"Dep. 1","Season":"Fall 1","ShipFrom":"NINGBO","ShipFromId":127,"ShipTo":"HONG KONG","ShipToId":291,"PaymentCurrencyCode":"USD","PaymentTerms":"Duno 12","Status":1,"Stage":20,"CargoReadyDate":"2020-03-20T00:00:00","PORemark":"no po remark","POTerms":"no po terms","HazardousMaterialsInstruction":"no in.","SpecialHandlingInstruction":"no handling","CarrierName":"no carrier","GatewayName":"Hong Kong","ShipFromName":"no name","ShipToName":"no name","NotifyUserId":null,"ContainerType":10,"Contacts":[{"Id":0,"PurchaseOrderId":0,"OrganizationId":462,"OrganizationCode":"MLS-SUP1","OrganizationRole":"Supplier","CompanyName":"MLS Supplier 1","AddressLine1":null,"AddressLine2":"Ham Nghi Road","AddressLine3":null,"AddressLine4":null,"Department":null,"ContactName":"Melissa Sup1","Name":null,"ContactNumber":"0123456789","ContactEmail":"testingmainguyen+sup1@gmail.com","References":null,"PurchaseOrder":null,"RowVersion":null,"CreatedBy":null,"CreatedDate":"0001-01-01T00:00:00","UpdatedBy":null,"UpdatedDate":null},{"Id":0,"PurchaseOrderId":0,"OrganizationId":462,"OrganizationCode":"MLS-SUP1","OrganizationRole":"Shipper","CompanyName":"MLS Supplier 1","AddressLine1":null,"AddressLine2":"Ham Nghi Road","AddressLine3":null,"AddressLine4":null,"Department":null,"ContactName":"Melissa Sup1","Name":null,"ContactNumber":"0123456789","ContactEmail":"testingmainguyen+sup1@gmail.com","References":null,"PurchaseOrder":null,"RowVersion":null,"CreatedBy":null,"CreatedDate":"0001-01-01T00:00:00","UpdatedBy":null,"UpdatedDate":null},{"Id":0,"PurchaseOrderId":0,"OrganizationId":461,"OrganizationCode":"MLS-PRIN01","OrganizationRole":"Principal","CompanyName":"MLS Principal 1","AddressLine1":null,"AddressLine2":"Ham Nghi Road","AddressLine3":null,"AddressLine4":null,"Department":null,"ContactName":"Melissa","Name":null,"ContactNumber":"0123456789","ContactEmail":"testingmainguyen+prin1@email.com","References":null,"PurchaseOrder":null,"RowVersion":null,"CreatedBy":null,"CreatedDate":"0001-01-01T00:00:00","UpdatedBy":null,"UpdatedDate":null},{"Id":0,"PurchaseOrderId":0,"OrganizationId":461,"OrganizationCode":"MLS-PRIN01","OrganizationRole":"Consignee","CompanyName":"MLS Principal 1","AddressLine1":null,"AddressLine2":"Ham NghiRoad","AddressLine3":null,"AddressLine4":null,"Department":null,"ContactName":"Melissa","Name":null,"ContactNumber":"0123456789","ContactEmail":"testingmainguyen+prin1@email.com","References":null,"PurchaseOrder":null,"RowVersion":null,"CreatedBy":null,"CreatedDate":"0001-01-01T00:00:00","UpdatedBy":null,"UpdatedDate":null}],"LineItems":[{"Id":806,"POLineKey":"3062023","PurchaseOrderId":242,"LineOrder":10,"OrderedUnitQty":4000,"BookedUnitQty":4000,"BalanceUnitQty":0,"ProductCode":"3062023","ProductName":"Product 004","UnitUOM":10,"PackageUOM":10,"ProductFamily":"Electronic","HSCode":"HS004","SupplierProductCode":"no supplier product code","MinPackageQty":0,"MinOrderQty":0,"UnitPrice":130.0,"CurrencyCode":"USD","Commodity":"Household Textiles","CountryCodeOfOrigin":"AU","ReferenceNumber1":"ref 1","ReferenceNumber2":"ref 2","ShippingMarks":"no shipmark","DescriptionOfGoods":"no des goods","PackagingInstruction":"no pack in.","ProductRemark":"no remark","RowVersion":"AAAAAAABBV0=","CreatedBy":"logistics.testing@groovetechnology.com","CreatedDate":"2020-07-01T02:56:00.3933365","UpdatedBy":"logistics.testing@groovetechnology.com","UpdatedDate":"2020-07-01T03:33:42.47389Z"},{"Id":808,"POLineKey":"3062021","PurchaseOrderId":242,"LineOrder":30,"OrderedUnitQty":3000,"BookedUnitQty":3000,"BalanceUnitQty":0,"ProductCode":"3062021","ProductName":"Product 002","UnitUOM":10,"PackageUOM":10,"ProductFamily":"Electronic","HSCode":"HS002","SupplierProductCode":"no supplier product code","MinPackageQty":0,"MinOrderQty":0,"UnitPrice":300.0,"CurrencyCode":"USD","Commodity":"Electrical Goods","CountryCodeOfOrigin":"HK","ReferenceNumber1":"ref 1","ReferenceNumber2":"ref 2","ShippingMarks":"no shipmark","DescriptionOfGoods":"no des goods","PackagingInstruction":"no pack in.","ProductRemark":"no remark","RowVersion":"AAAAAAABBV8=","CreatedBy":"logistics.testing@groovetechnology.com","CreatedDate":"2020-07-01T02:56:00.3933361","UpdatedBy":"logistics.testing@groovetechnology.com","UpdatedDate":"2020-07-01T03:33:42.4738902Z"},{"Id":809,"POLineKey":"3062020","PurchaseOrderId":242,"LineOrder":40,"OrderedUnitQty":1300,"BookedUnitQty":1300,"BalanceUnitQty":0,"ProductCode":"3062020","ProductName":"Product 001","UnitUOM":10,"PackageUOM":10,"ProductFamily":"Electronic","HSCode":"HS001","SupplierProductCode":"no supplier product code","MinPackageQty":0,"MinOrderQty":0,"UnitPrice":130.0,"CurrencyCode":"USD","Commodity":"Household Textiles","CountryCodeOfOrigin":"CN","ReferenceNumber1":"ref 1","ReferenceNumber2":"ref 2","ShippingMarks":"no shipmark","DescriptionOfGoods":"no des goods","PackagingInstruction":"no pack in.","ProductRemark":"no remark","RowVersion":"AAAAAAABBWA=","CreatedBy":"logistics.testing@groovetechnology.com","CreatedDate":"2020-07-01T02:56:00.3933335","UpdatedBy":"logistics.testing@groovetechnology.com","UpdatedDate":"2020-07-01T03:33:42.4738903Z"},{"Id":810,"POLineKey":"3062022","PurchaseOrderId":242,"LineOrder":20,"OrderedUnitQty":3000,"BookedUnitQty":3000,"BalanceUnitQty":0,"ProductCode":"3062022","ProductName":"Product 003","UnitUOM":10,"PackageUOM":10,"ProductFamily":"Electronic","HSCode":"HS003","SupplierProductCode":"no supplier product code","MinPackageQty":0,"MinOrderQty":0,"UnitPrice":300.0,"CurrencyCode":"USD","Commodity":"Household Textiles","CountryCodeOfOrigin":"AU","ReferenceNumber1":"ref 1","ReferenceNumber2":"ref 2","ShippingMarks":"no shipmark","DescriptionOfGoods":"no des goods","PackagingInstruction":"no pack in.","ProductRemark":"no remark","RowVersion":"AAAAAAABBWI=","CreatedBy":"logistics.testing@groovetechnology.com","CreatedDate":"2020-07-01T02:56:00.3933363","UpdatedBy":"logistics.testing@groovetechnology.com","UpdatedDate":"2020-07-01T03:33:42.4738905Z"}],"RowVersion":"AAAAAAABBYU=","CreatedBy":"logistics.testing@groovetechnology.com","CreatedDate":"2020-07-01T02:56:00.3928804","UpdatedBy":"logistics.testing@groovetechnology.com","UpdatedDate":"2020-07-01T03:33:42.4738877Z"}
	--';

	-- Variables

	DECLARE @CompareNotAllowOverride INT;
	SET @CompareNotAllowOverride = 10;

	-- General settings
	DECLARE @AllowToBookIn INT;

	-- PO verification settings
	DECLARE @ExpectedShipDateVerification INT;
	DECLARE @ExpectedDeliveryDateVerification INT;
	DECLARE @ShipperVerification INT;
	DECLARE @ConsigneeVerification INT;
	DECLARE @ShipFromLocationVerification INT;
	DECLARE @ShipToLocationVerification INT;
	DECLARE @PaymentTermsVerification INT;
	DECLARE @PaymentCurrencyVerification INT;
	DECLARE @ModeOfTransportVerification INT;
	DECLARE @IncotermVerification INT;
	DECLARE @MovementTypeVerification INT;
	DECLARE @PreferredCarrierVerification INT;
	

	-- Product verification settings
	DECLARE @ProductCodeVerification INT;
	DECLARE @CommodityVerification INT;
	DECLARE @HSCodeVerification INT;
	DECLARE @CountryOfOriginVerification INT;


	DECLARE @JSonCurrentValue NVARCHAR(MAX);

	-- Result
	DECLARE @BookingsTable TABLE (
		[Id] BIGINT NOT NULL
	)

	DECLARE @PurchaseOrderAdhocChangesTable TABLE (
		[PurchaseOrderId] [bigint] NULL,
		[Priority] [int] NOT NULL
	)

	-- Tables to contain all columns which need to be compared.
	DECLARE @PurchaseOrdersTable TABLE (
		[POKey] [varchar](612) NOT NULL,
		[PONumber] [varchar](512) NOT NULL,
		[POIssueDate] [datetime2](7) NULL,
		[CargoReadyDate] [datetime2](7) NULL,
		[Incoterm] [varchar](3) NULL,
		[NumberOfLineItems] [bigint] NULL,
		[CustomerReferences] [nvarchar](512) NULL,
		[Department] [nvarchar](512) NULL,
		[Season] [nvarchar](512) NULL,
		[PaymentTerms] [nvarchar](512) NULL,
		[ModeOfTransport] [nvarchar](max) NULL,
		[CarrierCode] [nvarchar](128) NULL,
		[GatewayCode] [nvarchar](50) NULL,
		[ShipFrom] [nvarchar](512) NULL,
		[ShipTo] [nvarchar](512) NULL,
		[PaymentCurrencyCode] [nvarchar](16) NULL,
		[EarliestDeliveryDate] [datetime2](7) NULL,
		[EarliestShipDate] [datetime2](7) NULL,
		[ExpectedDeliveryDate] [datetime2](7) NULL,
		[ExpectedShipDate] [datetime2](7) NULL,
		[LatestDeliveryDate] [datetime2](7) NULL,
		[LatestShipDate] [datetime2](7) NULL,		
		[PORemark] [nvarchar](max) NULL,
		[POTerms] [nvarchar](512) NULL,
		[HazardousMaterialsInstruction] [varchar](max) NULL,
		[SpecialHandlingInstruction] [nvarchar](max) NULL,
		[CarrierName] [nvarchar](512) NULL,
		[GatewayName] [nvarchar](512) NULL,
		[ShipFromName] [nvarchar](512) NULL,
		[ShipToName] [nvarchar](512) NULL,
		[ContainerType] [int] NULL,
		[Status] [int] NULL,
		[POType] [int] NOT NULL,
		[BlanketPOId] [bigint] NULL,
		--N = New value, C = Current data
		[Source] CHAR(1) NULL
	)
	DECLARE @PurchaseOrderContactsTable TABLE (
		[OrganizationCode] [nvarchar](35) NOT NULL,
		[OrganizationRole] [varchar](50) NOT NULL,
		--N = New value, C = Current data
		[Source] CHAR(1) NULL
	)
	DECLARE @PurchaseOrderLineItemsTable TABLE (
		[Id] [bigint] NOT NULL,
		[POLineKey] [varchar](750) NOT NULL,
		[LineOrder] [int] NULL,
		[OrderedUnitQty] [int] NOT NULL,
		[ProductCode] [nvarchar](128) NULL,
		[ProductName] [nvarchar](max) NULL,
		[UnitUOM] [int] NOT NULL,
		[UnitPrice] [decimal](18, 4) NOT NULL,
		[CurrencyCode] [nvarchar](16) NULL,		
		[ProductFamily] [nvarchar](max) NULL,
		[HSCode] [nvarchar](128) NULL,
		[SupplierProductCode] [nvarchar](128) NULL,
		[MinPackageQty] [int] NULL,
		[MinOrderQty] [int] NULL,		
		[PackageUOM] [int] NULL,
		[CountryCodeOfOrigin] [nvarchar](128) NULL,
		[Commodity] [nvarchar](128) NULL,		
		[ReferenceNumber1] [nvarchar](128) NULL,
		[ReferenceNumber2] [nvarchar](128) NULL,
		[ShippingMarks] [nvarchar](max) NULL,
		[DescriptionOfGoods] [nvarchar](max) NULL,
		[PackagingInstruction] [nvarchar](max) NULL,
		[ProductRemark] [nvarchar](max) NULL,
		--N = New value, C = Current data
		[Source] CHAR(1) NULL
	)

	-- Store current data of PO
	INSERT INTO @PurchaseOrdersTable
	SELECT 
		[POKey],
		[PONumber],
		[POIssueDate],
		[CargoReadyDate],
		[Incoterm] [varchar],
		[NumberOfLineItems],
		[CustomerReferences],
		[Department],
		[Season],
		[PaymentTerms],
		[ModeOfTransport],
		[CarrierCode],
		[GatewayCode],
		[ShipFrom],
		[ShipTo],
		[PaymentCurrencyCode],
		[EarliestDeliveryDate],
		[EarliestShipDate],
		[ExpectedDeliveryDate],
		[ExpectedShipDate],
		[LatestDeliveryDate],
		[LatestShipDate],		
		[PORemark],
		[POTerms],
		[HazardousMaterialsInstruction],
		[SpecialHandlingInstruction],
		[CarrierName],
		[GatewayName],
		[ShipFromName],
		[ShipToName],
		[ContainerType],
		[Status],
		[POType],
		[BlanketPOId],
		NULL
	FROM PurchaseOrders WITH(NOLOCK) 
	WHERE Id = @PurchaseOrderId

	INSERT INTO @PurchaseOrderContactsTable
	SELECT 
		[OrganizationCode],
		[OrganizationRole],
		NULL
	FROM PurchaseOrderContacts WITH(NOLOCK)
	WHERE PurchaseOrderId = @PurchaseOrderId 

	INSERT INTO @PurchaseOrderLineItemsTable
	SELECT
		[Id],
		[POLineKey],
		[LineOrder],
		[OrderedUnitQty],
		[ProductCode],
		[ProductName],
		[UnitUOM],
		[UnitPrice],
		[CurrencyCode],		
		[ProductFamily],
		[HSCode] [nvarchar],
		[SupplierProductCode],
		[MinPackageQty],
		[MinOrderQty],		
		[PackageUOM],
		[CountryCodeOfOrigin],
		[Commodity],		
		[ReferenceNumber1],
		[ReferenceNumber2],
		[ShippingMarks],
		[DescriptionOfGoods],
		[PackagingInstruction],
		[ProductRemark],
		NULL
	FROM POLineItems WITH(NOLOCK)
	WHERE PurchaseOrderId = @PurchaseOrderId 	


	SET @JSonCurrentValue = CONCAT('{"PO":', (SELECT * FROM @PurchaseOrdersTable FOR JSON AUTO));
	SET @JSonCurrentValue = CONCAT(@JSonCurrentValue, ',"PurchaseOrderContacts":', (SELECT * FROM @PurchaseOrderContactsTable FOR JSON AUTO));
	SET @JSonCurrentValue = CONCAT(@JSonCurrentValue, ',"PurchaseOrderLineItems":', (SELECT * FROM @PurchaseOrderLineItemsTable FOR JSON AUTO));
	SET @JSonCurrentValue = CONCAT(@JSonCurrentValue, '}');	

	-- SET [Source] as current data
	UPDATE @PurchaseOrdersTable
	SET [Source] = 'C'
	UPDATE @PurchaseOrderContactsTable
	SET [Source] = 'C'
	UPDATE @PurchaseOrderLineItemsTable
	SET [Source] = 'C'
	
	-- In case @PrincipalOrganizationId = 0 AND @BuyerComplianceId = 0, try to get Principal organzation of PO
	IF (@PrincipalOrganizationId = 0 AND @BuyerComplianceId = 0)
	BEGIN
		SELECT TOP(1) @PrincipalOrganizationId = POC.OrganizationId
		FROM PurchaseOrderContacts POC WITH (NOLOCK)
		WHERE POC.PurchaseOrderId = @PurchaseOrderId AND POC.OrganizationRole = 'Principal'
	END

	-- Store POVerifications into variables
	SELECT TOP(1) @ExpectedShipDateVerification =  VER.ExpectedShipDateVerification,
			@ExpectedDeliveryDateVerification = VER.ExpectedDeliveryDateVerification,
			@ShipperVerification = VER.ShipperVerification,
			@ConsigneeVerification = VER.ConsigneeVerification,
			@ShipFromLocationVerification = VER.ShipFromLocationVerification,
			@ShipToLocationVerification = VER.ShipToLocationVerification,
			@PaymentTermsVerification = VER.PaymentTermsVerification,
			@PaymentCurrencyVerification = VER.PaymentCurrencyVerification,
			@ModeOfTransportVerification = VER.ModeOfTransportVerification,
			@IncotermVerification = VER.IncotermVerification,
			@MovementTypeVerification = VER.MovementTypeVerification,
			@PreferredCarrierVerification = VER.PreferredCarrierVerification,
			@AllowToBookIn = BC.AllowToBookIn
	FROM PurchaseOrderVerificationSettings VER WITH (NOLOCK)
		INNER JOIN BuyerCompliances BC ON VER.BuyerComplianceId = BC.Id AND BC.[Status] = 1 AND (BC.OrganizationId = @PrincipalOrganizationId OR BC.Id = @BuyerComplianceId)

	SELECT TOP(1) @ProductCodeVerification =  VER.ProductCodeVerification,
			@CommodityVerification = VER.CommodityVerification,
			@HSCodeVerification = VER.HsCodeVerification,
			@CountryOfOriginVerification = VER.CountryOfOriginVerification
	FROM ProductVerificationSettings VER WITH (NOLOCK)
		INNER JOIN BuyerCompliances BC ON VER.BuyerComplianceId = BC.Id AND BC.[Status] = 1 AND (BC.OrganizationId = @PrincipalOrganizationId OR BC.Id = @BuyerComplianceId)


	-- Parse JsonNewValue to Tables	
	INSERT INTO @PurchaseOrdersTable
	SELECT *, 'N'
	FROM OPENJSON(@JsonNewValue)
	WITH (
		[POKey] [varchar](612) '$.POKey',
		[PONumber] [varchar](512) '$.PONumber',
		[POIssueDate] [datetime2](7) '$.POIssueDate',
		[CargoReadyDate] [datetime2](7) '$.CargoReadyDate',
		[Incoterm] [varchar](3) '$.Incoterm',
		[NumberOfLineItems] [bigint] '$.NumberOfLineItems',
		[CustomerReferences] [nvarchar](512) '$.CustomerReferences',
		[Department] [nvarchar](512) '$.Department',
		[Season] [nvarchar](512) '$.Season',
		[PaymentTerms] [nvarchar](512) '$.PaymentTerms',
		[ModeOfTransport] [nvarchar](max) '$.ModeOfTransport',
		[CarrierCode] [nvarchar](128) '$.CarrierCode',
		[GatewayCode] [nvarchar](50) '$.GatewayCode',
		[ShipFrom] [nvarchar](512) '$.ShipFrom',
		[ShipTo] [nvarchar](512) '$.ShipTo',
		[PaymentCurrencyCode] [nvarchar](16) '$.PaymentCurrencyCode',
		[EarliestDeliveryDate] [datetime2](7) '$.EarliestDeliveryDate',
		[EarliestShipDate] [datetime2](7) '$.EarliestShipDate',
		[ExpectedDeliveryDate] [datetime2](7) '$.ExpectedDeliveryDate',
		[ExpectedShipDate] [datetime2](7) '$.ExpectedShipDate',
		[LatestDeliveryDate] [datetime2](7) '$.LatestDeliveryDate',
		[LatestShipDate] [datetime2](7) '$.LatestShipDate',		
		[PORemark] [nvarchar](max) '$.PORemark',
		[POTerms] [nvarchar](512) '$.POTerms',
		[HazardousMaterialsInstruction] [varchar](max) '$.HazardousMaterialsInstruction',
		[SpecialHandlingInstruction] [nvarchar](max) '$.SpecialHandlingInstruction',
		[CarrierName] [nvarchar](512) '$.CarrierName',
		[GatewayName] [nvarchar](512) '$.GatewayName',
		[ShipFromName] [nvarchar](512) '$.ShipFromName',
		[ShipToName] [nvarchar](512) '$.ShipToName',
		[ContainerType] [int] '$.ContainerType',
		[Status] [int] '$.Status',
		[POType] [int] '$.POType',
		[BlanketPOId] [bigint] '$.BlanketPOId'
	);
	--SELECT * FROM @PurchaseOrdersTable

	INSERT INTO @PurchaseOrderContactsTable
	SELECT *, 'N'
	FROM OPENJSON(JSON_QUERY(@JsonNewValue,'$.Contacts'))
	WITH (
		[OrganizationCode] [nvarchar](35) '$.OrganizationCode',
		[OrganizationRole] [varchar](50) '$.OrganizationRole'
	);
	--SELECT * FROM @PurchaseOrderContactsTable
	
	INSERT INTO @PurchaseOrderLineItemsTable
	SELECT *, 'N'	
	FROM OPENJSON(JSON_QUERY(@JsonNewValue,'$.LineItems'))
	WITH (
		[Id] [bigint] '$.Id',
		[POLineKey] [varchar](750) '$.POLineKey',
		[LineOrder] [int] '$.LineOrder',
		[OrderedUnitQty] [int] '$.OrderedUnitQty',
		[ProductCode] [nvarchar](128) '$.ProductCode',
		[ProductName] [nvarchar](max) '$.ProductName',
		[UnitUOM] [int] '$.UnitUOM',
		[UnitPrice] [decimal](18, 4) '$.UnitPrice',
		[CurrencyCode] [nvarchar](16) '$.CurrencyCode',		
		[ProductFamily] [nvarchar](max) '$.ProductFamily',
		[HSCode] [nvarchar](128) '$.HSCode',
		[SupplierProductCode] [nvarchar](128) '$.SupplierProductCode',
		[MinPackageQty] [int] '$.MinPackageQty',
		[MinOrderQty] [int] '$.MinOrderQty',		
		[PackageUOM] [int] '$.PackageUOM',
		[CountryCodeOfOrigin] [nvarchar](128) '$.CountryCodeOfOrigin',
		[Commodity] [nvarchar](128) '$.Commodity',		
		[ReferenceNumber1] [nvarchar](128) '$.ReferenceNumber1',
		[ReferenceNumber2] [nvarchar](128) '$.ReferenceNumber2',
		[ShippingMarks] [nvarchar](max) '$.ShippingMarks',
		[DescriptionOfGoods] [nvarchar](max) '$.DescriptionOfGoods',
		[PackagingInstruction] [nvarchar](max) '$.PackagingInstruction',
		[ProductRemark] [nvarchar](max) '$.ProductRemark'
	);

	-- Expected Ship Date Verification -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ExpectedShipDate) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@ExpectedShipDateVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Expected Delivery Date Verification -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ExpectedDeliveryDate) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@ExpectedDeliveryDateVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Shipper Verification -> Case 1/3
	IF ( EXISTS (	
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'C' AND OrganizationRole = 'Shipper' GROUP BY OrganizationCode
		EXCEPT
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'N' AND OrganizationRole = 'Shipper' GROUP BY OrganizationCode
		)
		OR
		EXISTS (	
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'N' AND OrganizationRole = 'Shipper' GROUP BY OrganizationCode
		EXCEPT
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'C' AND OrganizationRole = 'Shipper' GROUP BY OrganizationCode
		)
	)
	BEGIN
		IF (@ShipperVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Consignee Verification -> Case 1/3
	IF ( EXISTS (	
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'C' AND OrganizationRole = 'Consignee' GROUP BY OrganizationCode
		EXCEPT
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'N' AND OrganizationRole = 'Consignee' GROUP BY OrganizationCode
		)
		OR
		EXISTS (	
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'N' AND OrganizationRole = 'Consignee' GROUP BY OrganizationCode
		EXCEPT
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'C' AND OrganizationRole = 'Consignee' GROUP BY OrganizationCode
		)
	)
	BEGIN
		IF (@ConsigneeVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END	

	-- Ship From Location Verification -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ISNULL(ShipFrom, '')) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@ShipFromLocationVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Ship To Location Verification -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ISNULL(ShipTo, '')) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@ShipToLocationVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Payment Terms Verification -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ISNULL(PaymentTerms, '')) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@PaymentTermsVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Payment Currency Verification -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ISNULL(PaymentCurrencyCode, '')) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@PaymentCurrencyVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Mode Of Transport Verification -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ISNULL(ModeOfTransport, '')) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@ModeOfTransportVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Incoterm Verification -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ISNULL(Incoterm, '')) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@IncotermVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Movement Type Verification
	-- NOT APPLY at the moment

	-- Preferred Carrier Verification  -> Case 1/3
	IF ( 1 < (SELECT COUNT (DISTINCT ISNULL(CarrierCode, '')) FROM @PurchaseOrdersTable))
	BEGIN
		IF (@PreferredCarrierVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END	

	-- Product Code Verification: Editing -> Case 1/3
	IF (EXISTS (
			SELECT 1 FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'N' AND Id > 0 AND ISNULL(ProductCode, '') NOT IN (SELECT ISNULL(ProductCode, '') FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'C')
		)
	)
	BEGIN
		IF (@ProductCodeVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END	

	-- Commodity Verification: Editing -> Case 1/3
	IF (EXISTS (
			SELECT 1 FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'N' AND Id > 0 AND ISNULL(Commodity, '') NOT IN (SELECT ISNULL(Commodity, '') FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'C')
		)
	)
	BEGIN
		IF (@CommodityVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END	

	-- HS Code Verification: Editing  -> Case 1/3
	IF (EXISTS (
			SELECT 1 FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'N' AND Id > 0 AND ISNULL(HSCode, '') NOT IN (SELECT ISNULL(HSCode, '') FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'C')
		)
	)
	BEGIN
		IF (@HSCodeVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END		

	-- Country Of Origin Verification: Editing  -> Case 1/3
	IF (EXISTS (
			SELECT 1 FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'N' AND Id > 0 AND ISNULL(CountryCodeOfOrigin, '') NOT IN (SELECT ISNULL(CountryCodeOfOrigin, '') FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'C')
		)
	)
	BEGIN
		IF (@CountryOfOriginVerification = @CompareNotAllowOverride)
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (1)
			END
		ELSE
			BEGIN
				INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
				VALUES (3)
			END
	END

	-- Purchase order line items: Removing/Adding  -> Case 2
	IF ( EXISTS (	
		SELECT Id FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'C'
		EXCEPT
		SELECT Id FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'N'
		)
		OR
		EXISTS (
		SELECT Id FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'N'
		EXCEPT
		SELECT Id FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'C'
		)
	)
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (2)
	END

	-- POType Verification -> Case 1
	IF (1 < (SELECT COUNT (DISTINCT POType) FROM @PurchaseOrdersTable))
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (1)
	END

	-- BlanketPOId Verification -> Case 1
	IF (1 < (SELECT COUNT (DISTINCT BlanketPOId) FROM @PurchaseOrdersTable))
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (1)
	END

	-- Supplier Verification  -> Case 1
	IF ( EXISTS (	
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'C' AND OrganizationRole = 'Supplier' GROUP BY OrganizationCode
		EXCEPT
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'N' AND OrganizationRole = 'Supplier' GROUP BY OrganizationCode
		)
		OR
		EXISTS (	
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'N' AND OrganizationRole = 'Supplier' GROUP BY OrganizationCode
		EXCEPT
		SELECT ISNULL(OrganizationCode, ''), COUNT(*) AS [RowCount] FROM @PurchaseOrderContactsTable WHERE [Source] = 'C' AND OrganizationRole = 'Supplier' GROUP BY OrganizationCode
		)
	)
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (1)
	END	

	-- PO Status: Going to deactivate PO -> Case 1
	IF ( EXISTS (	
			SELECT 1 FROM @PurchaseOrdersTable WHERE [Source] = 'N' AND [STATUS] = '0'
		)
		AND EXISTS (	
			SELECT 1 FROM @PurchaseOrdersTable WHERE [Source] = 'C' AND [STATUS] != '0'
		)
	)
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (1)
	END		

    -- Case 3: PurchaseOrders table
	IF ( EXISTS (	
		SELECT
			[ContainerType]
		FROM @PurchaseOrdersTable WHERE [Source] = 'C'
		EXCEPT
		SELECT
			[ContainerType]
		FROM @PurchaseOrdersTable WHERE [Source] = 'N'
		)
		OR
		EXISTS (
		SELECT
			[ContainerType]
		FROM @PurchaseOrdersTable WHERE [Source] = 'N'
		EXCEPT
		SELECT
			[ContainerType]
		FROM @PurchaseOrdersTable WHERE [Source] = 'C'
		)
	)
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (3)
	END	

	-- Case 2: PurchaseOrders table
	IF ( EXISTS (	
		SELECT
			ISNULL([POKey], ''),
			ISNULL([PONumber], ''),
			[NumberOfLineItems],
			[Status]
		FROM @PurchaseOrdersTable WHERE [Source] = 'C'
		EXCEPT
		SELECT
			ISNULL([POKey], ''),
			ISNULL([PONumber], ''),
			[NumberOfLineItems],
			[Status]
		FROM @PurchaseOrdersTable WHERE [Source] = 'N'
		)
		OR
		EXISTS (	
		SELECT
			ISNULL([POKey], ''),
			ISNULL([PONumber], ''),
			[NumberOfLineItems],
			[Status]
		FROM @PurchaseOrdersTable WHERE [Source] = 'N'
		EXCEPT
		SELECT 
			ISNULL([POKey], ''),
			ISNULL([PONumber], ''),
			[NumberOfLineItems],
			[Status]
		FROM @PurchaseOrdersTable WHERE [Source] = 'C'
		)
	)
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (2)
	END		

	-- Case 2: PurchaseOrderContacts table
	IF ( EXISTS (	
		SELECT 
			ISNULL([OrganizationCode], ''),
			ISNULL([OrganizationRole], '')
		FROM @PurchaseOrderContactsTable WHERE OrganizationRole != 'Shipper' AND OrganizationRole != 'Consignee' AND OrganizationRole != 'Supplier' AND [Source] = 'C'
		EXCEPT
		SELECT
			ISNULL([OrganizationCode], ''),
			ISNULL([OrganizationRole], '')
		FROM @PurchaseOrderContactsTable WHERE OrganizationRole != 'Shipper' AND OrganizationRole != 'Consignee' AND OrganizationRole != 'Supplier' AND [Source] = 'N'
		)
		OR
		EXISTS (	
		SELECT 
			ISNULL([OrganizationCode], ''),
			ISNULL([OrganizationRole], '')
		FROM @PurchaseOrderContactsTable WHERE OrganizationRole != 'Shipper' AND OrganizationRole != 'Consignee' AND OrganizationRole != 'Supplier' AND [Source] = 'N'
		EXCEPT
		SELECT 
			ISNULL([OrganizationCode], ''),
			ISNULL([OrganizationRole], '')
		FROM @PurchaseOrderContactsTable WHERE OrganizationRole != 'Shipper' AND OrganizationRole != 'Consignee' AND OrganizationRole != 'Supplier' AND [Source] = 'C'
		)
	)
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (2)
	END	

	-- Case 2: POLineItems table
	IF ( EXISTS (	
		SELECT
			[LineOrder],
			[OrderedUnitQty],
			[UnitUOM],	
			[PackageUOM],
			ISNULL([ShippingMarks], ''),
			ISNULL([DescriptionOfGoods], '')
		FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'C'
		EXCEPT
		SELECT 
			[LineOrder],
			[OrderedUnitQty],
			[UnitUOM],	
			[PackageUOM],
			ISNULL([ShippingMarks], ''),
			ISNULL([DescriptionOfGoods], '')
		FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'N'
		)
		OR
		EXISTS (	
		SELECT 
			[LineOrder],
			[OrderedUnitQty],
			[UnitUOM],	
			[PackageUOM],
			ISNULL([ShippingMarks], ''),
			ISNULL([DescriptionOfGoods], '')
		FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'N'
		EXCEPT
		SELECT
			[LineOrder],
			[OrderedUnitQty],
			[UnitUOM],	
			[PackageUOM],
			ISNULL([ShippingMarks], ''),
			ISNULL([DescriptionOfGoods], '')
		FROM @PurchaseOrderLineItemsTable WHERE [Source] = 'C'
		)
	)
	BEGIN
		INSERT INTO @PurchaseOrderAdhocChangesTable ([Priority])
		VALUES (2)
	END	

	--DEBUGGER;
	--SELECT * FROM @PurchaseOrdersTable
	--SELECT * FROM @PurchaseOrderContactsTable
	--SELECT * FROM @PurchaseOrderLineItemsTable
	--SELECT * FROM @PurchaseOrderAdhocChangesTable

	UPDATE @PurchaseOrderAdhocChangesTable
	SET PurchaseOrderId = @PurchaseOrderId

	--SELECT TOP (1) * FROM @PurchaseOrderAdhocChangesTable ORDER BY [Priority] ASC

	--SELECT POFFO.POFulfillmentId AS [ImpactedBookingIds] FROM POFulfillmentOrders POFFO WHERE POFFO.PurchaseOrderId = @PurchaseOrderId
	

	INSERT INTO dbo.PurchaseOrderAdhocChanges(CreatedBy, CreatedDate, POFulfillmentId, PurchaseOrderId, JsonCurrentData, JsonNewData, [Priority], [Message])
	SELECT DISTINCT 'System',
		GETUTCDATE(),
		POFFO.POFulfillmentId, 
		AH.PurchaseOrderId,
		@JSonCurrentValue,
		@JsonNewValue, 
		AH.[Priority],
		CASE WHEN AH.[Priority] = 1 THEN N'PO being revised, you need to cancel the booking and place a new one.'
			 WHEN AH.[Priority] = 2 THEN N'PO being revised, you need to refresh the PO items.'
			 ELSE N'PO being revised, you may wish to amend the booking.' END
	FROM 
		-- Only select the top priority 1 > 2 > 3
		(SELECT TOP(1) * FROM @PurchaseOrderAdhocChangesTable ORDER BY [Priority] ASC) AH
		-- @PurchaseOrderAdhocChangesTable AH

		INNER JOIN POFulfillmentOrders POFFO ON AH.PurchaseOrderId = POFFO.PurchaseOrderId

		INNER JOIN POFulfillments POFF ON POFF.Id = POFFO.POFulfillmentId

		-- Status is Active AND Stage not Closed
	WHERE  POFF.[Status] = 10 AND POFF.Stage != 50 
		-- ORDER BY AH.[Priority] ASC


	DECLARE @CurrentPOType int = (SELECT POType FROM @PurchaseOrdersTable WHERE [Source] = 'C')
	DECLARE @CurrentBlanketPOId bigint = (SELECT BlanketPOId FROM @PurchaseOrdersTable WHERE [Source] = 'C')

	-- If "Allow to book Blanket PO", trigger PO Revised for the booking if its allocated PO revised.
	IF (@AllowToBookIn = 20 AND @CurrentPOType = 30)
	BEGIN
		INSERT INTO dbo.PurchaseOrderAdhocChanges(CreatedBy, CreatedDate, POFulfillmentId, PurchaseOrderId, JsonCurrentData, JsonNewData, [Priority], [Message])
		SELECT DISTINCT 'System',
			GETUTCDATE(),
			POFFO.POFulfillmentId, 
			AH.PurchaseOrderId,
			@JSonCurrentValue,
			@JsonNewValue,
			AH.[Priority],
			CASE WHEN AH.[Priority] = 1 THEN N'PO being revised, you need to cancel the booking and place a new one.'
				 WHEN AH.[Priority] = 2 THEN N'PO being revised, you need to refresh the PO items.'
				 ELSE N'PO being revised, you may wish to amend the booking.' END
		FROM POFulfillmentOrders POFFO 
			INNER JOIN POFulfillments POFF ON POFF.Id = POFFO.POFulfillmentId
			OUTER APPLY (
				-- Only select the top priority 1 > 2 > 3
				-- @PurchaseOrderAdhocChangesTable AH
				SELECT TOP(1) * FROM @PurchaseOrderAdhocChangesTable ORDER BY [Priority] ASC
			) AH
			-- Status is Active AND Stage not Closed
		WHERE  POFF.[Status] = 10
			   AND POFF.Stage != 50
			   AND POFFO.PurchaseOrderId = @CurrentBlanketPOId
	END
	-- If "Allow to book Allocated PO", trigger PO Revised for the booking if its blanket PO revised.
	ELSE IF (@AllowToBookIn = 30 AND @CurrentPOType = 20)
	BEGIN
		INSERT INTO dbo.PurchaseOrderAdhocChanges(CreatedBy, CreatedDate, POFulfillmentId, PurchaseOrderId, JsonCurrentData, JsonNewData, [Priority], [Message])
		SELECT DISTINCT 'System',
			GETUTCDATE(),
			POFFO.POFulfillmentId, 
			AH.PurchaseOrderId,
			@JSonCurrentValue,
			@JsonNewValue,
			AH.[Priority],
			CASE WHEN AH.[Priority] = 1 THEN N'PO being revised, you need to cancel the booking and place a new one.'
				 WHEN AH.[Priority] = 2 THEN N'PO being revised, you need to refresh the PO items.'
				 ELSE N'PO being revised, you may wish to amend the booking.' END
		FROM PurchaseOrders PO 
			INNER JOIN POFulfillmentOrders POFFO ON PO.Id = POFFO.PurchaseOrderId
			INNER JOIN POFulfillments POFF ON POFF.Id = POFFO.POFulfillmentId
			OUTER APPLY (
				-- Only select the top priority 1 > 2 > 3
				-- @PurchaseOrderAdhocChangesTable AH
				SELECT TOP(1) * FROM @PurchaseOrderAdhocChangesTable ORDER BY [Priority] ASC
			) AH
			-- Status is Active AND Stage not Closed
		WHERE  POFF.[Status] = 10
			   AND POFF.Stage != 50
			   AND PO.BlanketPOId = AH.PurchaseOrderId
	END

END