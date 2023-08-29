-- =============================================
-- Author:		Cuong Duong
-- Created date: 05 Feb 2021
-- Description:	To create index on cargo details, query data with other tables
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_UserAuditLogs_Searching' AND object_id = OBJECT_ID('[dbo].[UserAuditLogs]')) 
BEGIN  
	DROP INDEX [IX_UserAuditLogs_Searching] ON [dbo].[UserAuditLogs]
END
GO

CREATE NONCLUSTERED INDEX [IX_UserAuditLogs_Searching] ON [dbo].[UserAuditLogs]
(
	[Email] ASC,
	[AccessDateTime] DESC,
	[Feature] ASC,
	[OperatingSystem] ASC,
	[Browser] ASC,
	[ScreenSize] ASC,
	[UserAgent] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO