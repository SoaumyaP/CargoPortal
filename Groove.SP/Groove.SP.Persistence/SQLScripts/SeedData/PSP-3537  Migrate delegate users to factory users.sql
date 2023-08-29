-- Execute only once

DECLARE @CreatedBy NVARCHAR(100) = 'System'
DECLARE @CreatedDate DATETIME2(7) = GETDATE()

BEGIN TRAN

UPDATE UserRoles
SET RoleId = 13
WHERE UserId IN (
	SELECT U.Id
	FROM Organizations O
	CROSS APPLY 
	(
		SELECT *
		FROM UserProfiles U
		WHERE U.OrganizationId = O.Id
	)U
	CROSS APPLY
	(
		SELECT *
		FROM UserRoles UR
		WHERE U.Id = UR.UserId
	)UR
	WHERE ParentId IS NOT NULL AND ParentId <> '' AND O.OrganizationType = 1 
	AND O.Id NOT IN (SELECT SupplierId FROM CustomerRelationship)
)

-- Set default permission for Factory role

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,1,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,17,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,18,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,19,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,23,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,24,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,25,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,26,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

INSERT INTO RolePermissions (RoleId,PermissionId,CreatedBy,CreatedDate,UpdatedBy,UpdatedDate)
VALUES (13,112,@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate)

COMMIT TRAN


