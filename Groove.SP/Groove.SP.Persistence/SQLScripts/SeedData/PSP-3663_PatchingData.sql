
;

SET NOCOUNT ON;

BEGIN TRANSACTION;

-- Set Shipment id range to run

DECLARE @ShipmentIdFrom BIGINT = 0;
DECLARE @ShipmentIdTo BIGINT = 0;

-- Variable declarations

DECLARE @Id BIGINT;
DECLARE @Id1 BIGINT;
DECLARE @Id2 BIGINT;
DECLARE @Id3 BIGINT;


BEGIN /* To clone Shipments */


DECLARE @ShipmentToInsertTable AS TABLE
(
	[NewShipmentId] [bigint] NOT NULL,
	[ClonedFromShipmentId] [bigint] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentNo] [varchar](50) NOT NULL,
	[BuyerCode] [nvarchar](max) NULL,
	[CustomerReferenceNo] [varchar](3000) NULL,
	[ModeOfTransport] [nvarchar](512) NULL,
	[CargoReadyDate] [datetime2](7) NOT NULL,
	[BookingDate] [datetime2](7) NOT NULL,
	[ShipmentType] [nvarchar](128) NULL,
	[ShipFrom] [nvarchar](128) NOT NULL,
	[ShipFromETDDate] [datetime2](7) NOT NULL,
	[ShipTo] [nvarchar](128) NULL,
	[ShipToETADate] [datetime2](7) NULL,
	[Movement] [nvarchar](128) NULL,
	[TotalPackage] [decimal](18, 4) NOT NULL,
	[TotalPackageUOM] [nvarchar](20) NULL,
	[TotalUnit] [decimal](18, 4) NOT NULL,
	[TotalUnitUOM] [nvarchar](20) NULL,
	[TotalGrossWeight] [decimal](18, 4) NOT NULL,
	[TotalGrossWeightUOM] [nvarchar](20) NULL,
	[TotalNetWeight] [decimal](18, 4) NOT NULL,
	[TotalNetWeightUOM] [nvarchar](20) NULL,
	[TotalVolume] [decimal](18, 4) NOT NULL,
	[TotalVolumeUOM] [nvarchar](20) NULL,
	[ServiceType] [nvarchar](128) NULL,
	[Incoterm] [nvarchar](128) NULL,
	[Status] [nvarchar](128) NOT NULL,
	[IsFCL] [bit] NOT NULL,
	[POFulfillmentId] [bigint] NULL,
	[BookingNo] [varchar](50) NULL,
	[IsItineraryConfirmed] [bit] NOT NULL,
	[OrderType] [int] NOT NULL,
	[CarrierContractNo] [varchar](50) NULL,
	[AgentReferenceNo] [varchar](3000) NULL,
	[ShipperReferenceNo] [varchar](3000) NULL,
	[Factor] [int] NULL
	
);


WITH ShipmentCTE
AS
(
	SELECT *
	FROM Shipments SH
	WHERE 
	SH.Id >= @ShipmentIdFrom AND SH.Id < @ShipmentIdTo
	AND SH.OrderType = 2 -- Cruise shipment
	AND NOT EXISTS (
		SELECT 1 FROM Shipments WHERE ShipmentNo = CONCAT(SH.ShipmentNo, 'CSP') AND OrderType = 1
	)
)


INSERT INTO Shipments ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[ShipmentNo]
           ,[BuyerCode]
           ,[CustomerReferenceNo]
           ,[ModeOfTransport]
           ,[CargoReadyDate]
           ,[BookingDate]
           ,[ShipmentType]
           ,[ShipFrom]
           ,[ShipFromETDDate]
           ,[ShipTo]
           ,[ShipToETADate]
           ,[Movement]
           ,[TotalPackage]
           ,[TotalPackageUOM]
           ,[TotalUnit]
           ,[TotalUnitUOM]
           ,[TotalGrossWeight]
           ,[TotalGrossWeightUOM]
           ,[TotalNetWeight]
           ,[TotalNetWeightUOM]
           ,[TotalVolume]
           ,[TotalVolumeUOM]
           ,[ServiceType]
           ,[Incoterm]
           ,[Status]
           ,[IsFCL]
           ,[POFulfillmentId]
           ,[BookingNo]
           ,[IsItineraryConfirmed]
           ,[OrderType]
           ,[CarrierContractNo]
           ,[AgentReferenceNo]
           ,[ShipperReferenceNo]
           ,[Factor]
		   )

OUTPUT		inserted.[Id]
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
			,inserted.[UpdatedBy]
			,inserted.[UpdatedDate]
			,inserted.[ShipmentNo]
			,inserted.[BuyerCode]
			,inserted.[CustomerReferenceNo]
			,inserted.[ModeOfTransport]
			,inserted.[CargoReadyDate]
			,inserted.[BookingDate]
			,inserted.[ShipmentType]
			,inserted.[ShipFrom]
			,inserted.[ShipFromETDDate]
			,inserted.[ShipTo]
			,inserted.[ShipToETADate]
			,inserted.[Movement]
			,inserted.[TotalPackage]
			,inserted.[TotalPackageUOM]
			,inserted.[TotalUnit]
			,inserted.[TotalUnitUOM]
			,inserted.[TotalGrossWeight]
			,inserted.[TotalGrossWeightUOM]
			,inserted.[TotalNetWeight]
			,inserted.[TotalNetWeightUOM]
			,inserted.[TotalVolume]
			,inserted.[TotalVolumeUOM]
			,inserted.[ServiceType]
			,inserted.[Incoterm]
			,inserted.[Status]
			,inserted.[IsFCL]
			,inserted.[POFulfillmentId]
			,inserted.[BookingNo]
			,inserted.[IsItineraryConfirmed]
			,inserted.[OrderType]
			,inserted.[CarrierContractNo]
			,inserted.[AgentReferenceNo]
			,inserted.[ShipperReferenceNo]
			,inserted.[Factor]
INTO @ShipmentToInsertTable (
			[NewShipmentId]
			,[CreatedBy]
			,[CreatedDate]
			,[UpdatedBy]
			,[UpdatedDate]
			,[ShipmentNo]
			,[BuyerCode]
			,[CustomerReferenceNo]
			,[ModeOfTransport]
			,[CargoReadyDate]
			,[BookingDate]
			,[ShipmentType]
			,[ShipFrom]
			,[ShipFromETDDate]
			,[ShipTo]
			,[ShipToETADate]
			,[Movement]
			,[TotalPackage]
			,[TotalPackageUOM]
			,[TotalUnit]
			,[TotalUnitUOM]
			,[TotalGrossWeight]
			,[TotalGrossWeightUOM]
			,[TotalNetWeight]
			,[TotalNetWeightUOM]
			,[TotalVolume]
			,[TotalVolumeUOM]
			,[ServiceType]
			,[Incoterm]
			,[Status]
			,[IsFCL]
			,[POFulfillmentId]
			,[BookingNo]
			,[IsItineraryConfirmed]
			,[OrderType]
			,[CarrierContractNo]
			,[AgentReferenceNo]
			,[ShipperReferenceNo]
			,[Factor]
		   )

