SET IDENTITY_INSERT [dbo].[Reports] ON 
GO

IF NOT EXISTS (SELECT 1 FROM [dbo].[Reports] WITH (NOLOCK) WHERE Id = 1)
BEGIN
	INSERT [dbo].[Reports] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportName], [ReportUrl], [ReportDescription], [LastRunTime], [ReportGroup], [StoredProcedureName]) 
	VALUES (1, NULL, CAST(N'2020-08-04T14:49:30.2866667' AS DateTime2), NULL, NULL, N'Booked Status Report', N'/booked-status-report', N'Booked Status Report', CAST(N'2020-08-11T10:53:12.3296142' AS DateTime2), N'PO report', N'spu_ProceedBookedStatusReport')
END

GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Reports] WITH (NOLOCK) WHERE Id = 2)
BEGIN
	INSERT [dbo].[Reports] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportName], [ReportUrl], [ReportDescription], [LastRunTime], [ReportGroup], [StoredProcedureName]) 
	VALUES (2, NULL, CAST(N'2020-08-04T14:49:30.2866667' AS DateTime2), NULL, NULL, N'Not Booked Status Report', N'/not-booked-status-report', N'Not Booked Status Report', CAST(N'2020-08-11T10:46:34.1848090' AS DateTime2), N'PO report', N'spu_ProceedNotBookedStatusReport')
END

GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[Reports] WITH (NOLOCK) WHERE Id = 3)
BEGIN
	INSERT [dbo].[Reports] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportName], [ReportUrl], [ReportDescription], [LastRunTime], [ReportGroup], [StoredProcedureName])  
	VALUES (3, NULL, GETDATE(), NULL, NULL, N'Master Summary Report', N'/master-summary-report', N'Summary Report by PO and Item level', GETDATE(), N'PO report', N'spu_ProceedMasterSummaryReport')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Reports] WITH (NOLOCK) WHERE Id = 4)
BEGIN
	INSERT [dbo].[Reports] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportName], [ReportUrl], [ReportDescription], [LastRunTime], [ReportGroup])  
	VALUES (4, NULL, GETDATE(), NULL, NULL, N'Master Summary Report New', N'/telerik-report?category=CSPortal-Local&reportkey=Master Summary Report (PO Level)&reportserverurl=http://desktop0117:83/', N'Summary Report by PO and Item level', GETDATE(), N'PO report')
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[Reports] WITH (NOLOCK) WHERE Id = 5)
BEGIN
	INSERT [dbo].[Reports] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportName], [ReportUrl], [ReportDescription], [LastRunTime], [ReportGroup])  
	VALUES (5, NULL, GETDATE(), NULL, NULL, N'Active PO Report', N'/telerik-report?category=CSPortal-Local&reportkey=Active PO Report&reportserverurl=http://desktop0117:83/', N'', GETDATE(), N'PO report')
END

GO
SET IDENTITY_INSERT [dbo].[Reports] OFF
GO

--Insert [ReportPermissions]

SET IDENTITY_INSERT [dbo].[ReportPermissions] ON 
GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[ReportPermissions] WITH (NOLOCK) WHERE ReportId = 1 AND RoleId = 1)
BEGIN
	INSERT [dbo].[ReportPermissions] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportId], [RoleId], [OrganizationIds]) 
	VALUES (1, N'logistics.testing@groovetechnology.com', CAST(N'2020-08-11T09:59:17.7619196' AS DateTime2), NULL, NULL, 1, 1, NULL)
END

GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[ReportPermissions] WITH (NOLOCK) WHERE ReportId = 2 AND RoleId = 1)
BEGIN
	INSERT [dbo].[ReportPermissions] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportId], [RoleId], [OrganizationIds]) 
	VALUES (2, N'logistics.testing@groovetechnology.com', CAST(N'2020-08-11T09:59:17.7619196' AS DateTime2), NULL, NULL, 2, 1, NULL)
END

GO
IF NOT EXISTS (SELECT 1 FROM [dbo].[ReportPermissions] WITH (NOLOCK) WHERE ReportId = 3 AND RoleId = 1)
BEGIN
	INSERT [dbo].[ReportPermissions] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportId], [RoleId], [OrganizationIds]) 
	VALUES (38, N'logistics.testing@groovetechnology.com', GETDATE() , NULL, NULL, 3, 1, NULL)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[ReportPermissions] WITH (NOLOCK) WHERE ReportId = 4 AND RoleId = 1)
BEGIN
	INSERT [dbo].[ReportPermissions] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportId], [RoleId], [OrganizationIds]) 
	VALUES (44, N'logistics.testing@groovetechnology.com', GETDATE() , NULL, NULL, 4, 1, NULL)
END

IF NOT EXISTS (SELECT 1 FROM [dbo].[ReportPermissions] WITH (NOLOCK) WHERE ReportId = 5 AND RoleId = 1)
BEGIN
	INSERT [dbo].[ReportPermissions] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [ReportId], [RoleId], [OrganizationIds]) 
	VALUES (50, N'logistics.testing@groovetechnology.com', GETDATE() , NULL, NULL, 5, 1, NULL)
END

GO
SET IDENTITY_INSERT [dbo].[ReportPermissions] OFF
GO
