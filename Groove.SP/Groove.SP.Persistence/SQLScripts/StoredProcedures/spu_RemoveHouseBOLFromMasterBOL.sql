SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_RemoveHouseBOLFromMasterBOL', 'P') IS NOT NULL
DROP PROC dbo.spu_RemoveHouseBOLFromMasterBOL
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 14 June 2021
-- Description:	To unlink master bill of lading from shipment.
-- =============================================
CREATE PROCEDURE [dbo].[spu_RemoveHouseBOLFromMasterBOL]

	 @masterBOLId BIGINT,
     @houseBOLId BIGINT,	
     @updatedBy NVARCHAR(512)	

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	DECLARE @updatedDate DATETIME2(7)
	SET @updatedDate = GETUTCDATE();

	-- Variable to stored list of updated id
	DECLARE @consignmentIdTable TABLE (
		Id BIGINT NOT NULL
	)

	-- Section 1: update four related tables

	-- Update then get list of updated Consignment Id
	UPDATE Consignments
	SET MasterBillId = NULL,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	OUTPUT inserted.Id
	INTO @consignmentIdTable (Id)
	WHERE HouseBillId = @houseBOLId

	-- Update by consignment id existing in the above list
	UPDATE ConsignmentItineraries
	SET MasterBillId = NULL,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	WHERE ConsignmentId IN (SELECT Id FROM @consignmentIdTable)

	-- Update by House BL Id
	UPDATE BillOfLadingItineraries
	SET MasterBillOfLadingId = NULL,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	WHERE BillOfLadingId = @houseBOLId

	-- Update by House BL Id
	UPDATE BillOfLadingShipmentLoads
	SET MasterBillOfLadingId = NULL,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	WHERE BillOfLadingId = @houseBOLId

	-- Section 2: if the removed House BL is the last one in the list: Remove all records related to Master BLId

	IF NOT EXISTS (SELECT 1 FROM BillOfLadingShipmentLoads BSL WHERE BSL.MasterBillOfLadingId = @masterBOLId)
	BEGIN

		-- MasterBillItineraries
		DELETE
		FROM MasterBillItineraries
		WHERE MasterBillOfLadingId = @masterBOLId

		-- MasterBillContacts
		DELETE
		FROM MasterBillContacts
		WHERE MasterBillOfLadingId = @masterBOLId
			AND OrganizationRole NOT IN ('Origin Agent' , 'Destination Agent')

	END


END