SELECT		[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,CONCAT([ShipmentNo], 'CSP') -- Add Suffix by CSP
           ,[BuyerCode]
           ,[CustomerReferenceNo]
           ,[ModeOfTransport]
           ,[CargoReadyDate]
           ,[BookingDate]
           ,[ShipmentType]
           ,[ShipFrom]
           ,[ShipFromETDDate]
           ,[ShipTo]
           ,[ShipToETADate]
           ,[Movement]
           ,[TotalPackage]
           ,[TotalPackageUOM]
           ,[TotalUnit]
           ,[TotalUnitUOM]
           ,[TotalGrossWeight]
           ,[TotalGrossWeightUOM]
           ,[TotalNetWeight]
           ,[TotalNetWeightUOM]
           ,[TotalVolume]
           ,[TotalVolumeUOM]
           ,[ServiceType]
           ,[Incoterm]
           ,[Status]
           ,[IsFCL]
           ,[POFulfillmentId]
           ,[BookingNo]
           ,[IsItineraryConfirmed]
           ,1 -- Set to Freight
           ,[CarrierContractNo]
           ,[AgentReferenceNo]
           ,[ShipperReferenceNo]
           ,[Factor]
FROM ShipmentCTE CTE

UPDATE @ShipmentToInsertTable
SET ClonedFromShipmentId = SH.Id
FROM @ShipmentToInsertTable SHToInsert
INNER JOIN Shipments SH ON SHToInsert.ShipmentNo = CONCAT(SH.ShipmentNo, 'CSP')

--SELECT *
--FROM Shipments
--WHERE Id IN (
--	SELECT Id FROM @ShipmentToInsertTable
--	UNION ALL
--	SELECT ClonedFromShipmentId FROM @ShipmentToInsertTable
--)
--ORDER BY ShipmentNo

-- To get added Shipments
SELECT * FROM @ShipmentToInsertTable

END

BEGIN /* To clone Shipment Contacts */

DECLARE @ShipmentContactToInsertTable AS TABLE (
	[NewShipmentContactId] [bigint] NOT NULL,
	[ClonedFromShipmentContactId] [bigint] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentId] [bigint] NOT NULL,
	[OrganizationId] [bigint] NOT NULL,
	[OrganizationRole] [varchar](50) NOT NULL,
	[CompanyName] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](250) NULL,
	[ContactName] [nvarchar](250) NULL,
	[ContactNumber] [nvarchar](100) NULL,
	[ContactEmail] [nvarchar](100) NULL
)


DECLARE id_cursor CURSOR FOR
SELECT SHC.Id
FROM ShipmentContacts SHC
WHERE EXISTS (SELECT 1 FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = SHC.ShipmentId)

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @Id  
WHILE @@FETCH_STATUS = 0  
BEGIN  

	INSERT INTO ShipmentContacts([CreatedBy]
			   ,[CreatedDate]
			   ,[UpdatedBy]
			   ,[UpdatedDate]
			   ,[ShipmentId]
			   ,[OrganizationId]
			   ,[OrganizationRole]
			   ,[CompanyName]
			   ,[Address]
			   ,[ContactName]
			   ,[ContactNumber]
			   ,[ContactEmail])
	OUTPUT		inserted.[Id]
				,@Id
				,inserted.[CreatedBy]
				,inserted.[CreatedDate]
				,inserted.[UpdatedBy]
				,inserted.[UpdatedDate]
				,inserted.[ShipmentId]
				,inserted.[OrganizationId]
				,inserted.[OrganizationRole]
				,inserted.[CompanyName]
				,inserted.[Address]
				,inserted.[ContactName]
				,inserted.[ContactNumber]
				,inserted.[ContactEmail]
	INTO @ShipmentContactToInsertTable (
				[NewShipmentContactId]
				,[ClonedFromShipmentContactId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,[ShipmentId]
				,[OrganizationId]
				,[OrganizationRole]
				,[CompanyName]
				,[Address]
				,[ContactName]
				,[ContactNumber]
				,[ContactEmail] )

	SELECT		
				SHC.[CreatedBy]
			   ,SHC.[CreatedDate]
			   ,SHC.[UpdatedBy]
			   ,SHC.[UpdatedDate]
			   ,SHToInsert.[NewShipmentId] -- Put Id from cloned Shipment
			   ,SHC.[OrganizationId]
			   ,SHC.[OrganizationRole]
			   ,SHC.[CompanyName]
			   ,SHC.[Address]
			   ,SHC.[ContactName]
			   ,SHC.[ContactNumber]
			   ,SHC.[ContactEmail]	
	FROM ShipmentContacts SHC
	INNER JOIN @ShipmentToInsertTable SHToInsert ON SHC.ShipmentId = SHToInsert.ClonedFromShipmentId
	WHERE SHC.Id = @Id

	FETCH NEXT FROM id_cursor   
    INTO @Id

END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

SELECT * FROM @ShipmentContactToInsertTable

END

BEGIN /* To clone Cargo Details */

DECLARE @CargoDetailsToInsertTable AS TABLE
(
	[NewCargoDetailsIdId] [bigint] NOT NULL,
	[ClonedFromCargoDetailsId] [bigint] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentId] [bigint] NOT NULL,
	[ClonedFromShipmentId] [bigint] NULL,
	[Sequence] [int] NOT NULL,
	[ShippingMarks] [nvarchar](max) NULL,
	[Description] [nvarchar](max) NULL,
	[Unit] [decimal](18, 4) NOT NULL,
	[UnitUOM] [nvarchar](20) NULL,
	[Package] [decimal](18, 4) NOT NULL,
	[PackageUOM] [nvarchar](20) NULL,
	[Volume] [decimal](18, 4) NOT NULL,
	[VolumeUOM] [nvarchar](20) NULL,
	[GrossWeight] [decimal](18, 4) NOT NULL,
	[GrossWeightUOM] [nvarchar](20) NULL,
	[NetWeight] [decimal](18, 4) NULL,
	[NetWeightUOM] [nvarchar](20) NULL,
	[Commodity] [nvarchar](128) NULL,
	[HSCode] [nvarchar](128) NULL,
	[ProductNumber] [nvarchar](128) NULL,
	[CountryOfOrigin] [nvarchar](128) NULL,
	[OrderType] [int] NOT NULL,
	[OrderId] [bigint] NULL,
	[ClonedFromOrderId] [bigint] NULL,
	[ItemId] [bigint] NULL,
	[ClonedFromItemId] [bigint] NULL,
	[ChargeableWeight] [decimal](18, 4) NULL,
	[ChargeableWeightUOM] [nvarchar](20) NULL,
	[VolumetricWeight] [decimal](18, 4) NULL,
	[VolumetricWeightUOM] [nvarchar](20) NULL	
);

