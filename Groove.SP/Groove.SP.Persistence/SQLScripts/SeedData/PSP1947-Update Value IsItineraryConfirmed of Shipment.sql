UPDATE
    [Shipments]
SET
    [IsItineraryConfirmed] = POFF.IsForwarderBookingItineraryReady
FROM
    [Shipments] SM
INNER JOIN
    [POFulfillments] POFF
ON 
    SM.POFulfillmentId = POFF.Id;