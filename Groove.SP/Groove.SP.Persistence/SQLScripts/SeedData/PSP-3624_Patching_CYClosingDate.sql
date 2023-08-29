--Business rule
--CY Booking (MovementType = CY/CFS or CY/CY) & Stage >= Booking Confirmed
	-- POFulfillments.CYClosingDate = 1st leg of linked active Shipment
	-- POFulfillmentBookingRequests.CYClosingDate = 1st leg of linked active Shipment

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN /* To patching POFulfillments */
DECLARE @updatedPOFulfillments TABLE (Id BIGINT, CYClosingDate DATETIME2(7));

;with CTE as (

	SELECT 
		SH.Id,
		SH.ShipmentNo,
		SH.POFulfillmentId,
		T.CYClosingDate
	FROM Shipments SH
	OUTER APPLY -- first leg
	(
		
		SELECT TOP(1) FS.CYClosingDate
		FROM Itineraries IT INNER JOIN ConsignmentItineraries CIT ON IT.Id = CIT.ItineraryId INNER JOIN FreightSchedulers FS ON FS.Id = IT.ScheduleId
		WHERE CIT.ShipmentId = SH.Id
		ORDER BY IT.[Sequence] ASC

	) T
	WHERE SH.[Status] = 'active'
)

UPDATE POF
SET POF.CYClosingDate = 
	CASE
		WHEN CTE.CYClosingDate IS NOT NULL THEN CTE.CYClosingDate
		ELSE T.CargoClosingDate
	END
OUTPUT inserted.Id, inserted.CYClosingDate into @updatedPOFulfillments
FROM POFulfillments POF
LEFT JOIN CTE ON POF.Id = CTE.POFulfillmentId
OUTER APPLY --latest Booking Request
(
		SELECT TOP(1) RQ.CargoClosingDate
		FROM POFulfillmentBookingRequests RQ
		WHERE RQ.POFulfillmentId = POF.Id AND RQ.[Status] = 10
		ORDER BY Id DESC
) T
WHERE POF.MovementType = 1
AND POF.[Status] = 10
AND POF.Stage >= 30 -- Booking Confirmed

-- To get updated POFulfillments
SELECT * FROM @updatedPOFulfillments

END

BEGIN /* To patching POFulfillmentBookingRequests */

DECLARE @updatedPOFulfillmentBookingRequests TABLE (Id BIGINT, CYClosingDate DATETIME2(7));

UPDATE RQ
SET CYClosingDate = T.CYClosingDate
OUTPUT inserted.Id, inserted.CYClosingDate into @updatedPOFulfillmentBookingRequests
FROM POFulfillmentBookingRequests RQ INNER JOIN @updatedPOFulfillments T ON RQ.POFulfillmentId = T.Id
WHERE RQ.[Status] = 10

-- To get updated POFulfillmentBookingRequests
SELECT * FROM @updatedPOFulfillmentBookingRequests

END

COMMIT TRANSACTION

GO