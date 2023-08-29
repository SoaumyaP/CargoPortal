-- =============================================
-- Author:		Cuong Duong
-- Created date: 19 Jan 2021
-- Description:	To create index on purchase order searching grid
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_PurchaseOrders_Searching' AND object_id = OBJECT_ID('[dbo].[PurchaseOrders]')) 
BEGIN  
	DROP INDEX IX_PurchaseOrders_Searching ON [dbo].[PurchaseOrders]
END
GO

CREATE NONCLUSTERED INDEX [IX_PurchaseOrders_Searching] ON [dbo].[PurchaseOrders]
(
	[POType] ASC,
	[Stage] ASC,
	[PONumber] ASC,
	[CreatedDate] ASC,
	[CargoReadyDate] ASC,
	[Status] ASC
)
INCLUDE([Id],[BlanketPOId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO