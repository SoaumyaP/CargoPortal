

-- =============================================
-- Author:		Cuong Duong
-- Created date: 14 April 2021
-- Description:	To create index on organizations to query data with column EdisonCompanyCodeId (CSED shipping document import)
-- =============================================


SET ANSI_PADDING ON 
GO  

IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Organizations_EdisonCompanyCodeId' AND object_id = OBJECT_ID('[dbo].[Organizations]')) 
BEGIN  
	DROP INDEX [IX_Organizations_EdisonCompanyCodeId] ON [dbo].[Organizations]
END

CREATE NONCLUSTERED INDEX [IX_Organizations_EdisonCompanyCodeId] ON [dbo].[Organizations]
(
	[EdisonCompanyCodeId] ASC
) INCLUDE([Id]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO


