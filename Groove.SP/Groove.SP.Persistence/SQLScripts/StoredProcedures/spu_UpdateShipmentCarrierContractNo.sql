SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_UpdateShipmentCarrierContractNo', 'P') IS NOT NULL
DROP PROC dbo.spu_UpdateShipmentCarrierContractNo
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 19 July 2021
-- Description:	To update Shipment.CarrierContractNo by Master bill of lading or Shipment
-- =============================================
CREATE PROCEDURE [dbo].[spu_UpdateShipmentCarrierContractNo]
	@masterBOLId BIGINT = NULL,
	@shipmentId BIGINT = NULL,
	@updatedBy NVARCHAR(512)	

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	DECLARE @updatedDate DATETIME2(7)
	SET @updatedDate = GETUTCDATE();

	-- Variable to stored list of updated id
	DECLARE @shipmentIdTable TABLE (
		Id BIGINT NOT NULL
	)

	DECLARE @carrierContractNo VARCHAR(50)

	-- Section 1: get value of CarrierContractNo and related shipments needed to update
	IF (@masterBOLId IS NOT NULL)
	BEGIN
		-- Cases:
		-- 1. Shipment page: Assign House BL (if House BL already has Master BL)
		-- 2. Shipment page: Assign Master BL
		-- 3. Update Master BL
		SELECT @carrierContractNo = MB.CarrierContractNo
		FROM MasterBills MB
		WHERE MB.Id = @masterBOLId

		INSERT INTO @shipmentIdTable
		SELECT CSM.ShipmentId
		FROM Consignments CSM
		WHERE CSM.MasterBillId = @masterBOLId AND CSM.ShipmentId IS NOT NULL
	END
	ELSE 
	BEGIN
		-- Cases: 
		-- 1. House BL page: Link Shipment
		-- 2. House BL page: Assign Master BL
		-- 3. Master BL page: Link House BL
		SELECT @carrierContractNo = MB.CarrierContractNo
		FROM MasterBills MB
		WHERE MB.Id IN ( 
			SELECT TOP(1) CSM.MasterBillId
			FROM Consignments CSM
			WHERE CSM.ShipmentId = @shipmentId AND CSM.MasterBillId IS NOT NULL
		)

		INSERT INTO @shipmentIdTable
		SELECT @shipmentId

	END

	-- Section 2: update related shipments
	-- only update if new value is valid/existing in table ContractMaster
	Update Shipments
	SET CarrierContractNo = @carrierContractNo, UpdatedBy = @updatedBy, UpdatedDate = @updatedDate
	WHERE Id IN (SELECT Id FROM @shipmentIdTable)
		AND EXISTS (SELECT 1 FROM ContractMaster CM WHERE CM.CarrierContractNo = @carrierContractNo)

END

