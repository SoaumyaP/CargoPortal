
/****** Object:  Table [dbo].[BuyerCompliances]    Script Date: 5/13/2022 10:32:40 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BuyerCompliances]') AND type in (N'U'))
DROP TABLE [dbo].[BuyerCompliances]

/****** Object:  Table [dbo].[BuyerCompliances]    Script Date: 5/13/2022 10:32:40 AM ******/
SET ANSI_NULLS ON

SET QUOTED_IDENTIFIER ON

CREATE TABLE [dbo].[BuyerCompliances](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[OrganizationId] [bigint] NOT NULL,
	[OrganizationName] [nvarchar](max) NULL,
	[PrincipleCode] [nvarchar](max) NULL,
	[Name] [nvarchar](max) NULL,
	[EffectiveDate] [datetime2](7) NOT NULL,
	[EnforceCommercialInvoiceFormat] [bit] NOT NULL,
	[EnforcePackingListFormat] [bit] NOT NULL,
	[ShortShipTolerancePercentage] [decimal](18, 2) NULL,
	[OvershipTolerancePercentage] [decimal](18, 2) NULL,
	[Status] [int] NOT NULL,
	[Stage] [int] NOT NULL,
	[PurchaseOrderTransmissionMethods] [int] NOT NULL,
	[PurchaseOrderTransmissionFrequency] [int] NOT NULL,
	[ApprovalAlertFrequency] [int] NOT NULL,
	[PurchaseOrderTransmissionNotes] [nvarchar](max) NULL,
	[BookingPolicyAction] [int] NOT NULL,
	[BookingApproverSetting] [int] NOT NULL,
	[BookingApproverUser] [nvarchar](max) NULL,
	[ApprovalDuration] [int] NOT NULL,
	[IsAssignedAgent] [bit] NOT NULL,
	[HSCodeShipFromCountryIds] [nvarchar](max) NULL,
	[HSCodeShipToCountryIds] [nvarchar](max) NULL,
	[AllowToBookIn] [int] NOT NULL,
	[ProgressNotifyDay] [int] NOT NULL,
	[IsProgressCargoReadyDate] [bit] NOT NULL,
	[IsCompulsory] [bit] NOT NULL,
	[IntegrateWithWMS] [bit] NOT NULL,
	[ServiceType] [int] NOT NULL,
	[AgentAssignmentMethod] [int] NOT NULL,
	[EmailNotificationTime] [nvarchar](30) NULL,
	[IsEmailNotificationToSupplier] [bit] NOT NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((0)) FOR [ApprovalDuration]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((0)) FOR [IsAssignedAgent]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((30)) FOR [AllowToBookIn]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((0)) FOR [ProgressNotifyDay]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((0)) FOR [IsProgressCargoReadyDate]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((0)) FOR [IsCompulsory]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((0)) FOR [IntegrateWithWMS]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((10)) FOR [ServiceType]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT ((1)) FOR [AgentAssignmentMethod]

ALTER TABLE [dbo].[BuyerCompliances] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsEmailNotificationToSupplier]

