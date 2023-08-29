

IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_BuyerCompliances_Stage' AND object_id = OBJECT_ID('[dbo].[BuyerCompliances]')) 
BEGIN  
	DROP INDEX [IX_BuyerCompliances_Stage] ON [dbo].[BuyerCompliances]
END
GO

CREATE NONCLUSTERED INDEX [IX_BuyerCompliances_Stage] ON [dbo].[BuyerCompliances]
(
	[Stage] ASC,
	[OrganizationId] ASC,
	[IsAllowShowAdditionalInforPOListing] ASC
)
INCLUDE([ProgressNotifyDay],[IsProgressCargoReadyDate]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO


