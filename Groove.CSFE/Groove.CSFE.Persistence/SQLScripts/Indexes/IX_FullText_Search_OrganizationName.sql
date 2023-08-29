
GO  

IF NOT EXISTS(SELECT * FROM sys.fulltext_catalogs WHERE name = 'OrganizationFullTextCatalog')
BEGIN 
	CREATE FULLTEXT CATALOG OrganizationFullTextCatalog;	
END


CREATE FULLTEXT INDEX ON Organizations
(
	NAME
)
KEY INDEX PK_Organizations ON OrganizationFullTextCatalog

GO
