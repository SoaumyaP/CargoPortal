GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('spu_POStatistics_Unbooked', 'P') IS NOT NULL
DROP PROC dbo.spu_POStatistics_Unbooked
GO
-- =============================================
-- Author:		Hau Nguyen
-- Create date: 26 Jan 2021
-- Description: Count number of PO has the cargo ready date > today - 14, and still not booked.
--				Formula: Cargo Ready Date > Today-14, AND No booking related or, Draft Booking related.
-- =============================================
CREATE PROCEDURE [dbo].[spu_POStatistics_Unbooked]
	-- Input
	@isInternal BIT = 0,
	@affiliates NVARCHAR(MAX),
	@FromDate Datetime2(7),
	@ToDate Datetime2(7)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @submittedPOFulfillmentTable TABLE (
		POFFId BIGINT NOT NULL,
		FulfilledFromPOType INT NOT NULL
	)
	DECLARE @bookedPurchaseOrderIdTable TABLE (Id BIGINT NOT NULL)
	DECLARE @unbookedPurchaseOrderIdTable TABLE (Id BIGINT NOT NULL)
	DECLARE @resultTable TABLE (Total INT NOT NULL DEFAULT 0)

	INSERT INTO @submittedPOFulfillmentTable
	SELECT POFF.Id AS [POFFId], POFF.FulfilledFromPOType
	FROM POFulfillments POFF WITH(NOLOCK)
	WHERE POFF.Status = 10
	-- Stage > Draf
	AND POFF.Stage > 10

	-- IF POFF is blanket
	-- => Get distinct purchase-order-id on [POFulfillmentAllocatedOrders]
	INSERT INTO @bookedPurchaseOrderIdTable
	SELECT ALC.PurchaseOrderId AS Id
	FROM @submittedPOFulfillmentTable INNER JOIN POFulfillmentAllocatedOrders ALC WITH(NOLOCK) ON POFFId = ALC.POFulfillmentId
	WHERE FulfilledFromPOType = 20
	-- UNION For exception case: The allocated PO is in both allocated booking and blanket booking
	UNION
	-- IF POFF is not blanket
	-- => Get distinct purchase-order-id on [POFulfillmentOrders]
	SELECT ORD.PurchaseOrderId AS Id
	FROM @submittedPOFulfillmentTable INNER JOIN POFulfillmentOrders ORD WITH(NOLOCK) ON POFFId = ORD.POFulfillmentId

	INSERT INTO @unbookedPurchaseOrderIdTable
	SELECT PO.Id FROM PurchaseOrders PO WITH(NOLOCK)
	WHERE PO.Status = 1 
		-- the CRD within [Today+1; Today+14]
		AND PO.CargoReadyDate >= @FromDate AND PO.CargoReadyDate <= @ToDate
	EXCEPT
	SELECT * FROM @bookedPurchaseOrderIdTable
	
	IF (@isInternal = 0)
	BEGIN
		INSERT INTO @resultTable
		SELECT COUNT(POIDS.Id)
		FROM @unbookedPurchaseOrderIdTable POIDS
		WHERE EXISTS (
			SELECT *
			FROM PurchaseOrderContacts POC WITH(NOLOCK)
			WHERE POC.OrganizationId IN (SELECT VALUE FROM dbo.fn_SplitStringToTable(@affiliates, ','))
			AND POC.PurchaseOrderId = POIDS.Id
		)
	END
	ELSE
	BEGIN
		INSERT INTO @resultTable
		SELECT COUNT(Id) FROM @unbookedPurchaseOrderIdTable
	END

	-- Output
	SELECT * FROM @resultTable

END