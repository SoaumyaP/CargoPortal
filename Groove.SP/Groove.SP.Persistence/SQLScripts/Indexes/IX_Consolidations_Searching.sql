-- =============================================
-- Author:		Hau Nguyen
-- Created date: 27 April 2021
-- Description:	To create index on consolidations searching grid
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Consolidations_Searching' AND object_id = OBJECT_ID('[dbo].[Consolidations]')) 
BEGIN  
	DROP INDEX IX_Consolidations_Searching ON [dbo].[Consolidations]
END
GO

CREATE NONCLUSTERED INDEX [IX_Consolidations_Searching] ON [dbo].[Consolidations]
(
	[ConsolidationNo] ASC,
	[OriginCFS] ASC,
	[CFSCutoffDate] ASC,
	[ContainerNo] ASC,
	[LoadingDate] ASC,
	[EquipmentType] ASC,
	[Stage] ASC
)
INCLUDE([Id],[ContainerId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO