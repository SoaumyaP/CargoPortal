
SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_IntegrationLogs_PostingDate_Id' AND object_id = OBJECT_ID('[dbo].[IntegrationLogs]')) 
BEGIN  
	DROP INDEX [IX_IntegrationLogs_PostingDate_Id] ON [dbo].[IntegrationLogs]
END
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_IntegrationLogs_Searching' AND object_id = OBJECT_ID('[dbo].[IntegrationLogs]')) 
BEGIN  
DROP INDEX [IX_IntegrationLogs_Searching] ON [dbo].[IntegrationLogs]
END
CREATE NONCLUSTERED INDEX [IX_IntegrationLogs_Searching] ON [dbo].[IntegrationLogs]
(
	[PostingDate] DESC,
	[Status] ASC
)
INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

