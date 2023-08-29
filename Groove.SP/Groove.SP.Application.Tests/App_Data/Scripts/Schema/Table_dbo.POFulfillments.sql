



IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[POFulfillments]') AND type in (N'U'))
DROP TABLE [dbo].[POFulfillments]


CREATE TABLE [dbo].[POFulfillments](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Number] [nvarchar](20) NULL,
	[Owner] [nvarchar](max) NULL,
	[Status] [int] NOT NULL,
	[Stage] [int] NOT NULL,
	[CargoReadyDate] [datetime2](7) NULL,
	[Incoterm] [int] NOT NULL,
	[IsPartialShipment] [bit] NOT NULL,
	[ModeOfTransport] [int] NOT NULL,
	[PreferredCarrier] [bigint] NOT NULL,
	[LogisticsService] [int] NOT NULL,
	[MovementType] [int] NOT NULL,
	[ShipFrom] [bigint] NOT NULL,
	[ShipTo] [bigint] NOT NULL,
	[ShipFromName] [nvarchar](128) NULL,
	[ShipToName] [nvarchar](128) NULL,
	[ExpectedShipDate] [datetime2](7) NULL,
	[ExpectedDeliveryDate] [datetime2](7) NULL,
	[Remarks] [nvarchar](max) NULL,
	[IsForwarderBookingItineraryReady] [bit] NOT NULL,
	[IsGeneratePlanToShip] [bit] NOT NULL,
	[IsRejected] [bit] NOT NULL,
	[IsFulfilledFromPO] [bit] NOT NULL,
	[DeliveryPort] [nvarchar](128) NULL,
	[ReceiptPort] [nvarchar](128) NULL,
	[IsNotifyPartyAsConsignee] [bit] NOT NULL,
	[IsShipperPickup] [bit] NOT NULL,
	[DeliveryPortId] [bigint] NULL,
	[ReceiptPortId] [bigint] NULL,
	[IsContainDangerousGoods] [bit] NOT NULL,
	[IsBatteryOrChemical] [bit] NOT NULL,
	[IsCIQOrFumigation] [bit] NOT NULL,
	[IsExportLicence] [bit] NOT NULL,
	[BookingDate] [datetime2](7) NULL,
	[FulfilledFromPOType] [int] NOT NULL,
	[AgentAssignmentMode] [nvarchar](32) NOT NULL,
	[FulfillmentType] [int] NOT NULL,
	[VesselName] [nvarchar](512) NULL,
	[VoyageNo] [nvarchar](512) NULL,
	[PlantNo] [nvarchar](max) NULL,
	[ActualTimeArrival] [datetime2](7) NULL,
	[CompanyNo] [nvarchar](256) NULL,
	[ContainerNo] [nvarchar](256) NULL,
	[ETADestination] [datetime2](7) NULL,
	[ETDOrigin] [datetime2](7) NULL,
	[HAWBNo] [nvarchar](256) NULL,
	[NameofInternationalAccount] [nvarchar](256) NULL,
	[SONo] [nvarchar](256) NULL,
	[ConfirmBy] [nvarchar](256) NULL,
	[ConfirmedAt] [datetime2](7) NULL,
	[ConfirmedHubArrivalDate] [datetime2](7) NULL,
	[LoadingBay] [nvarchar](512) NULL,
	[Time] [nvarchar](20) NULL,
	[DeliveryMode] [nvarchar](256) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsGeneratePlanToShip]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsRejected]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsFulfilledFromPO]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsNotifyPartyAsConsignee]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsShipperPickup]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsContainDangerousGoods]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsBatteryOrChemical]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsCIQOrFumigation]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((0)) FOR [IsExportLicence]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((10)) FOR [FulfilledFromPOType]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT (N'Default') FOR [AgentAssignmentMode]

ALTER TABLE [dbo].[POFulfillments] ADD  DEFAULT ((1)) FOR [FulfillmentType]











