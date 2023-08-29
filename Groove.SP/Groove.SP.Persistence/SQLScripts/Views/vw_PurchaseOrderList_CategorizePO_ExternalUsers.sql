
IF EXISTS(SELECT * FROM sys.views WHERE NAME = 'vw_PurchaseOrderList_CategorizePO_ExternalUsers')
BEGIN
	 DROP VIEW vw_PurchaseOrderList_CategorizePO_ExternalUsers;
END
GO

CREATE VIEW [dbo].[vw_PurchaseOrderList_CategorizePO_ExternalUsers]
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
            SUP.CompanyName AS Supplier,
			SUP.OrganizationId AS SupplierId,
			CUS.OrganizationId AS CustomerId,
			(
					SELECT TOP(1) PC.CompanyName FROM PurchaseOrderContacts PC WITH(NOLOCK) WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationRole = 'Consignee'
			) AS Consignee,
			POC.OrganizationId AS [OrganizationId],-- to filter with affiliate
			POC.OrganizationRole AS [OrganizationRole],
			(
					 SELECT TOP(1) bcCTE.IsProgressCargoReadyDate
					 FROM BuyerCompliancesCTE BCCTE
					 WHERE PO.Id = BCCTE.PurchaseOrderId
			) AS IsProgressCargoReadyDates,
			(
					SELECT TOP(1) bcCTE.ProgressNotifyDay
					FROM BuyerCompliancesCTE BCCTE 
					WHERE PO.Id = BCCTE.PurchaseOrderId
		     ) AS ProgressNotifyDay,
			 PO.ProductionStarted,
			 PO.ModeOfTransport,
			 PO.ShipFrom,
			 PO.ShipTo,
			 PO.Incoterm,
			 PO.ExpectedDeliveryDate,
			 PO.ExpectedShipDate,
			 PO.ContainerType,
			 PO.PORemark
		FROM PurchaseOrders PO WITH(NOLOCK)
		INNER JOIN PurchaseOrderContacts POC WITH(NOLOCK) ON PO.Id = POC.PurchaseOrderId
		OUTER APPLY (
			SELECT TOP(1) PC.CompanyName, PC.OrganizationId FROM PurchaseOrderContacts PC WITH(NOLOCK) WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationRole = 'Supplier'
		) SUP
		OUTER APPLY (
			SELECT TOP(1) PC.CompanyName, PC.OrganizationId FROM PurchaseOrderContacts PC WITH(NOLOCK) WHERE PO.Id = PC.PurchaseOrderId AND PC.OrganizationRole = 'Principal'
		) CUS
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
)
SELECT *
FROM PurchaseOrdersCTE
	
GO

