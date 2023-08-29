SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dong Tran
-- Create date: 11-21-2022
-- Description:	Count number of Vessel Arrival for external users
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_VesselArrival_Statistics_External]
@Affiliates Nvarchar(Max),
@DelegatedOrganizationId Bigint,
@CustomerRelationships Nvarchar(Max),
@FromDate Datetime2(7),
@ToDate Datetime2(7)
AS
BEGIN
	DECLARE @result INT;
	SET @result = 0;

	IF @CustomerRelationships IS NULL OR @CustomerRelationships = ''
		BEGIN
			SELECT 
				FS.CarrierName,
				FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate
			FROM PurchaseOrders PO
			INNER JOIN POFulfillmentOrders POFO ON PO.Id = POFO.PurchaseOrderId
			INNER JOIN POFulfillments POF ON POF.Id = POFO.POFulfillmentId
			INNER JOIN Shipments S ON S.POFulfillmentId = POF.Id
			INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
			INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			AND  FS.ETADate >= @FromDate AND FS.ETADate <= @ToDate
			WHERE EXISTS 
			(
				SELECT 1
				FROM PurchaseOrderContacts POC 
				WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationId IN (SELECT value FROM [dbo].[fn_SplitStringToTable](@Affiliates,','))
			)
			GROUP BY 
				FS.CarrierName,
				FS.VesselName ,
				FS.Voyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate

			UNION ALL

			SELECT 
				FS.CarrierName,
				FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate
			FROM PurchaseOrders PO
			INNER JOIN CargoDetails C ON C.OrderId = PO.Id
			INNER JOIN Shipments S ON S.Id = C.ShipmentId AND S.POFulfillmentId IS NULL
			INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
			INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			AND  FS.ETADate >= @FromDate AND FS.ETADate <= @ToDate
			WHERE EXISTS 
			(
				SELECT 1
				FROM PurchaseOrderContacts POC 
				WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationId IN (SELECT value FROM [dbo].[fn_SplitStringToTable](@Affiliates,','))
			)
			GROUP BY 
				FS.CarrierName,
				FS.VesselName ,
				FS.Voyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate
		END
	ELSE
		BEGIN
		SELECT 
				FS.CarrierName,
				FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate
			FROM PurchaseOrders PO
			INNER JOIN POFulfillmentOrders POFO ON PO.Id = POFO.PurchaseOrderId
			INNER JOIN POFulfillments POF ON POF.Id = POFO.POFulfillmentId
			INNER JOIN Shipments S ON S.POFulfillmentId = POF.Id
			INNER JOIN ConsignmentItineraries CI  ON CI.ShipmentId = S.Id
			INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			AND  FS.ETADate >= @FromDate AND FS.ETADate <= @ToDate
			WHERE EXISTS (
				SELECT 1
				FROM PurchaseOrderContacts POC 
				WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationId = @DelegatedOrganizationId AND POC.OrganizationRole = 'Delegation'
				)
			GROUP BY 
				FS.CarrierName,
				FS.VesselName ,
				FS.Voyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate

			UNION ALL 

			SELECT 
				FS.CarrierName,
				FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate
			FROM PurchaseOrders PO
			INNER JOIN POFulfillmentOrders POFO ON PO.Id = POFO.PurchaseOrderId
			INNER JOIN POFulfillments POF ON POF.Id = POFO.POFulfillmentId
			INNER JOIN Shipments S ON S.POFulfillmentId = POF.Id
			INNER JOIN ConsignmentItineraries CI  ON CI.ShipmentId = S.Id
			INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			AND  FS.ETADate >= @FromDate AND FS.ETADate <= @ToDate
			CROSS APPLY
			(
				SELECT sc.OrganizationId AS SupplierId
				FROM PurchaseOrderContacts sc
				WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
			) POC
			CROSS APPLY
			(
				 SELECT sc.OrganizationId AS CustomerId
				 FROM PurchaseOrderContacts sc
				 WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
			) POC1
			WHERE 
				CAST(POC.SupplierID AS NVARCHAR(20)) + ','+ CAST(POC1.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] FROM dbo.fn_SplitStringToTable(@CustomerRelationships, ';') tmp )
		
			GROUP BY 
				FS.CarrierName,
				FS.VesselName ,
				FS.Voyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate

			UNION ALL

			SELECT 
				FS.CarrierName,
				FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate
			FROM PurchaseOrders PO
			INNER JOIN CargoDetails C ON C.OrderId = PO.Id
			INNER JOIN Shipments S ON S.Id = C.ShipmentId AND S.POFulfillmentId IS NULL
			INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
			INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			AND  FS.ETADate >= @FromDate AND FS.ETADate <= @ToDate
			WHERE EXISTS (
				SELECT 1
				FROM PurchaseOrderContacts POC 
				WHERE POC.PurchaseOrderId = PO.Id AND POC.OrganizationId = @DelegatedOrganizationId AND POC.OrganizationRole = 'Delegation'
				)
			GROUP BY 
				FS.CarrierName,
				FS.VesselName ,
				FS.Voyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate

			UNION ALL 

			SELECT 
				FS.CarrierName,
				FS.VesselName + '/' + FS.Voyage AS VesselVoyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate
			FROM PurchaseOrders PO
			INNER JOIN CargoDetails C ON C.OrderId = PO.Id
			INNER JOIN Shipments S ON S.Id = C.ShipmentId AND S.POFulfillmentId IS NULL
			INNER JOIN ConsignmentItineraries CI ON CI.ShipmentId = S.Id
			INNER JOIN Itineraries I ON CI.ItineraryId = I.Id
			INNER JOIN FreightSchedulers FS ON I.ScheduleId = FS.Id AND FS.ModeOfTransport = 'Sea'
			AND  FS.ETADate >= @FromDate AND FS.ETADate <= @ToDate
			CROSS APPLY
			(
				SELECT sc.OrganizationId AS SupplierId
				FROM PurchaseOrderContacts sc
				WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Supplier'
			) POC
			CROSS APPLY
			(
				 SELECT sc.OrganizationId AS CustomerId
				 FROM PurchaseOrderContacts sc
				 WHERE PO.Id = sc.PurchaseOrderId AND sc.OrganizationRole = 'Principal'
			) POC1
			WHERE 
				CAST(POC.SupplierID AS NVARCHAR(20)) + ','+ CAST(POC1.CustomerId AS NVARCHAR(20)) IN (SELECT tmp.[Value] FROM dbo.fn_SplitStringToTable(@CustomerRelationships, ';') tmp )
			GROUP BY 
				FS.CarrierName,
				FS.VesselName ,
				FS.Voyage,
				FS.LocationFromName,
				FS.ETDDate,
				fs.LocationToName,
				FS.ETADate
		END

	SELECT @result = @@ROWCOUNT
	RETURN @result
END