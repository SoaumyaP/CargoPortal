
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Dong Tran
-- Create date: June-10-2021
-- Description: Unlink a shipment from HouseBL page
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_UnlinkShipmentFromHouseBL]
 @houseBLId BIGINT,
 @shipmentId BIGINT,
 @isTheLastLinkedShipment BIT = 0,
 @updatedBy NVARCHAR(512)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @updatedDate DATETIME2(7)
	SET @updatedDate = GETUTCDATE();

	-- Variable to stored list of updated id
	DECLARE @consignmentIdTable TABLE (
		Id BIGINT NOT NULL
	)

	DECLARE @ShipmentLoadTEMP TABLE (
		[Id] BIGINT
	)
	INSERT INTO @ShipmentLoadTEMP
	SELECT SL.Id
		FROM ShipmentLoads (NOLOCK) SL
		WHERE 
			EXISTS 
				(
					SELECT 1
					FROM Shipments (NOLOCK) S
					WHERE 
						S.Id = SL.ShipmentId
						AND S.Id = @shipmentId
				)
	BEGIN TRANSACTION;
	DELETE 
	FROM ShipmentBillOfLadings
	WHERE 
		ShipmentId = @shipmentId
		AND BillOfLadingId = @houseBLId

	DELETE 
	FROM BillOfLadingConsignments 
	WHERE 
		ShipmentId = @shipmentId
		AND BillOfLadingId = @houseBLId
	
	DELETE 
	FROM BillOfLadingShipmentLoads
	WHERE 
		BillOfLadingId = @houseBLId
		AND ShipmentLoadId IN (SELECT Id FROM @ShipmentLoadTEMP)

	IF @isTheLastLinkedShipment = 1
	BEGIN 
		DELETE FROM BillOfLadingItineraries
		WHERE 
			BillOfLadingId = @houseBLId

		DELETE FROM BillOfLadingContacts
		WHERE 
			BillOfLadingId = @houseBLId
			AND OrganizationRole NOT IN ('Origin Agent' , 'Destination Agent')
	END

	-- Remove Shipment will also remove MasterBillId out of Consignments, ConsignmentItineraries

	UPDATE Consignments
	SET 
		HouseBillId = NULL,
		MasterBillId = NULL,
		UpdatedDate = @updatedDate,
		UpdatedBy = @updatedBy
	OUTPUT inserted.Id
	INTO @consignmentIdTable
	WHERE 
		ShipmentId = @shipmentId
		AND HouseBillId = @houseBLId

	
	UPDATE ConsignmentItineraries
	SET 
		MasterBillId = NULL,
		UpdatedDate = @updatedDate,
		UpdatedBy = @updatedBy
	WHERE ConsignmentId IN (SELECT Id FROM @consignmentIdTable)

	-- TotalGrossWeight, TotalNetWeight, TotalPackage, and TotalVolume should be recalculated after user removed the added Shipment
	DECLARE @adjustNetWeight decimal(18,4),
			@adjustGrossWeight decimal(18,4),
			@adjustPackage decimal(18,4),
			@adjustVolume decimal(18,4)
	SET @adjustNetWeight = 0;
	SET @adjustGrossWeight = 0;
	SET @adjustPackage = 0;
	SET @adjustVolume = 0;


	SELECT	@adjustNetWeight = SUM(COALESCE(SLD.NetWeight, 0)),
			@adjustGrossWeight = SUM(SLD.GrossWeight),
			@adjustPackage = SUM(SLD.Package),
			@adjustVolume = SUM(SLD.Volume)
	FROM ShipmentLoadDetails SLD
	WHERE SLD.ShipmentId = @shipmentId

	UPDATE BillOfLadings
	SET TotalNetWeight = TotalNetWeight - @adjustNetWeight,
		TotalGrossWeight = TotalGrossWeight - @adjustGrossWeight,
		TotalPackage = TotalPackage - @adjustPackage,
		TotalVolume = TotalVolume - @adjustVolume
	WHERE Id = @houseBLId AND ModeOfTransport = 'Sea' AND @adjustNetWeight IS NOT NULL AND @adjustGrossWeight IS NOT NULL AND @adjustPackage IS NOT NULL

	COMMIT;
END