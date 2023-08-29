/****** Object:  Table [dbo].[PurchaseOrders]   Script Date: 5/18/2022 10:46:06 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Shipments]') AND type in (N'U'))
DROP TABLE [dbo].[Shipments]

/****** Object:  Table [dbo].[Shipments]    Script Date: 5/18/2022 10:46:06 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[Shipments](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[ShipmentNo] [varchar](50) NOT NULL,
	[BuyerCode] [nvarchar](max) NULL,
	[CustomerReferenceNo] [varchar](3000) NULL,
	[ModeOfTransport] [nvarchar](512) NULL,
	[CargoReadyDate] [datetime2](7) NOT NULL,
	[BookingDate] [datetime2](7) NOT NULL,
	[ShipmentType] [nvarchar](128) NULL,
	[ShipFrom] [nvarchar](128) NOT NULL,
	[ShipFromETDDate] [datetime2](7) NOT NULL,
	[ShipTo] [nvarchar](128) NULL,
	[ShipToETADate] [datetime2](7) NULL,
	[Movement] [nvarchar](128) NULL,
	[TotalPackage] [decimal](18, 4) NOT NULL,
	[TotalPackageUOM] [nvarchar](20) NULL,
	[TotalUnit] [decimal](18, 4) NOT NULL,
	[TotalUnitUOM] [nvarchar](20) NULL,
	[TotalGrossWeight] [decimal](18, 4) NOT NULL,
	[TotalGrossWeightUOM] [nvarchar](20) NULL,
	[TotalNetWeight] [decimal](18, 4) NOT NULL,
	[TotalNetWeightUOM] [nvarchar](20) NULL,
	[TotalVolume] [decimal](18, 4) NOT NULL,
	[TotalVolumeUOM] [nvarchar](20) NULL,
	[ServiceType] [nvarchar](128) NULL,
	[Incoterm] [nvarchar](128) NULL,
	[Status] [nvarchar](128) NOT NULL,
	[IsFCL] [bit] NOT NULL,
	[POFulfillmentId] [bigint] NULL,
	[BookingNo] [varchar](50) NULL,
	[IsItineraryConfirmed] [bit] NOT NULL,
	[OrderType] [int] NOT NULL,
	[CarrierContractNo] [varchar](50) NULL,
	[AgentReferenceNo] [varchar](3000) NULL,
	[ShipperReferenceNo] [varchar](3000) NULL,
	[Factor] [int] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[Shipments] ADD  DEFAULT ((0)) FOR [IsItineraryConfirmed]

ALTER TABLE [dbo].[Shipments] ADD  DEFAULT ((1)) FOR [OrderType]


