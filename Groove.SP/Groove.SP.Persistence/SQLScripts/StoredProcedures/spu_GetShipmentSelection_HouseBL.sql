
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Dong Tran
-- Create date: June-04-2021
-- Description: Search shipment on link to shipment popup
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_GetShipmentSelection_HouseBL]
	@billOfLadingId bigint,
	@shipmentNo nvarchar(250),
	@modeOfTransport nvarchar(10),
	@executionAgentId bigint
AS
BEGIN
	SET NOCOUNT ON;
	WITH ShipmentCTE AS
	(
		SELECT 
				S.Id,
				S.ShipmentNo,
				S.TotalPackage,
				S.TotalPackageUOM,
				S.TotalVolume,
				S.TotalVolumeUOM,
				S.CargoReadyDate,
				C.ExecutionAgentName,
				LatestMilestone.ActivityDescription  LatestMilestone,
				CASE 
					WHEN SLD.Id IS NULL AND S.IsFCL = 1 THEN 1 ELSE 0
				END IsConfirmContainer,
				CASE 
					WHEN CON.Stage IS NULL AND S.IsFCL = 0 THEN 1 ELSE 0
				END IsConfirmConsolidation

		FROM Shipments S (NOLOCK)
		INNER JOIN Consignments C (NOLOCK) 
			ON S.Id = C.ShipmentId
			AND C.ExecutionAgentId = @executionAgentId AND C.HouseBillId IS NULL
		OUTER APPLY 
		(
			SELECT TOP(1)
					A.Id,
					A.ActivityDescription,
					A.ActivityDate
			FROM Activities A (NOLOCK)
			INNER JOIN GlobalIdActivities GA (NOLOCK)
					ON GA.ActivityId = A.Id
					AND GA.GlobalId =  CONCAT('SHI_', S.Id)
					AND A.ActivityType = 'SM'
			ORDER BY A.ActivityDate DESC
		) LatestMilestone
		OUTER APPLY 
		(
			SELECT TOP 1(SLD.Id) 
			FROM ShipmentLoadDetails SLD (NOLOCK)
			WHERE S.Id = SLD.ShipmentId
		) SLD
		OUTER APPLY 
		(
			SELECT TOP(1) C.Stage
			FROM ShipmentLoads SL (NOLOCK)
			OUTER APPLY
			(
				SELECT C.Stage
				FROM Consolidations C (NOLOCK)
				WHERE 
					SL.ConsolidationId = C.Id 
					AND C.Stage = 20
			) C
			WHERE 
				SL.ShipmentId = S.Id
				AND SL.ConsolidationId IS NOT NULL
		) CON
		WHERE 
			S.Status = 'ACTIVE'
			AND ShipmentNo LIKE CONCAT(@shipmentNo,'%')
			AND S.ModeOfTransport = @modeOfTransport
			AND NOT EXISTS 
						(
							SELECT 1
							FROM ShipmentBillOfLadings SBOL(NOLOCK)
							WHERE 
								SBOL.ShipmentId = S.Id
								AND SBOL.BillOfLadingId = @billOfLadingId
						)
	),
	ShipmentContactCTE AS
	(
		SELECT 
				SC.ShipmentId,
				SC.CompanyName,
				SC.OrganizationRole
		FROM ShipmentContacts SC (NOLOCK)
		WHERE SC.ShipmentId IN (SELECT Id FROM ShipmentCTE)
	)
	SELECT 
			*,
			(
				SELECT TOP(1) SCCTE.CompanyName
				FROM ShipmentContactCTE SCCTE
				WHERE 
					SCCTE.ShipmentId = ShipmentCTE.Id 
					AND SCCTE.OrganizationRole = 'Shipper'
			) Shipper,
			(
				SELECT TOP(1) SCCTE.CompanyName
				FROM ShipmentContactCTE SCCTE
				WHERE 
					SCCTE.ShipmentId = ShipmentCTE.Id 
					AND SCCTE.OrganizationRole = 'Consignee'
			) Consignee
	FROM ShipmentCTE
END