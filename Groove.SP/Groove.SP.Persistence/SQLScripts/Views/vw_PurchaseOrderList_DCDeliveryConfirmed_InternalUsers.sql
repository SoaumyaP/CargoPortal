
IF EXISTS(SELECT * FROM sys.views WHERE NAME = 'vw_PurchaseOrderList_DCDeliveryConfirmed_InternalUsers')
BEGIN
	 DROP VIEW vw_PurchaseOrderList_DCDeliveryConfirmed_InternalUsers;
END
GO

CREATE VIEW [dbo].[vw_PurchaseOrderList_DCDeliveryConfirmed_InternalUsers]
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
            (SELECT TOP(1) PC.CompanyName FROM PurchaseOrderContacts PC WITH(NOLOCK) WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationRole = 'Supplier') AS Supplier,
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
		CROSS APPLY (
				SELECT  ACT.ActivityDate
				FROM POFulfillmentOrders POFFO WITH(NOLOCK)
				INNER JOIN POFulfillments POFF WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId AND POFF.[Status] = 10 AND POFF.Stage >= 20
		        INNER JOIN Shipments SHI WITH (NOLOCK) ON SHI.POFulfillmentId = POFF.Id AND SHI.[Status] = 'active'
				CROSS APPLY (
							SELECT TOP 1 ACT.ActivityDate
							FROM GlobalIdActivities GACT WITH(NOLOCK) 
							INNER JOIN Activities ACT WITH(NOLOCK) ON ACT.Id = GACT.ActivityId
							WHERE ACT.ActivityCode = '2053' AND GACT.GlobalId = CONCAT('SHI_', SHI.Id)
						)ACT
				WHERE POFFO.PurchaseOrderId = PO.Id AND POFFO.[Status] = 1
					AND NOT EXISTS (
							SELECT 1
							FROM GlobalIdActivities GACT WITH(NOLOCK)
							INNER JOIN Activities ACT WITH(NOLOCK) ON ACT.Id = GACT.ActivityId
							WHERE ACT.ActivityCode = '2054' AND GACT.GlobalId = CONCAT('SHI_', SHI.Id)
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
