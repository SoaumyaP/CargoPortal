SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'report')
BEGIN
	EXEC('CREATE SCHEMA [report]')
END

IF OBJECT_ID('report.spu_ActivePOReport', 'P') IS NOT NULL
DROP PROC [report].spu_ActivePOReport
GO

-- =============================================
-- Author:		Hau Ng
-- Create date: 03 Nov 2021
-- Description:	Get data for Active PO Report run on Telerik Reporting Server
-- =============================================
CREATE PROCEDURE [report].[spu_ActivePOReport]
	@SelectedCustomerId BIGINT,
	@PORetentionPeriod BIGINT = 14
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Variable declaration
	DECLARE @EquipmentTypeTbl TABLE (Id INT NOT NULL, Code NVARCHAR(10), [Name] NVARCHAR(50) NOT NULL)

	-- Set values for variables
	INSERT INTO @EquipmentTypeTbl
	VALUES	(3, '20DG', '20'' Dangerous Container'), 
			(5, '20FR', '20'' Flat Rack'),
			(7, '20GH', '20'' GOH Container'),
			(10, '20GP', '20'' Container'),
			(11, '20HC', '20'' High Cube'),
			(12, '20HT', '20'' HT Container'),
			(13, '20HW', '20'' High Wide'),
			(14, '20NOR', '20'' Reefer Dry'),
			(15, '20OS', '20'' Both Full Side Door Opening Container'),
			(16, '20OT', '20'' Open Top Container'),
			(20, '40GP', '40'' Container'),
			(21, '40HC', '40'' High Cube'),
			(22, '40HG', '40'' HC GOH Container'),
			(23, '40HNOR', '40'' HC Reefer Dry Container'),
			(24, '40HO', '40'' HC Open Top Container'),
			(25, '40HQDG', '40'' HQ DG Container'),
			(26, '40HR', '40'' HC Reefer Container'),
			(27, '40HW', '40'' High Cube Pallet Wide'),
			(28, '40NOR', '40'' Reefer Dry'),
			(29, '40OT', '40'' Open Top Container'),
			(30, '20RF', '20'' Reefer'),
			(31, '20TK', '20'' Tank Container'),
			(32, '20VH', '20'' Ventilated Container'),
			(33, '40DG', '40'' Dangerous Container'),
			(34, '40FQ', '40'' High Cube Flat Rack'),
			(35, '40FR', '40'' Flat Rack'),
			(36, '40GH', '40'' GOH Container'),
			(37, '40PS', '40'' Plus'),
			(40, '40RF', '40'' Reefer'),
			(41, '40TK', '40'' Tank'),
			(51, '45GO', '45'' GOH'),
			(52, '45HC', '45'' High Cube'),
			(54, '45HG', '45'' HC GOH Container'),
			(55, '45HT', '45'' Hard Top Container'),
			(56, '45HW', '45'' HC Pallet Wide'),
			(57, '45RF', '45'' Reefer Container'),
			(58, '48HC', '48'' HC Container'),
			(50, 'Air', 'Air'),
			(60, 'LCL', 'LCL'),
			(70, 'Truck', 'Truck')

	-- Filtering data section
	-- 1.Filter on Purchase order
	;
	WITH PurchaseOrderCTE AS
	(
		SELECT 
			PO.*,
			POCC.CompanyName AS [Customer], -- column C
			POCS.CompanyName AS [Supplier], -- column D
			EQ.Code AS [POContainerTypeCode] -- for further info
		FROM PurchaseOrders PO WITH(NOLOCK)
		INNER JOIN PurchaseOrderContacts POCC WITH(NOLOCK) -- filter on Customer,  PO belongs to selected customer
			ON PO.Id = POCC.PurchaseOrderId
			AND POCC.OrganizationRole = 'Principal'
			AND POCC.OrganizationId = @SelectedCustomerId
		LEFT JOIN PurchaseOrderContacts POCS WITH(NOLOCK) -- Supplier
			ON PO.Id = POCS.PurchaseOrderId
			AND POCS.OrganizationRole = 'Supplier'
		LEFT JOIN @EquipmentTypeTbl EQ ON EQ.Id = PO.ContainerType 
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

	-- 2.Filter on Shipment
	,PurchaseOrder1CTE AS
	(
		SELECT 
			 PO.Id AS [PO ID] -- column A,
			,PO.Customer AS [Customer] -- column C
			,PO.Supplier AS [Supplier] -- column D
			,PO.PONumber AS [Customer PO] -- column E
			,PO.CargoReadyDate AS [Ready Date] -- column F
			,LOCF.[Name] AS [Origin] -- column I
			,LOCT.[Name] AS [Destination] -- column J

			,T.ShipmentNo AS [Shipment ID] -- column B
			,T.ShipFromETDDate AS [ETD] -- Column K
			,T.ShipToETADate AS [ETA] -- Column L
			,T.TotalPackage AS [Total Pkgs] -- column P
			,T.ShipmentId AS [ShipmentId] -- for further join
			,T.ShipTo AS [ShipTo] -- for further join
			,T.BillOfLadingNo AS [Bill Of Lading No.] -- column T
			,PO.POContainerTypeCode -- for further info

		FROM PurchaseOrderCTE PO
		LEFT JOIN Locations LOCF ON LOCF.Id = PO.ShipFromId
		LEFT JOIN Locations LOCT ON LOCT.Id = PO.ShipToId
		OUTER APPLY (
			SELECT 
				SHI.Id AS [ShipmentId],
				SHI.ShipmentNo,
				SHI.ShipFromETDDate,
				SHI.ShipToETADate,
				SHI.ShipTo,
				T1.TotalPackage,
				BL.BillOfLadingNo
			FROM Shipments SHI (NOLOCK) LEFT JOIN ( SELECT CD.ShipmentId, SUM(CD.Package) AS TotalPackage
													FROM CargoDetails CD (NOLOCK)
													WHERE CD.OrderType = 1 -- Freight shipment
													GROUP BY CD.ShipmentId
												  ) T1 ON SHI.Id = T1.ShipmentId
										LEFT JOIN ShipmentBillOfLadings SBL (NOLOCK) ON SBL.ShipmentId = SHI.Id LEFT JOIN BillOfLadings BL (NOLOCK) ON BL.Id = SBL.BillOfLadingId
			-- Shipment status must be Active
			WHERE SHI.[Status] = 'Active' 
			AND SHI.POFulfillmentId IN (SELECT POFFO.POFulfillmentId FROM POFulfillmentOrders POFFO (NOLOCK) WHERE POFFO.PurchaseOrderId = PO.Id )
		) T
	)

	--SELECT * FROM PurchaseOrder1CTE ORDER BY [PO ID]

	-- 3. Filter more on the Containers, first/last Itinerary (leg) and other columns
	,PurchaseOrder2CTE AS
	(
		SELECT
			 CTE.* -- Main data columns
			 ,C.ContainerNo AS [Container Num] -- column N
			 ,CASE
					  WHEN C.ContainerType IS NOT NULL
						THEN C.ContainerType
					  ELSE CTE.POContainerTypeCode
			  END AS [Container Size] -- column O
			 ,CASE 
					  WHEN I1.FLegId IS NOT NULL 
						AND ((I1.FLegDischargePort != CTE.ShipTo AND I2.LLegId IS NULL) -- Only one leg and DischargePort != ShipToName
						OR (I2.LLegId IS NOT NULL AND I1.FLegId != I2.LLegId)) -- Two legs
						THEN I1.FLegVesselVoyage
					  ELSE NULL
			  END AS [Pre-carrying Vessel/Aircraft] -- column G
			 ,CASE 
					  WHEN I1.FLegId IS NOT NULL 
						AND ((I1.FLegDischargePort != CTE.ShipTo AND I2.LLegId IS NULL) -- Only one leg and DischargePort != ShipToName
						OR (I2.LLegId IS NOT NULL AND I1.FLegId != I2.LLegId)) -- Two legs
						THEN I1.FLegDischargePortCode
					  ELSE NULL
			  END AS [Transhipment Port] -- column S
			 ,CASE
					  WHEN I2.LLegId IS NOT NULL AND I2.LLegDischargePort = CTE.ShipTo
						 THEN I2.LLegVesselVoyage
					  WHEN I1.FLegId IS NOT NULL AND I1.FLegDischargePort = CTE.ShipTo
						 THEN I1.FLegVesselVoyage
					  ELSE NULL
			  END AS [Mother Vessel/Aircraft] -- column H
			 ,CASE
                  WHEN E.[Location] IS NOT NULL AND E.[Location] <> ''
                     THEN CONCAT(E.ActivityDescription, ' (', E.[Location], ')')
                  ELSE E.ActivityDescription
             END AS [LatestActivity] -- column Q
		FROM PurchaseOrder1CTE CTE
		OUTER APPLY (
				SELECT TOP 1 CTN.*
				FROM CargoDetails CD (NOLOCK)
					INNER JOIN ShipmentLoadDetails LD (NOLOCK) ON CD.Id = LD.CargoDetailId
					INNER JOIN Containers CTN (NOLOCK) ON CTN.Id = LD.ContainerId
				WHERE CD.OrderId = CTE.[PO ID] AND CD.ShipmentId = CTE.ShipmentId
		) C
		-- The first Itinerary
		OUTER APPLY (
				SELECT TOP(1) ITI.Id AS [FLegId],
					IIF((ITI.VesselName IS NOT NULL AND ITI.VesselName <> '') OR (ITI.Voyage IS NOT NULL AND ITI.Voyage <> ''), CONCAT(ITI.VesselName, '/', ITI.Voyage), NULL) AS FLegVesselVoyage,
					ITI.LoadingPort AS [FLegLoadingPort],
					ITI.DischargePort AS [FLegDischargePort],
					LOCT.[Name] AS [FLegDischargePortCode]

				FROM Itineraries ITI WITH(NOLOCK)
				INNER JOIN ConsignmentItineraries COI WITH(NOLOCK) ON COI.ItineraryId = ITI.Id
				LEFT JOIN Locations LOCT ON LOCT.[LocationDescription] = ITI.DischargePort
				WHERE COI.ShipmentId = CTE.ShipmentId
				ORDER BY ITI.[Sequence] ASC, ITI.[Id] ASC
		) I1

		-- The last Itinerary
		OUTER APPLY (
				SELECT TOP(1) ITI.Id AS [LLegId],
				IIF((ITI.VesselName IS NOT NULL AND ITI.VesselName <> '') OR (ITI.Voyage IS NOT NULL AND ITI.Voyage <> ''), CONCAT(ITI.VesselName, '/', ITI.Voyage), NULL) AS LLegVesselVoyage,
				ITI.LoadingPort AS [LLegLoadingPort],
				ITI.DischargePort AS [LLegDischargePort]

				FROM Itineraries ITI WITH(NOLOCK)
				INNER JOIN ConsignmentItineraries COI WITH(NOLOCK) ON COI.ItineraryId = ITI.Id
				WHERE 
					ITI.Id != I1.FLegId -- Not the first leg in case there is only one leg
					AND COI.ShipmentId = CTE.ShipmentId
				ORDER BY ITI.[Sequence] DESC, ITI.[Id] DESC
		) I2

		-- EventName of latest activity in PO page (cross module)
		OUTER APPLY (
			SELECT TOP 1 ACT.ActivityDescription, ACT.[Location]
			FROM Activities ACT (NOLOCK) INNER JOIN GlobalIdActivities GIDACT (NOLOCK) ON ACT.Id = GIDACT.ActivityId
			INNER JOIN (
				SELECT CONCAT('CPO_', CTE.[PO ID]) as GlobalIdActivity
				UNION

				SELECT
					CONCAT('POF_', POFF.Id) as GlobalIdActivity
				FROM POFulfillments POFF (NOLOCK)
				WHERE POFF.[Status] = 10 --active
				AND EXISTS (
					SELECT 1
					FROM POFulfillmentOrders POFFOD (NOLOCK)
					WHERE POFFOD.POFulfillmentId = POFF.Id AND POFFOD.PurchaseOrderId = CTE.[PO ID])
				UNION

				SELECT 
					CONCAT('SHI_', CD.ShipmentId) as GlobalIdActivity
				FROM CargoDetails CD (NOLOCK)
				WHERE CD.OrderId = CTE.[PO ID]
				UNION

				SELECT 
					CONCAT('CTN_', sl.ContainerId) as GlobalIdActivity
				FROM ShipmentLoads sl (NOLOCK)
				WHERE sl.ContainerId IS NOT NULL AND sl.ShipmentId IN (
					SELECT CD.ShipmentId
					FROM CargoDetails CD (NOLOCK)
					WHERE CD.OrderId = CTE.[PO ID])
				UNION

				SELECT DISTINCT
					CONCAT('FSC_', i.ScheduleId) as GlobalIdActivity
				FROM ConsignmentItineraries csmi (NOLOCK) INNER JOIN Itineraries i (NOLOCK) ON csmi.ItineraryId = i.Id AND i.ModeOfTransport = 'sea' AND i.ScheduleId IS NOT NULL
				WHERE csmi.ShipmentId IN (
					SELECT CD.ShipmentId
					FROM CargoDetails CD (NOLOCK)
					WHERE CD.OrderId = CTE.[PO ID])
			) T ON T.GlobalIdActivity = GIDACT.GlobalId
			INNER JOIN EventCodes E ON E.ActivityCode = ACT.ActivityCode
			ORDER BY CAST(GIDACT.ActivityDate as DATE) DESC, E.SortSequence DESC
		) E
	)

	--SELECT * FROM PurchaseOrder2CTE ORDER BY [PO ID]

	-- Data returned section
	-- Order of columns must be matched to report design
	, DataReturnCTE AS (
		SELECT
			PO.[PO ID] --Column A
			,PO.[Shipment ID] --Column B
			,PO.Customer --Column C
			,PO.Supplier --Column D
			,PO.[Customer PO] --Column E
			,(CASE 
				WHEN PO.[Ready Date] IS NULL THEN NULL
				ELSE FORMAT(PO.[Ready Date], 'yyyy-MM-dd')
			END ) AS [Ready Date] --Column F
			,PO.[Pre-carrying Vessel/Aircraft] --Column G
			,PO.[Mother Vessel/Aircraft] --Column H
			,PO.Origin --Column I
			,PO.Destination --Column J
			,CASE 
				WHEN PO.ETD IS NULL THEN NULL
				ELSE FORMAT(PO.ETD, 'yyyy-MM-dd')
			  END AS [ETD] --Column K
			,CASE 
				WHEN PO.ETA IS NULL THEN NULL
				ELSE FORMAT(PO.ETA, 'yyyy-MM-dd')
			  END AS [ETA] --Column L
			,CASE
				WHEN PO.[Container Size] IS NOT NULL
					THEN IIF(PO.[Container Size] = 'LCL', 'LCL', 'FCL')
				ELSE NULL
			 END AS [Process Type] -- column M
			,PO.[Container Num] --Column N
			,PO.[Container Size] --Column O
			,COALESCE(CAST(PO.[Total Pkgs] AS INT), 0) AS [Total Pkgs] --Column P
			,COALESCE(PO.[LatestActivity], 'Released') AS [Status] --Column Q
			,NULL AS [Release Status] --Column R
			,PO.[Transhipment Port] --Column S
			,PO.[Bill Of Lading No.] --Column T
		FROM PurchaseOrder2CTE PO
	)

	SELECT * FROM DataReturnCTE
	ORDER BY [PO ID]
END
GO