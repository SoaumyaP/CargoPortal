SET IDENTITY_INSERT [dbo].[UserProfiles] ON
GO
INSERT [dbo].[UserProfiles] ([Id], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AccountNumber], [Username], [Email], [Name], [Title], [Department], [ProfilePicture], [Phone], [CompanyName], [Status], [IsInternal], [LocalTimeZone], [CountryId], [OrganizationId], [OrganizationName], [OrganizationRoleId], [OrganizationType], [LastSignInDate])
VALUES (1, N'System', CAST(N'2019-01-01T00:00:0.0' AS DateTime2), N'System', CAST(N'2019-01-01T00:00:0.0' AS DateTime2), N'69F56', N'logistics.testing@groovetechnology.com', N'logistics.testing@groovetechnology.com', N'Groove Logistics Testing', N'Title', N'Department', NULL, N'', N'Cargo Services Far East', 2, 1, N'{"value":"(UTC) Coordinated Universal Time","offset":0,"isDst":false}', 1, NULL, N'Cargo Services Far East', NULL, 0, CAST(N'2019-01-01T00:00:0.0' AS DateTime2))
GO
SET IDENTITY_INSERT [dbo].[UserProfiles] OFF
GO

INSERT [dbo].[UserRoles] ([UserId], [RoleId], [CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate])
VALUES (1, 1, N'System', CAST(N'2019-01-01T00:00:0.0' AS DateTime2), N'System', CAST(N'2019-01-01T00:00:0.0' AS DateTime2))
GO
