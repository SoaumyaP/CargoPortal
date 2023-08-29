IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyMasterBills' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].trg_ModifyMasterBills;
END
GO

CREATE TRIGGER [dbo].trg_ModifyMasterBills ON [dbo].[MasterBills] AFTER
INSERT, UPDATE, DELETE 
AS

--insert 
If exists (Select * from inserted) and not exists(Select * from deleted)
begin
    INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
	SELECT 'MBL_' + CAST(inserted.Id AS varchar), 'MasterBill', inserted.Id ,GETUTCDATE()
	FROM inserted 
end

--delete
If exists(select * from deleted) and not exists(Select * from inserted)
begin 

	DELETE GlobalIdAttachments
	FROM deleted
	WHERE GlobalIdAttachments.GlobalId = 'MBL_' + CAST(deleted.Id AS varchar)

    DELETE GlobalIds
	FROM deleted
	WHERE GlobalIds.EntityType = 'MasterBill'
	AND GlobalIds.EntityId = deleted.Id
end
