
/****** Object:  Table [cruise].[CruiseOrderItemChangeLogs]    Script Date: 2/5/2021 10:40:49 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [cruise].[CruiseOrderItemChangeLogs](
	[Id] [bigint] IDENTITY(1,1) NOT NULL,
	[RowVersion] [timestamp] NULL,
	[CreatedBy] [nvarchar](256) NULL,
	[CreatedDate] [datetime2](7) NOT NULL,
	[UpdatedBy] [nvarchar](256) NULL,
	[UpdatedDate] [datetime2](7) NULL,
	[JsonCurrentData] [nvarchar](max) NULL,
	[JsonNewData] [nvarchar](max) NULL,
	[CruiseOrderItemId] [bigint] NOT NULL,
 CONSTRAINT [PK_CruiseOrderItemChangeLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

