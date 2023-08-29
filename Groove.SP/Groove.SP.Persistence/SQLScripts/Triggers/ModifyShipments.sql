IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyShipments' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].trg_ModifyShipments;
END
GO

CREATE TRIGGER [dbo].trg_ModifyShipments ON [dbo].[Shipments] AFTER
INSERT, UPDATE, DELETE 
AS

--insert 
If exists (Select * from inserted) and not exists(Select * from deleted)
begin
    INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
	SELECT 'SHI_' + CAST(inserted.Id AS varchar), 'Shipment', inserted.Id ,GETUTCDATE()
	FROM inserted 
end

--delete
If exists(select * from deleted) and not exists(Select * from inserted)
begin 

	DELETE GlobalIdActivities
	FROM deleted
	WHERE GlobalIdActivities.GlobalId = 'SHI_' + CAST(deleted.Id AS varchar)

	DELETE GlobalIdAttachments
	FROM deleted
	WHERE GlobalIdAttachments.GlobalId = 'SHI_' + CAST(deleted.Id AS varchar)

    DELETE GlobalIds
	FROM deleted
	WHERE GlobalIds.EntityType = 'Shipment'
	AND GlobalIds.EntityId = deleted.Id
end