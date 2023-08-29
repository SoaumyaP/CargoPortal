GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('spu_GetCargoDetailList_ByContainerId', 'P') IS NOT NULL
DROP PROC dbo.spu_GetCargoDetailList_ByContainerId
GO
-- =============================================
-- Author:		Hau Nguyen
-- Create date: 25 March 2020
-- Description:	Get list of Cargo Detail Including Purchare Order/Line Item Info by Container Id 
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetCargoDetailList_ByContainerId]
	@ContainerId NVARCHAR(MAX)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for procedure here

	DECLARE @CargoDetailResult TABLE (
		[Id] BIGINT,
		[ShippingMarks] VARCHAR(MAX),
		[Description] VARCHAR(MAX),
		[Package] DECIMAL(18, 4),
		[PackageUOM] VARCHAR(20),
		[Volume] DECIMAL(18, 4),
		[VolumeUOM] VARCHAR(20),
		[GrossWeight] DECIMAL(18, 4),
		[GrossWeightUOM] VARCHAR(20),
		[Commodity] VARCHAR(128),
		[HSCode] VARCHAR(128),
		[PurchaseOrderId] BIGINT NULL,
		[CustomerPONumber] VARCHAR(MAX),
		[POLineItemId] BIGINT NULL,
		[ProductCode] VARCHAR(128),
		[LineOrder] INT,
		[OrderType] INT
	)

	INSERT INTO @CargoDetailResult
	SELECT cd.[Id]
		 , cd.[ShippingMarks]
		 , cd.[Description]
		 , cd.[Package]
		 , cd.[PackageUOM]
		 , cd.[Volume]
		 , cd.[VolumeUOM]
		 , cd.[GrossWeight]
		 , cd.[GrossWeightUOM]
		 , cd.[Commodity]
		 , cd.[HSCode]
		 , cd.OrderId AS [PurchaseOrderId]
		 , CASE
				WHEN cd.OrderType = 1 THEN po.PONumber
				ELSE co.PONumber
		   END AS [CustomerPONumber]
		 , cd.ItemId AS [POLineItemId]
		 , CASE
				WHEN cd.OrderType = 1 THEN po.ProductCode
				ELSE co.ItemId
		   END AS [ProductCode]
		 , CASE
				WHEN cd.OrderType = 1 THEN po.LineOrder
				ELSE co.POLine
		   END AS [LineOrder]
		 , cd.OrderType
	FROM CargoDetails cd WITH (NOLOCK) 
	INNER JOIN ShipmentLoadDetails sld WITH (NOLOCK) ON cd.Id = sld.CargoDetailId AND sld.ContainerId = @ContainerId
	OUTER APPLY (
		SELECT TOP 1 pod.PONumber, it.ProductCode, it.LineOrder
		FROM PurchaseOrders pod WITH (NOLOCK) INNER JOIN POLineItems it WITH (NOLOCK) ON pod.id = it.PurchaseOrderId AND it.Id = cd.ItemId
		WHERE cd.OrderType = 1 AND pod.id = cd.OrderId
	) po
	OUTER APPLY (
		SELECT TOP 1 cod.PONumber, it.ItemId, it.POLine
		FROM cruise.CruiseOrders cod WITH (NOLOCK) INNER JOIN cruise.CruiseOrderItems it WITH (NOLOCK) ON cod.Id = it.OrderId AND it.Id = cd.ItemId
		WHERE cd.OrderType = 2 AND cod.Id = cd.OrderId
	) co

	SELECT * FROM @CargoDetailResult
END