using AutoMapper;
using Groove.SP.Application.ApplicationBackgroundJob;
using Groove.SP.Application.Attachment.Services.Interfaces;
using Groove.SP.Application.Attachment.ViewModels;
using Groove.SP.Application.EmailSetting.Services;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Provider.EmailSender;
using Groove.SP.Application.Providers.BlobStorage;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Application.WarehouseFulfillment.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Infrastructure.DinkToPdf;
using Groove.SP.Infrastructure.QRCoder;
using Groove.SP.Infrastructure.RazorLight;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RazorLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Groove.SP.Application.ApplicationBackgroundJob.SendMailBackgroundJobs;

namespace Groove.SP.Application.WarehouseFulfillment
{
    public class ProceedWarehouseShippingOrderForm
    {
        private readonly AppConfig _appConfig;
        private readonly IAttachmentService _attachmentService;
        private readonly IPOFulfillmentRepository _poFulfillmentRepository;
        private readonly IRepository<BuyerComplianceModel> _buyerComplianceRepository;

        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IBlobStorage _blobStorage;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHtmlStringBuilder _htmlStringBuilder;
        private readonly IPdfGenerator _pdfGenerator;
        private readonly IQRCodeBuilder _qrCodeBuilder;
        private readonly IDataQuery _dataQuery;
        private readonly IMapper Mapper;

        public ProceedWarehouseShippingOrderForm(
            IOptions<AppConfig> appConfig,
            IAttachmentService attachmentService,
            ICSFEApiClient csfeApiClient,
            IPOFulfillmentRepository poFulfillmentRepository,
            IRepository<BuyerComplianceModel> buyerComplianceRepository,
            IBlobStorage blobStorage,
            IPdfGenerator pdfGenerator,
            IHtmlStringBuilder htmlStringBuilder,
            IUnitOfWorkProvider unitOfWorkProvider,
            IQRCodeBuilder qrCodeBuilder,
            IDataQuery dataQuery
            )
        {
            _appConfig = appConfig.Value;
            _attachmentService = attachmentService;
            _poFulfillmentRepository = poFulfillmentRepository;
            _buyerComplianceRepository = buyerComplianceRepository;
            _csfeApiClient = csfeApiClient;
            _blobStorage = blobStorage;
            _htmlStringBuilder = htmlStringBuilder;
            _pdfGenerator = pdfGenerator;
            _qrCodeBuilder = qrCodeBuilder;
            _unitOfWork = unitOfWorkProvider.CreateUnitOfWorkForBackgroundJob();
            _dataQuery = dataQuery;
            this.Mapper = this._unitOfWork.GetMapper();
        }

        [DisplayName("Proceed confirmation for Warehouse Booking Id = {0}")]
        public async Task ExecuteAsync(long pofId, string userName)
        {
            var warehouseBooking = await _poFulfillmentRepository.QueryAsNoTracking(c => c.Id == pofId, null, s => s.Include(a => a.Orders).Include(d => d.Contacts)).SingleOrDefaultAsync();
            var attachmentFileName = $"{DateTime.UtcNow:yyyyMMddHHmmss}-{"Shipping Order"} Form.pdf";
            var warehouseSOFormViewModel = await CreateWarehouseSOFormViewModel(warehouseBooking);
            var warehouseSOFormPdf = await GenerateWarehouseSOFormPdf(warehouseSOFormViewModel);
            await UploadSOFormAttachmentAsync(warehouseSOFormPdf, warehouseBooking.Number, warehouseBooking.Id, attachmentFileName, userName);
            await SendEmailToOwnerAndCCSuplierAsync(warehouseBooking, attachmentFileName, warehouseSOFormPdf);
        }

        private async Task<WarehouseSOFormViewModel> CreateWarehouseSOFormViewModel(POFulfillmentModel warehouseBooking)
        {
            var warehouseSOFormViewModel = new WarehouseSOFormViewModel();
            var principalContact = warehouseBooking.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal);
            var principalOrg = await _csfeApiClient.GetOrganizationByIdAsync(principalContact.OrganizationId);

            var warehouseLocation = await GetWarehouseLocationByPrincipalAssignmentId(principalContact.OrganizationId);
            if (warehouseLocation != null)
            {
                warehouseSOFormViewModel.AddressLine1 = warehouseLocation.AddressLine1;
                warehouseSOFormViewModel.AddressLine2 = warehouseLocation.AddressLine2;
                warehouseSOFormViewModel.AddressLine3 = warehouseLocation.AddressLine3;
                warehouseSOFormViewModel.AddressLine4 = warehouseLocation.AddressLine4;
                warehouseSOFormViewModel.LocationName = warehouseLocation.LocationName;
                warehouseSOFormViewModel.WorkingHours = warehouseLocation.WorkingHours;
                warehouseSOFormViewModel.Remarks = warehouseLocation.Remarks;
            }

