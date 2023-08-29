SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Dong Tran
-- Create date: 11-21-2022
-- Description:	Count number of Vessel Arrival
-- =============================================
CREATE OR ALTER PROCEDURE [dbo].[spu_VesselArrival_Statistics_Internal]
@FromDate Datetime2(7),
@ToDate Datetime2(7)
AS
BEGIN
	DECLARE @result INT;
	SET @result = 0;

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
	GROUP BY 
		FS.CarrierName,
		FS.VesselName ,
		FS.Voyage,
		FS.LocationFromName,
		FS.ETDDate,
		fs.LocationToName,
		FS.ETADate

	SELECT @result = @@ROWCOUNT
	RETURN @result
END