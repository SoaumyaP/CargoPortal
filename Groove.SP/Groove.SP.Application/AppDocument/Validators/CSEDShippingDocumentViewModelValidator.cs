using FluentValidation;
using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Application.Common;
using System;
using System.Linq;

namespace Groove.SP.Application.AppDocument.Validators
{
    public class CSEDShippingDocumentViewModelValidator : BaseValidation<CSEDShippingDocumentViewModel>
    {
        public CSEDShippingDocumentViewModelValidator()
        {
            // DocumentId must required and not new
            RuleFor(x => x.documentId)
                 .Must(x => !x.Equals(Guid.Empty))
                 .WithMessage($"{nameof(CSEDShippingDocumentViewModel.documentId)} is empty or invalid.");

            // DocumentType
            RuleFor(x => x.documentType)
                .Must(x => !string.IsNullOrEmpty(x) && ValidDocumentTypes.Contains(x.ToLowerInvariant()))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.documentType)} is invalid. Supported values are: {string.Join(", ", ValidDocumentTypes)}.");
            
            // DocumentName
            RuleFor(x => x.documentName)
                .Must(x => !string.IsNullOrEmpty(x))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.documentName)} is empty.");

            // CreatedDate
            RuleFor(x => x.createdDate)
                .Must(x => !DateTime.MinValue.Equals(x))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.createdDate)} is empty or invalid.");

            // UploadBy
            RuleFor(x => x.uploadBy)
                .Must(x => !string.IsNullOrEmpty(x))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.uploadBy)} is empty.");

            // DocumentPath
            RuleFor(x => x.documentPath)
                .Must(x => !string.IsNullOrEmpty(x))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.documentPath)} is empty.");

            // Check some logic on data
            CheckDataLogicForInvoice();
            CheckDataLogicForHouseBL();
            CheckDataLogicForManifest();
            CheckDataLogicForAttachment();

        }

        private string[] ValidDocumentTypes = new[]
        {
            CSEDShippingDocumentDocumentType.SeaHouseBL,
            CSEDShippingDocumentDocumentType.SeaInvoice,
            CSEDShippingDocumentDocumentType.SeaManifest,
            CSEDShippingDocumentDocumentType.Attachment

        };

        private string[] ValidDocumentSubTypes = new[]
        {
            CSEDShippingDocumentDocumentSubType.AnimalHairsDeclaration,
            CSEDShippingDocumentDocumentSubType.CertificateOfOrigin,
            CSEDShippingDocumentDocumentSubType.CommercialInvoice,
            CSEDShippingDocumentDocumentSubType.FreightInvoice,
            CSEDShippingDocumentDocumentSubType.FumigationCertificate,
            CSEDShippingDocumentDocumentSubType.HouseBL,
            CSEDShippingDocumentDocumentSubType.ImportSecurityFiling,
            CSEDShippingDocumentDocumentSubType.MasterBL,
            CSEDShippingDocumentDocumentSubType.PackingDeclaration,
            CSEDShippingDocumentDocumentSubType.PackingList

        };

        private string[] ValidBillTypes = new[]
        {
            CSEDShippingDocumentBillType.ShippingOrder,
            CSEDShippingDocumentBillType.OceanBill,
            CSEDShippingDocumentBillType.HouseBill,

        };

        private string[] ValidInvoiceTypes = new[]
        {
            CSEDShippingDocumentInvoiceType.Normal,
            CSEDShippingDocumentInvoiceType.Freight,
            CSEDShippingDocumentInvoiceType.Internal,

        };

        private void CheckDataLogicForInvoice()
        {
            // Check correct data if document type is invoice
            RuleFor(x => x)
                .Must(x => x.invoiceDate.HasValue)
                .When(x => x.documentType.Equals(CSEDShippingDocumentDocumentType.SeaInvoice, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.invoiceDate)} is empty.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.documentSubType) && ValidInvoiceTypes.Contains(x.documentSubType.ToUpperInvariant()))
                .When(x => x.documentType.Equals(CSEDShippingDocumentDocumentType.SeaInvoice, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.documentSubType)} is invalid. Supported values are: {string.Join(", ", ValidInvoiceTypes)}.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.invoiceBillBy))
                .When(x => x.documentType.Equals(CSEDShippingDocumentDocumentType.SeaInvoice, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.invoiceBillBy)} is empty.");

            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.invoiceBillTo))
                .When(x => x.documentType.Equals(CSEDShippingDocumentDocumentType.SeaInvoice, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.invoiceBillTo)} is empty.");
        }

        private void CheckDataLogicForHouseBL()
        {
            // Check correct data if document type is house BL
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.blShipmentNumber))
                .When(x => x.documentType.Equals(CSEDShippingDocumentDocumentType.SeaHouseBL, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.blShipmentNumber)} is empty.");

        }

        private void CheckDataLogicForManifest()
        {
            // Check correct data if document type is manifest
            RuleFor(x => x)
                .Must(x => !string.IsNullOrEmpty(x.oceanBlNumber))
                .When(x => x.documentType.Equals(CSEDShippingDocumentDocumentType.SeaManifest, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.oceanBlNumber)} is empty.");

        }

        private void CheckDataLogicForAttachment()
        {
            // Check correct data if document type is attachment

            // DocumentSubType
            RuleFor(x => x.documentSubType)
                .Must(x => !string.IsNullOrEmpty(x) && ValidDocumentSubTypes.Contains(x.ToLowerInvariant()))
                .When(x => CSEDShippingDocumentDocumentType.Attachment.Equals(x.documentType, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.documentSubType)} is invalid. Supported values are: {string.Join(", ", ValidDocumentSubTypes)}.");

            // BillType
            RuleFor(x => x.billType)
                .Must(x => !string.IsNullOrEmpty(x) && ValidBillTypes.Contains(x, StringComparer.OrdinalIgnoreCase))
                .When(x => CSEDShippingDocumentDocumentType.Attachment.Equals(x.documentType, StringComparison.OrdinalIgnoreCase))
                .WithMessage($"{nameof(CSEDShippingDocumentViewModel.billType)} is invalid. Supported values are: {string.Join(", ", ValidBillTypes)}.");


        }
    }
}