DECLARE id_cursor CURSOR FOR
SELECT CD.Id, CD.ShipmentId, CD.OrderId, CD.ItemId
FROM CargoDetails CD
WHERE --CD.OrderType = 2 -- Filter to cruise cargo details
CD.ShipmentId IN ( SELECT ClonedFromShipmentId FROM @ShipmentToInsertTable ) -- Exists in the above cloned shipments

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @Id, @Id1, @Id2, @Id3
WHILE @@FETCH_STATUS = 0  
BEGIN  

	INSERT INTO CargoDetails([CreatedBy]
			,[CreatedDate]
			,[UpdatedBy]
			,[UpdatedDate]
			,[ShipmentId]
			,[Sequence]
			,[ShippingMarks]
			,[Description]
			,[Unit]
			,[UnitUOM]
			,[Package]
			,[PackageUOM]
			,[Volume]
			,[VolumeUOM]
			,[GrossWeight]
			,[GrossWeightUOM]
			,[NetWeight]
			,[NetWeightUOM]
			,[Commodity]
			,[HSCode]
			,[ProductNumber]
			,[CountryOfOrigin]
			,[OrderType]
			,[OrderId]
			,[ItemId]
			,[ChargeableWeight]
			,[ChargeableWeightUOM]
			,[VolumetricWeight]
			,[VolumetricWeightUOM]														
		   )
OUTPUT		inserted.[Id]
			,@Id
			,inserted.[CreatedBy]
			,inserted.[CreatedDate]
			,inserted.[UpdatedBy]
			,inserted.[UpdatedDate]
			,inserted.[ShipmentId]
			,@Id1
			,inserted.[Sequence]
			,inserted.[ShippingMarks]
			,inserted.[Description]
			,inserted.[Unit]
			,inserted.[UnitUOM]
			,inserted.[Package]
			,inserted.[PackageUOM]
			,inserted.[Volume]
			,inserted.[VolumeUOM]
			,inserted.[GrossWeight]
			,inserted.[GrossWeightUOM]
			,inserted.[NetWeight]
			,inserted.[NetWeightUOM]
			,inserted.[Commodity]
			,inserted.[HSCode]
			,inserted.[ProductNumber]
			,inserted.[CountryOfOrigin]
			,inserted.[OrderType]
			,inserted.[OrderId]
			,@Id2
			,inserted.[ItemId]
			,@Id3
			,inserted.[ChargeableWeight]
			,inserted.[ChargeableWeightUOM]
			,inserted.[VolumetricWeight]
			,inserted.[VolumetricWeightUOM]	
INTO @CargoDetailsToInsertTable ([NewCargoDetailsIdId]
			,[ClonedFromCargoDetailsId]
			,[CreatedBy]
			,[CreatedDate]
			,[UpdatedBy]
			,[UpdatedDate]
			,[ShipmentId]
			,[ClonedFromShipmentId]
			,[Sequence]
			,[ShippingMarks]
			,[Description]
			,[Unit]
			,[UnitUOM]
			,[Package]
			,[PackageUOM]
			,[Volume]
			,[VolumeUOM]
			,[GrossWeight]
			,[GrossWeightUOM]
			,[NetWeight]
			,[NetWeightUOM]
			,[Commodity]
			,[HSCode]
			,[ProductNumber]
			,[CountryOfOrigin]
			,[OrderType]
			,[OrderId]
			,[ClonedFromOrderId]
			,[ItemId]
			,[ClonedFromItemId]
			,[ChargeableWeight]
			,[ChargeableWeightUOM]
			,[VolumetricWeight]
			,[VolumetricWeightUOM]														
		   )
SELECT		[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           , (SELECT TOP(1) [NewShipmentId] FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = CTE.ShipmentId) -- Put new ShipmentId that cloned above
           ,[Sequence]
           ,[ShippingMarks]
           ,[Description]
           ,[Unit]
           ,[UnitUOM]
           ,[Package]
           ,[PackageUOM]
           ,[Volume]
           ,[VolumeUOM]
           ,[GrossWeight]
           ,[GrossWeightUOM]
           ,[NetWeight]
           ,[NetWeightUOM]
           ,[Commodity]
           ,[HSCode]
           ,[ProductNumber]
           ,[CountryOfOrigin]
           ,1 -- Set [OrderType] to Freight
           ,T.[ClonedPOId] -- Set to Id of cloned PO
           ,T1.[ClonedItemId] -- Set to Id of cloned PO Item
           ,[ChargeableWeight]
           ,[ChargeableWeightUOM]
           ,[VolumetricWeight]
           ,[VolumetricWeightUOM]
FROM CargoDetails CTE
OUTER APPLY
(
	-- To get clone PO Id
	SELECT TOP(1) Id AS ClonedPOId FROM PurchaseOrders WHERE CruiseOrderId = CTE.OrderId
)T
OUTER APPLY
(
	-- CTE.ItemId -> link to cruise
	SELECT TOP(1) POL.Id AS [ClonedItemId] 
	FROM PurchaseOrders PO
	INNER JOIN POLineItems POL ON PO.Id = POL.PurchaseOrderId
	INNER JOIN cruise.CruiseOrderItems COI ON COI.Id = CTE.ItemId AND POL.LineOrder = COI.POLine
	WHERE CTE.OrderId = PO.CruiseOrderId
)T1
WHERE CTE.Id = @Id

	FETCH NEXT FROM id_cursor   
    INTO @Id, @Id1, @Id2, @Id3
END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

-- To get added CargoDetails
SELECT * FROM @CargoDetailsToInsertTable

END

BEGIN /* To clone Consignments */

DECLARE @ConsignmentToInsertTable AS TABLE
(
	[NewConsignmentId] [bigint] NOT NULL,
	[ClonedFromConsignmentId] [bigint] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentId] [bigint] NOT NULL,
	[ConsignmentType] [nvarchar](128) NULL,
	[ConsignmentDate] [datetime2](7) NULL,
	[ExecutionAgentId] [bigint] NOT NULL,
	[ShipFrom] [nvarchar](512) NULL,
	[ShipFromETDDate] [datetime2](7) NOT NULL,
	[ShipTo] [nvarchar](512) NULL,
	[ShipToETADate] [datetime2](7) NOT NULL,
	[Status] [nvarchar](128) NULL,
	[ModeOfTransport] [nvarchar](512) NULL,
	[Movement] [nvarchar](128) NULL,
	[Unit] [decimal](18, 4) NOT NULL,
	[UnitUOM] [nvarchar](20) NULL,
	[Package] [decimal](18, 4) NOT NULL,
	[PackageUOM] [nvarchar](20) NULL,
	[Volume] [decimal](18, 4) NOT NULL,
	[VolumeUOM] [nvarchar](20) NULL,
	[GrossWeight] [decimal](18, 4) NOT NULL,
	[GrossWeightUOM] [nvarchar](20) NULL,
	[NetWeight] [decimal](18, 4) NOT NULL,
	[NetWeightUOM] [nvarchar](20) NULL,
	[HouseBillId] [bigint] NULL,
	[MasterBillId] [bigint] NULL,
	[TriangleTradeFlag] [bit] NOT NULL,
	[MemoBOLFlag] [bit] NOT NULL,
	[Sequence] [int] NOT NULL,
	[ExecutionAgentName] [nvarchar](512) NULL,
	[AgentReferenceNumber] [nvarchar](max) NULL,
	[ConfirmedDate] [datetime2](7) NULL,
	[ConsignmentHouseBL] [nvarchar](max) NULL,
	[ConsignmentMasterBL] [nvarchar](max) NULL,
	[ServiceType] [nvarchar](128) NULL,
	[IsDeleted] [bit] NOT NULL	
);

