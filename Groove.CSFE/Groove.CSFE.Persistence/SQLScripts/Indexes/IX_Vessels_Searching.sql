
-- Author:		Dong Tran
-- Created date:  1/8/2021 10:05:33 AM ******/

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Vessels_Searching' AND object_id = OBJECT_ID('[dbo].[Vessels]')) 
BEGIN  
	DROP INDEX IX_Vessels_Searching ON [dbo].[Vessels]
END

GO
CREATE NONCLUSTERED INDEX [IX_Vessels_Searching] ON [dbo].[Vessels]
(
	[Status] ASC,
	[IsRealVessel] ASC,
	[Name] ASC,
	[Code] ASC
) INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
