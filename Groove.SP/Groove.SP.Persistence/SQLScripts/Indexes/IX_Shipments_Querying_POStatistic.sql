-- =============================================
-- Author:		Cuong Duong
-- Created date: 15 Mar 2021
-- Description:	To create index on shipment table for grid in statistic mode
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Shipments_Querying_POStatistic' AND object_id = OBJECT_ID('[dbo].[Shipments]')) 
BEGIN  
	DROP INDEX [IX_Shipments_Querying_POStatistic] ON [dbo].[Shipments]
END
GO

CREATE NONCLUSTERED INDEX [IX_Shipments_Querying_POStatistic] ON [dbo].[Shipments]
(
	
	[ModeOfTransport] ASC,
	[Movement] ASC,
	[ServiceType] ASC,
	[IsFCL] ASC,
	[ShipToETADate] ASC,
	[ShipFromETDDate] ASC
)
INCLUDE([Id], [ShipmentNo], [BookingNo], [POFulfillmentId]) 
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
ON [PRIMARY]

GO



