

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET NOCOUNT ON
GO

--------------------------------
-- RUN IT ONLY ONCE
--------------------------------

TRUNCATE TABLE AttachmentTypePermissions


DECLARE @CreatedDate DATETIME2(7) = GETUTCDATE()
DECLARE @CreatedBy NVARCHAR(9) = 'System'
DECLARE @RoleId BIGINT;

DECLARE @ShippingOrderForm NVARCHAR(50) = 'Shipping Order Form',
    @PackingList NVARCHAR(50) = 'Packing List',
    @CommercialInvoice NVARCHAR(50) = 'Commercial Invoice',
    @MasterBL NVARCHAR(50) = 'Master BL',
    @HouseBL NVARCHAR(50) = 'House BL',
    @Manifest NVARCHAR(50) = 'Manifest',
    @Others NVARCHAR(50) = 'Others',
    @ExportLicense NVARCHAR(50) = 'Export License',
    @CertificateOfOrigin NVARCHAR(50) = 'Certificate of Origin',
    @FormA NVARCHAR(50) = 'Form A',
    @FumigationCertificate NVARCHAR(50) = 'Fumigation Certificate',
    @PackingDeclaration NVARCHAR(50) = 'Packing Declaration',
    @MSDS NVARCHAR(50) = 'MSDS',
    @LetterOfCredit NVARCHAR(50) = 'Letter of Credit',
    @InsuranceCertificate NVARCHAR(50) = 'Insurance Certificate',
    @BookingForm NVARCHAR(50) = 'Booking Form',
    @Miscellaneous NVARCHAR(50) = 'Miscellaneous';

-- Ignore seeding data for some roles
BEGIN TRANSACTION

DECLARE RoleCur CURSOR FOR
SELECT Id
FROM dbo.Roles
WHERE [Name] NOT IN ('Sale', 'Registered User', 'Guest', 'Pending')

OPEN RoleCur               

FETCH NEXT FROM RoleCur    
      INTO @RoleId

WHILE @@FETCH_STATUS = 0         
BEGIN
                                
    INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @ShippingOrderForm, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @PackingList, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @CommercialInvoice, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @MasterBL, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @HouseBL, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @Manifest, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @Others, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @ExportLicense, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @CertificateOfOrigin, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @FormA, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @FumigationCertificate, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @PackingDeclaration, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @MSDS, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @LetterOfCredit, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @InsuranceCertificate, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @BookingForm, @RoleId);
	INSERT INTO [dbo].[AttachmentTypePermissions] ([CreatedBy], [CreatedDate], [UpdatedBy], [UpdatedDate], [AttachmentType], [RoleId])
		VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate, @Miscellaneous, @RoleId);
			   

    FETCH NEXT FROM RoleCur 
          INTO @RoleId
END

CLOSE RoleCur              
DEALLOCATE RoleCur         

COMMIT TRANSACTION


