
/****** Object:  Index [IX_ReportingUserProfiles_ReportUsername_Lookup]    Script Date: 8/16/2021 9:31:24 AM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[report].[ReportingUserProfiles]') AND name = N'IX_ReportingUserProfiles_ReportUsername_Lookup')
DROP INDEX [IX_ReportingUserProfiles_ReportUsername_Lookup] ON [report].[ReportingUserProfiles]
GO
/****** Object:  Index [IX_ReportingUserProfiles_OrganizationId_Lookup]    Script Date: 8/16/2021 9:31:24 AM ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[report].[ReportingUserProfiles]') AND name = N'IX_ReportingUserProfiles_OrganizationId_Lookup')
DROP INDEX [IX_ReportingUserProfiles_OrganizationId_Lookup] ON [report].[ReportingUserProfiles]
GO
/****** Object:  Table [report].[ReportingUserProfiles]    Script Date: 8/16/2021 9:31:24 AM ******/
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[report].[ReportingUserProfiles]') AND type in (N'U'))
DROP TABLE [report].[ReportingUserProfiles]
GO
/****** Object:  Table [report].[ReportingUserProfiles]    Script Date: 8/16/2021 9:31:24 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [report].[ReportingUserProfiles](
	[OrganizationId] [bigint] NOT NULL,
	[ReportUsername] [nvarchar](256) NOT NULL,
	[SystemUser] [bit]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ReportingUserProfiles_OrganizationId_Lookup]    Script Date: 8/16/2021 9:31:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_ReportingUserProfiles_OrganizationId_Lookup] ON [report].[ReportingUserProfiles]
(
	[ReportUsername] ASC
)
INCLUDE([OrganizationId]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_ReportingUserProfiles_ReportUsername_Lookup]    Script Date: 8/16/2021 9:31:24 AM ******/
CREATE NONCLUSTERED INDEX [IX_ReportingUserProfiles_ReportUsername_Lookup] ON [report].[ReportingUserProfiles]
(
	[OrganizationId] ASC,
	[SystemUser] ASC
)
INCLUDE([ReportUsername]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
