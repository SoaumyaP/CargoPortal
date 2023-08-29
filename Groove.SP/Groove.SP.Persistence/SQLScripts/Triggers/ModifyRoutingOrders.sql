IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyRoutingOrders' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].trg_ModifyRoutingOrders;
END
GO

CREATE TRIGGER [dbo].trg_ModifyRoutingOrders ON [dbo].[RoutingOrders] AFTER
INSERT, UPDATE, DELETE 
AS

DECLARE @EntityCodePrefix varchar(4) = 'ROD_';
DECLARE @EntityType varchar(50) = 'RoutingOrder';   

--insert 
If exists (Select * from inserted) and not exists(Select * from deleted)
begin
    INSERT INTO GlobalIds(Id, EntityType, EntityId, CreatedDate)
	SELECT @EntityCodePrefix + CAST(inserted.Id AS varchar), @EntityType, inserted.Id ,GETUTCDATE()
	FROM inserted 
end

--delete
If exists(select * from deleted) and not exists(Select * from inserted)
begin 

	DELETE GlobalIdActivities
	FROM deleted
	WHERE GlobalIdActivities.GlobalId = @EntityCodePrefix + CAST(deleted.Id AS varchar)

	DELETE GlobalIdAttachments
	FROM deleted
	WHERE GlobalIdAttachments.GlobalId = @EntityCodePrefix + CAST(deleted.Id AS varchar)

    DELETE GlobalIds
	FROM deleted
	WHERE GlobalIds.EntityType = @EntityType
	AND GlobalIds.EntityId = deleted.Id
end