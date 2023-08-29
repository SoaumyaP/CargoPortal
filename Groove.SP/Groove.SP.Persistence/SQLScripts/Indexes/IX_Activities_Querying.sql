-- =============================================
-- Author:		Cuong Duong
-- Created date: 11 Mar 2021
-- Description:	To create index on purchase order searching grid in statistic mode
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Activities_Querying' AND object_id = OBJECT_ID('[dbo].[Activities]')) 
BEGIN  
	DROP INDEX [IX_Activities_Querying] ON [dbo].[Activities]
END
GO

CREATE NONCLUSTERED INDEX [IX_Activities_Querying] ON [dbo].[Activities]
(
	[ActivityType] ASC,
	[ActivityCode] ASC,
	[Location] ASC
)
INCLUDE([ActivityDate]) 
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
ON [PRIMARY]

GO



