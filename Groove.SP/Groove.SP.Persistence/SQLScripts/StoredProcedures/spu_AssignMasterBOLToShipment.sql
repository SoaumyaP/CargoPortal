SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_AssignMasterBOLToShipment', 'P') IS NOT NULL
DROP PROC dbo.spu_AssignMasterBOLToShipment
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 06 June 2021
-- Description:	To assign master bill of lading to shipment.
-- =============================================
CREATE PROCEDURE [dbo].[spu_AssignMasterBOLToShipment]
	
	@shipmentId BIGINT,
	@masterBOLId BIGINT,	
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
	SET MasterBillId = @masterBOLId,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	OUTPUT inserted.Id
	INTO @consignmentIdTable (Id)
	WHERE ShipmentId = @shipmentId 

	-- Update by consignment id existing in the above list
	UPDATE ConsignmentItineraries
	SET MasterBillId = @masterBOLId,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	WHERE ConsignmentId IN (SELECT Id FROM @consignmentIdTable)

	-- Section 2: create related data for MasterBillItineraries and MasterBillContacts is missing

	-- If they are not existing in MasterBillItineraries
	-- clone distinct by Itinerary Id
	INSERT INTO MasterBillItineraries (MasterBillOfLadingId, ItineraryId, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
	SELECT DISTINCT @masterBOLId, CIT.ItineraryId, @updatedBy, @updatedDate, @updatedBy, @updatedDate
	FROM ConsignmentItineraries CIT
	WHERE CIT.ShipmentId = @shipmentId 
		AND CIT.ConsignmentId IN (SELECT Id FROM @consignmentIdTable)
		AND CIT.ItineraryId NOT IN (SELECT ItineraryId 
									FROM MasterBillItineraries 
									WHERE MasterBillOfLadingId = @masterBOLId)
		AND EXISTS (SELECT * FROM Itineraries WHERE Id = CIT.ItineraryId AND ModeOfTransport = 'Sea')

	-- If they are not existing in MasterBillContacts
	-- clone distinct by Organization id and Organization role
	; WITH DistinctCTE 
	AS (
		SELECT DISTINCT SHC.OrganizationId, SHC.OrganizationRole
		FROM ShipmentContacts SHC
		WHERE SHC.ShipmentId = @shipmentId AND SHC.OrganizationRole IN ( 'Origin Agent', 'Destination Agent', 'Principal')

		EXCEPT 

		SELECT DISTINCT MBC.OrganizationId, MBC.OrganizationRole
		FROM MasterBillContacts MBC
		WHERE MBC.MasterBillOfLadingId = @masterBOLId  AND MBC.OrganizationRole IN ( 'Origin Agent', 'Destination Agent', 'Principal')
	)

	INSERT INTO MasterBillContacts (MasterBillOfLadingId, OrganizationId, OrganizationRole, CompanyName, [Address], ContactName, ContactNumber, ContactEmail, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
	
	
	SELECT @masterBOLId, SHC.*, @updatedBy, @updatedDate, @updatedBy, @updatedDate
	FROM DistinctCTE CTE
	CROSS APPLY
	(
		SELECT TOP(1) SHC.OrganizationId, SHC.OrganizationRole, SHC.CompanyName, SHC.[Address], SHC.ContactName, SHC.ContactNumber, SHC.ContactEmail
		FROM ShipmentContacts SHC
		WHERE SHC.ShipmentId = @shipmentId AND OrganizationId = CTE.OrganizationId AND OrganizationRole = CTE.OrganizationRole
		ORDER BY Id ASC
	) SHC


END

