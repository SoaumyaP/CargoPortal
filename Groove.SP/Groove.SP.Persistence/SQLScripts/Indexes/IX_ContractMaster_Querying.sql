-- =============================================
-- Author:		Cuong Duong
-- Created date: 13 Jul 2021
-- Description:	To create index on ContractMaster to query data (join MasterBills, ...)
-- =============================================

SET ANSI_PADDING ON 
GO  

IF  EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ContractMaster_Querying' AND object_id = OBJECT_ID('[dbo].[ContractMaster]')) 
BEGIN  
	DROP INDEX [IX_ContractMaster_Querying] ON [dbo].[ContractMaster]
END
GO

CREATE NONCLUSTERED INDEX [IX_ContractMaster_Querying] ON [dbo].[ContractMaster]
(
	[Status] ASC,
	[CarrierCode] ASC,
	[ValidFrom] ASC,
	[ValidTo] ASC,
	[RealContractNo] ASC
)
INCLUDE([Id],[CarrierContractNo],[ContractHolder]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO