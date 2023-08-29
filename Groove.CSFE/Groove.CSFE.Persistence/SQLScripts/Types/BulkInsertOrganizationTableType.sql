

IF TYPE_ID(N'BulkInsertOrganizationTableType') IS NOT NULL
BEGIN

	DROP TYPE [dbo].[BulkInsertOrganizationTableType]

END
GO


CREATE TYPE [dbo].[BulkInsertOrganizationTableType] AS TABLE(
	[CreatedBy] [varchar](128) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [varchar](128) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Code] [varchar](35) NOT NULL,
	[Name] [nvarchar](250) NOT NULL,
	[ContactEmail] [varchar](128) NULL,
	[ContactName] [nvarchar](256) NULL,
	[ContactNumber] [varchar](32) NULL,
	[Address] [nvarchar](500) NULL,
	[EdisonInstanceId] [nvarchar](32) NULL,
	[EdisonCompanyCodeId] [nvarchar](32) NULL,
	[CustomerPrefix] [varchar](5) NULL,
	[LocationId] [bigint] NULL,
	[OrganizationType] [int] NOT NULL,
	[Status] [int] NOT NULL,
	[AdminUser] [nvarchar](max) NULL,
	[WebsiteDomain] [nvarchar](max) NULL,
	[AddressLine2] [nvarchar](50) NULL,
	[AddressLine3] [nvarchar](50) NULL,
	[AddressLine4] [nvarchar](50) NULL,
	[TaxpayerId] [nvarchar](50) NULL,
	[WeChatOrWhatsApp] [varchar](32) NULL
)
GO