DECLARE id_cursor CURSOR FOR
SELECT CSM.Id
FROM Consignments CSM
WHERE  EXISTS ( SELECT 1 FROM @ShipmentToInsertTable SHToInsert WHERE CSM.ShipmentId = SHToInsert.ClonedFromShipmentId)

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @Id  
WHILE @@FETCH_STATUS = 0  
BEGIN  
	
	INSERT INTO Consignments ([CreatedBy]
			   ,[CreatedDate]
			   ,[UpdatedBy]
			   ,[UpdatedDate]
			   ,[ShipmentId]
			   ,[ConsignmentType]
			   ,[ConsignmentDate]
			   ,[ExecutionAgentId]
			   ,[ShipFrom]
			   ,[ShipFromETDDate]
			   ,[ShipTo]
			   ,[ShipToETADate]
			   ,[Status]
			   ,[ModeOfTransport]
			   ,[Movement]
			   ,[Unit]
			   ,[UnitUOM]
			   ,[Package]
			   ,[PackageUOM]
			   ,[Volume]
			   ,[VolumeUOM]
			   ,[GrossWeight]
			   ,[GrossWeightUOM]
			   ,[NetWeight]
			   ,[NetWeightUOM]
			   ,[HouseBillId]
			   ,[MasterBillId]
			   ,[TriangleTradeFlag]
			   ,[MemoBOLFlag]
			   ,[Sequence]
			   ,[ExecutionAgentName]
			   ,[AgentReferenceNumber]
			   ,[ConfirmedDate]
			   ,[ConsignmentHouseBL]
			   ,[ConsignmentMasterBL]
			   ,[ServiceType]
			   ,[IsDeleted])

	OUTPUT		inserted.[Id]
				,@Id
				,inserted.[CreatedBy]
				,inserted.[CreatedDate]
				,inserted.[UpdatedBy]
				,inserted.[UpdatedDate]
				,inserted.[ShipmentId]
				,inserted.[ConsignmentType]
				,inserted.[ConsignmentDate]
				,inserted.[ExecutionAgentId]
				,inserted.[ShipFrom]
				,inserted.[ShipFromETDDate]
				,inserted.[ShipTo]
				,inserted.[ShipToETADate]
				,inserted.[Status]
				,inserted.[ModeOfTransport]
				,inserted.[Movement]
				,inserted.[Unit]
				,inserted.[UnitUOM]
				,inserted.[Package]
				,inserted.[PackageUOM]
				,inserted.[Volume]
				,inserted.[VolumeUOM]
				,inserted.[GrossWeight]
				,inserted.[GrossWeightUOM]
				,inserted.[NetWeight]
				,inserted.[NetWeightUOM]
				,inserted.[HouseBillId]
				,inserted.[MasterBillId]
				,inserted.[TriangleTradeFlag]
				,inserted.[MemoBOLFlag]
				,inserted.[Sequence]
				,inserted.[ExecutionAgentName]
				,inserted.[AgentReferenceNumber]
				,inserted.[ConfirmedDate]
				,inserted.[ConsignmentHouseBL]
				,inserted.[ConsignmentMasterBL]
				,inserted.[ServiceType]
				,inserted.[IsDeleted]
	INTO @ConsignmentToInsertTable(
				[NewConsignmentId]
				,[ClonedFromConsignmentId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,[ShipmentId]
				,[ConsignmentType]
				,[ConsignmentDate]
				,[ExecutionAgentId]
				,[ShipFrom]
				,[ShipFromETDDate]
				,[ShipTo]
				,[ShipToETADate]
				,[Status]
				,[ModeOfTransport]
				,[Movement]
				,[Unit]
				,[UnitUOM]
				,[Package]
				,[PackageUOM]
				,[Volume]
				,[VolumeUOM]
				,[GrossWeight]
				,[GrossWeightUOM]
				,[NetWeight]
				,[NetWeightUOM]
				,[HouseBillId]
				,[MasterBillId]
				,[TriangleTradeFlag]
				,[MemoBOLFlag]
				,[Sequence]
				,[ExecutionAgentName]
				,[AgentReferenceNumber]
				,[ConfirmedDate]
				,[ConsignmentHouseBL]
				,[ConsignmentMasterBL]
				,[ServiceType]
				,[IsDeleted]
				)

	SELECT		CSM.[CreatedBy]
			   ,CSM.[CreatedDate]
			   ,CSM.[UpdatedBy]
			   ,CSM.[UpdatedDate]
			   ,(SELECT TOP(1) [NewShipmentId] FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = CSM.ShipmentId) -- Put Id from cloned Shipment
			   ,CSM.[ConsignmentType]
			   ,CSM.[ConsignmentDate]
			   ,CSM.[ExecutionAgentId]
			   ,CSM.[ShipFrom]
			   ,CSM.[ShipFromETDDate]
			   ,CSM.[ShipTo]
			   ,CSM.[ShipToETADate]
			   ,CSM.[Status]
			   ,CSM.[ModeOfTransport]
			   ,CSM.[Movement]
			   ,CSM.[Unit]
			   ,CSM.[UnitUOM]
			   ,CSM.[Package]
			   ,CSM.[PackageUOM]
			   ,CSM.[Volume]
			   ,CSM.[VolumeUOM]
			   ,CSM.[GrossWeight]
			   ,CSM.[GrossWeightUOM]
			   ,CSM.[NetWeight]
			   ,CSM.[NetWeightUOM]
			   ,CSM.[HouseBillId]
			   ,CSM.[MasterBillId]
			   ,CSM.[TriangleTradeFlag]
			   ,CSM.[MemoBOLFlag]
			   ,CSM.[Sequence]
			   ,CSM.[ExecutionAgentName]
			   ,CSM.[AgentReferenceNumber]
			   ,CSM.[ConfirmedDate]
			   ,CSM.[ConsignmentHouseBL]
			   ,CSM.[ConsignmentMasterBL]
			   ,CSM.[ServiceType]
			   ,CSM.[IsDeleted]
	FROM Consignments CSM
	WHERE CSM.Id = @Id
	

	FETCH NEXT FROM id_cursor   
    INTO @Id  
END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

SELECT * FROM @ConsignmentToInsertTable

END

BEGIN /* To clone Shipment Loads */

DECLARE @ShipmentLoadToInsertTable AS TABLE (
	[NewShipmentLoadId] [bigint] NOT NULL,
	[ClonedFromShipmentLoadId] [bigint] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentId] [bigint] NULL,
	[ConsignmentId] [bigint] NULL,
	[ConsolidationId] [bigint] NULL,
	[ContainerId] [bigint] NULL,
	[ModeOfTransport] [nvarchar](512) NULL,
	[IsFCL] [bit] NOT NULL,
	[LoadingPlace] [nvarchar](512) NULL,
	[LoadingPartyId] [bigint] NULL,
	[EquipmentType] [nvarchar](512) NULL,
	[CarrierBookingNo] [varchar](50) NULL
)


