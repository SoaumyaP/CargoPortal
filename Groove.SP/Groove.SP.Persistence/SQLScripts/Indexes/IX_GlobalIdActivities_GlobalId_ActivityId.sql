-- =============================================
-- Author:		 Dong Tran
-- Created date: 28 Jul 2022
-- Description:	 To create index on GlobalIdActivities for querying data on Shipment list
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_GlobalIdActivities_GlobalId_ActivityId' AND object_id = OBJECT_ID('[dbo].[GlobalIdActivities]')) 
BEGIN  
	DROP INDEX IX_GlobalIdActivities_GlobalId_ActivityId ON [dbo].[GlobalIdActivities]
END



CREATE NONCLUSTERED INDEX [IX_GlobalIdActivities_GlobalId_ActivityId] ON [dbo].[GlobalIdActivities]
(
	[GlobalId] ASC,
	[ActivityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


