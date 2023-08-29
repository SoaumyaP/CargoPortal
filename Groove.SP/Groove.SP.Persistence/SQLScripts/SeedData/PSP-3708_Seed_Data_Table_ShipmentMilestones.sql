DELETE SML
FROM ShipmentMilestones SML

INSERT INTO ShipmentMilestones ([ShipmentId], [ActivityDate], [ActivityCode], [Milestone])
SELECT INS.Id, T1.ActivityDate, T1.ActivityCode, T1.Milestone
FROM Shipments INS
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
					INNER JOIN Shipments SHM ON GA.GlobalId = CONCAT('SHI_', SHM.Id) AND SHM.Id = INS.Id 

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
			WHERE SHM.Id = INS.Id
					
	) AS CTE
		ORDER BY 
			CASE 
    			WHEN CTE.ModeOfTransport = 'sea' THEN CONVERT(CHAR(10), CTE.ActivityDate, 120) 
    			WHEN CTE.ModeOfTransport = 'air' THEN CTE.ActivityDate
    		END DESC,
    		CASE WHEN CTE.ActivityCode != '2054' THEN CTE.ActivityCode ELSE '9999' END DESC
) T1