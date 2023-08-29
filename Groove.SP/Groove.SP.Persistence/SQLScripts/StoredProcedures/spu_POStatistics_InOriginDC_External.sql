SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dong Tran
-- Create date: 2021-1-26
-- Description:	count number of POs that its shipment already in warehouse (cargo received): #2014-Cargo handover at origin exists but no #2029
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_POStatistics_InOriginDC_External]
	@Affiliates NVARCHAR(MAX),
	@FromDate Datetime2(7),
@ToDate Datetime2(7)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT COUNT(PurchaseOrders.Id) AS OriginDc 
	FROM PurchaseOrders WITH (NOLOCK)
	WHERE PurchaseOrders.[Status] = 1 -- active
		AND EXISTS (
					SELECT 1
					FROM PurchaseOrderContacts WITH (NOLOCK)  
					WHERE PurchaseOrderContacts.PurchaseOrderId = PurchaseOrders.Id 
						AND PurchaseOrderContacts.OrganizationId IN (SELECT [VALUE] FROM dbo.fn_SplitStringToTable(@Affiliates, ','))
		)
		AND EXISTS (
					SELECT 1
					FROM POFulfillmentOrders WITH (NOLOCK)
					INNER JOIN POFulfillments WITH (NOLOCK) ON POFulfillments.Id = POFulfillmentOrders.POFulfillmentId AND POFulfillments.[Status] = 10 -- active
					INNER JOIN Shipments WITH (NOLOCK) ON Shipments.POFulfillmentId  = POFulfillments.Id AND Shipments.[Status] = 'active'
					AND EXISTS (
								SELECT 1
								FROM GlobalIdActivities WITH (NOLOCK)
								INNER JOIN Activities A WITH (NOLOCK) ON A.Id = GlobalIdActivities.ActivityId
								WHERE GlobalId =  CONCAT('SHI_', Shipments.Id) AND ActivityCode = '2014'
									AND A.ActivityDate >= @FromDate AND A.ActivityDate <= @ToDate
								)
					AND NOT EXISTS 
								(
								SELECT 1
								FROM GlobalIdActivities GIDACT (NOLOCK)
								INNER JOIN Activities ACT (NOLOCK) ON ACT.Id = GIDACT.ActivityId
								INNER JOIN GlobalIds GID (NOLOCK) ON GID.Id = GIDACT.GlobalId AND GID.EntityType = 'FreightScheduler'
								INNER JOIN Itineraries IT (NOLOCK) ON IT.ScheduleId = GID.EntityId
								INNER JOIN ConsignmentItineraries CSMI (NOLOCK) ON CSMI.ItineraryId = IT.Id
								WHERE CSMI.ShipmentId = Shipments.Id AND ACT.ActivityCode IN ('7001', '7002')
								)
					AND NOT EXISTS 
								(
								SELECT 1
								FROM GlobalIdActivities WITH (NOLOCK)
								INNER JOIN Activities WITH (NOLOCK) ON Activities.Id = GlobalIdActivities.ActivityId
								WHERE GlobalId =  CONCAT('SHI_', Shipments.Id) AND ActivityCode = '2054'
								)
					WHERE POFulfillmentOrders.PurchaseOrderId = PurchaseOrders.Id AND POFulfillmentOrders.[Status] = 1 -- active
		)
										
END



