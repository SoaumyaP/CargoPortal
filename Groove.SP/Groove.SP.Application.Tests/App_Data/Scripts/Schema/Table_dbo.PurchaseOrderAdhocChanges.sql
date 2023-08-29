IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseOrderAdhocChanges]') AND type in (N'U'))
DROP TABLE [dbo].[PurchaseOrderAdhocChanges]

CREATE TABLE [dbo].[PurchaseOrderAdhocChanges](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[POFulfillmentId] [bigint] NOT NULL,
	[PurchaseOrderId] [bigint] NOT NULL,
	[JsonCurrentData] [nvarchar](max) NULL,
	[JsonNewData] [nvarchar](max) NULL,
	[Priority] [int] NOT NULL,
	[Message] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]