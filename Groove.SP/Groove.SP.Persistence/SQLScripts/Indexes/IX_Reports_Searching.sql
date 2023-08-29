-- =============================================
-- Author:		Cuong Duong
-- Created date: 15 Sep 2021
-- Description:	To create index on reports grid
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Reports_Searching' AND object_id = OBJECT_ID('[dbo].[Reports]')) 
BEGIN  
	DROP INDEX [IX_Reports_Searching] ON [dbo].[Reports]
END
GO

CREATE NONCLUSTERED INDEX [IX_Reports_Searching] ON [dbo].[Reports]
(
	[ReportName] ASC,
	[ReportDescription] ASC,
	[ReportGroup] ASC,
	[LastRunTime] ASC,
	[SchedulingApply] ASC
)
INCLUDE([Id],[StoredProcedureName],[TelerikReportId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO



