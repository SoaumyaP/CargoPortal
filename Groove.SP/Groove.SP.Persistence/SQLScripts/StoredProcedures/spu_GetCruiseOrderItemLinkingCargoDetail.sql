SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetCruiseOrderItemLinkingCargoDetail', 'P') IS NOT NULL
DROP PROC dbo.spu_GetCruiseOrderItemLinkingCargoDetail
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 04 Dec 2020
-- Description:	To get cruise order item information which should be linked to provided cargo detail
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetCruiseOrderItemLinkingCargoDetail]
	
	@shipmentId BIGINT,
	@cruiseOrderNumber NVARCHAR(512),
	@cruiseOrderItemPOLine INT,
	@cruiseOrderItemId NVARCHAR(100)

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	-- Variables
	DECLARE @PrincipalOrganizationId BIGINT
	DECLARE @ConsigneeCompanyName NVARCHAR(100)


	-- Get Principal organization id from shipment then matches with cruise order
	SET  @PrincipalOrganizationId = 
		(SELECT TOP(1) SC.OrganizationId
			FROM ShipmentContacts SC WITH (NOLOCK)
			INNER JOIN Shipments S ON S.Id = SC.ShipmentId
			WHERE SC.ShipmentId = @shipmentId ANd S.OrderType = 2 AND SC.OrganizationRole = 'Principal')

	-- Get Consignee company name from shipment then matches with cruise order
	SET  @ConsigneeCompanyName =
		(SELECT TOP(1) SC.CompanyName
			FROM ShipmentContacts SC WITH (NOLOCK)
			INNER JOIN Shipments S ON S.Id = SC.ShipmentId
			WHERE SC.ShipmentId = @shipmentId ANd S.OrderType = 2 AND SC.OrganizationRole = 'Consignee')


	-- Get cruise order item information which matches with current filter: principal organization, cruise order number, cruise order item po line
	-- NOTES: not filter on cruise order item id at the moment.

	-- Return data
	SELECT COI.Id AS [Id], COI.OrderId AS [OrderId], COI.POLine AS [POLine]
	FROM cruise.CruiseOrderItems COI WITH (NOLOCK)
	INNER JOIN cruise.CruiseOrders CO WITH (NOLOCK) ON CO.Id = COI.OrderId
	WHERE CO.PONumber = @cruiseOrderNumber AND COI.POLine = @cruiseOrderItemPOLine
		AND EXISTS (
			SELECT 1 FROM cruise.CruiseOrderContacts COC WITH (NOLOCK)
			WHERE COC.OrderId = CO.Id AND COC.OrganizationId = @PrincipalOrganizationId AND COC.OrganizationRole = 'Principal'
		)
		AND EXISTS (
			SELECT 1 FROM cruise.CruiseOrderContacts COC WITH (NOLOCK)
			WHERE COC.OrderId = CO.Id AND COC.CompanyName = @ConsigneeCompanyName AND COC.OrganizationRole = 'Consignee'
		)
END

