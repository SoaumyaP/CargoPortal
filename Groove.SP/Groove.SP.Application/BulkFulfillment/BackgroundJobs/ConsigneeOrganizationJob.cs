using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.BulkFulfillment.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Core.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Groove.SP.Application.ApplicationBackgroundJob.SendMailBackgroundJobs;

namespace Groove.SP.Application.BulkFulfillment.BackgroundJobs
{
    public class ConsigneeOrganizationJob
    {
        private readonly AppConfig _appConfig;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IBlobStorage _blobStorage;

        public ConsigneeOrganizationJob(
            IPOFulfillmentRepository poFulfillmentRepository,
            IOptions<AppConfig> appConfig,
            IBlobStorage blobStorage
            )
        {
            _poFulfillmentRepository = poFulfillmentRepository;
            _appConfig = appConfig.Value;
            _blobStorage = blobStorage;
        }

        [JobDisplayName("Booking#{1} - A new organization needs to import")]
        public async Task ExecuteAsync(long bookingId, string bookingNumber)
        {
            var bulkBooking = await _poFulfillmentRepository.GetAsNoTrackingAsync(c => c.Id == bookingId, null, c => c.Include(c => c.Contacts));
            if (bulkBooking != null)
            {
                var consigneeContact = bulkBooking.Contacts.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.Consignee);
                if (consigneeContact != null)
                {
                    byte[] fileContent = await _blobStorage.GetBlobByRelativePathAsync($"template:organization:OrganizationImportTemplate.xlsx");

                    using (Stream fileStream = new MemoryStream(fileContent))
                    {
                        using (var xlPackage = new ExcelPackage(fileStream))
                        {
                            var sheet = xlPackage.Workbook.Worksheets[0];

                            ExcelRange cell;
                            string cellName = "";

                            // OrganizationType
                            cellName = "A2";
                            cell = sheet.Cells[cellName];
                            cell.Value = "";

                            // OrganizationCode
                            cellName = "B2";
                            cell = sheet.Cells[cellName];
                            cell.Value = "";

                            // OrganizationName
                            cellName = "C2";
                            cell = sheet.Cells[cellName];
                            cell.Value = consigneeContact.CompanyName;

                            // Address
                            cellName = "D2";
                            cell = sheet.Cells[cellName];
                            cell.Value = consigneeContact.Address;

                            // AddressLine2
                            cellName = "E2";
                            cell = sheet.Cells[cellName];
                            cell.Value = consigneeContact.AddressLine2;

                            // AddressLine3
                            cellName = "F2";
                            cell = sheet.Cells[cellName];
                            cell.Value = consigneeContact.AddressLine3;

                            // AddressLine4
                            cellName = "G2";
                            cell = sheet.Cells[cellName];
                            cell.Value = consigneeContact.AddressLine4;

                            //// Location Code
                            //cellName = "H2";
                            //cell = sheet.Cells[cellName];
                            //cell.Value = "";

                            // ContactName
                            cellName = "I2";
                            cell = sheet.Cells[cellName];
                            cell.Value = consigneeContact.ContactName;

                            // ContactEmail
                            cellName = "J2";
                            cell = sheet.Cells[cellName];
                            cell.Value = consigneeContact.ContactEmail;

                            // ContactNumber
                            cellName = "K2";
                            cell = sheet.Cells[cellName];
                            cell.Value = consigneeContact.ContactNumber;

                            sheet.DeleteRow(3, 2);

                            fileContent = xlPackage.GetAsByteArray();
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(_appConfig.ImportConsigneeOrganizationEmail))
                    {
                        BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailAsync(
                       $"Booking#{bulkBooking.Number} - A new organization needs to import",
                       "ConsigneeOrganization",
                       new ConsigneeOrganizationEmailTemplateViewModel
                       {
                           Name = "",
                           CompanyName = consigneeContact.CompanyName
                       },
                       _appConfig.ImportConsigneeOrganizationEmail,
                       $"Shipment Portal: New Consignee Organization needs to import (Booking#{bulkBooking.Number})",
                       new SPEmailAttachment()
                       {
                           AttachmentContent = fileContent,
                           AttachmentName = "CSP_OrganizationImportTemplate.xlsx"
                       })
                   );
                    }
                }
            }
        }
    }
}
