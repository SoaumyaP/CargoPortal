
GO
-- Run it only one time on each environment

DECLARE @CreatedBy nvarchar(50) = 'System';
DECLARE @CreateDate datetime2(7) = GETUTCDATE();
DECLARE @SurveyTbl TABLE
(
	Id bigint
)

BEGIN TRAN

INSERT INTO Permissions (CreatedBy,CreatedDate,Name,[Order])
VALUES (@CreatedBy, @CreateDate,'Organization.SurveyList',1731)

INSERT INTO Permissions (CreatedBy,CreatedDate,Name,[Order])
VALUES (@CreatedBy, @CreateDate,'Organization.SurveyDetail',1732)

INSERT INTO Permissions (CreatedBy,CreatedDate, Name, [Order])
VALUES (@CreatedBy, @CreateDate,'Organization.SurveyDetail.Add',1733)

INSERT INTO Permissions (CreatedBy,CreatedDate, Name, [Order])
VALUES (@CreatedBy, @CreateDate,'Organization.SurveyDetail.Edit',1734)

INSERT INTO @SurveyTbl
SELECT Id FROM Permissions WHERE [Order] IN (1731, 1732, 1733, 1734)

INSERT INTO RolePermissions (RoleId, PermissionId, CreatedBy, CreatedDate)
SELECT 1, Id, @CreatedBy, @CreateDate 
FROM @SurveyTbl

COMMIT TRAN
