-- =============================================
-- Author:		Cuong Duong
-- Created date: 25 Mar 2021
-- Description:	To create index on POFulfillmentOrders for querying data
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_POFulfillmentOrders_Querying' AND object_id = OBJECT_ID('[dbo].[POFulfillmentOrders]')) 
BEGIN  
	DROP INDEX [IX_POFulfillmentOrders_Querying] ON [dbo].[POFulfillmentOrders]
END
GO

CREATE NONCLUSTERED INDEX [IX_POFulfillmentOrders_Querying] ON [dbo].[POFulfillmentOrders]
(
	[CustomerPONumber] ASC,
	[ProductCode] ASC
)INCLUDE([Id], [POFulfillmentId], [PurchaseOrderId], [POLineItemId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

