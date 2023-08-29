SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dong Tran
-- Create date: 2021-1-26
-- Description:	Count number of POs that its shipment in customs cleared: #2041-Shipment import customs cleared exists and no #2054
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_POStatistics_CustomsCleared_External]
	@Affiliates NVARCHAR(MAX),
	@FromDate Datetime2(7),
	@ToDate Datetime2(7)				
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT COUNT(PurchaseOrders.Id) AS CustomsCleared 
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
								FROM GlobalIdActivities WITH (NOLOCK)
								INNER JOIN Activities A WITH (NOLOCK) ON A.Id = GlobalIdActivities.ActivityId
								WHERE GlobalId =  CONCAT('SHI_', Shipments.Id) AND ActivityCode = '2041'
								AND A.ActivityDate >= @FromDate AND A.ActivityDate <= @ToDate
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