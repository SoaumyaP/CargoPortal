SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetReportPermission', 'P') IS NOT NULL
DROP PROC dbo.spu_GetReportPermission
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 16 March 2020
-- Description:	Get permission for report
-- =============================================
CREATE PROCEDURE spu_GetReportPermission
	@ReportId BIGINT

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	DECLARE @GrantInternal BIT = 1,
	@GrantPrincipal BIT = 0,
	@GrantShipper BIT = 0,
	@GrantAgent BIT = 0,
	@GrantWarehouse BIT = 0

	DECLARE @ReportPermissionTbl TABLE
	(
		ReportId BIGINT,
		OrganizationIds NVARCHAR(1024),
		RoleId BIGINT,
		CreatedDate DATETIME2(7),
		CreatedBy NVARCHAR(256)
	)

	INSERT INTO @ReportPermissionTbl
	SELECT ReportId, OrganizationIds, RoleId, CreatedDate, CreatedBy
	FROM ReportPermissions
	WHERE ReportId = @ReportId
	
	SELECT @GrantInternal = 1
	FROM @ReportPermissionTbl
	WHERE RoleId IN (SELECT Id FROM Roles WHERE IsInternal = 1)

	SELECT @GrantPrincipal = 1
	FROM @ReportPermissionTbl
	WHERE RoleId = 8 --Principal role

	SELECT @GrantShipper = 1
	FROM @ReportPermissionTbl
	WHERE RoleId = 9 --Shipper role

	SELECT @GrantAgent = 1
	FROM @ReportPermissionTbl
	WHERE RoleId = 4 --Agent role

	SELECT @GrantWarehouse = 1
	FROM @ReportPermissionTbl
	WHERE RoleId = 12 --Warehouse role

	SELECT TOP(1)
		ReportId,
		OrganizationIds,
		@GrantInternal AS [GrantInternal],
		@GrantPrincipal AS [GrantPrincipal],
		@GrantShipper AS [GrantShipper],
		@GrantAgent AS [GrantAgent],
		@GrantWarehouse as [GrantWarehouse],
		CreatedDate AS [CreatedDate],
		CreatedBy AS [CreatedBy]

	FROM @ReportPermissionTbl

END
GO
