SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_POLineItems_PurchaseOrderId' AND object_id = OBJECT_ID('[dbo].[POLineItems]')) 
BEGIN  
	DROP INDEX [IX_POLineItems_PurchaseOrderId] ON [dbo].[POLineItems]
END
GO

/****** Object:  Index [IX_POLineItems_PurchaseOrderId]    Script Date: 2023-05-24 3:22:32 PM ******/
CREATE NONCLUSTERED INDEX [IX_POLineItems_PurchaseOrderId] ON [dbo].[POLineItems]
(
	[PurchaseOrderId] ASC
)
INCLUDE([LineOrder],[ProductCode],[GridValue]) 
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
