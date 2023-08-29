IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POFulfillmentContacts]') AND type in (N'U'))
DROP TABLE [dbo].[POFulfillmentContacts]


CREATE TABLE [dbo].[POFulfillmentContacts](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[POFulfillmentId] [bigint] NOT NULL,
	[OrganizationId] [bigint] NOT NULL,
	[OrganizationRole] [varchar](50) NOT NULL,
	[CompanyName] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](250) NULL,
	[ContactName] [nvarchar](100) NULL,
	[ContactNumber] [nvarchar](100) NULL,
	[ContactEmail] [nvarchar](100) NULL,
	[AddressLine2] [nvarchar](50) NULL,
	[AddressLine3] [nvarchar](50) NULL,
	[AddressLine4] [nvarchar](50) NULL
) ON [PRIMARY]