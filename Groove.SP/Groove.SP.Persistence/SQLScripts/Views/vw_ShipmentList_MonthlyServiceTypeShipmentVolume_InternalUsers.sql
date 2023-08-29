
IF EXISTS(SELECT * FROM sys.views WHERE NAME = 'vw_ShipmentList_MonthlyServiceTypeShipmentVolume_InternalUsers')
BEGIN
	 DROP VIEW vw_ShipmentList_MonthlyServiceTypeShipmentVolume_InternalUsers;
END
GO

CREATE VIEW [dbo].[vw_ShipmentList_MonthlyServiceTypeShipmentVolume_InternalUsers]
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
			T1.Shipper,
			T2.Consignee,
			SHI.ServiceType -- for further filter on grid
			,T4.*
		FROM Shipments SHI WITH(NOLOCK)		

		OUTER APPLY
		(
				SELECT TOP(1) SC.CompanyName AS Shipper
				FROM ShipmentContacts SC WITH(NOLOCK)
				WHERE SHI.Id = SC.ShipmentId AND SC.OrganizationRole = 'Shipper'
		) T1
		OUTER APPLY
		(
				SELECT TOP(1) SC.CompanyName AS Consignee
				FROM ShipmentContacts SC WITH(NOLOCK)
				WHERE SHI.Id = SC.ShipmentId AND SC.OrganizationRole = 'Consignee'
		) T2
		OUTER APPLY
		(
				SELECT *
    			FROM ShipmentMilestones SMS WITH(NOLOCK)
				WHERE SMS.ShipmentId = SHI.Id
		) T4
		WHERE
			SHI.Status = 'Active'
			AND SHI.ModeOfTransport = 'Sea'
			AND (
				SHI.IsFCL = 0 
				OR EXISTS (
					SELECT 1
					FROM ShipmentLoads SHL WITH(NOLOCK)
					WHERE SHL.ShipmentId = SHI.Id AND SHL.ContainerId IS NOT NULL
				)
			)
GO
