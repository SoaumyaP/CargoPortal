-- =============================================
-- Author:			Phuoc Le
-- Created date:	28 July 2020
-- Description:		PSP-1784: [PROD] Patch Booking Date for existing Bookings
-- =============================================


WITH UpdatedBookingCTE AS
(
	-- Edison Booking 
	SELECT poff.Id, br.BookedDate AS BookingDate
	FROM POFulfillments poff JOIN POFulfillmentBookingRequests br
	ON br.POFulfillmentId = poff.Id
	WHERE br.Status = 10

	UNION

	-- Non Edison Booking
	SELECT poff.Id, s.BookingDate
	FROM POFulfillments poff JOIN Shipments s
	ON s.POFulfillmentId = poff.Id
	WHERE s.Status = 'active' AND NOT EXISTS (
		SELECT 1
		FROM POFulfillmentBookingRequests br
		WHERE br.POFulfillmentId = poff.Id AND br.Status = 10
	)
)

UPDATE POFulfillments
SET BookingDate = ub.BookingDate
FROM POFulfillments poff JOIN UpdatedBookingCTE ub ON ub.Id = poff.Id


