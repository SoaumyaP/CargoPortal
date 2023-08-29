SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_ValidateBlanketPurchaseOrderFulfillment', 'P') IS NOT NULL
DROP PROC dbo.spu_ValidateBlanketPurchaseOrderFulfillment
GO


-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 19 Sep 2020
-- Description:	Validate data of blanket purchase order prior to submit Booking
-- =============================================
CREATE PROCEDURE [dbo].[spu_ValidateBlanketPurchaseOrderFulfillment]
	@blanketPOFFId BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;	
    
	DECLARE @result NVARCHAR(256)
	DECLARE @blanketPOTable TABLE
	(
		Id BIGINT PRIMARY KEY NOT NULL,
		POType INT NOT NULL
	)

	DECLARE @blanketQuantityTable TABLE
	(
		ProductCode NVARCHAR(128) NOT NULL,
		OrderedUnitQty BIGINT NOT NULL
	)
	DECLARE @allocatedQuantityTable TABLE
	(
		ProductCode NVARCHAR(128) NOT NULL,
		OrderedUnitQty BIGINT NOT NULL
	)

	-- Store list of purchase orders which current POFF is using
	INSERT INTO @blanketPOTable
	SELECT DISTINCT PurchaseOrderId, PO.POType
	FROM POFulfillmentOrders POFFO WITH (NOLOCK) 
	INNER JOIN PurchaseOrders PO  WITH (NOLOCK) ON POFFO.PurchaseOrderId = PO.Id
	WHERE POFulfillmentId = @blanketPOFFId AND PO.POType = 20 AND POFFO.[Status] = 1 AND PO.[Status] = 1

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
		-- If there is any PO type <> Blanket
		EXISTS (
			SELECT * FROM @blanketPOTable WHERE POType != 20
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
		SET @result = 'false#Blanket PO quantity is incorrect for booking.'
	END
	ELSE
	BEGIN
		SET @result = ''
	END

	SELECT @result AS [Result]
END
GO

