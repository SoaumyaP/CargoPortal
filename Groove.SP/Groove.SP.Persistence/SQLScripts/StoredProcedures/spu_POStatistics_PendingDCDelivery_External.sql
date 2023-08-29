SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dong Tran
-- Create date: 2021-1-26
-- Description:	Count number of POs that its shipments has event #2039–Shipment actual arrival at discharge port but no event #2054-Shipment handover to consignee yet
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_POStatistics_PendingDCDelivery_External]
@Affiliates NVARCHAR(MAX),
@FromDate Datetime2(7),
@ToDate Datetime2(7)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT COUNT(PurchaseOrders.Id) AS PendingDCDelivery 
	FROM PurchaseOrders WITH (NOLOCK)
	WHERE PurchaseOrders.[Status] = 1
	
		AND EXISTS (
					SELECT 1
					FROM PurchaseOrderContacts WITH (NOLOCK)  
					WHERE PurchaseOrderContacts.PurchaseOrderId = PurchaseOrders.Id 
						AND PurchaseOrderContacts.OrganizationId IN (SELECT [VALUE] FROM dbo.fn_SplitStringToTable(@Affiliates, ','))
		)
		AND EXISTS (
					SELECT 1
					FROM POFulfillmentOrders WITH (NOLOCK)
					INNER JOIN POFulfillments WITH (NOLOCK) ON POFulfillments.Id = POFulfillmentOrders.POFulfillmentId AND POFulfillments.[Status] = 10
					INNER JOIN Shipments WITH (NOLOCK) ON Shipments.POFulfillmentId  = POFulfillments.Id AND Shipments.[Status] = 'active'
					AND EXISTS (
								SELECT 1
								FROM GlobalIdActivities GACT WITH(NOLOCK)
								INNER JOIN Activities ACT WITH(NOLOCK) ON ACT.Id = GACT.ActivityId
								INNER JOIN GlobalIds GID (NOLOCK) ON GID.Id = GACT.GlobalId AND GID.EntityType = 'FreightScheduler'
								INNER JOIN Itineraries IT (NOLOCK) ON IT.ScheduleId = GID.EntityId
								INNER JOIN ConsignmentItineraries CSMI (NOLOCK) ON CSMI.ItineraryId = IT.Id
								WHERE ACT.ActivityCode = '7002' AND CSMI.ShipmentId = Shipments.Id AND ACT.[Location] = Shipments.ShipTo
								AND ACT.ActivityDate >= @FromDate AND ACT.ActivityDate <= @ToDate
								)
					AND NOT EXISTS 
								(
								SELECT 1
								FROM GlobalIdActivities WITH (NOLOCK)
								INNER JOIN Activities WITH (NOLOCK) ON Activities.Id = GlobalIdActivities.ActivityId
								WHERE GlobalId =  CONCAT('SHI_', Shipments.Id) AND ActivityCode = '2054'
								)
					WHERE POFulfillmentOrders.PurchaseOrderId = PurchaseOrders.Id AND POFulfillmentOrders.[Status] = 1
		)
										
END