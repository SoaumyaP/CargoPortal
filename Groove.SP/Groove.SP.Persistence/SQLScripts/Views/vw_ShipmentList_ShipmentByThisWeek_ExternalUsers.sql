
IF EXISTS(SELECT * FROM sys.views WHERE NAME = 'vw_ShipmentList_ShipmentByThisWeek_ExternalUsers')
BEGIN
	 DROP VIEW vw_ShipmentList_ShipmentByThisWeek_ExternalUsers;
END
GO

CREATE VIEW [dbo].[vw_ShipmentList_ShipmentByThisWeek_ExternalUsers]
AS

	SELECT 
			SHI.Id,
			SHI.ShipmentNo,
			SHI.CustomerReferenceNo,
			SHI.ShipFromETDDate,
			SHI.BookingDate,
			SHI.ShipFrom,
			SHI.ShipTo,
			SHI.[Status],
			T1.Shipper, -- for further filter on grid
			T2.Consignee, -- for further filter on grid			
			C1.Carrier, -- for further filter on grid
			SC.OrganizationId AS [OrganizationId] -- to filter with affiliate
			,T4.*
		FROM Shipments SHI WITH(NOLOCK)
		INNER JOIN ShipmentContacts SC WITH(NOLOCK) ON SHI.Id = SC.ShipmentId 
		OUTER APPLY (
			SELECT DISTINCT I.CarrierName AS Carrier
			FROM [ConsignmentItineraries] CI WITH(NOLOCK) 
			INNER JOIN [Itineraries] I WITH(NOLOCK) ON I.Id = CI.ItineraryId
			WHERE SHI.Id = CI.ShipmentId 
		) C1
		OUTER APPLY
		(
				SELECT DISTINCT SC.CompanyName AS Shipper
				FROM ShipmentContacts SC WITH(NOLOCK)
				WHERE SHI.Id = SC.ShipmentId AND SC.OrganizationRole = 'Shipper'
		) T1
		OUTER APPLY
		(
				SELECT DISTINCT SC.CompanyName AS Consignee
				FROM ShipmentContacts SC WITH(NOLOCK)
				WHERE SHI.Id = SC.ShipmentId AND SC.OrganizationRole = 'Consignee'
		) T2
		OUTER APPLY
		(
				SELECT *
    			FROM ShipmentMilestones SMS WITH(NOLOCK)
				WHERE SMS.ShipmentId = SHI.Id
		) T4
		WHERE SHI.[Status] = 'Active'
GO
