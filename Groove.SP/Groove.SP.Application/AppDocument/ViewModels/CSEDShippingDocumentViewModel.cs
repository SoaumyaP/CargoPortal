using Microsoft.Graph;
using System;
using System.Collections.Generic;

namespace Groove.SP.Application.AppDocument.ViewModels
{
    public class CSEDShippingDocumentViewModel
    {
        public Guid documentId { get; set; }
        public string documentType { get; set; }
        public string documentSubType { get; set; }
        public string documentCode { get; set; }
        public string dbName { get; set; }
        public string documentName { get; set; }
        public string contentType { get; set; }
        public DateTime createdDate { get; set; }
        public string documentPath { get; set; }
        public string uploadBy { get; set; }

        public DateTime? invoiceDate { get; set; }
        public string invoiceBLNumber { get; set; }
        public string invoiceJobNumber { get; set; }
        public string invoiceBillBy { get; set; }
        public string invoiceBillTo { get; set; }
        public string blShipmentNumber { get; set; }
        public string oceanBlNumber { get; set; }

        public DateTime? invoiceSubmissionToCruise { get; set; }
        public DateTime? invoiceDueDate { get; set; }

        public string billType { get; set; }


    }

    public class CSEDShippingDocumentDocumentType
    {
        public const string SeaHouseBL = "sea_house_bill";
        public const string SeaInvoice = "sea_invoice";
        public const string SeaManifest = "sea_manifest";
        public const string Attachment = "attachment";

    }

    public class CSEDShippingDocumentDocumentSubType
    {
        public const string AnimalHairsDeclaration = "animal_hairs_declaration";
        public const string CertificateOfOrigin = "certificate_of_origin";
        public const string CommercialInvoice = "commercial_invoice";
        public const string FreightInvoice = "freight_invoice";
        public const string FumigationCertificate = "fumigation_cert";
        public const string HouseBL = "hawb_hbl";
        public const string ImportSecurityFiling = "import_security_filing";
        public const string MasterBL = "ocean_bill_of_lading";
        public const string PackingDeclaration = "packing_declaration";
        public const string PackingList = "packing_list";

        public static Dictionary<string, string> ValueMapping = new()
        {
            { AnimalHairsDeclaration, "Animal Hairs Declaration" },
            { CertificateOfOrigin, "Certificate of Origin"},
            { CommercialInvoice, "Commercial Invoice"},
            { FreightInvoice, "Freight Invoice"},
            { FumigationCertificate, "Fumigation Certificate"},
            { HouseBL, "House BL"},
            { ImportSecurityFiling, "Import Security Filing"},
            { MasterBL, "Master BL"},
            { PackingDeclaration, "Packing Declaration"},
            { PackingList, "Packing List"}

        };

    }

    public class CSEDShippingDocumentBillType
    {
        public const string ShippingOrder = "shippingOrder";
        public const string OceanBill = "oceanBill";
        public const string HouseBill = "houseBill";
    }

    public class CSEDShippingDocumentInvoiceType
    {
        public const string Normal = "N";
        public const string Freight = "F";
        public const string Internal  = "I";

    }
}
