
/****** Object:  Table [dbo].[PurchaseOrderContacts]    Script Date: 5/12/2022 5:46:26 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseOrderContacts]') AND type in (N'U'))
DROP TABLE [dbo].[PurchaseOrderContacts]

/****** Object:  Table [dbo].[PurchaseOrderContacts]    Script Date: 5/12/2022 5:46:26 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[PurchaseOrderContacts](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[PurchaseOrderId] [bigint] NOT NULL,
	[OrganizationId] [bigint] NOT NULL,
	[OrganizationCode] [nvarchar](35) NOT NULL,
	[OrganizationRole] [varchar](50) NOT NULL,
	[CompanyName] [nvarchar](100) NULL,
	[AddressLine4] [nvarchar](250) NULL,
	[AddressLine1] [nvarchar](250) NULL,
	[AddressLine2] [nvarchar](250) NULL,
	[AddressLine3] [nvarchar](250) NULL,
	[Department] [nvarchar](250) NULL,
	[ContactName] [nvarchar](250) NULL,
	[Name] [nvarchar](100) NULL,
	[ContactNumber] [nvarchar](100) NULL,
	[ContactEmail] [nvarchar](100) NULL,
	[References] [nvarchar](100) NULL
) ON [PRIMARY]

