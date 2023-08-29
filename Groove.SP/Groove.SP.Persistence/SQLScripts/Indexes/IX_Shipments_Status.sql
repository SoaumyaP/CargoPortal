
-- =============================================
-- Author:		Cuong Duong
-- Created date: 12 July 2022
-- Description:	To create index on shipment's status that supports container list
-- =============================================

SET ANSI_PADDING ON 
GO  

IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Shipments_Status' AND object_id = OBJECT_ID('[dbo].[Shipments]')) 
BEGIN  
	DROP INDEX [IX_Shipments_Status] ON [dbo].[Shipments]
END
GO

SET ANSI_PADDING ON
GO


CREATE NONCLUSTERED INDEX [IX_Shipments_Status] ON [dbo].[Shipments]
(
	[Status] ASC
)
INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

