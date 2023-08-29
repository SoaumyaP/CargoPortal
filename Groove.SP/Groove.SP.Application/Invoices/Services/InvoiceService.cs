using AutoMapper.QueryableExtensions;
using Groove.SP.Application.AppDocument.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Invoices.Services.Interfaces;
using Groove.SP.Application.Invoices.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Infrastructure.CSFE.Models;
using IdentityModel;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Groove.SP.Application.Invoices.Services
{
    public class InvoiceService : ServiceBase<InvoiceModel, InvoiceViewModel>, IInvoiceService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<UserProfileModel> _userProfileRepository;
        private readonly IRepository<BillOfLadingModel> _billOfLadingRepository;
        private readonly AppConfig _appConfig;
        private readonly ICSFEApiClient _csfeApiClient;
        public InvoiceService(IUnitOfWorkProvider unitOfWorkProvider, 
            IRepository<UserProfileModel> userProfileRepository,
            IRepository<BillOfLadingModel> billOfLadingRepository,
            IHttpContextAccessor httpContextAccessor, 
            IOptions<AppConfig> appConfig,
            ICSFEApiClient csfeApiClient)
            : base(unitOfWorkProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _userProfileRepository = userProfileRepository;
            _billOfLadingRepository = billOfLadingRepository;
            _appConfig = appConfig.Value;
            _csfeApiClient = csfeApiClient;
        }        

        public async Task<InvoiceViewModel> GetAsync(string invoiceNo)
        {
            var model = await this.Repository.GetAsync(x => x.InvoiceNo == invoiceNo);
            var viewModel = Mapper.Map<InvoiceViewModel>(model);
            return viewModel;
        }

        public async Task<string> ImportInvoice(ImportInvoiceViewModel viewModel, string blobId, string fileName)
        {
            var invoice = new InvoiceModel
            {
                InvoiceNo = viewModel.InvoiceNo,
                BillBy = viewModel.BillBy,
                BillByOrganizationId = viewModel.BillByOrganizationId,
                BillToOrganizationId = viewModel.BillToOrganizationId,
                BillOfLadingNo = viewModel.BillOfLadingNo,
                BillTo = viewModel.BillTo,
                ETADate = viewModel.ETADate,
                ETDDate = viewModel.ETDDate,
                BlobId = blobId,
                FileName = fileName,
                JobNo = viewModel.JobNo,
                InvoiceDate = viewModel.InvoiceDate.Value,
                InvoiceType = viewModel.InvoiceType
            };

            invoice.Audit();
            await Repository.AddAsync(invoice);
            await UnitOfWork.SaveChangesAsync();

            return invoice.InvoiceNo;
        }

        public async Task ImportCSEDSeaInvoiceAsync(CSEDShippingDocumentViewModel shippingDocument, string blobId)
        {
            // BillBy/BillTo = ediSON company code       
            var edisonCompanyCodes = new[] { shippingDocument.invoiceBillBy, shippingDocument.invoiceBillTo };
            // ignore null value
            edisonCompanyCodes = edisonCompanyCodes.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            // set default empty values
            IEnumerable<Organization> organizations = Enumerable.Empty<Organization>();

            if (edisonCompanyCodes?.Count() > 0)
            {
                organizations = await _csfeApiClient.GetOrganizationsByEdisonCompanyIdCodesAsync(edisonCompanyCodes);
            }

            // In case documentType/invoice type = N, retrieve ETA/ETD from House BL if invoiceBLNumber has value
            DateTime? eta, etd;
            eta = etd = null;
            if (shippingDocument.documentSubType.Equals(CSEDShippingDocumentInvoiceType.Normal, StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrEmpty(shippingDocument.invoiceBLNumber))
            {
                var houseBL = await _billOfLadingRepository.GetAsNoTrackingAsync(x => x.BillOfLadingNo == shippingDocument.invoiceBLNumber);
                if (houseBL != null)
                {
                    eta = houseBL.ShipToETADate;
                    etd = houseBL.ShipFromETDDate;
                }
            }

            var invoice = new InvoiceModel
            {
                InvoiceNo = shippingDocument.documentCode,
                BillOfLadingNo = shippingDocument.invoiceBLNumber,

                // try to retrieve from Master Data. If unmatched, set null to database
                BillTo = organizations.FirstOrDefault(x=>x.EdisonCompanyCodeId == shippingDocument.invoiceBillTo)?.Name,  
                BillToOrganizationId = organizations.FirstOrDefault(x=>x.EdisonCompanyCodeId == shippingDocument.invoiceBillTo)?.Id,
                BillBy = organizations.FirstOrDefault(x => x.EdisonCompanyCodeId == shippingDocument.invoiceBillBy)?.Name,
                BillByOrganizationId = organizations.FirstOrDefault(x => x.EdisonCompanyCodeId == shippingDocument.invoiceBillBy)?.Id,

                ETADate = eta,
                ETDDate = etd,
                BlobId = blobId,
                FileName = shippingDocument.documentName,
                JobNo = shippingDocument.invoiceJobNumber,
                InvoiceDate = shippingDocument.invoiceDate.Value,
                InvoiceType = shippingDocument.documentSubType,

                DateOfSubmissionToCruise = shippingDocument.invoiceSubmissionToCruise,
                PaymentDueDate = shippingDocument.invoiceDueDate,

                // fulfill some audit information
                CreatedDate = shippingDocument.createdDate,
                CreatedBy = shippingDocument.uploadBy,
                UpdatedDate = shippingDocument.createdDate,
                UpdatedBy = shippingDocument.uploadBy                
            };

            await Repository.AddAsync(invoice);
            await UnitOfWork.SaveChangesAsync();

        }

        public async Task<InvoiceUpdatePaymentViewModel> UpdatePaymentAsync(InvoiceUpdatePaymentViewModel viewModel)
        {
            var invoice = await Repository.GetAsync(c => c.InvoiceNo == viewModel.InvoiceNo);
            if (invoice == null)
            {
                throw new AppEntityNotFoundException($"Invoice {viewModel.InvoiceNo} not found!");
            }
            Mapper.Map(viewModel, invoice);
            await UnitOfWork.SaveChangesAsync();

            return Mapper.Map<InvoiceUpdatePaymentViewModel>(invoice);
        }
    }
}
