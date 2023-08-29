-- =============================================
-- Author:		Cuong Duong
-- Created date: 15 Sep 2021
-- Description:	To create index on scheduling/tasks grid
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Schedulings_Querying' AND object_id = OBJECT_ID('[dbo].[Schedulings]')) 
BEGIN  
	DROP INDEX [IX_Schedulings_Querying] ON [dbo].[Schedulings]
END
GO

CREATE NONCLUSTERED INDEX [IX_Schedulings_Querying] ON [dbo].[Schedulings]
(
	[CSPortalReportId] ASC,
	[CreatedOrganizationId] ASC
)
INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO



