IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyBillOfLadings' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].trg_ModifyBillOfLadings;
END
GO

CREATE TRIGGER [dbo].trg_ModifyBillOfLadings ON [dbo].[BillOfLadings] AFTER
INSERT, UPDATE, DELETE 
AS

--insert 
If exists (Select * from inserted) and not exists(Select * from deleted)
begin
    INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
	SELECT 'BOL_' + CAST(inserted.Id AS varchar), 'BillOfLading', inserted.Id ,GETUTCDATE()
	FROM inserted 
end

--delete
If exists(select * from deleted) and not exists(Select * from inserted)
begin 

	DELETE GlobalIdAttachments
	FROM deleted
	WHERE GlobalIdAttachments.GlobalId = 'BOL_' + CAST(deleted.Id AS varchar)

    DELETE GlobalIds
	FROM deleted
	WHERE GlobalIds.EntityType = 'BillOfLading'
	AND GlobalIds.EntityId = deleted.Id
end