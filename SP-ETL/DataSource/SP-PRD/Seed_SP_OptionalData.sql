SET IDENTITY_INSERT [dbo].[UserProfiles] ON 
GO
INSERT [dbo].[UserProfiles] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AccountNumber], [Username], [Email], [Name], [Title], [Department], [ProfilePicture], [Phone], [CompanyName], [Status], [IsInternal], [CountryId], [OrganizationId], [OrganizationCode], [OrganizationName], [OrganizationRoleId], [OrganizationType], [LastSignInDate]) VALUES (1, N'System', CAST(N'2020-04-27T03:25:07.6716195' AS DateTime2), N'System', CAST(N'2020-04-27T03:25:07.6716211' AS DateTime2), N'B4BA6', N'shipmentportal@cargofe.com', N'shipmentportal@cargofe.com', N'zzzShipment Portal', NULL, NULL, NULL, N'', N'Cargo Services Far East', 2, 1, NULL, NULL, NULL, N'Cargo Services Far East', NULL, 0, CAST(N'2020-04-27T03:25:08.9575988' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[UserProfiles] OFF
GO
INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate]) VALUES (1, 1, N'System', CAST(N'2020-04-27T03:25:07.6719726' AS DateTime2), N'System', CAST(N'2020-04-27T03:25:07.6719745' AS DateTime2))
GO

