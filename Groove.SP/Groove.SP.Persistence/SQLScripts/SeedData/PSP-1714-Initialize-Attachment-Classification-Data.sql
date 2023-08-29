

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
SET NOCOUNT ON
GO

--------------------------------
-- RUN IT ONLY ONCE
--------------------------------

TRUNCATE TABLE AttachmentTypeClassifications
GO

DECLARE @CreatedDate DATETIME2(7) = GETUTCDATE()
DECLARE @CreatedBy NVARCHAR(9) = 'System'
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
DECLARE @ShipmentET NVARCHAR(3) = 'SHI',
    @BillOfLadingET NVARCHAR(3) = 'BOL',
    @MasterBillET NVARCHAR(3) = 'MBL',
    @ContainerET NVARCHAR(3) = 'CTN',
    @ConsignmentET NVARCHAR(3) = 'CSM',
    @CustomerPOET NVARCHAR(3) = 'CPO',
    @POFulfillmentET NVARCHAR(3) = 'POF',
    @CruiseOrderET NVARCHAR(3) = 'CRO',
    @CruiseOrderItemET NVARCHAR(3) = 'COI';

BEGIN TRANSACTION


INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @CommercialInvoice, 10)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @PackingList, 20)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @ExportLicense, 30)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @CertificateOfOrigin, 40)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @FormA, 50)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @FumigationCertificate, 60)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @PackingDeclaration, 70)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @MSDS, 80)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @LetterOfCredit, 90)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @InsuranceCertificate, 100)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @ShippingOrderForm, 110)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @BookingForm, 120)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @POFulfillmentET, @Others, 130)


INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @MasterBillET, @MasterBL, 10)

INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @BillOfLadingET, @HouseBL, 10)

INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @CommercialInvoice, 10)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @PackingList, 20)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @ExportLicense, 30)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @CertificateOfOrigin, 40)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @FormA, 50)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @FumigationCertificate, 60)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @PackingDeclaration, 70)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @MSDS, 80)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @LetterOfCredit, 90)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @InsuranceCertificate, 100)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @HouseBL, 110)
INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ShipmentET, @Others, 120)

INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ConsignmentET, @Miscellaneous, 10)

INSERT INTO [dbo].[AttachmentTypeClassifications] ([CreatedBy],[CreatedDate],[UpdatedBy],[UpdatedDate],[EntityType],[AttachmentType],[Order])
VALUES(@CreatedBy, @CreatedDate, @CreatedBy, @CreatedDate, @ContainerET, @Manifest, 10)


COMMIT TRANSACTION


