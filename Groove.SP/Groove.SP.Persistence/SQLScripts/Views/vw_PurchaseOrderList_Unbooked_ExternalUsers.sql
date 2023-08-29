
IF EXISTS(SELECT * FROM sys.views WHERE NAME = 'vw_PurchaseOrderList_Unbooked_ExternalUsers')
BEGIN
	 DROP VIEW vw_PurchaseOrderList_Unbooked_ExternalUsers;
END
GO

CREATE VIEW [dbo].[vw_PurchaseOrderList_Unbooked_ExternalUsers]
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
	
			AND NOT EXISTS (
				SELECT 1
				FROM POFulfillmentOrders POFFO WITH(NOLOCK)
				INNER JOIN POFulfillments POFF WITH(NOLOCK) ON POFF.Id = POFFO.POFulfillmentId AND POFF.[Status] = 10 AND POFF.Stage > 10
				WHERE POFFO.PurchaseOrderId = PO.Id
			)
			AND NOT EXISTS (
				SELECT 1
				FROM POFulfillmentAllocatedOrders POFFAO WITH(NOLOCK)
				INNER JOIN POFulfillments POFF WITH(NOLOCK) ON POFF.Id = POFFAO.POFulfillmentId AND POFF.[Status] = 10 AND POFF.Stage > 10 AND POFF.FulfilledFromPOType = 20
				WHERE POFFAO.PurchaseOrderId = PO.Id
			)
)
SELECT *
FROM PurchaseOrdersCTE
	
GO