DECLARE id_cursor CURSOR FOR
SELECT SHL.Id
FROM ShipmentLoads SHL
WHERE EXISTS (SELECT 1 FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = SHL.ShipmentId)

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @Id  
WHILE @@FETCH_STATUS = 0  
BEGIN  

	INSERT INTO ShipmentLoads ([CreatedBy]
			   ,[CreatedDate]
			   ,[UpdatedBy]
			   ,[UpdatedDate]
			   ,[ShipmentId]
			   ,[ConsignmentId]
			   ,[ConsolidationId]
			   ,[ContainerId]
			   ,[ModeOfTransport]
			   ,[IsFCL]
			   ,[LoadingPlace]
			   ,[LoadingPartyId]
			   ,[EquipmentType]
			   ,[CarrierBookingNo])
	OUTPUT		inserted.[Id]
				,@Id
				,inserted.[CreatedBy]
				,inserted.[CreatedDate]
				,inserted.[UpdatedBy]
				,inserted.[UpdatedDate]
				,inserted.[ShipmentId]
				,inserted.[ConsignmentId]
				,inserted.[ConsolidationId]
				,inserted.[ContainerId]
				,inserted.[ModeOfTransport]
				,inserted.[IsFCL]
				,inserted.[LoadingPlace]
				,inserted.[LoadingPartyId]
				,inserted.[EquipmentType]
				,inserted.[CarrierBookingNo]
	INTO @ShipmentLoadToInsertTable (
				[NewShipmentLoadId]
				,[ClonedFromShipmentLoadId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,[ShipmentId]
				,[ConsignmentId]
				,[ConsolidationId]
				,[ContainerId]
				,[ModeOfTransport]
				,[IsFCL]
				,[LoadingPlace]
				,[LoadingPartyId]
				,[EquipmentType]
				,[CarrierBookingNo] )

	SELECT		SHL.[CreatedBy]
			   ,SHL.[CreatedDate]
			   ,SHL.[UpdatedBy]
			   ,SHL.[UpdatedDate]
			   ,(SELECT TOP(1) [NewShipmentId] FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = SHL.ShipmentId) -- Put Id from cloned Shipment
			   ,(SELECT TOP(1) [NewConsignmentId] FROM @ConsignmentToInsertTable WHERE ClonedFromConsignmentId = SHL.ConsignmentId) -- Put Id from cloned Consignment
			   ,SHL.[ConsolidationId]
			   ,SHL.[ContainerId]
			   ,SHL.[ModeOfTransport]
			   ,SHL.[IsFCL]
			   ,SHL.[LoadingPlace]
			   ,SHL.[LoadingPartyId]
			   ,SHL.[EquipmentType]
			   ,SHL.[CarrierBookingNo]
	FROM ShipmentLoads SHL
	WHERE SHL.Id = @Id

	FETCH NEXT FROM id_cursor   
    INTO @Id

END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

SELECT * FROM @ShipmentLoadToInsertTable

END

BEGIN /* To clone Shipment Loads Details */

DECLARE @ShipmentLoadDetailsToInsertTable AS TABLE (
	[NewShipmentLoadDetailsId] [bigint] NOT NULL,
	[ClonedFromShipmentLoadDetailsId] [bigint] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentId] [bigint] NULL,
	[ConsignmentId] [bigint] NULL,
	[CargoDetailId] [bigint] NULL,
	[ShipmentLoadId] [bigint] NULL,
	[ContainerId] [bigint] NULL,
	[ConsolidationId] [bigint] NULL,
	[Package] [decimal](18, 4) NOT NULL,
	[PackageUOM] [nvarchar](20) NULL,
	[Unit] [decimal](18, 4) NOT NULL,
	[UnitUOM] [nvarchar](20) NULL,
	[Volume] [decimal](18, 4) NOT NULL,
	[VolumeUOM] [nvarchar](20) NULL,
	[GrossWeight] [decimal](18, 4) NOT NULL,
	[GrossWeightUOM] [nvarchar](20) NULL,
	[NetWeight] [decimal](18, 4) NULL,
	[NetWeightUOM] [nvarchar](20) NULL,
	[Sequence] [int] NULL
)


