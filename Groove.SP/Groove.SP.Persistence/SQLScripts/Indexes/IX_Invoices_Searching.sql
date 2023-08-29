-- =============================================
-- Author:		Cuong Duong
-- Created date: 06 May 2021
-- Description:	To create index on invoices, searching grid
-- =============================================

SET ANSI_PADDING ON 
GO  
IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Invoices_Searching' AND object_id = OBJECT_ID('[dbo].[Invoices]')) 
BEGIN  
	DROP INDEX [IX_Invoices_Searching] ON [dbo].[Invoices]
END
GO

CREATE NONCLUSTERED INDEX [IX_Invoices_Searching] ON [dbo].[Invoices]
(
	[InvoiceNo] ASC,
	[InvoiceDate] ASC,
	[ETDDate] ASC,
	[ETADate] ASC,
	[BillOfLadingNo] ASC,
	[JobNo] ASC,
	[BillTo] ASC,
	[BillBy] ASC,
	[InvoiceType] ASC
)
INCLUDE([Id],[BlobId],[FileName]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO