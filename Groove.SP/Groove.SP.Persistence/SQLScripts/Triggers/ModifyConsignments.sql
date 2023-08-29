IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyConsignments' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].trg_ModifyConsignments;
END
GO

CREATE TRIGGER [dbo].trg_ModifyConsignments ON [dbo].[Consignments] AFTER
INSERT, UPDATE, DELETE 
AS

--insert 
If exists (Select * from inserted) and not exists(Select * from deleted)
begin
    INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
	SELECT 'CSM_' + CAST(inserted.Id AS varchar), 'Consignment', inserted.Id ,GETUTCDATE()
	FROM inserted 
end

--delete
If exists(select * from deleted) and not exists(Select * from inserted)
begin 

	DELETE GlobalIdActivities
	FROM deleted
	WHERE GlobalIdActivities.GlobalId = 'CSM_' + CAST(deleted.Id AS varchar)

    DELETE GlobalIds
	FROM deleted
	WHERE GlobalIds.EntityType = 'Consignment'
	AND GlobalIds.EntityId = deleted.Id
end