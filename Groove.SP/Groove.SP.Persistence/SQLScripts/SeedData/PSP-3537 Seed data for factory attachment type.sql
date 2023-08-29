-- Execute only once

DECLARE @CreatedDate DATETIME2(7) = GETUTCDATE()
DECLARE @CreatedBy NVARCHAR(9) = 'System'
DECLARE @FactoryCommercialInvoice NVARCHAR(50) = 'Factory Commercial Invoice'
DECLARE @Alias NVARCHAR(50) = 'Commercial Invoice'
DECLARE @Order int = 11

BEGIN TRAN
-- AttachmentTypeClassifications
INSERT INTO AttachmentTypeClassifications (CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,EntityType,[Order])
VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate,@FactoryCommercialInvoice,'POF' ,@Order)

INSERT INTO AttachmentTypeClassifications (CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,EntityType,[Order])
VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate,@FactoryCommercialInvoice,'SHI' ,@Order)

 --Update permission Admin, CSR, Agent, Cruise Agent, Shipper
INSERT INTO AttachmentTypePermissions (CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,RoleId,CheckContractHolder)
VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate,@FactoryCommercialInvoice,1 ,0)

INSERT INTO AttachmentTypePermissions (CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,RoleId,CheckContractHolder)
VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate,@FactoryCommercialInvoice,2 ,0)

INSERT INTO AttachmentTypePermissions (CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,RoleId,CheckContractHolder)
VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate,@FactoryCommercialInvoice,4 ,0)

INSERT INTO AttachmentTypePermissions (CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,RoleId,CheckContractHolder)
VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate,@FactoryCommercialInvoice,9 ,0)

INSERT INTO AttachmentTypePermissions (CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,RoleId,CheckContractHolder)
VALUES (@CreatedBy,@CreatedDate,@CreatedBy,@CreatedDate,@FactoryCommercialInvoice,10 ,0)

 --Grant permission for Factory user role
INSERT INTO AttachmentTypePermissions (CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,RoleId,CheckContractHolder)
SELECT CreatedBy,CreatedDate,UpdatedBy,UpdatedDate,AttachmentType,13,CheckContractHolder
FROM AttachmentTypePermissions
WHERE RoleId = 9 AND AttachmentType <> 'Commercial Invoice'

UPDATE AttachmentTypePermissions
SET Alias = @Alias
WHERE AttachmentType = @FactoryCommercialInvoice AND RoleId = 13 

COMMIT TRAN