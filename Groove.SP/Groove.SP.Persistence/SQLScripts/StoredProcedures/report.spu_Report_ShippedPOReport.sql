SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'report')
BEGIN
	EXEC('CREATE SCHEMA [report]')
END

IF OBJECT_ID('report.spu_ShippedPOReport', 'P') IS NOT NULL
DROP PROC [report].spu_ShippedPOReport
GO

-- =============================================
-- Author:		Cuong Duong
-- Create date: 11 Nov 2021
-- Description:	Get data for Shipped PO Report
-- =============================================
CREATE PROCEDURE [report].[spu_ShippedPOReport]
	@SelectedCustomerId BIGINT,
	@PORetentionPeriod BIGINT = 14
AS
BEGIN

	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Variable declaration

	-- Set values for variables


	-- Filtering data section
	-- 1.Filter on Purchase order
	;
	WITH PurchaseOrderCTE AS
	(
		SELECT 
			PO.Id AS [POId], -- for further join
			PO.CustomerReferences, -- column A
			T1.ShipToETADate AS [ETA], -- column C
			T1.ShipmentNo AS [Shipment Id], -- column E
			T2.ContainerNo AS [Conainter Number], -- column F
			PO.PONumber, -- column G
			T4.Event7002Date, -- column B
			T3.Event7001Date, -- column H
			POCC.OrganizationId AS [CustomerId], -- for further join
			POCS.OrganizationId AS [SupplierId] -- for further join
		FROM PurchaseOrders PO WITH(NOLOCK)
		INNER JOIN PurchaseOrderContacts POCC WITH(NOLOCK) -- filter on Customer,  PO belongs to selected customer
			ON PO.Id = POCC.PurchaseOrderId
			AND POCC.OrganizationRole = 'Principal'
			AND POCC.OrganizationId = @SelectedCustomerId
		LEFT JOIN PurchaseOrderContacts POCS WITH(NOLOCK) -- Supplier
			ON PO.Id = POCS.PurchaseOrderId
			AND POCS.OrganizationRole = 'Supplier'

		OUTER APPLY (
			SELECT 
				SHI.Id AS [ShipmentId],
				SHI.ShipmentNo,
				SHI.ShipToETADate,
				SHI.ShipTo
			FROM Shipments SHI (NOLOCK)
			WHERE 
				SHI.[Status] = 'Active' -- Shipment is active
				AND SHI.POFulfillmentId IN (SELECT POFFO.POFulfillmentId FROM POFulfillmentOrders POFFO (NOLOCK) WHERE POFFO.PurchaseOrderId = PO.Id )
			
		) T1
		OUTER APPLY (
			SELECT TOP 1 CTN.*
			FROM Containers CTN (NOLOCK)
			INNER JOIN ShipmentLoadDetails LD (NOLOCK) ON CTN.Id = LD.ContainerId
			WHERE LD.ShipmentId = T1.ShipmentId
		) T2

		-- There is event 7001 triggered
		CROSS APPLY
		(
			SELECT TOP(1)
				GIDACT.ActivityDate AS [Event7001Date]			
			FROM GlobalIdActivities GIDACT (NOLOCK)
			INNER JOIN Activities ACT (NOLOCK) ON GIDACT.ActivityId = ACT.Id AND ACT.ActivityCode = '7001'
			WHERE GIDACT.GlobalId IN (
				SELECT CONCAT('FSC_', I.ScheduleId)
				FROM Itineraries I (NOLOCK) 
				INNER JOIN ConsignmentItineraries CSMI (NOLOCK) ON CSMI.ItineraryId = I.Id
				INNER JOIN Shipments SHI (NOLOCK) ON CSMI.ShipmentId = SHI.Id
				INNER JOIN CargoDetails CD (NOLOCK) ON CD.OrderId = PO.Id AND CD.ShipmentId = SHI.Id
				WHERE I.ModeOfTransport = 'sea' AND I.ScheduleId IS NOT NULL
			)
			ORDER BY GIDACT.ActivityDate DESC
		
		) T3
		-- There is event 7002 triggered = shipment.ShipTo
		OUTER APPLY
		(
			SELECT DISTINCT 
				GIDACT.ActivityDate AS [Event7002Date]
			FROM GlobalIdActivities GIDACT (NOLOCK)
			INNER JOIN Activities ACT (NOLOCK) ON GIDACT.ActivityId = ACT.Id AND ACT.ActivityCode = '7002'
			WHERE GIDACT.GlobalId IN (
				SELECT CONCAT('FSC_', I.ScheduleId)
				FROM Itineraries I (NOLOCK) 
				INNER JOIN ConsignmentItineraries CSMI (NOLOCK) ON CSMI.ItineraryId = I.Id
				INNER JOIN Shipments SHI (NOLOCK) ON CSMI.ShipmentId = SHI.Id
				INNER JOIN CargoDetails CD (NOLOCK) ON CD.OrderId = PO.Id AND CD.ShipmentId = SHI.Id
				WHERE I.ModeOfTransport = 'sea' AND I.ScheduleId IS NOT NULL
			)
			AND GIDACT.[Location] = T1.ShipTo		
		) T4
		WHERE
			-- PO is Active
			PO.[Status] = 1
			-- PO < Closed OR PO closed less than 14 days
			AND (PO.Stage < 60 OR EXISTS (
				SELECT 1
				FROM GlobalIdActivities GIDACT (NOLOCK)
				INNER JOIN Activities ACT (NOLOCK) ON GIDACT.ActivityId = ACT.Id
				WHERE GIDACT.GlobalId = CONCAT('CPO_', PO.Id) AND ACT.ActivityCode = '1010' AND DATEDIFF(DAY, GIDACT.ActivityDate, GETDATE()) < @PORetentionPeriod
			))
				
	)

	-- 2.Get more data fields
	,PurchaseOrder1CTE AS
	(
		SELECT 
			PO.Event7002Date AS [ATA] -- column B		
			,T4.ActivityDescription AS [Status] -- column D		
			,PO.PONumber AS [Customer PO] -- column G
			,PO.Event7001Date AS [ATD] -- column H
			,PO.*

		FROM PurchaseOrderCTE PO
	
		-- EventName of latest activity in PO page (cross module)
		OUTER APPLY (
			SELECT TOP 1 ACT.ActivityDescription
			FROM Activities ACT (NOLOCK) INNER JOIN GlobalIdActivities GIDACT (NOLOCK) ON ACT.Id = GIDACT.ActivityId
			INNER JOIN (
				SELECT CONCAT('CPO_', PO.POId) as GlobalIdActivity
				UNION

				SELECT
					CONCAT('POF_', POFF.Id) as GlobalIdActivity
				FROM POFulfillments POFF (NOLOCK)
				WHERE POFF.[Status] = 10 --active
				AND EXISTS (
					SELECT 1
					FROM POFulfillmentOrders POFFOD (NOLOCK)
					WHERE POFFOD.POFulfillmentId = POFF.Id AND POFFOD.PurchaseOrderId = PO.POId)
				UNION

				SELECT 
					CONCAT('SHI_', CD.ShipmentId) as GlobalIdActivity
				FROM CargoDetails CD (NOLOCK)
				WHERE CD.OrderId = PO.POId
				UNION

				SELECT 
					CONCAT('CTN_', sl.ContainerId) as GlobalIdActivity
				FROM ShipmentLoads sl (NOLOCK)
				WHERE sl.ContainerId IS NOT NULL AND sl.ShipmentId IN (
					SELECT CD.ShipmentId
					FROM CargoDetails CD (NOLOCK)
					WHERE CD.OrderId = PO.POId)
				UNION

				SELECT DISTINCT
					CONCAT('FSC_', i.ScheduleId) as GlobalIdActivity
				FROM ConsignmentItineraries csmi (NOLOCK) INNER JOIN Itineraries i (NOLOCK) ON csmi.ItineraryId = i.Id AND i.ModeOfTransport = 'sea' AND i.ScheduleId IS NOT NULL
				WHERE csmi.ShipmentId IN (
					SELECT CD.ShipmentId
					FROM CargoDetails CD (NOLOCK)
					WHERE CD.OrderId = PO.POId)
			) T ON T.GlobalIdActivity = GIDACT.GlobalId
			INNER JOIN EventCodes E ON E.ActivityCode = ACT.ActivityCode
			ORDER BY CAST(GIDACT.ActivityDate as DATE) DESC, E.SortSequence DESC
		) T4
		
	)

	-- Data returned section
	-- Order of columns must be matched to report design
	SELECT
		PO.CustomerReferences AS [Customer Ref] --Column A
		,CASE 
			WHEN PO.ATA IS NULL THEN NULL
			ELSE FORMAT(PO.ATA, 'yyyy-MM-dd')
			END AS [ATA] --Column B
		,CASE 
			WHEN PO.ETA IS NULL THEN NULL
			ELSE FORMAT(PO.ETA, 'yyyy-MM-dd')
			END AS [ETA] --Column C
		,COALESCE(PO.[Status], 'Released') AS [Status] --Column D
		,PO.[Shipment Id] --Column E
		,PO.[Conainter Number] --Column F
		,Po.[Customer PO] --Column G
		,FORMAT(PO.ATD, 'yyyy-MM-dd') AS [ATD] --Column H
	FROM PurchaseOrder1CTE PO
	ORDER BY [POId]

END
GO