DECLARE id_cursor CURSOR FOR
SELECT SHLD.Id
FROM ShipmentLoadDetails SHLD
WHERE EXISTS (SELECT 1 FROM @ShipmentLoadToInsertTable WHERE ClonedFromShipmentLoadId = SHLD.ShipmentLoadId)

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @Id  
WHILE @@FETCH_STATUS = 0  
BEGIN  

	INSERT INTO ShipmentLoadDetails ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[ShipmentId]
           ,[ConsignmentId]
           ,[CargoDetailId]
           ,[ShipmentLoadId]
           ,[ContainerId]
           ,[ConsolidationId]
           ,[Package]
           ,[PackageUOM]
           ,[Unit]
           ,[UnitUOM]
           ,[Volume]
           ,[VolumeUOM]
           ,[GrossWeight]
           ,[GrossWeightUOM]
           ,[NetWeight]
           ,[NetWeightUOM]
           ,[Sequence])
	OUTPUT		inserted.[Id]
				,@Id
				,inserted.[CreatedBy]
				,inserted.[CreatedDate]
				,inserted.[UpdatedBy]
				,inserted.[UpdatedDate]
				,inserted.[ShipmentId]
				,inserted.[ConsignmentId]
				,inserted.[CargoDetailId]
				,inserted.[ShipmentLoadId]
				,inserted.[ContainerId]
				,inserted.[ConsolidationId]
				,inserted.[Package]
				,inserted.[PackageUOM]
				,inserted.[Unit]
				,inserted.[UnitUOM]
				,inserted.[Volume]
				,inserted.[VolumeUOM]
				,inserted.[GrossWeight]
				,inserted.[GrossWeightUOM]
				,inserted.[NetWeight]
				,inserted.[NetWeightUOM]
				,inserted.[Sequence]
	INTO @ShipmentLoadDetailsToInsertTable (
				[NewShipmentLoadDetailsId]
				,[ClonedFromShipmentLoadDetailsId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,[ShipmentId]
				,[ConsignmentId]
				,[CargoDetailId]
				,[ShipmentLoadId]
				,[ContainerId]
				,[ConsolidationId]
				,[Package]
				,[PackageUOM]
				,[Unit]
				,[UnitUOM]
				,[Volume]
				,[VolumeUOM]
				,[GrossWeight]
				,[GrossWeightUOM]
				,[NetWeight]
				,[NetWeightUOM]
				,[Sequence] )

	SELECT		SHLD.[CreatedBy]
				,SHLD.[CreatedDate]
				,SHLD.[UpdatedBy]
				,SHLD.[UpdatedDate]
				,(SELECT TOP(1) [NewShipmentId] FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = SHLD.ShipmentId) -- Put Id from cloned Shipment
				,(SELECT TOP(1) [NewConsignmentId] FROM @ConsignmentToInsertTable WHERE ClonedFromConsignmentId = SHLD.ConsignmentId) -- Put Id from cloned Consignment
				,(SELECT TOP(1) [NewCargoDetailsIdId] FROM @CargoDetailsToInsertTable WHERE ClonedFromCargoDetailsId = SHLD.CargoDetailId) -- Put Id from cloned Cargo details
				,(SELECT TOP(1) [NewShipmentLoadId] FROM @ShipmentLoadToInsertTable WHERE ClonedFromShipmentLoadId = SHLD.ShipmentLoadId) -- Put Id from cloned Shipment load
				,SHLD.[ContainerId]
				,SHLD.[ConsolidationId]
				,SHLD.[Package]
				,SHLD.[PackageUOM]
				,SHLD.[Unit]
				,SHLD.[UnitUOM]
				,SHLD.[Volume]
				,SHLD.[VolumeUOM]
				,SHLD.[GrossWeight]
				,SHLD.[GrossWeightUOM]
				,SHLD.[NetWeight]
				,SHLD.[NetWeightUOM]
				,SHLD.[Sequence]
	FROM ShipmentLoadDetails SHLD
	WHERE SHLD.Id = @Id

	FETCH NEXT FROM id_cursor   
    INTO @Id

END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

SELECT * FROM @ShipmentLoadDetailsToInsertTable

END

BEGIN /* To clone Activities */

DECLARE @GlobalIdActivitiesToInsertTable AS TABLE (
	[NewGlobalIdActivityId] [bigint] NOT NULL,
	[ClonedFromGlobalIdActivityId] [bigint] NULL,
	[GlobalId] [nvarchar](450) NULL,
	[ActivityId] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Location] [nvarchar](max) NULL,
	[Remark] [nvarchar](max) NULL,
	[ActivityDate] [datetime2](7) NOT NULL
)


