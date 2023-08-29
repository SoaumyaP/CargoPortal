SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('spu_POStatistics_Booked', 'P') IS NOT NULL
DROP PROC dbo.spu_POStatistics_Booked
GO
-- =============================================
-- Author:		Hau Nguyen
-- Create date: 26 Jan 2021
-- Description:	Count number of POs which already booking submitted (include booked and partial booked), exclude booking with activity #2014
--			    Formula: Booking Stage >= FB Request, AND Shipment does not have #2014-cargo handover at origin event.
-- =============================================
CREATE PROCEDURE [dbo].[spu_POStatistics_Booked]
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
	DECLARE @purchaseOrderIdTable TABLE (Id BIGINT NOT NULL)
	DECLARE @resultTable TABLE (Total INT NOT NULL DEFAULT 0)

	INSERT INTO @submittedPOFulfillmentTable
	SELECT POFF.Id AS [POFFId], POFF.FulfilledFromPOType
	FROM POFulfillments POFF WITH(NOLOCK)
	WHERE POFF.Status = 10
	-- Stage >= FB Request
	AND POFF.Stage >= 20
	-- Shipment does not have #2014-cargo handover at origin event.
	AND NOT EXISTS (
		SELECT SM.Id
		FROM GlobalIdActivities GBID WITH(NOLOCK)
		INNER JOIN Shipments SM WITH(NOLOCK) ON GBID.GlobalId = CONCAT('SHI_', SM.Id) AND SM.Status = 'Active'
		INNER JOIN Activities ACTS WITH(NOLOCK) ON GBID.ActivityId = ACTS.Id AND ACTS.ActivityCode = 2014 AND ACTS.ActivityType = 'SM'
		WHERE SM.POFulfillmentId = POFF.ID
	)

	-- IF POFF is blanket
	-- => Get distinct purchase-order-id on [POFulfillmentAllocatedOrders]
	INSERT INTO @purchaseOrderIdTable
	SELECT ALC.PurchaseOrderId AS Id
	FROM @submittedPOFulfillmentTable 
	INNER JOIN POFulfillmentAllocatedOrders ALC WITH(NOLOCK) ON POFFId = ALC.POFulfillmentId
	WHERE FulfilledFromPOType = 20
		AND EXISTS (
			SELECT 1
			FROM PurchaseOrders PO WITH(NOLOCK)
			WHERE 
				PO.Id = ALC.PurchaseOrderId
				-- the CRD within [Today+1; Today+14]
				AND PO.CargoReadyDate >= @FromDate AND PO.CargoReadyDate <= @ToDate
			)
	-- UNION For exception case: The allocated PO is in both allocated booking and blanket booking
	UNION
	-- IF POFF is not blanket
	-- => Get distinct purchase-order-id on [POFulfillmentOrders]
	SELECT ORD.PurchaseOrderId AS Id
	FROM @submittedPOFulfillmentTable 
	INNER JOIN POFulfillmentOrders ORD WITH(NOLOCK) ON POFFId = ORD.POFulfillmentId
	WHERE EXISTS (
			SELECT 1
			FROM PurchaseOrders PO WITH(NOLOCK)
			WHERE 
				PO.Id = ORD.PurchaseOrderId
				-- the CRD within [Today+1; Today+14]
				AND PO.CargoReadyDate >= @FromDate AND PO.CargoReadyDate <= @ToDate
			)
	
	IF (@isInternal = 0)
	BEGIN
		INSERT INTO @resultTable
		SELECT COUNT(POIDS.Id)
		FROM @purchaseOrderIdTable POIDS INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON POIDS.Id = PO.Id
		WHERE EXISTS (
			SELECT *
			FROM PurchaseOrderContacts POC WITH(NOLOCK)
			WHERE POC.OrganizationId IN (SELECT VALUE FROM dbo.fn_SplitStringToTable(@affiliates, ','))
			AND POC.PurchaseOrderId = POIDS.Id
		)
		AND PO.Status = 1
	END
	ELSE
	BEGIN
		INSERT INTO @resultTable
		SELECT COUNT(POIDS.Id)
		FROM @purchaseOrderIdTable POIDS INNER JOIN PurchaseOrders PO WITH(NOLOCK) ON POIDS.Id = PO.Id
		WHERE PO.Status = 1
	END

	-- Output
	SELECT * FROM @resultTable

END