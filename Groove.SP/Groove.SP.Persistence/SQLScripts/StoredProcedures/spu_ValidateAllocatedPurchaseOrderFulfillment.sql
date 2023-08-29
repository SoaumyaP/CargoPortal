SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_ValidateAllocatedPurchaseOrderFulfillment', 'P') IS NOT NULL
DROP PROC dbo.spu_ValidateAllocatedPurchaseOrderFulfillment
GO


-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 13 Nov 2020
-- Description:	Validate data of allocated purchase order prior to submit Booking
-- =============================================
CREATE PROCEDURE [dbo].[spu_ValidateAllocatedPurchaseOrderFulfillment]
	@allocatedPOFFId BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;	
    
	DECLARE @result NVARCHAR(256)
	DECLARE @allocatedPOTable TABLE
	(
		Id BIGINT PRIMARY KEY NOT NULL,
		POType INT NOT NULL
	)

	DECLARE @blanketPOTable TABLE
	(
		Id BIGINT PRIMARY KEY NOT NULL,
		POType INT NOT NULL
	)

	DECLARE @allocatedQuantityTable TABLE
	(
		ProductCode NVARCHAR(128) NOT NULL,
		OrderedUnitQty BIGINT NOT NULL
	)

	DECLARE @blanketQuantityTable TABLE
	(
		ProductCode NVARCHAR(128) NOT NULL,
		OrderedUnitQty BIGINT NOT NULL
	)	

	-- Store list of purchase orders which current POFF is using
	INSERT INTO @allocatedPOTable
	SELECT DISTINCT PurchaseOrderId, PO.POType
	FROM POFulfillmentOrders POFFO WITH (NOLOCK) 
	INNER JOIN PurchaseOrders PO WITH (NOLOCK) ON POFFO.PurchaseOrderId = PO.Id
	WHERE POFulfillmentId = @allocatedPOFFId AND PO.POType = 30 AND POFFO.[Status] = 1 AND PO.[Status] = 1

	-- Store list of blanket purchase orders
	INSERT INTO @blanketPOTable
	SELECT DISTINCT BPO.Id, BPO.POType
	FROM PurchaseOrders BPO WITH (NOLOCK)
	INNER JOIN PurchaseOrders APO WITH (NOLOCK) ON BPO.Id = APO.BlanketPOId
	INNER JOIN @allocatedPOTable POT ON POT.Id = APO.Id
	WHERE APO.[Status] = 1 AND APO.POType = 30 AND BPO.[Status] = 1 AND BPO.POType = 20

	-- Store OrderedUnitQty grouped by ProductCode for blanket PO
	INSERT INTO @blanketQuantityTable
	SELECT POL.ProductCode, SUM(POL.OrderedUnitQty)
	FROM @blanketPOTable PO 
	INNER JOIN POLineItems POL WITH (NOLOCK) ON PO.Id = POL.PurchaseOrderId
	GROUP BY POL.ProductCode

	-- Store OrderedUnitQty grouped by ProductCode for all allocated POs
	INSERT INTO @allocatedQuantityTable
	SELECT POL.ProductCode, SUM(POL.OrderedUnitQty)
	FROM PurchaseOrders PO WITH (NOLOCK)
	INNER JOIN @blanketPOTable POT ON POT.Id = PO.BlanketPOId
	INNER JOIN POLineItems POL WITH (NOLOCK) ON PO.Id = POL.PurchaseOrderId
	WHERE PO.[Status] = 1 AND PO.POType = 30
	GROUP BY POL.ProductCode

	-- Comparing on OrderedUnitQty
	IF (
		-- If there is any PO type != Allocated
		EXISTS (
			SELECT * FROM @allocatedPOTable WHERE POType != 30
		)
		OR
		EXISTS (
			SELECT * FROM @blanketQuantityTable
			EXCEPT 
			SELECT * FROM @allocatedQuantityTable
		)
		OR
		EXISTS (
			SELECT * FROM @allocatedQuantityTable
			EXCEPT 
			SELECT * FROM @blanketQuantityTable
		)
	)
	BEGIN
		SET @result = 'false#Allocated PO quantity is incorrect for booking.'
	END
	ELSE
	BEGIN
		SET @result = ''
	END

	SELECT @result AS [Result]
END
GO

