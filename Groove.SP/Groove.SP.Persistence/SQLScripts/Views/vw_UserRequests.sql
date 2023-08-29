IF OBJECT_ID('[dbo].[vw_UserRequests]', 'V') IS NOT NULL
    DROP VIEW [dbo].[vw_UserRequests];

GO

IF OBJECT_ID('[dbo].[UserRequestsView]', 'V') IS NOT NULL
    DROP VIEW [dbo].UserRequestsView;

GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vw_UserRequests]
AS

SELECT
    u.Id,
    u.Username, 
    u.AccountNumber,
    u.Email,
    u.Name,
    u.Phone,
    u.CompanyName,
	u.CompanyAddressLine1,
	u.CompanyAddressLine2,
	u.CompanyAddressLine3,
	u.CompanyAddressLine4,
    u.CompanyWeChatOrWhatsApp,
	u.Customer,
    u.Status,
    u.IsInternal,
    u.OrganizationId,
    u.OrganizationCode,
    u.OrganizationName,
    u.OrganizationRoleId,
    u.OrganizationType,
	u.OPCountryId,
	u.OPLocationName,
	u.OPContactEmail,
	u.OPContactName,
	u.TaxpayerId,
    u.LastSignInDate,
    u.CreatedDate,
    u.CreatedBy,
    u.RowVersion,
    u.UpdatedDate,
    u.UpdatedBy,
    r.Id AS RoleId,
    r.Name AS RoleName
FROM UserProfiles u
    LEFT JOIN UserRoles ur ON u.Id = ur.UserId
    LEFT JOIN Roles r ON ur.RoleId = r.Id
WHERE u.Status = 1    -- Pending
    OR u.Status = 0   -- Rejected
    OR u.Status = -1  -- Deleted

GO