            var pricipalOrgId = warehouseBooking?.Contacts?.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal)?.OrganizationId;
            if (pricipalOrgId != null && pricipalOrgId.Value > 0)
            {
                var warehouseAssignments = await _csfeApiClient.GetWarehouseAssignmentsByOrgIdAsync(pricipalOrgId.Value);
                if (warehouseAssignments != null && warehouseAssignments.Any())
                {
                    // Try to get from warehouse assignments if possible
                    var warehouseAssignment = warehouseAssignments.FirstOrDefault();
                    warehouseSOFormViewModel.ContactPhone = warehouseAssignment?.ContactPhone;
                    warehouseSOFormViewModel.ContactEmail = warehouseAssignment?.ContactEmail;
                    warehouseSOFormViewModel.ContactPerson = warehouseAssignment?.ContactPerson;
                }
            }

            warehouseSOFormViewModel.CompanyName = principalContact.CompanyName;
            warehouseSOFormViewModel.CustomerAddress = principalContact.Address;
            warehouseSOFormViewModel.CustomerAddressLine2 = principalContact.AddressLine2;
            warehouseSOFormViewModel.CustomerAddressLine3 = principalContact.AddressLine3;
            warehouseSOFormViewModel.CustomerAddressLine4 = principalContact.AddressLine4;
            warehouseSOFormViewModel.CustomerLogo = principalOrg.OrganizationLogo;
            warehouseSOFormViewModel.FixedLogo = GetCargoServiceLogo();

