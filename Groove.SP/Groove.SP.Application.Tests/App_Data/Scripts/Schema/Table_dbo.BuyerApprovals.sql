IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BuyerApprovals]') AND type in (N'U'))
DROP TABLE [dbo].[BuyerApprovals]

CREATE TABLE [dbo].[BuyerApprovals](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[Status] [int] NOT NULL,
	[Stage] [int] NOT NULL,
	[POFulfillmentId] [bigint] NOT NULL,
	[Transaction] [nvarchar](max) NULL,
	[Reference] [varchar](13) NULL,
	[Owner] [nvarchar](max) NULL,
	[ExceptionType] [int] NOT NULL,
	[DueOnDate] [datetime2](7) NULL,
	[Customer] [nvarchar](max) NULL,
	[ApproverSetting] [int] NOT NULL,
	[ApproverUser] [nvarchar](max) NULL,
	[ApproverOrgId] [bigint] NULL,
	[ResponseOn] [datetime2](7) NULL,
	[RequestByOrganization] [nvarchar](max) NULL,
	[Requestor] [nvarchar](max) NULL,
	[ExceptionActivity] [nvarchar](max) NULL,
	[ActivityDate] [datetime2](7) NOT NULL,
	[AlertNotificationFrequencyType] [int] NOT NULL,
	[Severity] [nvarchar](max) NULL,
	[ExceptionDetail] [nvarchar](max) NULL,
	[Reason] [nvarchar](max) NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]