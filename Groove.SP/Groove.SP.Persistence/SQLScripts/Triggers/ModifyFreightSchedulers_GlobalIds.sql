IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyFreightSchedulers_GlobalIds' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].trg_ModifyFreightSchedulers_GlobalIds;
END
GO

CREATE TRIGGER [dbo].trg_ModifyFreightSchedulers_GlobalIds ON [dbo].[FreightSchedulers] AFTER
INSERT, UPDATE, DELETE 
AS

DECLARE @EntityCodePrefix varchar(4) = 'FSC_';   
DECLARE @EntityType varchar(50) = 'FreightScheduler';   

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

    DELETE GlobalIds
	FROM deleted
	WHERE GlobalIds.EntityType = @EntityType
	AND GlobalIds.EntityId = deleted.Id
end