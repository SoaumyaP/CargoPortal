
-- Author:	Hau Nguyen
-- Created date:  7/28/2021 10:05:33 AM ******/

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Carriers_Searching' AND object_id = OBJECT_ID('[dbo].[Carriers]')) 
BEGIN  
	DROP INDEX IX_Carriers_Searching ON [dbo].[Carriers]
END

GO
CREATE NONCLUSTERED INDEX [IX_Carriers_Searching] ON [dbo].[Carriers]
(
	[ModeOfTransport] ASC,
	[CarrierCode] ASC,
	[Name] ASC,
	[Status] ASC
) INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
