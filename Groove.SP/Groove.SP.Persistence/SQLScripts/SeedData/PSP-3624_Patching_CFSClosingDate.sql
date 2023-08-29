--Business rule
--CFS Booking (MovementType = CFS/CY or CFS/CFS) & Stage >= Booking Confirmed
	-- POFulfillments.CFSClosingDate = POFulfillmentBookingRequests.CargoClosingDate
	-- POFulfillmentBookingRequests.CFSClosingDate = POFulfillmentBookingRequests.CargoClosingDate

SET NOCOUNT ON;

BEGIN TRANSACTION;

BEGIN /* To patching POFulfillments */

DECLARE @updatedPOFulfillments TABLE (Id BIGINT, CFSClosingDate DATETIME2(7));

UPDATE POF
SET POF.CFSClosingDate = T.CargoClosingDate
OUTPUT inserted.Id, inserted.CFSClosingDate into @updatedPOFulfillments
FROM POFulfillments POF
CROSS APPLY --latest Booking Request
(
		
		SELECT TOP(1) RQ.CargoClosingDate
		FROM POFulfillmentBookingRequests RQ
		WHERE RQ.POFulfillmentId = POF.Id
		ORDER BY RQ.Id DESC
) T
WHERE POF.MovementType = 2
AND POF.[Status] = 10
AND POF.Stage >= 30 -- Booking Confirmed

-- To get updated POFulfillments
SELECT * FROM @updatedPOFulfillments

END

BEGIN /* To patching POFulfillmentBookingRequests */

DECLARE @updatedPOFulfillmentBookingRequests TABLE (Id BIGINT, CFSClosingDate DATETIME2(7));

UPDATE RQ
SET CFSClosingDate = T.CFSClosingDate
OUTPUT inserted.Id, inserted.CFSClosingDate into @updatedPOFulfillmentBookingRequests
FROM POFulfillmentBookingRequests RQ INNER JOIN @updatedPOFulfillments T ON RQ.POFulfillmentId = T.Id
WHERE RQ.[Status] = 10

-- To get updated POFulfillmentBookingRequests
SELECT * FROM @updatedPOFulfillmentBookingRequests

END

COMMIT TRANSACTION

GO