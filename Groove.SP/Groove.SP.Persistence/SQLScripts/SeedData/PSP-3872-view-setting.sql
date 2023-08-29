BEGIN TRAN

INSERT INTO ViewSettings (Field, ModuleId, ViewType, CreatedBy,UpdatedBy,CreatedDate,UpdatedDate)
VALUES ('scheduleLineNo', 'PO.Detail.LineItems', 20,'System','System',GETDATE(),GETDATE());


INSERT INTO ViewRoleSettings(
		[ViewId],
		[RoleId],
		[CreatedBy],
		[CreatedDate],
		[UpdatedBy],
		[UpdatedDate]
	)
	SELECT
		VS.[ViewId],
		RL.[Id],
		N'System' as [CreatedBy],
		GETUTCDATE() as [CreatedDate],
		N'System' as [UpdatedBy],
		GETUTCDATE() as [UpdatedDate]
	FROM [Roles] RL 
	JOIN [ViewSettings] VS ON VS.Field = 'scheduleLineNo' AND ModuleId='PO.Detail.LineItems'
COMMIT TRAN
GO
