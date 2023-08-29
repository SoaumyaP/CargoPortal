-- =============================================
-- Author:		Cuong Duong
-- Created date: 19 Jan 2021
-- Description:	To create index on container searching grid
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Containers_Searching' AND object_id = OBJECT_ID('[dbo].[Containers]')) 
BEGIN  
	DROP INDEX IX_Containers_Searching ON [dbo].[Containers]
END
GO

CREATE NONCLUSTERED INDEX [IX_Containers_Searching] ON [dbo].[Containers]
(
	[ShipFromETDDate] ASC,
	[ContainerNo] ASC,
	[ShipFrom] ASC,
	[ShipTo] ASC,
	[ShipToETADate] ASC,
	[Movement] ASC
)
INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO