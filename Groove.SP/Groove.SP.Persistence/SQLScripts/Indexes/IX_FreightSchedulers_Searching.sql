-- =============================================
-- Author:		Dong Tran
-- Created date: 21 Dec 2020
-- Description:	PSP-2229, PSP-2218
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_FreightSchedulers_Searching' AND object_id = OBJECT_ID('[dbo].[FreightSchedulers]')) 
BEGIN  
	DROP INDEX IX_FreightSchedulers_Searching ON [dbo].[FreightSchedulers]
END
GO

CREATE NONCLUSTERED INDEX [IX_FreightSchedulers_Searching] ON [dbo].[FreightSchedulers]
(
	[ModeOfTransport] ASC,
	[CarrierName] ASC,
	[VesselName] ASC,
	[MAWB] ASC,
	[LocationFromName] ASC,
	[LocationToName] ASC,
	[ETDDate] ASC,
	[ETADate] ASC,
	[Voyage] ASC,
	[CarrierCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
