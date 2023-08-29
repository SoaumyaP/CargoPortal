SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GeneratePurchaseOrderFulfillmentAllocatedOrders', 'P') IS NOT NULL
DROP PROC dbo.spu_GeneratePurchaseOrderFulfillmentAllocatedOrders
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 20 Sep 2020
-- Description:	Generate data on POFulfillmentAllocatedOrders for blanket POFF. Only work for blanket booking.
-- =============================================
CREATE PROCEDURE [dbo].[spu_GeneratePurchaseOrderFulfillmentAllocatedOrders]
	@blanketPOFFId BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;	
    
	DECLARE @purchaseOrderIdTable TABLE
	(
		Id BIGINT PRIMARY KEY NOT NULL
	)

	DECLARE @containerType INT

	DECLARE @result TABLE
	(
		[PurchaseOrderId] [bigint] NOT NULL,
		[POLineItemId] [bigint] NOT NULL,
		[POFulfillmentId] [bigint] NOT NULL,
		[OrderedQty] [int] NOT NULL,
		[BookedQty] [int] NOT NULL,
		[BalanceQty] [int] NOT NULL,
		[LoadedQty] [int] NOT NULL,
		[OpenQty] [int] NOT NULL,
		[BookedPackage] [int] NULL,
		[Volume] [decimal](18, 4) NULL,
		[GrossWeight] [decimal](18, 2) NULL,
		[NetWeight] [decimal](18, 2) NULL,
		[ShipTo] [nvarchar](512) NULL,
		[ShipToId] [bigint] NULL,
		[ExpectedShipDate] [datetime2](7) NULL,
		[ExpectedDeliveryDate] [datetime2](7) NULL,
		[CargoReadyDate] [datetime2](7) NULL,
		[CustomerPONumber] [nvarchar](MAX) NULL,
		[ProductCode] [nvarchar](MAX) NULL,
		[ProductName] [nvarchar](MAX) NULL,
		[ContainerType] [int] NULL,
		[ProductNumber] [nvarchar](MAX) NULL
	)

	-- Store list of purchase order's Ids which current POFF is using
	INSERT INTO @purchaseOrderIdTable
	SELECT DISTINCT PO.Id
	FROM PurchaseOrders PO WITH (NOLOCK)
	INNER JOIN POFulfillmentOrders POFFO WITH (NOLOCK) ON PO.BlanketPOId = POFFO.PurchaseOrderId
	WHERE POFFO.POFulfillmentId = @blanketPOFFId AND PO.[Status] = 1 AND POFFO.[Status] = 1 AND PO.POType = 30

	SELECT TOP(1) @containerType = EquipmentType
	FROM POFulfillmentLoads WITH (NOLOCK)
	WHERE POFulfillmentId = @blanketPOFFId
	
	INSERT INTO @result
	SELECT 
		T1.PurchaseOrderId,
		T1.POLineItemId,
		T1.POFulfillmentId,
		T1.OrderedQty,
		T1.BookedQty,
		T1.BalanceQty,
		T1.LoadedQty,
		T1.OpenQty,
		T4.BookedPackage,
		T5.Volume,
		T6.GrossWeight,
		NULL AS [NetWeight],
		T1.ShipTo,
		T1.ShipToId,
		T1.ExpectedShipDate,
		T1.ExpectedDeliveryDate,
		T1.CargoReadyDate,
		T1.CustomerPONumber,
		T1.ProductCode,
		T1.ProductName,
		@containerType AS [ContainerType],
		T1.ProductNumber
	FROM
	(
		SELECT
			PO.PONumber AS [CustomerPONumber],
			PO.ShipTo AS [ShipTo],
			PO.ShipToId AS [ShipToId],
			PO.ExpectedShipDate AS [ExpectedShipDate],
			PO.ExpectedDeliveryDate AS [ExpectedDeliveryDate],
			PO.CargoReadyDate AS [CargoReadyDate],
			PO.Id AS [PurchaseOrderId],
			POL.Id AS [POLineItemId],
			@blanketPOFFId AS [POFulfillmentId],
			POl.ProductCode AS [ProductCode],
			POl.ProductName AS [ProductName],
			POL.OrderedUnitQty AS [OrderedQty],
			POl.OrderedUnitQty AS [BookedQty],
			0 AS [BalanceQty],
			0 AS [LoadedQty],
			POl.OrderedUnitQty AS [OpenQty],
			PO.PONumber + '~' + POl.ProductCode + '~' + CONVERT(nvarchar, POl.LineOrder) AS [ProductNumber]

		FROM POLineItems POL WITH (NOLOCK)
		INNER JOIN PurchaseOrders PO WITH (NOLOCK) ON PO.Id = POL.PurchaseOrderId		
		WHERE PO.Id IN (SELECT Id FROM @purchaseOrderIdTable)
	) T1
	OUTER APPLY
    (
		SELECT TOP(1) OrganizationCode AS [CustomerOrgCode]
		FROM PurchaseOrderContacts POC WITH (NOLOCK)
		WHERE t1.PurchaseOrderId = POC.PurchaseOrderId AND POC.OrganizationRole = 'Principal'
    ) T2
	OUTER APPLY
	(
		SELECT
			COALESCE(AM.OuterQuantity, 0) AS [OuterQuantity],
			COALESCE(AM.OuterGrossWeight, 0) AS [OuterGrossWeight],
			COALESCE(AM.OuterDepth, 0) * COALESCE(AM.OuterWidth, 0) * COALESCE(AM.OuterHeight, 0) / 1000000  AS [CBM]
		FROM ArticleMaster AM WITH (NOLOCK)
		WHERE T1.ProductCode = TRIM(AM.ItemNo) AND AM.CompanyCode = T2.CustomerOrgCode
	) T3
	OUTER APPLY
	(
		SELECT 
			CASE WHEN T3.OuterQuantity = 0 THEN 0
				ELSE CAST(CEILING(1.0 * T1.BookedQty /T3.OuterQuantity) AS INT) END AS [BookedPackage]
	)T4
	OUTER APPLY
	(
		SELECT 
			CASE WHEN T4.BookedPackage * T3.CBM = 0 THEN NULL
				ELSE ROUND(1.0 * T4.BookedPackage * T3.CBM, 3) END AS [Volume]
	)T5
	OUTER APPLY
	(
		SELECT 
			CASE WHEN T3.OuterGrossWeight * T4.BookedPackage = 0 THEN NULL
				ELSE ROUND(1.0 * T3.OuterGrossWeight * T4.BookedPackage, 2) END AS [GrossWeight]
	)T6

	SELECT *
	FROM @result
	ORDER BY ShipTo, PurchaseOrderId, POLineItemId
	
END
GO 