            var supplierContact = warehouseBooking.Contacts.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Supplier);
            if (supplierContact != null)
            {
                warehouseSOFormViewModel.SupplierCompanyName = supplierContact.CompanyName;
                warehouseSOFormViewModel.SupplierAddress = supplierContact.Address;
                warehouseSOFormViewModel.SupplierAddressLine2 = supplierContact.AddressLine2;
                warehouseSOFormViewModel.SupplierAddressLine3 = supplierContact.AddressLine3;
                warehouseSOFormViewModel.SupplierAddressLine4 = supplierContact.AddressLine4;
            }

            warehouseSOFormViewModel.BookingNo = warehouseBooking.Number;
            warehouseSOFormViewModel.ShipmentNo = $"{warehouseBooking.Number}01";
            warehouseSOFormViewModel.CompanyNo = warehouseBooking.CompanyNo;
            warehouseSOFormViewModel.PlanNo = warehouseBooking.PlantNo;
            warehouseSOFormViewModel.ConfirmedHubArrivalDate = $"{warehouseBooking.ConfirmedHubArrivalDate?.ToString("yyyy-MM-dd")} ({warehouseBooking.Time})" ?? "";
            warehouseSOFormViewModel.ExpectedHubArrivalDate = warehouseBooking.ExpectedDeliveryDate?.ToString("yyyy-MM-dd");
            warehouseSOFormViewModel.ActualTimeArrival = warehouseBooking.ActualTimeArrival?.ToString("yyyy-MM-dd") ?? "";
            warehouseSOFormViewModel.ContainerNo = warehouseBooking.ContainerNo;
            warehouseSOFormViewModel.HAWBNo = warehouseBooking.HAWBNo;
            warehouseSOFormViewModel.DeliveryMode = warehouseBooking.DeliveryMode;
            warehouseSOFormViewModel.Incoterm = warehouseBooking.Incoterm;
            warehouseSOFormViewModel.ETDOrigin = warehouseBooking.ETDOrigin?.ToString("yyyy-MM-dd") ?? "";
            warehouseSOFormViewModel.ETADestination = warehouseBooking.ETADestination?.ToString("yyyy-MM-dd") ?? "";
            warehouseSOFormViewModel.ShipFromName = warehouseBooking.ShipFromName;
            warehouseSOFormViewModel.NameofInternationalAccount = warehouseBooking.NameofInternationalAccount;
            warehouseSOFormViewModel.QRCode = _qrCodeBuilder.GenerateQRCode($";type:warehousebooking;booking:{warehouseBooking.Number};so:{warehouseBooking.SONo};scope:cargoreceived;");

            foreach (var item in warehouseBooking.Orders)
            {
                var product = Mapper.Map<WarehouseOrderSOFormViewModel>(item);
                var articleMaster = await GetArticleMaster(item.POLineItemId, item.ProductCode, principalOrg.Code, item.StyleNo, item.ColourCode, item.Size);
                product.StyleName = articleMaster?.StyleName;
                product.ColourName = articleMaster?.ColourName;
                product.Volume = Math.Round(item.Volume ?? 0, 3);
                warehouseSOFormViewModel.Orders.Add(product);
            }

            return warehouseSOFormViewModel;
        }

        private async Task<byte[]> GenerateWarehouseSOFormPdf(WarehouseSOFormViewModel warehouseSOFormViewModel)
        {
            // Read template by RazorLight
            var htmlString = await _htmlStringBuilder.CreateHtmlStringAsync("Pdf/WarehouseSOForm.cshtml", warehouseSOFormViewModel);
            var pdfDetail = new PdfDetail
            {
                DocumentTitle = "Shipping Order Form",
                HtmlContent = htmlString
            };

            // Then generate .pdf content
            var fileBytes = _pdfGenerator.GeneratePdf(pdfDetail);
            return fileBytes;
        }

        public async Task UploadSOFormAttachmentAsync(byte[] warehouseSOFormPdf, string bookingNumber, long id, string attachmentFileName, string userName)
        {
            //Upload file into storage
            var path = string.Empty;
            using (var stream = new MemoryStream(warehouseSOFormPdf))
            {
                path = await _blobStorage.PutBlobAsync("attachment", bookingNumber, stream);
            }

            var attachment = new AttachmentViewModel()
            {
                AttachmentType = AttachmentType.SHIPPING_ORDER_FORM,
                ReferenceNo = $"{bookingNumber}01",
                POFulfillmentId = id,
                Description = userName,
                UploadedBy = AppConstant.SYSTEM_USERNAME,
                UploadedDateTime = DateTime.UtcNow,
                BlobId = path,
                FileName = attachmentFileName
            };
            await _attachmentService.ImportAttachmentAsync(attachment);

        }

        public async Task SendEmailToOwnerAndCCSuplierAsync(POFulfillmentModel warehouseBooking, string attachmentFileName, byte[] warehouseSOFormPdf)
        {
            var supplier = warehouseBooking.Contacts.FirstOrDefault(c => c.OrganizationRole.Equals(OrganizationRole.Supplier, StringComparison.OrdinalIgnoreCase));

            // default recipients.
            var mailCCList = string.IsNullOrEmpty(supplier.ContactEmail) ? null : new List<string> { supplier.ContactEmail };
            var mailToList = new List<string>() { warehouseBooking.CreatedBy };

            BuyerComplianceModel buyerCompliance = null;
            Infrastructure.CSFE.Models.WarehouseAssignment warehouseAssignment = null;

            var pricipalOrgId = warehouseBooking?.Contacts?.FirstOrDefault(x => x.OrganizationRole == OrganizationRole.Principal)?.OrganizationId;
            if (pricipalOrgId != null && pricipalOrgId.Value > 0)
            {
                warehouseAssignment = (await _csfeApiClient.GetWarehouseAssignmentsByOrgIdAsync(pricipalOrgId.Value))?.FirstOrDefault();

                buyerCompliance = await _buyerComplianceRepository.GetAsNoTrackingAsync(x => x.OrganizationId == pricipalOrgId, includes: i => i.Include(x => x.EmailSettings));
            }
            Infrastructure.CSFE.Models.WarehouseLocation warehouseLocation = warehouseAssignment?.WarehouseLocation;
            var emailModel = new ConfirmedWarehouseBookingEmailModel
            {
                CreatedDate = warehouseBooking.CreatedDate,
                ConfirmedHubArrivalDate = warehouseBooking.ConfirmedHubArrivalDate,
                Period = warehouseBooking.Time,
                SONo = warehouseBooking.SONo,
                CompanyNo = warehouseBooking.CompanyNo,
                PlantNo = warehouseBooking.PlantNo,
                Supplier = supplier?.CompanyName,
                ContactName = warehouseAssignment?.ContactPerson,
                ContactNumber = warehouseAssignment?.ContactPhone,
                ContactEmail = warehouseAssignment?.ContactEmail,
                AddressLine1 = warehouseLocation?.AddressLine1,
                AddressLine2 = warehouseLocation?.AddressLine2,
                AddressLine3 = warehouseLocation?.AddressLine3,
                AddressLine4 = warehouseLocation?.AddressLine4,
                City = warehouseLocation?.Location.LocationDescription,
                Country = warehouseLocation?.Location.Country.Name
            };

            emailModel.Orders = (from order in warehouseBooking.Orders
                                group order by order.CustomerPONumber into grouped
                                select new WarehouseBookingOrderEmailModel
                                {
                                    PONumber = grouped.Key,
                                    CBM = grouped.Sum(y => Math.Round(y.Volume ?? 0, 3)),
                                    Ctn = grouped.Sum(y => y.BookedPackage ?? 0),
                                    Qty = grouped.Sum(y => y.FulfillmentUnitQty)

                                }).ToList();

            var emailAttachment = new SPEmailAttachment
            {
                AttachmentContent = warehouseSOFormPdf,
                AttachmentName = attachmentFileName
            };

            // Check Compliance Email Setting to define recipients.
            const char EMAIL_SEPARATOR = ';';
            var recipients = EmailSettingService.GetReceipientsFromBuyerCompliance(buyerCompliance?.EmailSettings?.ToList(), EmailSettingType.BookingConfirmed, mailToList, mailCCList);
            mailToList = recipients["sendTo"];
            mailCCList = recipients["cc"];

            BackgroundJob.Enqueue<SendMailBackgroundJobs>(x => x.SendMailWithCCAsync(
                $"Warehouse Booking has been confirmed #{warehouseBooking.Id}",
                "WarehouseBooking_Confirmed",
                emailModel,
                string.Join(EMAIL_SEPARATOR, mailToList),
                mailCCList,
                $"Re: {warehouseBooking.EmailSubject}",
                emailAttachment
                ));
        }

        private string GetCargoServiceLogo()
        {
            using (Image image = Image.FromFile("EmailTemplates/Pdf/Cargo Services logo.png"))
            {
                using (MemoryStream m = new MemoryStream())
                {
                    image.Save(m, image.RawFormat);
                    byte[] imageBytes = m.ToArray();

                    string base64String = Convert.ToBase64String(imageBytes);
                    return $"data:image/png;base64,{base64String}";
                }
            }
        }


        private async Task<WarehouseArticleMasterQueryModel> GetArticleMaster(long poLineItemId, string productCode, string principalOrgCode, string styleNo, string colourCode, string size)
        {
            IQueryable<WarehouseArticleMasterQueryModel> query;
            if (poLineItemId != 0)
            {
                var sql = @"
                        SELECT TOP 1 
	                        StyleName,
	                        ColourName
                        FROM  ArticleMaster
                        WHERE ItemNo = {0} AND CompanyCode = {1}
                      ";

                query = _dataQuery.GetQueryable<WarehouseArticleMasterQueryModel>(sql, productCode, principalOrgCode);
                var articleMaster = await query.FirstOrDefaultAsync();
                return articleMaster;
            }
            else
            {
                var sql = @"
                        SELECT TOP 1 
	                        StyleName,
	                        ColourName
                        FROM  ArticleMaster
                        WHERE StyleNo = {0} AND ColourCode = {1} and Size = {2}
                      ";
                query = _dataQuery.GetQueryable<WarehouseArticleMasterQueryModel>(sql, styleNo, colourCode, size);
                var articleMaster = await query.FirstOrDefaultAsync();
                return articleMaster;
            }
        }


        private async Task<WarehouseFulfillmentLocationSOFormQueryModel> GetWarehouseLocationByPrincipalAssignmentId(long organizationId)
        {
            IQueryable<WarehouseFulfillmentLocationSOFormQueryModel> query;
            var sql =
                  @"
                            SELECT TOP 1 
	                            WL.Name AS LocationName,
	                            WL.AddressLine1,
	                            WL.AddressLine2,
	                            WL.AddressLine3,
	                            WL.AddressLine4,
	                            WL.ContactEmail,
	                            WL.ContactPhone,
                                WL.ContactPerson,
                                WL.WorkingHours,
                                WL.Remarks
                            FROM WarehouseAssignments WA
                            INNER JOIN WarehouseLocations WL ON WA.WarehouseLocationId = WL.id
                            WHERE WA.OrganizationId = {0}
                           ";

            query = _dataQuery.GetQueryable<WarehouseFulfillmentLocationSOFormQueryModel>(sql, organizationId);
            var warehouseLocation = await query.FirstOrDefaultAsync();
            return warehouseLocation;
        }
    }

    public class ConfirmedWarehouseBookingEmailModel
    {
        public DateTime? ConfirmedHubArrivalDate { get; set; }
        public string Period { get; set; }
        public DateTime CreatedDate { get; set; }
        public string SONo { get; set; }
        public string PlantNo { get; set; }
        public string CompanyNo { get; set; }
        public string Supplier { get; set; }
        #region WarehouseLocation
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        #endregion WarehouseLocation

        #region WarehouseAssignment
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }
        #endregion WarehouseAssignment

        public List<WarehouseBookingOrderEmailModel> Orders { get; set; }
    }

    public class WarehouseBookingOrderEmailModel
    {
        public string PONumber { get; set; }
        public int Ctn { get; set; }
        public int Qty { get; set; }
        public decimal CBM { get; set; }
    }
}
