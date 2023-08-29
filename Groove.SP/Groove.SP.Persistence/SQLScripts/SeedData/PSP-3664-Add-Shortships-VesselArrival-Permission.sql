
GO
BEGIN TRAN
DECLARE @shortshipsPermissionId bigint;
DECLARE @VesselArrivalPermissionId bigint;

INSERT INTO Permissions (CreatedBy,CreatedDate,Name,[Order])
VALUES('System',GETUTCDATE(),'Dashboard.ShortshipsList',1014)

INSERT INTO Permissions (CreatedBy,CreatedDate,Name,[Order])
VALUES('System',GETUTCDATE(),'Dashboard.VesselArrivalList',1015)

SELECT @shortshipsPermissionId = Id  FROM Permissions WHERE [Order] = 1014
SELECT @VesselArrivalPermissionId = Id  FROM Permissions WHERE [Order] = 1015

INSERT INTO RolePermissions(RoleId,PermissionId,CreatedBy,CreatedDate)
VALUES (1,@shortshipsPermissionId,'System',GETUTCDATE())
INSERT INTO RolePermissions(RoleId,PermissionId,CreatedBy,CreatedDate)
VALUES (1,@VesselArrivalPermissionId,'System',GETUTCDATE())

INSERT INTO RolePermissions(RoleId,PermissionId,CreatedBy,CreatedDate)
VALUES (2,@shortshipsPermissionId,'System',GETUTCDATE())
INSERT INTO RolePermissions(RoleId,PermissionId,CreatedBy,CreatedDate)
VALUES (2,@VesselArrivalPermissionId,'System',GETUTCDATE())

INSERT INTO RolePermissions(RoleId,PermissionId,CreatedBy,CreatedDate)
VALUES (8,@shortshipsPermissionId,'System',GETUTCDATE())
INSERT INTO RolePermissions(RoleId,PermissionId,CreatedBy,CreatedDate)
VALUES (8,@VesselArrivalPermissionId,'System',GETUTCDATE())

COMMIT TRAN
GO
