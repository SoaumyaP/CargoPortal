
SET ANSI_PADDING ON 
GO  
IF EXISTS (SELECT * FROM sys.indexes WHERE name='IX_POFulfillmentOrders_LinktoPOPOFF' AND object_id = OBJECT_ID('[dbo].[POFulfillmentOrders]')) 
BEGIN  
	DROP INDEX [IX_POFulfillmentOrders_LinktoPOPOFF] ON [dbo].[POFulfillmentOrders]
END
GO

CREATE NONCLUSTERED INDEX [IX_POFulfillmentOrders_LinktoPOPOFF] ON [dbo].[POFulfillmentOrders]
(
	[PurchaseOrderId] ASC,
	[POFulfillmentId] ASC
)
INCLUDE([POLineItemId],[CustomerPONumber]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
