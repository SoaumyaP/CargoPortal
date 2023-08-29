-- =============================================
-- Author:		Dong Tran
-- Created date: 28 July 2021
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Location_Searching' AND object_id = OBJECT_ID('[dbo].[Locations]')) 
BEGIN  
	DROP INDEX IX_Location_Searching ON [dbo].[Locations]
END
GO

CREATE NONCLUSTERED INDEX [IX_Location_Searching] ON [dbo].[Locations]
(
	[Name] ASC,
	[EdiSonPortCode] ASC,
	[LocationDescription] ASC
)
INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO


