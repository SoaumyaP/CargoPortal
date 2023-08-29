SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GrantReportPermission', 'P') IS NOT NULL
DROP PROC dbo.spu_GrantReportPermission
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 16 March 2020
-- Description:	Grant permission for report
-- =============================================
CREATE PROCEDURE [dbo].[spu_GrantReportPermission]
	@ReportId BIGINT,
	@OrganizationIds NVARCHAR(1024) = NULL,
	@GrantInternal BIT = 1,
	@GrantPrincipal BIT = 0,
	@GrantShipper BIT = 0,
	@GrantAgent BIT = 0,
	@GrantWarehouse BIT = 0,
	@CreatedBy NVARCHAR(256),
	@CreatedDate DATETIME2

AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;
		
	IF (@CreatedDate IS NULL)
	BEGIN
		SET @CreatedDate = GETUTCDATE();
	END

	DELETE
	FROM ReportPermissions
	WHERE ReportId = @ReportId

	-- Insert roles as Internal: System Admin, CSR, Sale, Pending
	IF (@GrantInternal = 1)
	BEGIN
		INSERT INTO ReportPermissions(
			ReportId,
			OrganizationIds,
			RoleId,
			CreatedBy,
			CreatedDate,
			UpdatedBy,
			UpdatedDate)
		SELECT @ReportId,
			@OrganizationIds,
			Id,
			@CreatedBy,
			@CreatedDate,
			@CreatedBy,
			@CreatedDate
		FROM Roles
		WHERE IsInternal = 1
	END

	IF (@GrantPrincipal = 1)
	BEGIN
		INSERT INTO ReportPermissions(
			ReportId,
			OrganizationIds,
			RoleId,
			CreatedBy,
			CreatedDate,
			UpdatedBy,
			UpdatedDate)
		SELECT @ReportId,
			@OrganizationIds,
			8, --Principal role
			@CreatedBy,
			@CreatedDate,
			@CreatedBy,
			@CreatedDate
	END

	IF (@GrantShipper = 1)
	BEGIN
		INSERT INTO ReportPermissions(
			ReportId,
			OrganizationIds,
			RoleId,
			CreatedBy,
			CreatedDate,
			UpdatedBy,
			UpdatedDate)
		SELECT @ReportId,
			@OrganizationIds,
			9, --Shipper role
			@CreatedBy,
			@CreatedDate,
			@CreatedBy,
			@CreatedDate
	END

	IF (@GrantAgent = 1)
	BEGIN
		INSERT INTO ReportPermissions(
			ReportId,
			OrganizationIds,
			RoleId,
			CreatedBy,
			CreatedDate,
			UpdatedBy,
			UpdatedDate)
		SELECT @ReportId,
			@OrganizationIds,
			4, --Agent role
			@CreatedBy,
			@CreatedDate,
			@CreatedBy,
			@CreatedDate
	END 

	IF (@GrantWarehouse = 1)
	BEGIN
		INSERT INTO ReportPermissions(
			ReportId,
			OrganizationIds,
			RoleId,
			CreatedBy,
			CreatedDate,
			UpdatedBy,
			UpdatedDate)
		SELECT @ReportId,
			@OrganizationIds,
			12, --Warehouse role
			@CreatedBy,
			@CreatedDate,
			@CreatedBy,
			@CreatedDate
	END
END
GO