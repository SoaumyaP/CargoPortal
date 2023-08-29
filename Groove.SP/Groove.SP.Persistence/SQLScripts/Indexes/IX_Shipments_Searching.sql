-- =============================================
-- Author:		Cuong Duong
-- Created date: 15 Mar 2021
-- Description:	To create index on shipment grid
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Shipments_Searching' AND object_id = OBJECT_ID('[dbo].[Shipments]')) 
BEGIN  
	DROP INDEX [IX_Shipments_Searching] ON [dbo].[Shipments]
END
GO

CREATE NONCLUSTERED INDEX [IX_Shipments_Searching] ON [dbo].[Shipments]
(
	[ShipmentNo] ASC,
	[CustomerReferenceNo] ASC,
	[ShipFromETDDate] ASC,
	[BookingDate] ASC,
	[ShipFrom] ASC,
	[ShipTo] ASC,
	[Status] ASC
)
INCLUDE([Id], [BookingNo], [POFulfillmentId]) 
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
ON [PRIMARY]

GO



