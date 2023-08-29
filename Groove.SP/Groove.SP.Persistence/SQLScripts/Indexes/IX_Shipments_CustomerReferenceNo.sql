/* Disable fulltext index */
IF EXISTS (SELECT 1 FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Shipments]'))
    ALTER FULLTEXT INDEX ON [dbo].Shipments DISABLE
GO

/* Drop fulltext index */
IF EXISTS (SELECT 1 FROM sys.fulltext_indexes fti WHERE fti.object_id = OBJECT_ID(N'[dbo].[Shipments]'))
    DROP FULLTEXT INDEX ON [dbo].Shipments
GO

/* Drop the catalog */
IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE [name] = 'IX_Shipments_CustomerReferenceNo_Catalog')
    DROP FULLTEXT CATALOG [IX_Shipments_CustomerReferenceNo_Catalog]
GO

/* Create */
CREATE FULLTEXT CATALOG [IX_Shipments_CustomerReferenceNo_Catalog]
GO

CREATE FULLTEXT INDEX ON [dbo].Shipments(CustomerReferenceNo LANGUAGE 'English')
    KEY INDEX PK_Shipments
    ON [IX_Shipments_CustomerReferenceNo_Catalog]
    WITH STOPLIST = OFF;
GO