IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyPOFulfillments' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].trg_ModifyPOFulfillments;
END
GO

CREATE TRIGGER [dbo].trg_ModifyPOFulfillments ON [dbo].[POFulfillments] AFTER
INSERT, UPDATE, DELETE 
AS

--insert 
If exists (Select * from inserted) and not exists(Select * from deleted)
begin
    INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
	SELECT 'POF_' + CAST(inserted.Id AS varchar), 'POfulfillment', inserted.Id ,GETUTCDATE()
	FROM inserted 
end

--delete
If exists(select * from deleted) and not exists(Select * from inserted)
begin 

	DELETE GlobalIdApprovals
	FROM deleted
	WHERE GlobalIdApprovals.GlobalId = 'POF_' + CAST(deleted.Id AS varchar)

    DELETE GlobalIds
	FROM deleted
	WHERE GlobalIds.EntityType = 'POfulfillment'
	AND GlobalIds.EntityId = deleted.Id
end
