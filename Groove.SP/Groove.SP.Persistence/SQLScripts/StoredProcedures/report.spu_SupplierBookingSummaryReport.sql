SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'report')
BEGIN
	EXEC('CREATE SCHEMA [report]')
END

IF OBJECT_ID('report.spu_SupplierBookingSummaryReport', 'P') IS NOT NULL
DROP PROC [report].spu_SupplierBookingSummaryReport
GO

-- =============================================
-- Author:		Dong
-- Create date: 14 June 2022
-- Description:	Get data for Warehouse Booking, run on Telerik Reporting Server
-- =============================================
CREATE PROCEDURE [report].[spu_SupplierBookingSummaryReport]
	@SelectedCustomerId BIGINT,
	@PORetentionPeriod INT = 10
AS
BEGIN
	DECLARE @Day CHAR(16) = CONVERT(CHAR(16), GETUTCDATE() - @PORetentionPeriod, 120);

	SELECT
		PC.CompanyName AS 'Principal',
		WL.WarehouseCode AS 'Warehouse Code',
		POF.Number AS 'Booking Number',
		CONVERT(CHAR(10), POF.CreatedDate, 120) AS 'Booking Date',
		SUBSTRING(POFO.CustomerPONumber, 1, 2) AS 'Company',
		SUBSTRING(POFO.CustomerPONumber, 5, 2) AS 'Plan#',
		POFO.CustomerPONumber 'PO Number',
		CASE WHEN POFO.PurchaseOrderId <> 0 THEN 'Yes' ELSE 'No' END 'ASN received',
		PC1.CompanyName AS 'Supplier Name',
		POF.Forwarder AS 'Freight Forwarder',
		CONVERT(CHAR(10), POF.ExpectedDeliveryDate, 120) AS 'Expected Hub Arrival Date',
		POF.[Time] AS 'Delivery Time',
		MAX(CONVERT(CHAR(10), CI.InDate, 120)) AS 'Actual Hub Arrival Date',
		POF.ContainerNo AS 'Truck / Container',
		SUM(POFO.FulfillmentUnitQty) AS 'Piece',
		SUM(POFO.BookedPackage) AS 'Cartons',
		CAST(ROUND(SUM(POFO.Volume),3) AS DECIMAL(18,3)) AS 'CBM'
	FROM POFulfillments POF

	CROSS APPLY 
	(
		SELECT CompanyName
		FROM POFulfillmentContacts
		WHERE 
			POF.Id = POFulfillmentId AND OrganizationRole = 'Principal'
			AND OrganizationId = @SelectedCustomerId
	)PC

	OUTER APPLY 
	(
		SELECT CompanyName
		FROM POFulfillmentContacts
		WHERE POF.Id = POFulfillmentId AND OrganizationRole = 'SUPPLIER'
	)PC1

	OUTER APPLY 
	(
		SELECT TOP 1 WarehouseLocationId
		FROM WarehouseAssignments
		WHERE OrganizationId = @SelectedCustomerId
	)WA

	CROSS APPLY 
	(
		SELECT 
			Id,PurchaseOrderId, FulfillmentUnitQty, BookedPackage, Volume,
			CustomerPONumber
		FROM POFulfillmentOrders
		WHERE POF.Id = POFulfillmentId
	)POFO

	OUTER APPLY
	(
		SELECT  InDate
		FROM POFulfillmentCargoReceiveItems
		WHERE POFulfillmentOrderId = POFO.Id  
	)CI

	OUTER APPLY
	(
		SELECT CONCAT(Code,' - ',Name) AS WarehouseCode
		FROM WarehouseLocations
		WHERE Id = WA.WarehouseLocationId
	)WL

	WHERE
		
		POF.FulfillmentType = 3 
		AND (
			 --Booking stage is before Cargo Received 
			(POF.[Time] IS NULL AND CI.InDate IS NULL) 

			
			-- Booking stage is confirmed 
			OR (
				POF.[Time] IS NOT NULL AND NOT EXISTS(
						SELECT 1 
						FROM POFulfillmentCargoReceiveItems 
						WHERE InDate >= @Day AND POFulfillmentOrderId IN (SELECT Id FROM POFulfillmentOrders  WHERE POFulfillmentId = POF.Id)
					)
				AND POF.Stage = 30
				)

			-- Cargo receive with check all items 
			OR POF.[Time] IS NOT NULL AND CI.InDate >= @Day

			-- Cargo receive without check all items 
			OR (
				POF.[Time] IS NOT NULL AND EXISTS(
						SELECT 1 
						FROM POFulfillmentCargoReceiveItems 
						WHERE InDate >= @Day AND POFulfillmentOrderId IN (SELECT Id FROM POFulfillmentOrders  WHERE POFulfillmentId = POF.Id)
					)
				)

			-- ASN received = No
			OR (POFO.PurchaseOrderId = 0 
				 AND (
					 -- Cargo receive with check all items 
					Time IS NOT NULL AND CI.InDate < @Day

					-- Cargo receive without check all items 
					OR (
						Time IS NOT NULL AND EXISTS(
								SELECT 1 
								FROM POFulfillmentCargoReceiveItems 
								WHERE InDate < @Day AND POFulfillmentOrderId IN (SELECT Id FROM POFulfillmentOrders  WHERE POFulfillmentId = POF.Id)
							)
						)
				 )
			 )
			)

	GROUP BY 
		PC.CompanyName, 
		POF.Forwarder,
		WL.WarehouseCode,
		POF.Number, 
		POF.CreatedDate,
		POFO.CustomerPONumber,
		POFO.PurchaseOrderId,
		PC1.CompanyName,
		POF.ExpectedDeliveryDate,
		POF.[Time],
		POF.ContainerNo
END
GO