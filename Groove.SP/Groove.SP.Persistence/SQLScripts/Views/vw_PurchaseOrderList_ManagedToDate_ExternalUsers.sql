
IF EXISTS(SELECT * FROM sys.views WHERE NAME = 'vw_PurchaseOrderList_ManagedToDate_ExternalUsers')
BEGIN
	 DROP VIEW vw_PurchaseOrderList_ManagedToDate_ExternalUsers;
END
GO

CREATE VIEW [dbo].[vw_PurchaseOrderList_ManagedToDate_ExternalUsers]
AS
WITH BuyerCompliancesCTE AS
(
    SELECT
        POC.PurchaseOrderId,
        BC.IsProgressCargoReadyDate,
		BC.IsAllowShowAdditionalInforPOListing,
        BC.ProgressNotifyDay
    FROM BuyerCompliances BC (NOLOCK)
    INNER JOIN PurchaseOrderContacts POC (NOLOCK)
        ON POC.OrganizationId = BC.OrganizationId 
        AND POC.OrganizationRole = 'Principal'
        AND BC.Stage = 1
),
ShipmentCTE as (
	-- Shipment is closed 2054 and departure 7001 in this year
	SELECT SHI.Id, ACT.ActivityDate
	FROM POFulfillments POFF WITH (NOLOCK) INNER JOIN Shipments SHI WITH (NOLOCK) ON SHI.POFulfillmentId = POFF.Id AND SHI.[Status] = 'active'
	CROSS APPLY
	(
				SELECT ACT7001.ActivityDate
				FROM Activities ACT7001 WITH (NOLOCK)
				INNER JOIN GlobalIdActivities GLA WITH (NOLOCK) ON ACT7001.Id = GLA.ActivityId
				INNER JOIN GlobalIds GID (NOLOCK) ON GID.Id = GLA.GlobalId AND GID.EntityType = 'FreightScheduler'
				INNER JOIN Itineraries IT (NOLOCK) ON IT.ScheduleId = GID.EntityId
				INNER JOIN ConsignmentItineraries CSMI (NOLOCK) ON CSMI.ItineraryId = IT.Id
				WHERE ACT7001.ActivityCode = '7001' AND CSMI.ShipmentId = SHI.Id
	) ACT
	WHERE POFF.[Status] = 10
			-- Event 2054
			AND EXISTS (
					SELECT 1
					FROM Activities ACT2054 WITH (NOLOCK)
					INNER JOIN GlobalIdActivities GLA WITH (NOLOCK) ON ACT2054.Id = GLA.ActivityId
					WHERE ACT2054.ActivityCode = '2054' AND GLA.GlobalId = CONCAT('SHI_', SHI.Id)
				)
)

SELECT	
	PO.Id,
	IIF(BC.IsAllowShowAdditionalInforPOListing = 1, CONCAT(po.PONumber,'~',ProductCode,'~',GridValue,'~',LineOrder), po.PONumber) as PONumber,
	po.CustomerReferences, 
	PO.CreatedBy,
	PO.POIssueDate,
	PO.[Status],
	PO.Stage,
	PO.POType,
	PO.CargoReadyDate,
	PO.CreatedDate,
	ACT.ActivityDate,
    (SELECT TOP(1) PC.CompanyName FROM PurchaseOrderContacts PC WITH(NOLOCK) WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationRole = 'Supplier') AS Supplier,
	POC.OrganizationId AS [OrganizationId], -- to filter with affiliate
	(
			SELECT TOP(1) bcCTE.IsProgressCargoReadyDate
			FROM BuyerCompliancesCTE BCCTE 
			WHERE PO.Id = BCCTE.PurchaseOrderId
	) IsProgressCargoReadyDates,
	(
		SELECT TOP(1) bcCTE.ProgressNotifyDay
		FROM BuyerCompliancesCTE BCCTE 
		WHERE PO.Id = BCCTE.PurchaseOrderId
		) ProgressNotifyDay,
		po.ProductionStarted,
		po.ModeOfTransport,
							po.ShipFrom,
							po.ShipTo,
							po.Incoterm,
							po.ExpectedDeliveryDate,
							po.ExpectedShipDate,
							po.ContainerType,
							po.PORemark
FROM PurchaseOrders PO WITH(NOLOCK)
INNER JOIN PurchaseOrderContacts POC WITH(NOLOCK) ON PO.Id = POC.PurchaseOrderId

CROSS APPLY
(	
			SELECT MAX(CTE.ActivityDate) AS ActivityDate
			FROM Shipments SHI WITH (NOLOCK) INNER JOIN ShipmentCTE CTE on SHI.Id = CTE.Id
				INNER JOIN POFulfillmentOrders POD WITH (NOLOCK) ON SHI.POFulfillmentId = POD.POFulfillmentId
			WHERE POD.PurchaseOrderId = PO.Id AND POD.[Status] = 1
			GROUP BY POD.PurchaseOrderId
							
) ACT
OUTER APPLY
		(
			SELECT  IsAllowShowAdditionalInforPOListing
			FROM BuyerCompliancesCTE BC
			WHERE BC.PurchaseOrderId = PO.Id AND IsAllowShowAdditionalInforPOListing = 1
		)BC 
		OUTER APPLY
		(
			  SELECT TOP 1 ProductCode, GridValue, LineOrder
			  FROM POLineItems POL
			  WHERE POL.PurchaseOrderId = PO.Id 
				    AND BC.IsAllowShowAdditionalInforPOListing = 1
		)POL

WHERE PO.[Status] = 1 
	AND EXISTS (
		SELECT 1 
		FROM CargoDetails CD WITH (NOLOCK) 
		WHERE CD.OrderId = PO.Id
			-- Only for Freight
			AND CD.OrderType = 1
	)
	AND ACT.ActivityDate >= DATEADD(yy,DATEDIFF(yy,0,GETDATE())-1,0)
	AND ACT.ActivityDate <= DATEADD(yy, DATEDIFF(yy, 0, GETDATE()) + 1, -1)
	
GO

