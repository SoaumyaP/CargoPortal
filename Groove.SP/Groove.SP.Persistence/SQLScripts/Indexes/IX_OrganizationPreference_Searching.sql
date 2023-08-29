-- =============================================
-- Author:		Cuong Duong
-- Created date: 19 Mar 2021
-- Description:	To create index on organization preferences for searching
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_OrganizationPreferences_Searching' AND object_id = OBJECT_ID('[dbo].[OrganizationPreferences]')) 
BEGIN  
	DROP INDEX [IX_OrganizationPreferences_Searching] ON [dbo].[OrganizationPreferences]
END
GO

CREATE NONCLUSTERED INDEX [IX_OrganizationPreferences_Searching] ON [dbo].[OrganizationPreferences]
(
	[OrganizationId] ASC,
	[ProductCode] ASC
)
INCLUDE([HSCode], [ChineseDescription]) 
WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
ON [PRIMARY]

GO



