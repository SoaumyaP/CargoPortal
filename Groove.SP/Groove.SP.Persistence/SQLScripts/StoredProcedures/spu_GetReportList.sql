SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetReportList', 'P') IS NOT NULL
DROP PROC dbo.spu_GetReportList
GO

-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 16 March 2020
-- Description:	Get data for list of reports
-- =============================================
CREATE PROCEDURE spu_GetReportList
	@isInternal BIT,
	@roleId BIGINT,
	@organizationId BIGINT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;

	IF @isInternal = 1
	BEGIN
		select *
		from Reports

	END

	ELSE
	BEGIN
		select *
		from Reports r
		inner join ReportPermissions rp on rp.ReportId = r.Id
		where rp.RoleId = @roleId AND (rp.OrganizationId is null OR rp.OrganizationId = @organizationId)

	END
    
END
GO
