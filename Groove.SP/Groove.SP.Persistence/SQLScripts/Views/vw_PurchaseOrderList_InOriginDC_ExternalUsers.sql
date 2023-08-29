
IF EXISTS(SELECT * FROM sys.views WHERE NAME = 'vw_PurchaseOrderList_InOriginDC_ExternalUsers')
BEGIN
	 DROP VIEW vw_PurchaseOrderList_InOriginDC_ExternalUsers;
END
GO

CREATE VIEW [dbo].[vw_PurchaseOrderList_InOriginDC_ExternalUsers]
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
PurchaseOrdersCTE AS 
(
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
            (SELECT TOP(1) PC.CompanyName FROM PurchaseOrderContacts PC WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationRole = 'Supplier') AS Supplier,
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
		CROSS APPLY (
				SELECT  ACT.ActivityDate
				FROM POFulfillmentOrders POFFO WITH(NOLOCK)
				INNER JOIN POFulfillments POFF WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId AND POFF.[Status] = 10 AND POFF.Stage >= 20
				INNER JOIN Shipments SHI WITH (NOLOCK) ON SHI.POFulfillmentId = POFF.Id AND SHI.[Status] = 'active'
				CROSS APPLY (
							SELECT ACT.ActivityDate
							FROM GlobalIdActivities GACT WITH(NOLOCK)
							INNER JOIN Activities ACT WITH(NOLOCK) ON ACT.Id = GACT.ActivityId
							WHERE ACT.ActivityCode = '2014' AND GACT.GlobalId = CONCAT('SHI_', SHI.Id)
				)ACT
				WHERE POFFO.PurchaseOrderId = PO.Id AND POFFO.[Status] = 1
					AND NOT EXISTS (
							SELECT 1
							FROM GlobalIdActivities GACT WITH(NOLOCK)
							INNER JOIN Activities ACT WITH(NOLOCK) ON ACT.Id = GACT.ActivityId
							INNER JOIN GlobalIds G (NOLOCK) ON G.Id = GACT.GlobalId AND G.EntityType = 'FreightScheduler'
							INNER JOIN Itineraries IT (NOLOCK) ON IT.ScheduleId = G.EntityId
							INNER JOIN ConsignmentItineraries CSMI (NOLOCK) ON CSMI.ItineraryId = IT.Id
							WHERE CSMI.ShipmentId = SHI.Id AND ACT.ActivityCode IN ('7001', '7002')
						)
					AND NOT EXISTS 
								(
								SELECT 1
								FROM GlobalIdActivities WITH (NOLOCK)
								INNER JOIN Activities WITH (NOLOCK) ON Activities.Id = GlobalIdActivities.ActivityId
								WHERE GlobalId =  CONCAT('SHI_', SHI.Id) AND ActivityCode = '2054'
								)
			)ACT
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
)
SELECT 
		PO.Id,
		PO.PONumber,
		po.CustomerReferences, 
		PO.CreatedBy,
		PO.POIssueDate,
		PO.[Status],
		PO.Stage,
		PO.POType,
		PO.CargoReadyDate,
		PO.CreatedDate,
		MAX(PO.ActivityDate) AS ActivityDate,
		OrganizationId,
        PO.Supplier,
		IsProgressCargoReadyDates,
		ProgressNotifyDay,
		po.ProductionStarted,
		po.ModeOfTransport,
		po.ShipFrom,
		po.ShipTo,
		po.Incoterm,
		po.ExpectedDeliveryDate,
		po.ExpectedShipDate,
		po.ContainerType,
		po.PORemark
FROM PurchaseOrdersCTE PO
GROUP BY 	
		PO.Id,
		PO.PONumber,
		po.CustomerReferences, 
		PO.CreatedBy,
		PO.POIssueDate,
		PO.[Status],
		PO.Stage,
		PO.POType,
		PO.CargoReadyDate,
		PO.CreatedDate,
		OrganizationId,
		PO.Supplier,
		IsProgressCargoReadyDates,
		ProgressNotifyDay,
		po.ProductionStarted,
		po.ModeOfTransport,
		po.ShipFrom,
		po.ShipTo,
		po.Incoterm,
		po.ExpectedDeliveryDate,
		po.ExpectedShipDate,
		po.ContainerType,
		po.PORemark
	
GO

