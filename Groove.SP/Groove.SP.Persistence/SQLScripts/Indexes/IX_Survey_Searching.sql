
-- =============================================
-- Author:		Dong Tran
-- Created date: 14 Oct 2022
-- Description:	To create index on survey grid
-- =============================================

GO
SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Survey_Searching' AND object_id = OBJECT_ID('[dbo].[Surveys]')) 
BEGIN  
	DROP INDEX [IX_Survey_Searching] ON [dbo].[Surveys]
END
GO

/****** Object:  Index [IX_Survey_Searching]    Script Date: 10/14/2022 2:12:09 PM ******/
CREATE NONCLUSTERED INDEX [IX_Survey_Searching] ON [dbo].[Surveys]
(
	[Name] ASC,
	[Status] ASC,
	[PublishedDate] ASC,
	[CreatedBy] ASC,
	[CreatedDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