DECLARE id_cursor CURSOR FOR
SELECT GAC.Id
FROM GlobalIdActivities GAC
WHERE EXISTS (SELECT 1 FROM @ShipmentToInsertTable WHERE CONCAT('SHI_', ClonedFromShipmentId) = GAC.GlobalId)

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @Id  
WHILE @@FETCH_STATUS = 0  
BEGIN  

	INSERT INTO GlobalIdActivities([GlobalId]
           ,[ActivityId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[Location]
           ,[Remark]
           ,[ActivityDate])
	OUTPUT		inserted.[Id]
				,@Id
				,inserted.[GlobalId]
				,inserted.[ActivityId]
				,inserted.[CreatedBy]
				,inserted.[CreatedDate]
				,inserted.[UpdatedBy]
				,inserted.[UpdatedDate]
				,inserted.[Location]
				,inserted.[Remark]
				,inserted.[ActivityDate]
	INTO @GlobalIdActivitiesToInsertTable (
				[NewGlobalIdActivityId]
				,[ClonedFromGlobalIdActivityId]
				,[GlobalId]
				,[ActivityId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,[Location]
				,[Remark]
				,[ActivityDate]
				)

	SELECT	(SELECT TOP(1) CONCAT('SHI_' , [NewShipmentId]) FROM @ShipmentToInsertTable WHERE CONCAT('SHI_', ClonedFromShipmentId) = GAC.GlobalId) -- Put Id from cloned Shipment CONCAT('SHI_', @Id)
           ,[ActivityId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[Location]
           ,[Remark]
           ,[ActivityDate]
	FROM GlobalIdActivities GAC
	WHERE GAC.Id = @Id

	FETCH NEXT FROM id_cursor   
    INTO @Id

END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

SELECT * FROM @GlobalIdActivitiesToInsertTable

END

BEGIN /* To clone Attachments */

DECLARE @GlobalIdAttachmentToInsertTable AS TABLE (
	[NewGlobalId] [nvarchar](450) NOT NULL,
	[ClonedFromGlobalId] [nvarchar](450) NULL,
	[GlobalId] [nvarchar](450) NOT NULL,
	[AttachemntId] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL
)

DECLARE @String NVARCHAR(450);
DECLARE id_cursor CURSOR FOR
SELECT GAT.GlobalId, GAT.AttachemntId
FROM GlobalIdAttachments GAT
WHERE EXISTS (SELECT 1 FROM @ShipmentToInsertTable WHERE CONCAT('SHI_', ClonedFromShipmentId) = GAT.GlobalId)

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @String, @Id  
WHILE @@FETCH_STATUS = 0  
BEGIN  

	INSERT INTO GlobalIdAttachments([GlobalId]
           ,[AttachemntId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate])
	OUTPUT		inserted.[GlobalId]
				,@String
				,inserted.[GlobalId]
				,inserted.[AttachemntId]
				,inserted.[CreatedBy]
				,inserted.[CreatedDate]
				,inserted.[UpdatedBy]
				,inserted.[UpdatedDate]
	INTO @GlobalIdAttachmentToInsertTable (
				[NewGlobalId]
				,[ClonedFromGlobalId]
				,[GlobalId]
				,[AttachemntId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				)

	SELECT	(SELECT TOP(1) CONCAT ('SHI_', [NewShipmentId]) FROM @ShipmentToInsertTable WHERE CONCAT('SHI_', ClonedFromShipmentId) = GAT.GlobalId )
           ,GAT.[AttachemntId]
           ,GAT.[CreatedBy]
           ,GAT.[CreatedDate]
           ,GAT.[UpdatedBy]
           ,GAT.[UpdatedDate]
	FROM GlobalIdAttachments GAT
	WHERE GAT.GlobalId = @String AND GAT.AttachemntId = @Id

	FETCH NEXT FROM id_cursor   
    INTO @String, @Id

END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

SELECT * FROM @GlobalIdAttachmentToInsertTable

END

BEGIN /* To clone Shipment Dialog */

DECLARE @NotesToInsertTable AS TABLE (
	[NewId] [bigint] NOT NULL,
	[ClonedFromNoteId] [bigint] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[GlobalObjectId] [nvarchar](450) NOT NULL,
	[Category] [nvarchar](250) NULL,
	[NoteText] [nvarchar](max) NOT NULL,
	[ExtendedData] [nvarchar](max) NULL,
	[Owner] [nvarchar](max) NULL
)


DECLARE id_cursor CURSOR FOR
SELECT NTE.Id
FROM Notes NTE
WHERE EXISTS (SELECT 1 FROM @ShipmentToInsertTable WHERE CONCAT('SHI_', ClonedFromShipmentId) = NTE.GlobalObjectId)

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @Id  
WHILE @@FETCH_STATUS = 0  
BEGIN  

	INSERT INTO Notes([CreatedBy]
			   ,[CreatedDate]
			   ,[UpdatedBy]
			   ,[UpdatedDate]
			   ,[GlobalObjectId]
			   ,[Category]
			   ,[NoteText]
			   ,[ExtendedData]
			   ,[Owner])
	OUTPUT		inserted.[Id]
				,@Id
				,inserted.[CreatedBy]
				,inserted.[CreatedDate]
				,inserted.[UpdatedBy]
				,inserted.[UpdatedDate]
				,inserted.[GlobalObjectId]
				,inserted.[Category]
				,inserted.[NoteText]
				,inserted.[ExtendedData]
				,inserted.[Owner]
	INTO @NotesToInsertTable (
				[NewId]
				,[ClonedFromNoteId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,[GlobalObjectId]
				,[Category]
				,[NoteText]
				,[ExtendedData]
				,[Owner] )

	SELECT		[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,(SELECT TOP(1) CONCAT('SHI_', [NewShipmentId]) FROM @ShipmentToInsertTable WHERE CONCAT('SHI_', ClonedFromShipmentId) = NTE.GlobalObjectId) -- Put Id from cloned Shipment
				,[Category]
				,[NoteText]
				,[ExtendedData]
				,[Owner]
	FROM Notes NTE
	WHERE NTE.Id = @Id

	FETCH NEXT FROM id_cursor   
    INTO @Id

END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

SELECT * FROM @NotesToInsertTable

END

BEGIN /* To clone BillOfLadingConsignments */

DECLARE @BillOfLadingConsignmentsToInsertTable AS TABLE (
	[NewConsignmentId] [bigint] NOT NULL,
	[ClonedFromConsignmentId] [bigint] NULL,
	[ConsignmentId] [bigint] NOT NULL,
	[BillOfLadingId] [bigint] NOT NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentId] [bigint] NULL,
	[ClonedFromShipmentId] [bigint] NULL
)


DECLARE id_cursor CURSOR FOR
SELECT T.ConsignmentId, T.BillOfLadingId, T.ShipmentId
FROM BillOfLadingConsignments T
WHERE EXISTS (SELECT 1 FROM @ConsignmentToInsertTable WHERE ClonedFromConsignmentId = T.ConsignmentId)

OPEN id_cursor  
  
FETCH NEXT FROM id_cursor   
INTO @Id, @Id1, @Id2
WHILE @@FETCH_STATUS = 0  
BEGIN  

	INSERT INTO BillOfLadingConsignments([ConsignmentId]
           ,[BillOfLadingId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[ShipmentId])
	OUTPUT		inserted.[ConsignmentId]
				,@Id
				,inserted.[ConsignmentId]
				,inserted.[BillOfLadingId]
				,inserted.[CreatedBy]
				,inserted.[CreatedDate]
				,inserted.[UpdatedBy]
				,inserted.[UpdatedDate]
				,inserted.[ShipmentId]
				,@Id2
	INTO @BillOfLadingConsignmentsToInsertTable (
				[NewConsignmentId]
				,[ClonedFromConsignmentId]
				,[ConsignmentId]
				,[BillOfLadingId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,[ShipmentId]
				,[ClonedFromShipmentId])

	SELECT		(SELECT TOP(1) [NewConsignmentId] FROM @ConsignmentToInsertTable WHERE ClonedFromConsignmentId = T.ConsignmentId)
				,[BillOfLadingId]
				,[CreatedBy]
				,[CreatedDate]
				,[UpdatedBy]
				,[UpdatedDate]
				,(SELECT TOP(1) [NewShipmentId] FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = T.ShipmentId)
	
	FROM BillOfLadingConsignments T
	WHERE T.ConsignmentId = @Id AND T.BillOfLadingId = @Id1 AND T.ShipmentId = @Id2

	FETCH NEXT FROM id_cursor   
    INTO @Id, @Id1, @Id2

END   
CLOSE id_cursor;  
DEALLOCATE id_cursor;  

SELECT * FROM @BillOfLadingConsignmentsToInsertTable

END

BEGIN /* To clone BillOfLadingShipmentLoads */

DECLARE @BillOfLadingShipmentLoadsToInsertTable AS TABLE (
	[BillOfLadingId] [bigint] NOT NULL,
	[NewShipmentLoadId] [bigint] NOT NULL,
	[ClonedFromShipmentLoadId] [bigint] NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ContainerId] [bigint] NULL,
	[ConsolidationId] [bigint] NULL,
	[MasterBillOfLadingId] [bigint] NULL,
	[IsFCL] [bit] NOT NULL
)

INSERT INTO BillOfLadingShipmentLoads([BillOfLadingId]
           ,[ShipmentLoadId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[ContainerId]
           ,[ConsolidationId]
           ,[MasterBillOfLadingId]
           ,[IsFCL])

OUTPUT		inserted.[BillOfLadingId]
           ,inserted.[ShipmentLoadId]
           ,inserted.[CreatedBy]
           ,inserted.[CreatedDate]
           ,inserted.[UpdatedBy]
           ,inserted.[UpdatedDate]
           ,inserted.[ContainerId]
           ,inserted.[ConsolidationId]
           ,inserted.[MasterBillOfLadingId]
           ,inserted.[IsFCL]
INTO @BillOfLadingShipmentLoadsToInsertTable (
			[BillOfLadingId]
           ,[NewShipmentLoadId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[ContainerId]
           ,[ConsolidationId]
           ,[MasterBillOfLadingId]
           ,[IsFCL])

SELECT		[BillOfLadingId]
           ,(SELECT TOP(1) [NewShipmentLoadId] FROM @ShipmentLoadToInsertTable WHERE ClonedFromShipmentLoadId = T.ShipmentLoadId)
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[ContainerId]
           ,[ConsolidationId]
           ,[MasterBillOfLadingId]
           ,[IsFCL]
	
FROM BillOfLadingShipmentLoads T
WHERE EXISTS (SELECT 1 FROM @ShipmentLoadToInsertTable WHERE ClonedFromShipmentLoadId = T.ShipmentLoadId)

UPDATE @BillOfLadingShipmentLoadsToInsertTable
SET ClonedFromShipmentLoadId = (SELECT TOP(1) ClonedFromShipmentLoadId FROM @ShipmentLoadToInsertTable WHERE [NewShipmentLoadId] = NewShipmentLoadId)

SELECT * FROM @BillOfLadingShipmentLoadsToInsertTable

END

BEGIN /* To clone ShipmentBillOfLadings */

DECLARE @ShipmentBillOfLadingsToInsertTable AS TABLE (
	[NewShipmentId] [bigint] NOT NULL,
	[ClonedFromShipmentId] [bigint] NULL,
	[BillOfLadingId] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL
)

INSERT INTO [ShipmentBillOfLadings]
           ([ShipmentId]
           ,[BillOfLadingId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate])

OUTPUT		inserted.[ShipmentId]
           ,inserted.[BillOfLadingId]
           ,inserted.[CreatedBy]
           ,inserted.[CreatedDate]
           ,inserted.[UpdatedBy]
           ,inserted.[UpdatedDate]

INTO @ShipmentBillOfLadingsToInsertTable (
			[NewShipmentId]
           ,[BillOfLadingId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate])

SELECT		
           (SELECT TOP(1) [NewShipmentId] FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = T.ShipmentId)
		   ,[BillOfLadingId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
	
FROM ShipmentBillOfLadings T
WHERE EXISTS (SELECT 1 FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = T.ShipmentId)

UPDATE @ShipmentBillOfLadingsToInsertTable
SET ClonedFromShipmentId = (SELECT TOP(1) [ClonedFromShipmentId] FROM @ShipmentToInsertTable WHERE [NewShipmentId] = NewShipmentId)

SELECT * FROM @ShipmentBillOfLadingsToInsertTable

END

BEGIN /* To clone ConsignmentItineraries */

DECLARE @ConsignmentItinerariesToInsertTable AS TABLE (
	[NewConsignmentId] [bigint] NOT NULL,
	[ClonedFromConsignmentId] [bigint] NULL,
	[ItineraryId] [bigint] NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[NewShipmentId] [bigint] NULL,
	[ClonedFromShipmentId] [bigint] NULL,
	[MasterBillId] [bigint] NULL,
	[Sequence] [int] NOT NULL
)

INSERT INTO [dbo].[ConsignmentItineraries]
           ([ConsignmentId]
           ,[ItineraryId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[ShipmentId]
           ,[MasterBillId]
           ,[Sequence])

OUTPUT		inserted.[ConsignmentId]
           ,inserted.[ItineraryId]
           ,inserted.[CreatedBy]
           ,inserted.[CreatedDate]
           ,inserted.[UpdatedBy]
           ,inserted.[UpdatedDate]
           ,inserted.[ShipmentId]
           ,inserted.[MasterBillId]
           ,inserted.[Sequence]

INTO @ConsignmentItinerariesToInsertTable (
			[NewConsignmentId]
           ,[ItineraryId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[NewShipmentId]
           ,[MasterBillId]
           ,[Sequence])

SELECT		
           (SELECT TOP(1) [NewConsignmentId] FROM @ConsignmentToInsertTable WHERE ClonedFromConsignmentId = T.ConsignmentId)
		   ,[ItineraryId]
           ,[CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,(SELECT TOP(1) [NewShipmentId] FROM @ShipmentToInsertTable WHERE ClonedFromShipmentId = T.ShipmentId)
           ,[MasterBillId]
           ,[Sequence]
	
FROM [ConsignmentItineraries] T
WHERE EXISTS (SELECT 1 FROM @ConsignmentToInsertTable WHERE ClonedFromConsignmentId = T.ConsignmentId)

UPDATE @ConsignmentItinerariesToInsertTable
SET ClonedFromConsignmentId = (SELECT TOP(1) ClonedFromConsignmentId FROM @ConsignmentToInsertTable WHERE [NewConsignmentId] = NewConsignmentId),
ClonedFromShipmentId = (SELECT TOP(1) ClonedFromShipmentId FROM @ShipmentToInsertTable WHERE [NewShipmentId] = NewShipmentId)

SELECT * FROM @ConsignmentItinerariesToInsertTable

END

COMMIT TRANSACTION



