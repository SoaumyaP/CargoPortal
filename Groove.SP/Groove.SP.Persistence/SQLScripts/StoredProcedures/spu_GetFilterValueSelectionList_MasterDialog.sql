SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetFilterValueSelectionList_MasterDialog', 'P') IS NOT NULL
DROP PROC spu_GetFilterValueSelectionList_MasterDialog
GO

-- =============================================
-- Author:		Hau Nguyen
-- Create date: 17 March 2021
-- Description:	
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetFilterValueSelectionList_MasterDialog]
	@isInternal BIT,
	@organizationId BIGINT,
	@filterCriteria NVARCHAR(64),
	@filterValue VARCHAR(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;
	DECLARE @masterBOLFilterKey NVARCHAR(64) = 'MASTER BL No.';
	DECLARE @houseBOLFilterKey NVARCHAR(64) = 'House BL No.';
	DECLARE @containerFilterKey NVARCHAR(64) = 'Container No.';
	DECLARE @purchaseOrderFilterKey NVARCHAR(64) = 'Purchase Order No.';
	DECLARE @bookingFilterKey NVARCHAR(64) = 'Booking No.';
	DECLARE @shipmentFilterKey NVARCHAR(64) = 'Shipment No.';

	DECLARE @ResultTbl TABLE (
		Number NVARCHAR(MAX)
	)

	IF (@filterCriteria = @masterBOLFilterKey) -- [dbo].MasterBills
	BEGIN
		IF (@isInternal = 1)
		BEGIN
			INSERT INTO @ResultTbl
			SELECT msb.MasterBillOfLadingNo AS Number
			FROM MasterBills msb WITH(NOLOCK)
		END
		ELSE
		BEGIN
			INSERT INTO @ResultTbl
			SELECT msb.MasterBillOfLadingNo AS Number
			FROM MasterBills msb WITH(NOLOCK)
			WHERE EXISTS (
				SELECT 1
				FROM MasterBillContacts mc WITH(NOLOCK)
				WHERE mc.MasterBillOfLadingId = msb.Id AND mc.OrganizationId = @organizationId
			)
		END
	END
	ELSE IF (@filterCriteria = @houseBOLFilterKey) -- [dbo].BillOfLadings
	BEGIN
		IF (@isInternal = 1)
		BEGIN
			INSERT INTO @ResultTbl
			SELECT bol.BillOfLadingNo AS Number
			FROM BillOfLadings bol WITH(NOLOCK)
		END
		ELSE
		BEGIN
			INSERT INTO @ResultTbl
			SELECT bol.BillOfLadingNo AS Number
			FROM BillOfLadings bol WITH(NOLOCK)
			WHERE EXISTS(
				SELECT 1
				FROM BillOfLadingContacts bc WITH(NOLOCK)
				WHERE bc.BillOfLadingId = bol.Id AND bc.OrganizationId = @organizationId
			)
		END
	END
	ELSE IF (@filterCriteria = @containerFilterKey) -- [dbo].Containers
	BEGIN
		IF (@isInternal = 1)
		BEGIN
			INSERT INTO @ResultTbl
			SELECT c.ContainerNo AS Number
			FROM Containers c WITH(NOLOCK)
		END
		ELSE
		BEGIN
			INSERT INTO @ResultTbl
			SELECT c.ContainerNo AS Number
			FROM Containers c WITH(NOLOCK)
			WHERE (
					IsFCL = 1 AND Id IN (
						SELECT ContainerId
						FROM ShipmentLoads WITH (NOLOCK)
						INNER JOIN Shipments ON ShipmentLoads.ShipmentId = Shipments.Id AND Shipments.Status = 'active'
						INNER JOIN ShipmentContacts WITH (NOLOCK) ON ShipmentContacts.ShipmentId = Shipments.Id
						WHERE ShipmentLoads.ContainerId IS NOT NULL
								AND ShipmentContacts.OrganizationId = @organizationId
					)
				)
				OR
				(
					IsFCL = 0 AND Id IN (
						SELECT Consolidations.ContainerId
						FROM Consolidations WITH (NOLOCK)
						INNER JOIN ShipmentLoads WITH (NOLOCK) ON ShipmentLoads.ConsolidationId = Consolidations.Id
						INNER JOIN Shipments ON ShipmentLoads.ShipmentId = Shipments.Id AND Shipments.Status = 'active'
						INNER JOIN ShipmentContacts WITH (NOLOCK) ON ShipmentContacts.ShipmentId = Shipments.Id
						WHERE Consolidations.ContainerId IS NOT NULL
								AND ShipmentContacts.OrganizationId = @organizationId
					)
				)
		END
	END
	ELSE IF (@filterCriteria = @purchaseOrderFilterKey) -- [dbo].PurchaseOrders
	BEGIN
		IF (@isInternal = 1)
		BEGIN
			INSERT INTO @ResultTbl
			SELECT pod.PONumber AS Number
			FROM PurchaseOrders pod WITH(NOLOCK)
			WHERE pod.[Status] = 1
		END
		ELSE
		BEGIN
			INSERT INTO @ResultTbl
			SELECT pod.PONumber AS Number
			FROM PurchaseOrders pod WITH(NOLOCK)
			WHERE pod.[Status] = 1 AND EXISTS (
				SELECT 1
				FROM PurchaseOrderContacts pc WITH(NOLOCK)
				WHERE pc.PurchaseOrderId = pod.Id AND pc.OrganizationId = @organizationId
			)
		END
	END
	ELSE IF (@filterCriteria = @bookingFilterKey) -- [dbo].POFulfillments
	BEGIN
		IF (@isInternal = 1)
		BEGIN
			INSERT INTO @ResultTbl
			SELECT poff.Number
			FROM POFulfillments poff WITH(NOLOCK)
			WHERE poff.[Status] = 10
		END
		ELSE
		BEGIN
			INSERT INTO @ResultTbl
			SELECT poff.Number
			FROM POFulfillments poff WITH(NOLOCK)
			WHERE poff.[Status] = 10 AND EXISTS (
				SELECT 1
				FROM POFulfillmentContacts poffc WITH(NOLOCK)
				WHERE poffc.POFulfillmentId = poff.Id AND poffc.OrganizationId = @organizationId
			)
		END
	END
	ELSE IF (@filterCriteria = @shipmentFilterKey) -- [dbo].Shipments
	BEGIN
		IF (@isInternal = 1)
		BEGIN
			INSERT INTO @ResultTbl
			SELECT shi.ShipmentNo AS Number
			FROM Shipments shi WITH(NOLOCK)
			WHERE shi.[Status] = 'Active'
		END
		ELSE
		BEGIN
			INSERT INTO @ResultTbl
			SELECT shi.ShipmentNo AS Number
			FROM Shipments shi WITH(NOLOCK)
			WHERE shi.[Status] = 'Active' AND EXISTS (
				SELECT 1
				FROM ShipmentContacts sc WITH(NOLOCK)
				WHERE sc.ShipmentId = shi.Id AND sc.OrganizationId = @organizationId
			)
		END
	END
	-- Return data
	SELECT *
	FROM @ResultTbl
	WHERE Number LIKE CONCAT('%', @filterValue, '%')
END