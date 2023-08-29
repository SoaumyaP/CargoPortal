SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dong Tran
-- Create date: 2021-1-26
-- Description:	count number of POs that its shipment already departure: #2029-Shipment actual departure from origin port exists but no #2039 & #2054
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_POStatistics_InTransit_External]
@Affiliates NVARCHAR(MAX),
@FromDate Datetime2(7),
@ToDate Datetime2(7)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT COUNT(PurchaseOrders.Id) AS InTransit 
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
					INNER JOIN Shipments SHI WITH (NOLOCK) ON SHI.POFulfillmentId  = POFulfillments.Id AND SHI.[Status] = 'active'
					AND NOT EXISTS
					(
						SELECT 1
						FROM GlobalIdActivities GIDACT (NOLOCK)
						INNER JOIN Activities ACT (NOLOCK) ON ACT.Id = GIDACT.ActivityId
						WHERE (
							ACT.ActivityCode = '2054' AND GIDACT.GlobalId = CONCAT('SHI_', SHI.Id)
						)
					)
					WHERE POFulfillmentOrders.PurchaseOrderId = PurchaseOrders.Id AND POFulfillmentOrders.[Status] = 1
					AND EXISTS 
					(
								SELECT 1
								FROM ConsignmentItineraries CSMI (NOLOCK) INNER JOIN Itineraries IT (NOLOCK) ON IT.Id = CSMI.ItineraryId AND ScheduleId IS NOT NULL
								WHERE CSMI.ShipmentId = SHI.Id AND EXISTS (
									SELECT 1
									FROM  GlobalIdActivities GIDACT (NOLOCK)
									INNER JOIN Activities ACT7001 (NOLOCK) ON ACT7001.Id = GIDACT.ActivityId AND ACT7001.ActivityCode = '7001'
									WHERE GIDACT.GlobalId = (CONCAT('FSC_', IT.ScheduleId))
									AND ACT7001.ActivityDate >= @FromDate AND ACT7001.ActivityDate <= @ToDate
								)
					)
					AND NOT EXISTS
					(
								SELECT 1
								FROM ConsignmentItineraries CSMI (NOLOCK) INNER JOIN Itineraries IT (NOLOCK) ON IT.Id = CSMI.ItineraryId AND ScheduleId IS NOT NULL
								WHERE CSMI.ShipmentId = SHI.Id AND EXISTS (
									SELECT 1
									FROM  GlobalIdActivities GIDACT (NOLOCK)
									INNER JOIN Activities ACT7002 (NOLOCK) ON ACT7002.Id = GIDACT.ActivityId AND ACT7002.ActivityCode = '7002'
									WHERE GIDACT.GlobalId = (CONCAT('FSC_', IT.ScheduleId))
								)
					)
		)

						
END