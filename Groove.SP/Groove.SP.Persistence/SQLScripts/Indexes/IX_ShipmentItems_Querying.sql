-- =============================================
-- Author:		Cuong Duong
-- Created date: 25 Mar 2021
-- Description:	To create index on ShipmentItems for querying data
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ShipmentItems_Querying' AND object_id = OBJECT_ID('[dbo].[ShipmentItems]')) 
BEGIN  
	DROP INDEX [IX_ShipmentItems_Querying] ON [dbo].[ShipmentItems]
END
GO

CREATE NONCLUSTERED INDEX [IX_ShipmentItems_Querying] ON [dbo].[ShipmentItems]
(
	[CustomerPONumber] ASC,
	[ProductCode] ASC
)INCLUDE([Id], [ShipmentId], [PurchaseOrderId], [POLineItemId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

