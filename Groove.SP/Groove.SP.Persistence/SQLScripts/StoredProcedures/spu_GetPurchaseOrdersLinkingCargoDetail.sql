SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetPurchaseOrdersLinkingCargoDetail', 'P') IS NOT NULL
DROP PROC dbo.spu_GetPurchaseOrdersLinkingCargoDetail
GO

-- =============================================
-- Author:		Dong Tran
-- Create date: 08 April 2021
-- Description:	To get Purchase Order information which should be linked to provided cargo detail
-- Updated on 23 Nov 2022 by Cuong Duong CPT-102/ CPT-138: To support link to Freight Purchase Orders: 1. original freight, 2. cloned from Cruise
-- =============================================

CREATE PROCEDURE [dbo].[spu_GetPurchaseOrdersLinkingCargoDetail]
	@ShipmentId BIGINT,
	@PONumber NVARCHAR(MAX),
	@LineOrder BIGINT,
	@ScheduleLineNo BIGINT = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	DECLARE @PrincipalOrganizationId BIGINT;
	DECLARE @ShipmentNumber VARCHAR(50);
	
	SET @PrincipalOrganizationId = 
		(
			SELECT TOP(1) SC.OrganizationId
			FROM ShipmentContacts SC WITH (NOLOCK)
			INNER JOIN Shipments S ON S.Id = SC.ShipmentId
			WHERE 
				SC.ShipmentId = @ShipmentId
				AND S.OrderType = 1 
				AND SC.OrganizationRole = 'Principal'
		 );

	SET @ShipmentNumber =
		(
			SELECT TOP(1) ShipmentNo
			FROM Shipments WITH (NOLOCK)
			WHERE Id = @ShipmentId
		);

	IF (@ShipmentNumber LIKE '%CSP')
	BEGIN
		-- Shipment that cloned from Cruise -> link to cloned Purchase Order from Cruise
		-- Linked by Principal organization id, Purchase order number, item line order and Consignee companyName name

		DECLARE @ConsigneeOrganizationName NVARCHAR(100);
		SET @ConsigneeOrganizationName = 
		(
			SELECT TOP(1) SC.CompanyName
			FROM ShipmentContacts SC WITH (NOLOCK)
			INNER JOIN Shipments S ON S.Id = SC.ShipmentId
			WHERE 
				SC.ShipmentId = @ShipmentId
				AND S.OrderType = 1 -- Shipment cloned from Cruise has OrderType = 1 (Freight)
				AND SC.OrganizationRole = 'Consignee'
		)

		SELECT PO.Id AS OrderId, T2.Id AS ItemId
		FROM PurchaseOrders PO WITH (NOLOCK)
		CROSS APPLY (
			SELECT TOP(1) POC.PurchaseOrderId
			FROM PurchaseOrderContacts POC WITH (NOLOCK)
			WHERE 
				PO.Id = POC.PurchaseOrderId  
				AND POC.OrganizationRole = 'Principal' 
				AND POC.OrganizationId = @PrincipalOrganizationId
		) T
		CROSS APPLY (
			SELECT TOP(1) POC.PurchaseOrderId
			FROM PurchaseOrderContacts POC WITH (NOLOCK)
			WHERE 
				PO.Id = POC.PurchaseOrderId  
				AND POC.OrganizationRole = 'Consignee' 
				AND POC.CompanyName = @ConsigneeOrganizationName
		) T1
		CROSS APPLY (
			SELECT TOP(1) POLine.Id
			FROM POLineItems POLine WITH (NOLOCK)
			WHERE T.PurchaseOrderId = POLine.PurchaseOrderId AND POLine.LineOrder = @LineOrder
		) T2
		WHERE PONumber = @PONumber
		
	END
	ELSE
	BEGIN
		-- Freight Shipment -> link to original Freight Purchase Order
		SELECT PO.Id AS OrderId, T1.Id AS ItemId
		FROM PurchaseOrders PO WITH (NOLOCK)
		CROSS APPLY (
			SELECT TOP(1) POC.PurchaseOrderId
			FROM PurchaseOrderContacts POC WITH (NOLOCK)
			WHERE 
				PO.Id = POC.PurchaseOrderId
				AND POC.OrganizationRole = 'Principal'
				AND POC.OrganizationId = @PrincipalOrganizationId
		) T
		CROSS APPLY (
			SELECT TOP(1) POLine.Id
			FROM POLineItems POLine WITH (NOLOCK)
			WHERE T.PurchaseOrderId = POLine.PurchaseOrderId AND POLine.LineOrder = @LineOrder AND (@ScheduleLineNo IS NULL OR POLine.ScheduleLineNo = @ScheduleLineNo)
		) T1
		WHERE PONumber = @PONumber
	END
END

