IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_Insert_Translations' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].[trg_Insert_Translations];
END
GO

CREATE TRIGGER trg_Insert_Translations
ON Translations
AFTER INSERT AS
  DECLARE @UTCDATE DATETIME2(7) = GETUTCDATE()
  UPDATE Translations
  SET 
    UpdatedDate = @UTCDATE,
    CreatedDate = @UTCDATE
  WHERE [Key] IN (SELECT DISTINCT [Key] FROM Inserted)