SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('spu_GetActiveOrganizations', 'P') IS NOT NULL
DROP PROC dbo.spu_GetActiveOrganizations
GO


-- =============================================
-- Author:		Cuong Duong Duy
-- Create date: 17 Nov 2020
-- Description:	To get active organizations
-- =============================================
CREATE PROCEDURE [dbo].[spu_GetActiveOrganizations]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.

	SET NOCOUNT ON;	
    
	SELECT
		[Id],
		[Code],
		[Name],
		[Address],
		[AddressLine2],
		[AddressLine3],
		[AddressLine4],
		[ContactName],
		[ContactNumber],
		[ContactEmail],
		[AgentType],
		[CustomerPrefix],
		[OrganizationType],
		[IsBuyer],
		[Status],
		[WeChatOrWhatsApp]
	FROM Organizations
	-- Check on status = Active
	WHERE [Status] = 1
	
END
GO

