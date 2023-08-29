
;
DECLARE @CurrentUTC DATETIME2(7) = GETUTCDATE();

-- SECTION 1: AttachmentTypeClassifications

-- Shipment module
BEGIN TRANSACTION

-- Animal Hairs Declaration
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Animal Hairs Declaration' AND EntityType = 'SHI')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Animal Hairs Declaration'
           ,'SHI'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'SHI')
		   )
END

-- Freight Invoice
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Freight Invoice' AND EntityType = 'SHI')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Freight Invoice'
           ,'SHI'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'SHI')
		   )
END

-- Import Security Filing
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Import Security Filing' AND EntityType = 'SHI')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Import Security Filing'
           ,'SHI'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'SHI')
		   )
END

-- Master BL
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Master BL' AND EntityType = 'SHI')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Master BL'
           ,'SHI'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'SHI')
		   )
END

COMMIT TRANSACTION

-- Master Bill module
BEGIN TRANSACTION

-- Animal Hairs Declaration
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Animal Hairs Declaration' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Animal Hairs Declaration'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

-- Certificate of Origin
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Certificate of Origin' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Certificate of Origin'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

-- Commercial Invoice
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Commercial Invoice' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Commercial Invoice'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

-- Freight Invoice
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Freight Invoice' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Freight Invoice'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

-- Fumigation Certificate
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Fumigation Certificate' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Fumigation Certificate'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

-- House BL
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'House BL' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'House BL'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

-- Import Security Filing
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Import Security Filing' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Import Security Filing'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

-- Packing Declaration
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Packing Declaration' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Packing Declaration'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

-- Packing List
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Packing List' AND EntityType = 'MBL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Packing List'
           ,'MBL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'MBL')
		   )
END

COMMIT TRANSACTION

-- Bill of Lading module
BEGIN TRANSACTION

-- Animal Hairs Declaration
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Animal Hairs Declaration' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Animal Hairs Declaration'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

-- Certificate of Origin
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Certificate of Origin' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Certificate of Origin'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

-- Commercial Invoice
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Commercial Invoice' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Commercial Invoice'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

-- Freight Invoice
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Freight Invoice' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Freight Invoice'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

-- Fumigation Certificate
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Fumigation Certificate' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Fumigation Certificate'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

-- Import Security Filing
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Import Security Filing' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Import Security Filing'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

-- Master BL
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Master BL' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Master BL'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

-- Packing Declaration
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Packing Declaration' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Packing Declaration'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

-- Packing List
IF NOT EXISTS (SELECT 1 FROM AttachmentTypeClassifications WHERE AttachmentType = 'Packing List' AND EntityType = 'BOL')
BEGIN

	INSERT INTO [dbo].[AttachmentTypeClassifications]
           ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[EntityType]
           ,[Order])
     VALUES
           ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,'Packing List'
           ,'BOL'
           ,(SELECT MAX([Order]) + 10 FROM AttachmentTypeClassifications WHERE EntityType = 'BOL')
		   )
END

COMMIT TRANSACTION


-- SECTION 2: AttachmentTypePermissions

DECLARE @PermissionTable AS TABLE (
	[AttachmentType] NVARCHAR(64) NOT NULL,
	[RoleId] BIGINT NOT NULL
)

BEGIN TRANSACTION

INSERT @PermissionTable
SELECT 'Animal Hairs Declaration', [RoleId]
FROM AttachmentTypePermissions
WHERE AttachmentType = 'Packing List'

INSERT @PermissionTable
SELECT 'Import Security Filing', [RoleId]
FROM AttachmentTypePermissions
WHERE AttachmentType = 'Packing List'

INSERT @PermissionTable
SELECT 'Freight Invoice', 1

INSERT @PermissionTable
SELECT 'Freight Invoice', 2

INSERT @PermissionTable
SELECT 'Freight Invoice', 4

-- Cruise Agent
INSERT @PermissionTable
SELECT 'Freight Invoice', 10

MERGE INTO [dbo].[AttachmentTypePermissions] AS T
USING @PermissionTable AS S
ON T.AttachmentType = S.AttachmentType AND T.RoleId = S.RoleId
WHEN NOT MATCHED BY TARGET THEN
INSERT ([CreatedBy]
           ,[CreatedDate]
           ,[UpdatedBy]
           ,[UpdatedDate]
           ,[AttachmentType]
           ,[RoleId]
           ,[CheckContractHolder]
           ,[Alias]
		) VALUES ('System'
           ,@CurrentUTC
           ,'System'
           ,@CurrentUTC
           ,AttachmentType
           ,RoleId
           ,0
           ,NULL);


--INSERT INTO [dbo].[AttachmentTypePermissions]
--           ([CreatedBy]
--           ,[CreatedDate]
--           ,[UpdatedBy]
--           ,[UpdatedDate]
--           ,[AttachmentType]
--           ,[RoleId]
--           ,[CheckContractHolder]
--           ,[Alias])
--SELECT
--           'System'
--           ,@CurrentUTC
--           ,'System'
--           ,@CurrentUTC
--           ,AttachmentType
--           ,RoleId
--           ,0
--           ,NULL
--FROM @PermissionTable

COMMIT TRANSACTION

