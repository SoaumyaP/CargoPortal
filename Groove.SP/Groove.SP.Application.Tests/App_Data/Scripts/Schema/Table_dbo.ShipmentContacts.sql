/****** Object:  Table [dbo].[ShipmentContacts]    Script Date: 5/18/2022 10:53:50 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShipmentContacts]') AND type in (N'U'))
DROP TABLE [dbo].ShipmentContacts

/****** Object:  Table [dbo].[ShipmentContacts]    Script Date: 5/18/2022 10:53:50 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[ShipmentContacts](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentId] [bigint] NOT NULL,
	[OrganizationId] [bigint] NOT NULL,
	[OrganizationRole] [varchar](50) NOT NULL,
	[CompanyName] [nvarchar](100) NOT NULL,
	[Address] [nvarchar](250) NULL,
	[ContactName] [nvarchar](250) NULL,
	[ContactNumber] [nvarchar](100) NULL,
	[ContactEmail] [nvarchar](100) NULL
) ON [PRIMARY]