SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ConsignmentItineraries_ShipmentMilestone' AND [type] = 'TR')
BEGIN
	DROP TRIGGER [dbo].trg_ConsignmentItineraries_ShipmentMilestone;
END
GO


-- =============================================
-- Author:		Cuong Duong
-- Created date: January 30 2023
-- Description:	The trigger to update the latest milestone value for shipment
-- =============================================
CREATE TRIGGER  [dbo].[trg_ConsignmentItineraries_ShipmentMilestone] 
   ON  [dbo].[ConsignmentItineraries] 
   AFTER INSERT, UPDATE, DELETE
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    -- Insert statements for trigger here

	DECLARE @ShipmentIdTable TABLE (
		[ShipmentId] [bigint] NOT NULL
	)

	DECLARE @DistinctShipmentIdTable TABLE (
		[ShipmentId] [bigint] NOT NULL
	)

	DECLARE @ShipmentMilestoneTable TABLE (
		[ShipmentId] [bigint] NOT NULL,
		[ActivityDate] [datetime2](7) NOT NULL,
		[ActivityCode] [nvarchar](10) NOT NULL,
		[Milestone] [nvarchar](50) NOT NULL
	)
	
	INSERT INTO @ShipmentIdTable (ShipmentId)
	SELECT ShipmentId FROM inserted 
	UNION ALL 
	SELECT ShipmentId FROM deleted
	
	;
	INSERT INTO @DistinctShipmentIdTable
	SELECT DISTINCT T.ShipmentId
	FROM @ShipmentIdTable T
	

	INSERT INTO @ShipmentMilestoneTable ([ShipmentId], [ActivityDate], [ActivityCode], [Milestone])
	SELECT SMT.ShipmentId, T1.ActivityDate, T1.ActivityCode, T1.Milestone
	FROM @DistinctShipmentIdTable SMT
	CROSS APPLY
		(
			SELECT TOP 1 *
			FROM (
					SELECT SHM.Id, SHM.ShipmentNo, SHM.ModeOfTransport, A.ActivityDate, A.ActivityCode, 
    						CASE 
    							WHEN A.ActivityCode = '2005' THEN 'Shipment Booked'
    							WHEN A.ActivityCode = '2014' THEN 'Handover from Shipper'
    							WHEN A.ActivityCode = '2054' THEN 'Handover to Consignee'
								WHEN A.ActivityCode = '7003' THEN 'In Transit'
								WHEN A.ActivityCode = '7004' THEN 'Arrival at Port'
    						END Milestone
    						FROM GlobalIdActivities GA
    						INNER JOIN Activities A WITH(INDEX(1)) on GA.ActivityId = A.Id AND A.ActivityCode IN ('2005','2014','2054','7003','7004')
							INNER JOIN Shipments SHM ON GA.GlobalId = CONCAT('SHI_', SHM.Id) AND SHM.Id = SMT.ShipmentId 

					UNION ALL

					SELECT SHM.Id, SHM.ShipmentNo, SHM.ModeOfTransport, A.ActivityDate, A.ActivityCode, 
    				CASE 
                        WHEN A.ActivityCode = '7001' OR A.ActivityCode = '7003' THEN 'In Transit'
    					WHEN A.ActivityCode = '7002' OR A.ActivityCode = '7004' THEN 'Arrival at Port'
    				END Milestone
    				FROM Activities A WITH(INDEX(1))
					INNER JOIN GlobalIdActivities GA ON GA.ActivityId = A.Id
					INNER JOIN Itineraries ITI ON CONCAT('FSC_', ITI.ScheduleId) = GA.GlobalId AND (ITI.ModeOfTransport = 'sea' OR ITI.ModeOfTransport = 'air')
					INNER JOIN ConsignmentItineraries CI ON CI.ItineraryId = ITI.Id
					INNER JOIN Shipments SHM ON SHM.Id = CI.ShipmentId
					WHERE SHM.Id = SMT.ShipmentId
					
			) AS CTE
				ORDER BY 
					CASE 
    					WHEN CTE.ModeOfTransport = 'sea' THEN CONVERT(CHAR(10), CTE.ActivityDate, 120) 
    					WHEN CTE.ModeOfTransport = 'air' THEN CTE.ActivityDate
    				END DESC,
    				CASE WHEN CTE.ActivityCode != '2054' THEN CTE.ActivityCode ELSE '9999' END DESC
		) T1

	DELETE SML
	FROM ShipmentMilestones SML
	WHERE SML.ShipmentId IN (SELECT ShipmentId FROM @DistinctShipmentIdTable)


	INSERT INTO ShipmentMilestones ([ShipmentId], [ActivityDate], [ActivityCode], [Milestone])
	SELECT INS.ShipmentId, INS.ActivityDate, INS.ActivityCode, INS.Milestone
	FROM @ShipmentMilestoneTable INS


END
GO