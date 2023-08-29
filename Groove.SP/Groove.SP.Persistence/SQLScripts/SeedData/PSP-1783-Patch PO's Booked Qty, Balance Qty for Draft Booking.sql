-- =============================================
-- Author:			Phuoc Le
-- Created date:	27 July 2020
-- Description:		PSP-1783 [PROD] Patch PO's Booked Qty, Balance Qty for Draft Booking
-- =============================================


WITH POFulfillmentOrdersCTE AS 
(
	SELECT POLineItemId, SUM(FulfillmentUnitQty) AS [BookedQty]
	FROM POFulfillments poff JOIN POFulfillmentOrders pfo ON pfo.POFulfillmentId = poff.Id
	WHERE poff.Stage = 10 AND poff.Status = 10 AND NOT EXISTS (
		SELECT 1
		FROM BuyerApprovals ba
		WHERE ba.POFulfillmentId = poff.Id AND ba.Stage = 10
	)
	GROUP BY POLineItemId
)


UPDATE POLineItems
SET BalanceUnitQty = BalanceUnitQty + CTE.BookedQty, BookedUnitQty = BookedUnitQty - CTE.BookedQty
FROM POLineItems POL
INNER JOIN POFulfillmentOrdersCTE CTE ON POL.Id = CTE.POLineItemId;

