
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

SET ANSI_PADDING ON
GO


IF  NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ShipmentMilestones]') AND type in (N'U'))
BEGIN

	CREATE TABLE [dbo].[ShipmentMilestones](
		[ShipmentId] [bigint] NOT NULL,
		[ActivityDate] [datetime2](7) NOT NULL,
		[ActivityCode] [nvarchar](10) NOT NULL,
		[Milestone] [nvarchar](50) NOT NULL,
	 CONSTRAINT [PK_ShipmentMilestones] PRIMARY KEY CLUSTERED 
	(
		[ShipmentId] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

	CREATE NONCLUSTERED INDEX [IX_ShipmentMilestones_Query] ON [dbo].[ShipmentMilestones]
	(
		[ActivityCode] ASC,
		[Milestone] ASC
	)
	INCLUDE([ShipmentId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]


END

GO
