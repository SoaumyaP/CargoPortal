-- =============================================
-- Author:		 Dong Tran
-- Created date: 28 Jul 2022
-- Description:	 To create index on ShipmentContacts for querying data on Shipment list
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ShipmentContacts_OrganizationRole_ShipmentId' AND object_id = OBJECT_ID('[dbo].[ShipmentContacts]')) 
BEGIN  
	DROP INDEX IX_ShipmentContacts_OrganizationRole_ShipmentId ON [dbo].[ShipmentContacts]
END

/****** Object:  Index [IX_ShipmentContacts_OrganizationRole_ShipmentId]    Script Date: 7/28/2022 10:40:45 AM ******/
CREATE NONCLUSTERED INDEX [IX_ShipmentContacts_OrganizationRole_ShipmentId] ON [dbo].[ShipmentContacts]
(
	[ShipmentId] ASC,
	[OrganizationRole] ASC
)
INCLUDE([CompanyName]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


