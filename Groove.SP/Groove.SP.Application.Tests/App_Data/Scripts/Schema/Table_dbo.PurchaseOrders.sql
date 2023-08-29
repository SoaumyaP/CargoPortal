
/****** Object:  Table [dbo].[PurchaseOrders]    Script Date: 5/12/2022 5:46:26 PM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PurchaseOrders]') AND type in (N'U'))
DROP TABLE [dbo].[PurchaseOrders]

/****** Object:  Table [dbo].[PurchaseOrders]    Script Date: 5/12/2022 5:46:26 PM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[PurchaseOrders](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](128) NOT NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[PONumber] [varchar](512) NOT NULL,
	[CarrierCode] [nvarchar](128) NULL,
	[EarliestDeliveryDate] [datetime2](7) NULL,
	[EarliestShipDate] [datetime2](7) NULL,
	[ExpectedDeliveryDate] [datetime2](7) NULL,
	[ExpectedShipDate] [datetime2](7) NULL,
	[GatewayCode] [nvarchar](50) NULL,
	[Incoterm] [varchar](3) NULL,
	[LatestDeliveryDate] [datetime2](7) NULL,
	[LatestShipDate] [datetime2](7) NULL,
	[ModeOfTransport] [nvarchar](max) NULL,
	[NumberOfLineItems] [bigint] NULL,
	[POIssueDate] [datetime2](7) NULL,
	[CustomerReferences] [nvarchar](512) NULL,
	[Department] [nvarchar](512) NULL,
	[Season] [nvarchar](512) NULL,
	[ShipFrom] [nvarchar](512) NULL,
	[ShipFromId] [bigint] NULL,
	[ShipTo] [nvarchar](512) NULL,
	[ShipToId] [bigint] NULL,
	[PaymentCurrencyCode] [nvarchar](16) NULL,
	[PaymentTerms] [nvarchar](512) NULL,
	[Status] [int] NOT NULL,
	[Stage] [int] NOT NULL,
	[CargoReadyDate] [datetime2](7) NULL,
	[PORemark] [nvarchar](max) NULL,
	[POTerms] [nvarchar](512) NULL,
	[HazardousMaterialsInstruction] [varchar](max) NULL,
	[SpecialHandlingInstruction] [nvarchar](max) NULL,
	[CarrierName] [nvarchar](512) NULL,
	[GatewayName] [nvarchar](512) NULL,
	[ShipFromName] [nvarchar](512) NULL,
	[ShipToName] [nvarchar](512) NULL,
	[NotifyUserId] [bigint] NULL,
	[POKey] [varchar](612) NOT NULL,
	[ContainerType] [int] NULL,
	[BlanketPOId] [bigint] NULL,
	[POType] [int] NOT NULL,
	[ProductionStarted] [bit] NOT NULL,
	[ProposeDate] [datetime2](7) NULL,
	[QCRequired] [bit] NOT NULL,
	[Remark] [nvarchar](max) NULL,
	[ShortShip] [bit] NOT NULL,
	[SplitShipment] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[PurchaseOrders] ADD  DEFAULT (N'') FOR [POKey]

ALTER TABLE [dbo].[PurchaseOrders] ADD  DEFAULT ((10)) FOR [POType]

ALTER TABLE [dbo].[PurchaseOrders] ADD  DEFAULT ((0)) FOR [ProductionStarted]

ALTER TABLE [dbo].[PurchaseOrders] ADD  DEFAULT ((0)) FOR [QCRequired]

ALTER TABLE [dbo].[PurchaseOrders] ADD  DEFAULT ((0)) FOR [ShortShip]

ALTER TABLE [dbo].[PurchaseOrders] ADD  DEFAULT ((0)) FOR [SplitShipment]

