SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetShipmentSelectionList_Cruise', 'P') IS NOT NULL
DROP PROC spu_GetShipmentSelectionList_Cruise
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 10 Dec 2020
-- Description:	To get shipment options as searching by shipment number and filtered by cruise order
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetShipmentSelectionList_Cruise]
	
	@shipmentNumber VARCHAR(50),
	@cruiseOrderId BIGINT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	-- Variables
	DECLARE @PrincipalOrganizationId BIGINT


	-- Get Principal organization id from provided cruise order
	SET  @PrincipalOrganizationId = 
		(SELECT TOP(1) COC.OrganizationId
			FROM cruise.CruiseOrderContacts COC WITH (NOLOCK)
			WHERE COC.OrderId = @cruiseOrderId AND COC.OrganizationRole = 'Principal')


	-- Get shipment information which matches with current filter: principal organization, shipment number

	-- Return data
	SELECT S.Id AS [Id], S.ShipmentNo AS [ShipmentNumber]
	FROM Shipments S WITH (NOLOCK)
	WHERE S.OrderType = 2 AND S.[Status] = 'active' AND S.ShipmentNo LIKE CONCAT('%', @shipmentNumber, '%')
		AND EXISTS (
			SELECT 1 FROM ShipmentContacts SC WITH (NOLOCK)
			WHERE SC.ShipmentId = S.Id AND SC.OrganizationId = @PrincipalOrganizationId AND SC.OrganizationRole = 'Principal'
		)
END

