-- =============================================
-- Author:		Cuong Duong
-- Created date: 19 Jan 2021
-- Description:	To create index on cargo details, query data with other tables
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CargoDetails_Querying' AND object_id = OBJECT_ID('[dbo].[CargoDetails]')) 
BEGIN  
	DROP INDEX IX_CargoDetails_Querying ON [dbo].[CargoDetails]
END
GO

CREATE NONCLUSTERED INDEX [IX_CargoDetails_Querying] ON [dbo].[CargoDetails]
(
	[OrderType] ASC,
	[ShipmentId] ASC,
	[OrderId] ASC,
	[ItemId] ASC
)
INCLUDE([Id], [ProductNumber]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO