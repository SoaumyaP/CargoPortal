-- =============================================
-- Author:		Hau Nguyen
-- Created date: 13 May 2022
-- Description:	To create index on Article Master searching grid
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ArticleMaster_Searching' AND object_id = OBJECT_ID('[dbo].[ArticleMaster]')) 
BEGIN  
	DROP INDEX IX_ArticleMaster_Searching ON [dbo].[ArticleMaster]
END
GO

CREATE NONCLUSTERED INDEX [IX_ArticleMaster_Searching] ON [dbo].[ArticleMaster]
(
	[CompanyCode] ASC,
	[ItemNo] ASC,
	[Status] ASC
)
INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO