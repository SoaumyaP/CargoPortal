using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Invoices.Services.Interfaces;
using Groove.SP.Application.Invoices.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Invoices.Services
{
    public class InvoiceListService : ServiceBase<InvoiceModel, InvoiceViewModel>, IInvoiceListService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        protected readonly IServiceProvider ServiceProvider;
        protected readonly IRepository<BillOfLadingModel> _billOfLadingRepository;

        public InvoiceListService(IDataQuery dataQuery,
                                IRepository<BillOfLadingModel> billOfLadingRepository,
                                IOptions<AppConfig> appConfig,
                                IUnitOfWorkProvider unitOfWorkProvider): base(unitOfWorkProvider)
        {
            _appConfig = appConfig.Value;
            _billOfLadingRepository = billOfLadingRepository;
            _dataQuery = dataQuery;
        }

        public async Task<DataSourceResult> GetListInvoiceAsync(DataSourceRequest request, IdentityInfo currentUser)
        {
            IQueryable<InvoiceQueryModel> query;

            // Filtering against date range
            DateTime baseDate = DateTime.UtcNow.Date;
            DateTime dateFrom = baseDate.AddDays(_appConfig.DataAccess.Invoice.Before);
            DateTime dateTo = baseDate.AddDays(_appConfig.DataAccess.Invoice.After);

            string sql = @"
                            SELECT I.[Id]
                             ,[InvoiceType]
                             ,CASE [InvoiceType]
                                   WHEN 'N' THEN 'Invoice'
                                   WHEN 'F' THEN 'Statement'
                                   WHEN 'I' THEN 'Manual'
                              END InvoiceTypeName
                             ,I.[CreatedDate]
                             ,[InvoiceNo]
                             ,[InvoiceDate]
                             ,[ETDDate]
                             ,[ETADate]
                             ,I.[BillOfLadingNo]
                             ,[JobNo]
                             ,[BillTo]
                             ,[BillBy]
                             ,[FileName]
                             ,[DateOfSubmissionToCruise]
                             ,[PaymentDueDate]
                             ,PaymentStatus
							 ,PaymentDate
                             ,CASE PaymentStatus
                                   WHEN 1 THEN 'label.paid'
                                   WHEN 2 THEN 'label.partial'
							 END PaymentStatusName
                         FROM [Invoices] I WITH (NOLOCK)
                         WHERE I.[InvoiceDate] >= {0} AND I.[InvoiceDate] <= {1} 
                        ";

            if(!currentUser.IsInternal)
            {
                sql += @"   
                            AND (I.BillByOrganizationId = {2} OR I.BillToOrganizationId = {2})
                        ";
            }

            query = _dataQuery.GetQueryable<InvoiceQueryModel>(sql, dateFrom, dateTo, currentUser.OrganizationId);

            // Custom filter for field '..Date' only
            ModifyFilters(request.Filters);

            if (request.Sorts != null)
            {
                var sortInvoiceType = request.Sorts.SingleOrDefault(c => c.Member == "invoiceType");
                if (sortInvoiceType != null)
                {
                    sortInvoiceType.Member = "InvoiceTypeName";
                }

                var sortPaymentStatus = request.Sorts.SingleOrDefault(c => c.Member == nameof(InvoiceQueryModel.PaymentStatus));
                if (sortPaymentStatus != null)
                {
                    sortPaymentStatus.Member = nameof(InvoiceQueryModel.PaymentStatusName);
                }
            }

            var invoices = await query.ToDataSourceResultAsync(request);
            var data = (List<InvoiceQueryModel>)invoices.Data;
            data.ForEach(c => c.SetBillOfLadingNos());

            var houseBLNos = data.SelectMany(c => c.BillOfLadingNos.Select(c => c.BillOfLadingNo)).Distinct().ToList();
            var billOfLadings =await _billOfLadingRepository.QueryAsNoTracking(c => houseBLNos.Contains(c.BillOfLadingNo)).ToListAsync();
            foreach (var item in data)
            {
                foreach (var houseBLInfo in item.BillOfLadingNos)
                {
                    var isExisting = billOfLadings.Any(c => c.BillOfLadingNo == houseBLInfo.BillOfLadingNo);
                    if (isExisting)
                    {
                        houseBLInfo.IsExistingInSystem = true;
                    }
                }
            }

            return invoices;
        }
    }
}
