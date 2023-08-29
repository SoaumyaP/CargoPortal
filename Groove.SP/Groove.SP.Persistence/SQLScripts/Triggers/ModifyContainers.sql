IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyContainers' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].trg_ModifyContainers;
END
GO

CREATE TRIGGER [dbo].trg_ModifyContainers ON [dbo].[Containers] AFTER
INSERT, UPDATE, DELETE 
AS

--insert 
If exists (Select * from inserted) and not exists(Select * from deleted)
begin
    INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
	SELECT 'CTN_' + CAST(inserted.Id AS varchar), 'Container', inserted.Id ,GETUTCDATE()
	FROM inserted 
end

--delete
If exists(select * from deleted) and not exists(Select * from inserted)
begin 

	DELETE GlobalIdActivities
	FROM deleted
	WHERE GlobalIdActivities.GlobalId = 'CTN_' + CAST(deleted.Id AS varchar)

	DELETE GlobalIdAttachments
	FROM deleted
	WHERE GlobalIdAttachments.GlobalId = 'CTN_' + CAST(deleted.Id AS varchar)

    DELETE GlobalIds
	FROM deleted
	WHERE GlobalIds.EntityType = 'Container'
	AND GlobalIds.EntityId = deleted.Id
end