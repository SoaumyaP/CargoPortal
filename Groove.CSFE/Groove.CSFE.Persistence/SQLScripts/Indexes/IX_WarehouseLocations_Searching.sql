
-- =============================================
-- Author:		Cuong Duong
-- Created date: 26 Nov 2021
-- Description:	To create index on WarehouseLocations for searching
-- =============================================

SET ANSI_PADDING ON 
GO  

IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_WarehouseLocations_Searching' AND object_id = OBJECT_ID('[dbo].[WarehouseLocations]')) 
BEGIN  
	DROP INDEX [IX_WarehouseLocations_Searching] ON [dbo].[WarehouseLocations]
END

GO


CREATE NONCLUSTERED INDEX [IX_WarehouseLocations_Searching] ON [dbo].[WarehouseLocations]
(
	[Code] ASC,
	[Name] ASC,
	[ContactPerson] ASC
)
INCLUDE([LocationId],[OrganizationId],[Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
