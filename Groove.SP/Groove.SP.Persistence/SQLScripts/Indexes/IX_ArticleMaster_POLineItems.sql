-- =============================================
-- Author:		Cuong Duong
-- Created date: 18 May 2020
-- Description:	#PSP-1541 [Booking] Customer PO: adding Booked Package, Volume, Gross Weight, Net Weight
-- =============================================

SET ANSI_PADDING ON 
GO  
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ArticleMaster_POLineItems' AND object_id = OBJECT_ID('[dbo].[ArticleMaster]')) 
BEGIN  
	CREATE NONCLUSTERED INDEX [IX_ArticleMaster_POLineItems] ON [dbo].[ArticleMaster]
	(
		[CompanyCode] ASC,
		[ItemNo] ASC
	)
	INCLUDE([OuterQuantity],[OuterDepth],[OuterHeight],[OuterWidth],[OuterGrossWeight]) 
	WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) 
	ON [PRIMARY]
END
GO


