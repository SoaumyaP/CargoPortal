-- =============================================
-- Author:		Cuong Duong
-- Created date: 25 Mar 2021
-- Description:	To create index on BillOfLadings for querying data
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_BillOfLadings_Querying' AND object_id = OBJECT_ID('[dbo].[BillOfLadings]')) 
BEGIN  
	DROP INDEX [IX_BillOfLadings_Querying] ON [dbo].[BillOfLadings]
END
GO

CREATE NONCLUSTERED INDEX [IX_BillOfLadings_Querying] ON [dbo].[BillOfLadings]
(
	[BillOfLadingNo] ASC
)INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

