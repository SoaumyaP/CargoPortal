UPDATE [POFulfillments]
SET [ReceiptPort] = COALESCE([ShipFromName], '')
WHERE [ReceiptPort] IS NULL OR [ReceiptPort] = ''


UPDATE [POFulfillments]
SET [DeliveryPort] = COALESCE([ShipToName], '')
WHERE [DeliveryPort] IS NULL OR [DeliveryPort] = ''