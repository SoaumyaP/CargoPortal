SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
IF OBJECT_ID('spu_POStatistics_ManagedToDate', 'P') IS NOT NULL
DROP PROC dbo.spu_POStatistics_ManagedToDate
GO
-- =============================================
-- Author:		Cuong Duong
-- Create date: 27 Jan 2021
-- Description:	Count Freight PO "this year" only (sample: 2021), and shipment has #2054 (Shipment Closed)
-- =============================================

CREATE PROCEDURE [dbo].[spu_POStatistics_ManagedToDate]
	-- Input
	@IsInternal BIT = 0,
	@Affiliates NVARCHAR(MAX),
	@FromDate Datetime2(7),
	@ToDate Datetime2(7)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @CurrentYear INT
	SET @CurrentYear = YEAR(GETDATE())

	DECLARE @LatestCurrencyRateTbl TABLE (
		Code NCHAR(3),
		LatestStartDate DATETIME2(7)
	)
	INSERT INTO @LatestCurrencyRateTbl(Code, LatestStartDate)
	SELECT Code, MAX(StartDate) StartDate
    FROM [dbo].[CurrencyExchangeRates] WITH (NOLOCK)
	WHERE StartDate <= GETDATE()
    GROUP BY Code

	DECLARE @CurrencyRateTbl TABLE (
		CurrencyCode NCHAR(3),
		CurrencyRateToUSD DECIMAL(5,4)
	)
	INSERT INTO @CurrencyRateTbl(CurrencyCode,CurrencyRateToUSD)
	SELECT c.Code, c.ExchangeRate
	FROM [dbo].[CurrencyExchangeRates] c WITH (NOLOCK)
	INNER JOIN @LatestCurrencyRateTbl l
	ON c.Code = l.Code AND c.StartDate = l.LatestStartDate

	--SELECT * FROM @CurrencyRateTbl

	-- If internal user
	IF (@IsInternal = 1)
	BEGIN
		;WITH CTE AS
		(
			SELECT 
				PO.Id AS PurchaseOrderId, 
				CD.Id AS CargoDetailId,
				CD.ItemId AS ItemId,
				ISNULL(CD.Volume, 0) AS Volume, 
				ISNULL(CD.Package, 0) AS Package,
				CD.Unit AS UnitQuantity,
				ISNULL(T2.UnitPrice, 0) AS UnitPrice,
				ISNULL(T2.CurrencyRateToUSD, 0) AS CurrencyRateToUSD
			FROM PurchaseOrders PO WITH (NOLOCK)

			INNER JOIN CargoDetails CD WITH (NOLOCK) ON CD.OrderId = PO.Id 
														-- Only for Freight
														AND CD.OrderType = 1

			OUTER APPLY (
				SELECT TOP(1) CurrencyRateToUSD, UnitPrice
				FROM POLineItems POL WITH (NOLOCK)
				INNER JOIN @CurrencyRateTbl TBL ON TBL.CurrencyCode = POL.CurrencyCode
				WHERE POL.Id = CD.ItemId

			) T2

			WHERE 
			-- Must active Purchase order
			PO.[Status] = 1	
			
			-- Shipment is closed 2054 and departure 7001 in this year
			AND EXISTS (	SELECT 1 
							FROM POFulfillmentOrders POFFO WITH (NOLOCK)
								INNER JOIN POFulfillments POFF WITH (NOLOCK) ON POFF.Id = POFFO.POFulfillmentId AND POFF.[Status] = 10
								INNER JOIN Shipments SHI WITH (NOLOCK) ON SHI.POFulfillmentId  = POFF.Id AND SHI.[Status] = 'active'								
							WHERE  POFFO.PurchaseOrderId = PO.Id AND POFFO.[Status] = 1 
									-- Event 2054
									AND EXISTS (
											SELECT 1
											FROM Activities ACT2054 WITH (NOLOCK)
											INNER JOIN GlobalIdActivities GLA WITH (NOLOCK) ON GLA.GlobalId = CONCAT('SHI_', SHI.Id) AND ACT2054.Id = GLA.ActivityId
											WHERE ACT2054.ActivityCode = '2054'
										)
									-- Event 7001 of this year
									AND EXISTS (
											SELECT 1
											FROM Activities ACT7001 WITH (NOLOCK)
											INNER JOIN GlobalIdActivities GLA WITH (NOLOCK) ON ACT7001.Id = GLA.ActivityId
											INNER JOIN GlobalIds GID (NOLOCK) ON GID.Id = GLA.GlobalId AND GID.EntityType = 'FreightScheduler'
											INNER JOIN Itineraries IT (NOLOCK) ON IT.ScheduleId = GID.EntityId
											INNER JOIN ConsignmentItineraries CSMI (NOLOCK) ON CSMI.ItineraryId = IT.Id
											WHERE ACT7001.ActivityCode = '7001' AND ACT7001.ActivityDate >= @FromDate AND ACT7001.ActivityDate <= @ToDate AND CSMI.ShipmentId = SHI.Id
										)
						)
		)

		-- Return data
		SELECT 
			CAST(COUNT(DISTINCT PurchaseOrderId) AS BIGINT) AS NumberOfPO,
			CAST(ISNULL(SUM(Volume), 0) AS DECIMAL(18,3)) AS TotalCBM,
			CAST(ISNULL(SUM (Package), 0) AS BIGINT) AS TotalUnits, 
			CAST(ISNULL(SUM(UnitPrice * UnitQuantity * CurrencyRateToUSD), 0) AS DECIMAL(18,2)) AS TotalFOBPrice
		FROM CTE
	END
	ELSE
	BEGIN
		-- External users

		IF OBJECT_ID('tempdb..#AffiliatesTbl') IS NOT NULL DROP TABLE #AffiliatesTbl
		CREATE TABLE #AffiliatesTbl ([OrganizationId] BIGINT)
		CREATE CLUSTERED INDEX IX_AffiliatesTbl_OrganizationId ON #AffiliatesTbl ([OrganizationId] ASC)

		INSERT INTO #AffiliatesTbl
		SELECT [Value] 	
		FROM fn_SplitStringToTable(@Affiliates,',')

		;WITH CTE AS
		(
			SELECT 
				PO.Id AS PurchaseOrderId, 
				CD.Id AS CargoDetailId,
				CD.ItemId AS ItemId,
				ISNULL(CD.Volume, 0) AS Volume, 
				ISNULL(CD.Package, 0) AS Package,
				CD.Unit AS UnitQuantity,
				ISNULL(T2.UnitPrice, 0) AS UnitPrice,
				ISNULL(T2.CurrencyRateToUSD, 0) AS CurrencyRateToUSD
			FROM PurchaseOrders PO WITH (NOLOCK)

			INNER JOIN CargoDetails CD WITH (NOLOCK) ON CD.OrderId = PO.Id 
														-- Only for Freight
														AND CD.OrderType = 1

			OUTER APPLY (
				SELECT TOP(1) CurrencyRateToUSD, UnitPrice
				FROM POLineItems POL WITH (NOLOCK)
				INNER JOIN @CurrencyRateTbl TBL ON TBL.CurrencyCode = POL.CurrencyCode
				WHERE POL.Id = CD.ItemId

			) T2

			WHERE 
			-- Must active Purchase order
			PO.[Status] = 1	
			
			-- Check on affiliate
			AND EXISTS (
					SELECT 1
					FROM PurchaseOrderContacts POC WITH(NOLOCK)
					WHERE POC.OrganizationId IN (SELECT [OrganizationId] FROM #AffiliatesTbl)
					AND POC.PurchaseOrderId = PO.Id
			)
			-- Shipment is closed 2054 and departure 7001 in this year
			AND EXISTS (	SELECT 1 
							FROM POFulfillmentOrders POFFO WITH (NOLOCK)
								INNER JOIN POFulfillments POFF WITH (NOLOCK) ON POFF.Id = POFFO.POFulfillmentId AND POFF.[Status] = 10
								INNER JOIN Shipments SHI WITH (NOLOCK) ON SHI.POFulfillmentId  = POFF.Id AND SHI.[Status] = 'active'								
							WHERE  POFFO.PurchaseOrderId = PO.Id AND POFFO.[Status] = 1 
									-- Event 2054
									AND EXISTS (
											SELECT 1
											FROM Activities ACT2054 WITH (NOLOCK)
											INNER JOIN GlobalIdActivities GLA WITH (NOLOCK) ON GLA.GlobalId = CONCAT('SHI_', SHI.Id) AND ACT2054.Id = GLA.ActivityId
											WHERE ACT2054.ActivityCode = '2054' 
										)
									-- Event 7001 of this year
									AND EXISTS (
											SELECT 1
											FROM Activities ACT7001 WITH (NOLOCK)
											INNER JOIN GlobalIdActivities GLA WITH (NOLOCK) ON ACT7001.Id = GLA.ActivityId
											INNER JOIN GlobalIds GID (NOLOCK) ON GID.Id = GLA.GlobalId AND GID.EntityType = 'FreightScheduler'
											INNER JOIN Itineraries IT (NOLOCK) ON IT.ScheduleId = GID.EntityId
											INNER JOIN ConsignmentItineraries CSMI (NOLOCK) ON CSMI.ItineraryId = IT.Id
											WHERE ACT7001.ActivityCode = '7001' AND ACT7001.ActivityDate >= @FromDate AND ACT7001.ActivityDate <= @ToDate  AND CSMI.ShipmentId = SHI.Id
										)
						)
			)

		-- Return data
		SELECT 
			CAST(COUNT(DISTINCT PurchaseOrderId) AS BIGINT) AS NumberOfPO,
			CAST(ISNULL(SUM(Volume), 0) AS DECIMAL(18,3)) AS TotalCBM,
			CAST(ISNULL(SUM (Package), 0) AS BIGINT) AS TotalUnits, 
			CAST(ISNULL(SUM(UnitQuantity * UnitPrice * CurrencyRateToUSD), 0) AS DECIMAL(18,2)) AS TotalFOBPrice
		FROM CTE
	END
END

