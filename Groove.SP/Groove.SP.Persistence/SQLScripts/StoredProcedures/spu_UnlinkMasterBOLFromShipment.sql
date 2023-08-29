SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_UnlinkMasterBOLFromShipment', 'P') IS NOT NULL
DROP PROC dbo.spu_UnlinkMasterBOLFromShipment
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 06 June 2021
-- Description:	To unlink master bill of lading from shipment.
-- =============================================
CREATE PROCEDURE [dbo].[spu_UnlinkMasterBOLFromShipment]
	
	@shipmentId BIGINT,
	@updatedBy NVARCHAR(512)	

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	DECLARE @updatedDate DATETIME2(7)
	SET @updatedDate = GETUTCDATE();

	-- Variable to stored list of updated id
	DECLARE @masterBLIdTable TABLE (
		Id BIGINT
	)

	-- Section 1: update two related tables

	-- Remove MasterBL's Id out of table Consignments by ShipmentId
	UPDATE Consignments
	SET MasterBillId = NULL,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	OUTPUT deleted.MasterBillId
	INTO @masterBLIdTable
	WHERE ShipmentId = @shipmentId

	-- Remove MasterBL's Id out of table ConsignmentItineraries by ShipmentId
	UPDATE ConsignmentItineraries
	SET MasterBillId = NULL,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	WHERE ShipmentId = @shipmentId	

	-- Section 2: remove the last Shipment out of Master BL, delete all records in tables
	IF NOT EXISTS (SELECT 1 FROM Consignments CSM WHERE CSM.MasterBillId IN (SELECT Id FROM @masterBLIdTable))
	BEGIN

		-- MasterBillItineraries
		DELETE
		FROM MasterBillItineraries
		WHERE MasterBillOfLadingId IN (SELECT Id FROM @masterBLIdTable)

		-- MasterBillContacts, but not remove records with OrgRole = Origin Agent or Destination Agent.
		DELETE
		FROM MasterBillContacts
		WHERE MasterBillOfLadingId IN (SELECT Id FROM @masterBLIdTable)
			AND OrganizationRole NOT IN ('Origin Agent', 'Destination Agent')

	END

END

