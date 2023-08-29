-- =============================================
-- Author:		Cuong Duong
-- Created date: 25 Mar 2021
-- Description:	To create index on MasterBills for querying data
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MasterBills_Querying' AND object_id = OBJECT_ID('[dbo].[MasterBills]')) 
BEGIN  
	DROP INDEX [IX_MasterBills_Querying] ON [dbo].[MasterBills]
END
GO

CREATE NONCLUSTERED INDEX [IX_MasterBills_Querying] ON [dbo].[MasterBills]
(
	[IsDirectMaster] ASC,
	[MasterBillOfLadingNo] ASC	
)INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

