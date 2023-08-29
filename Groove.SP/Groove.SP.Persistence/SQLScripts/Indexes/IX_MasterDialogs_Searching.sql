-- =============================================
-- Author:		Hau Nguyen
-- Created date: 16 Mar 2021
-- Description:	PSP-2362
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MasterDialogs_Searching' AND object_id = OBJECT_ID('[dbo].[MasterDialogs]')) 
BEGIN  
	DROP INDEX IX_MasterDialogs_Searching ON [dbo].[MasterDialogs]
END
GO

CREATE NONCLUSTERED INDEX [IX_MasterDialogs_Searching] ON [dbo].[MasterDialogs]
(
	[CreatedDate] ASC,
	[Owner] ASC,
	[Category] ASC,
	[DisplayOn] ASC,
	[FilterCriteria] ASC,
	[FilterValue] ASC,
	[Message] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
