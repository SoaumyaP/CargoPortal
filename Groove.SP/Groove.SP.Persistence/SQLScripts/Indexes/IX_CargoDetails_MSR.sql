
-- =============================================
-- Author:		Cuong Duong
-- Created date: 18 April 2022
-- Description:	To create index on cargo details, that supports on Master Summary Report query
-- =============================================

SET ANSI_PADDING ON 
GO  


IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CargoDetails_MSR' AND object_id = OBJECT_ID('[dbo].[CargoDetails]')) 
BEGIN  
	DROP INDEX [IX_CargoDetails_MSR] ON [dbo].[CargoDetails]
END
GO

CREATE NONCLUSTERED INDEX [IX_CargoDetails_MSR] ON [dbo].[CargoDetails]
(
	[ShipmentId] ASC,
	[ItemId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


