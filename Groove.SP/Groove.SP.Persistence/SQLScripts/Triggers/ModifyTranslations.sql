IF EXISTS (SELECT * FROM sys.objects WHERE [name] = N'trg_ModifyTranslations' AND [type] = 'TR')
BEGIN
      DROP TRIGGER [dbo].[trg_ModifyTranslations];
END
GO

CREATE TRIGGER trg_ModifyTranslations
ON Translations
AFTER UPDATE AS
  UPDATE Translations
  SET UpdatedDate = GETUTCDATE()
  WHERE [Key] IN (SELECT DISTINCT [Key] FROM Inserted)