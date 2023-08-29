SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_AssignMasterBOLToHouseBOL', 'P') IS NOT NULL
DROP PROC dbo.spu_AssignMasterBOLToHouseBOL
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 31 May 2021
-- Description:	To assign master bill of lading to house bill of lading.
-- BillOfLadings (House BL) <-> MasterBills (Master BL)
-- =============================================
CREATE PROCEDURE [dbo].[spu_AssignMasterBOLToHouseBOL]
	
	@houseBOLId BIGINT,
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

	-- Variable to stored list of updated id
	DECLARE @consignmentItinerariesTable TABLE (
		ConsignmentId BIGINT,
		ItineraryId BIGINT
	)

	-- Section 1: update four related tables

	-- Update then get list of updated Consignment Id
	UPDATE Consignments
	SET MasterBillId = @masterBOLId,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	OUTPUT inserted.Id
	INTO @consignmentIdTable (Id)
	WHERE HouseBillId = @houseBOLId

	-- Update by consignment id existing in the above list
	UPDATE ConsignmentItineraries
	SET MasterBillId = @masterBOLId,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	OUTPUT inserted.ConsignmentId, inserted.ItineraryId
	INTO @consignmentItinerariesTable (ConsignmentId, ItineraryId)
	WHERE ConsignmentId IN (SELECT Id FROM @consignmentIdTable)

	-- Update by House BL Id
	UPDATE BillOfLadingItineraries
	SET MasterBillOfLadingId = @masterBOLId,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	WHERE BillOfLadingId = @houseBOLId

	-- Update by House BL Id
	UPDATE BillOfLadingShipmentLoads
	SET MasterBillOfLadingId = @masterBOLId,
		UpdatedBy = @updatedBy,
		UpdatedDate = @updatedDate
	WHERE BillOfLadingId = @houseBOLId

	-- Section 2: create related data for MasterBillItineraries and MasterBillContacts is missing

	-- If they are not existing in MasterBillItineraries
	-- clone distinct by Itinerary Id
	INSERT INTO MasterBillItineraries (MasterBillOfLadingId, ItineraryId, CreatedBy, CreatedDate, UpdatedBy, UpdatedDate)
	SELECT DISTINCT @masterBOLId, HBI.ItineraryId, @updatedBy, @updatedDate, @updatedBy, @updatedDate
	FROM BillOfLadingItineraries HBI
	WHERE HBI.BillOfLadingId = @houseBOLId 
		AND HBI.ItineraryId NOT IN (SELECT ItineraryId 
									FROM MasterBillItineraries 
									WHERE MasterBillOfLadingId = @masterBOLId)
		AND HBI.ItineraryId IN 
								(
									SELECT ItineraryId
									FROM @consignmentItinerariesTable CI
									INNER JOIN @consignmentIdTable C ON CI.ConsignmentId = C.Id
								)
		AND EXISTS (SELECT * FROM Itineraries WHERE Id = HBI.ItineraryId AND ModeOfTransport = 'Sea')

	-- If they are not existing in MasterBillContacts
	-- clone distinct by Organization id and Organization role
	; WITH DistinctCTE 
	AS (
		SELECT DISTINCT HBC.OrganizationId, HBC.OrganizationRole
		FROM BillOfLadingContacts HBC
		WHERE HBC.BillOfLadingId = @houseBOLId AND HBC.OrganizationRole IN ( 'Origin Agent', 'Destination Agent', 'Principal')

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
		SELECT TOP(1) HBC.OrganizationId, HBC.OrganizationRole, HBC.CompanyName, HBC.[Address], HBC.ContactName, HBC.ContactNumber, HBC.ContactEmail
		FROM BillOfLadingContacts HBC
		WHERE HBC.BillOfLadingId = @houseBOLId AND OrganizationId = CTE.OrganizationId AND OrganizationRole = CTE.OrganizationRole
		ORDER BY Id ASC
	) SHC

END

