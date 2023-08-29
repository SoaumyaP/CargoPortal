SET IDENTITY_INSERT [dbo].[UserProfiles] ON 
GO
INSERT [dbo].[UserProfiles] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AccountNumber], [Username], [Email], [Name], [Title], [Department], [ProfilePicture], [Phone], [CompanyName], [Status], [IsInternal], [CountryId], [OrganizationId], [OrganizationCode], [OrganizationName], [OrganizationRoleId], [OrganizationType], [LastSignInDate]) VALUES (1, N'System', CAST(N'2019-08-27T11:32:18.2246372' AS DateTime2), N'logistics.testing@groovetechnology.com', CAST(N'2019-08-28T02:29:36.0022872' AS DateTime2), N'77666', N'logistics.testing@groovetechnology.com', N'logistics.testing@groovetechnology.com', N'Groove LTL Testing', NULL, NULL, NULL, N'', N'Cargo Services Far East', 2, 1, NULL, NULL, NULL, N'Cargo Services Far East', NULL, 0, CAST(N'2020-04-09T15:07:49.3840476' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[UserProfiles] OFF
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (1, 1, N'System', CAST(N'2019-08-27T11:32:18.2246399' AS DateTime2), N'logistics.testing@groovetechnology.com', CAST(N'2019-08-28T02:29:36.0022889' AS DateTime2))
GO

