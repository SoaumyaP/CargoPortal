using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Core.Entities;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Groove.SP.Application.Exceptions;

using System.Data.Common;
using Groove.SP.Core.Data;
using Groove.SP.Infrastructure.CSFE.Models;
using Newtonsoft.Json;
using Groove.SP.Application.PurchaseOrders.Services.Interfaces;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Hangfire;
using Groove.SP.Application.PurchaseOrders.BackgroundJobs;
using Groove.SP.Application.Reports.Services.Interfaces;
using Groove.SP.Application.Scheduling.ViewModels;

namespace Groove.SP.Application.BuyerComplianceService.Services
{
    public class BuyerComplianceService : ServiceBase<BuyerComplianceModel, BuyerComplianceViewModel>, IBuyerComplianceService
    {
        private readonly IDataQuery _dataQuery;
        private readonly IRepository<POFulfillmentContactModel> _poffContactRepository;
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IReportService _reportService;
        public BuyerComplianceService(IUnitOfWorkProvider unitOfWorkProvider,
            ICSFEApiClient csfeApiClient,
            IRepository<POFulfillmentContactModel> poffRepository,
            IDataQuery dataQuery,
            IReportService reportService) : base(unitOfWorkProvider)
        {
            _poffContactRepository = poffRepository;
            _csfeApiClient = csfeApiClient;
            _dataQuery = dataQuery;
            _reportService = reportService;
        }

        protected override Func<IQueryable<BuyerComplianceModel>, IQueryable<BuyerComplianceModel>> FullIncludeProperties => x =>
        x.Include(m => m.BookingTimeless)
        .Include(m => m.BookingPolicies)
        .Include(m => m.ProductVerificationSetting)
        .Include(m => m.PurchaseOrderVerificationSetting)
        .Include(m => m.ShippingCompliance)
        .Include(m => m.ComplianceSelection)
        .Include(m => m.CargoLoadabilities)
        .Include(m => m.AgentAssignments)
        .Include(m => m.EmailSettings);

        protected override IDictionary<string, string> SortMap => new Dictionary<string, string>() {
            { "statusName", "status" }
        };

        public async Task<IEnumerable<BuyerComplianceModel>> GetListByOrgIdsAsync(List<long> orgIds)
        {
            return await Repository.Query(b => orgIds.Contains(b.OrganizationId) && b.Status == BuyerComplianceStatus.Active, includes: FullIncludeProperties).ToListAsync();
        }

        public async Task<IEnumerable<BuyerComplianceViewModel>> GetListAsync(long? organizationId)
        {
            Func<IQueryable<BuyerComplianceModel>, IQueryable<BuyerComplianceModel>> includeProperties = x =>
                x.Include(m => m.ProductVerificationSetting)
                .Include(m => m.PurchaseOrderVerificationSetting)
                .Include(m => m.ShippingCompliance)
                .Include(m => m.AgentAssignments)
                // CR for min-max validation 29/09/23
                .Include(m => m.BookingPolicies);

            var result = await Repository.Query(b => b.OrganizationId == organizationId, null, includeProperties).ToListAsync();
            var complianceVMs = Mapper.Map<IEnumerable<BuyerComplianceViewModel>>(result);

            foreach (var item in complianceVMs)
            {
                if (item != null && item.HSCodeShipFromCountryIds.Any())
                {
                    var locations = await _csfeApiClient.GetLocationsAsync(item.HSCodeShipFromCountryIds);
                    item.HSCodeShipFromIds = locations.Select(l => l.Id);
                }

                if (item != null && item.HSCodeShipToCountryIds.Any())
                {
                    var locations = await _csfeApiClient.GetLocationsAsync(item.HSCodeShipToCountryIds);
                    item.HSCodeShipToIds = locations.Select(l => l.Id);
                }
            }
            return complianceVMs;
        }

        public async Task<BuyerComplianceModel> GetByOrgIdAsync(long organizationId)
        {
            var buyerCompliance = await this.Repository.GetAsync(x => x.OrganizationId == organizationId, null, FullIncludeProperties);
            if (buyerCompliance != null)
            {
                buyerCompliance.BookingPolicies = buyerCompliance.BookingPolicies.OrderBy(x => x.Order).ToList();
            }
            return buyerCompliance;
        }

        public async Task<BuyerComplianceModel> GetByPOFFIdAsync(long poffId)
        {
            var qBuyerCompliances = Repository.GetListQueryable(FullIncludeProperties);
            var qPofulfillment = _poffContactRepository.GetListQueryable();

            var query = from buyer in qBuyerCompliances
                        join poffContact in qPofulfillment on buyer.OrganizationId equals poffContact.OrganizationId
                        where poffContact.POFulfillmentId.Equals(poffId) && poffContact.OrganizationRole.Equals("Principal")
                        select buyer;


            var buyerCompliance = await query.FirstOrDefaultAsync(x => x.Status.Equals(BuyerComplianceStatus.Active));
            if (buyerCompliance != null)
            {
                buyerCompliance.BookingPolicies = buyerCompliance.BookingPolicies.OrderBy(x => x.Order).ToList();
            }
            return buyerCompliance;
        }

        public async Task<BuyerComplianceModel> GetByPOFFAsync(POFulfillmentModel poff)
        {
            if (poff == null)
            {
                throw new AppEntityNotFoundException(nameof(poff));
            }

            var customer = poff.Contacts.Where(x => x.OrganizationRole == Core.Models.OrganizationRole.Principal).First();
            return await GetByOrgIdAsync(customer.OrganizationId);
        }

        public async Task<IEnumerable<DropDownListItem<long>>> GetDropDownByProgressCheckStatusAsync(int roleId, string affiliates, bool isProgressCargoReadyDate = true)
        {
            var buyerCompliances = await Repository.QueryAsNoTracking(x => x.Stage == BuyerComplianceStage.Activated && x.IsProgressCargoReadyDate == isProgressCargoReadyDate).ToListAsync();
            if (buyerCompliances?.Count() > 0)
            {
                var ids = buyerCompliances.Select(bc => bc.OrganizationId);
                if (roleId == (int)Role.Shipper)
                {
                    var affiliateCodes = JsonConvert.DeserializeObject<List<long>>(affiliates);
                    string organizationIds = string.Empty;
                    if (!string.IsNullOrEmpty(affiliates))
                    {
                        var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                        organizationIds = string.Join(",", listOfAffiliates);
                    }

                    IQueryable<CustomerRelationshipQueryModel> query;
                    string sql;
                    sql = @"
                            SELECT SupplierId, CustomerId, CustomerRefId
                            FROM CustomerRelationship
                            WHERE SupplierId IN (SELECT value FROM [dbo].[fn_SplitStringToTable]({0},','))
                                                  ";

                    query = _dataQuery.GetQueryable<CustomerRelationshipQueryModel>(sql, organizationIds);
                    var data = await query.ToListAsync();
                    var customerIds = data.Select(c => c.CustomerId);
                    buyerCompliances = buyerCompliances.Where(c => customerIds.Contains(c.OrganizationId)).ToList();
                }

                var refOrganizations = await _csfeApiClient.GetOrganizationByIdsAsync(ids);

                Func<long, Organization> OrganizationByIdFunc = (long orgId) => refOrganizations.SingleOrDefault(x => x.Id == orgId);
                return buyerCompliances.Select(bc => new DropDownListItem<long>()
                {
                    Text = OrganizationByIdFunc(bc.OrganizationId).Name,
                    Value = bc.OrganizationId
                });
            }
            return new List<DropDownListItem<long>> { };
        }

        public async Task<IEnumerable<DropDownListItem<long>>> GetWarehouseServiceTypeDropDownAsync()
        {
            var buyerCompliances = await Repository.QueryAsNoTracking(x =>
                x.Stage == BuyerComplianceStage.Activated
                && (x.ServiceType == BuyerComplianceServiceType.WareHouse || x.ServiceType == BuyerComplianceServiceType.FreightWareHouse || x.ServiceType == BuyerComplianceServiceType.WareHouseFreight)
                ).ToListAsync();
            if (buyerCompliances?.Count() > 0)
            {
                var refOrganizations = await _csfeApiClient.GetOrganizationByIdsAsync(buyerCompliances.Select(bc => bc.OrganizationId));

                Func<long, Organization> OrganizationByIdFunc = (long orgId) => refOrganizations.SingleOrDefault(x => x.Id == orgId);
                return buyerCompliances.Select(bc => new DropDownListItem<long>()
                {
                    Text = OrganizationByIdFunc(bc.OrganizationId).Name,
                    Value = bc.OrganizationId
                });
            }
            return new List<DropDownListItem<long>> { };
        }

        public async Task<BuyerComplianceViewModel> GetAsync(long id)
        {
            var buyerCompliance = await this.Repository.GetAsync(x => x.Id == id, null, FullIncludeProperties);

            if (buyerCompliance != null)
            {
                buyerCompliance.BookingPolicies = buyerCompliance.BookingPolicies.OrderBy(x => x.Order).ToList();
                buyerCompliance.AgentAssignments = buyerCompliance.AgentAssignments.OrderBy(x => x.Order).ToList();
            }
            return Mapper.Map<BuyerComplianceViewModel>(buyerCompliance);
        }

        public async Task<BuyerComplianceViewModel> CreateAsync(SaveBuyerComplianceViewModel model)
        {
            model.MergeAgentAssignment();
            var buyerCompliance = Mapper.Map<BuyerComplianceModel>(model);
            buyerCompliance.Status = BuyerComplianceStatus.Active;
            buyerCompliance.Stage = BuyerComplianceStage.Draft;

            buyerCompliance.ComplianceSelection.CarrierSelections.Split(',').ToList();

            // audit EmailSettings
            if (buyerCompliance.EmailSettings != null && buyerCompliance.EmailSettings.Any())
            {
                foreach (var item in buyerCompliance.EmailSettings)
                {
                    item.Audit(model.CreatedBy);
                }
            }

            await Repository.AddAsync(buyerCompliance);
            await this.UnitOfWork.SaveChangesAsync();

            if (model.OrganizationId.HasValue && model.OrganizationId.Value > 0)
            {
                _reportService.RegisterTelerikUserAsync(model.OrganizationId.Value);
            }

            return Mapper.Map<BuyerComplianceViewModel>(buyerCompliance);
        }

        public async Task<BuyerComplianceViewModel> UpdateAsync(SaveBuyerComplianceViewModel model)
        {
            model.ValidateAndThrow(true);
            var buyerCompliance = await Repository.GetAsync(x => x.Id == model.Id, null, FullIncludeProperties);

            if (buyerCompliance == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", model.Id)} not found!");
            }

            model.MergeAgentAssignment();
            Mapper.Map(model, buyerCompliance);

            Repository.Update(buyerCompliance);
            await this.UnitOfWork.SaveChangesAsync();

            TriggerEmailProgressCheckJob(buyerCompliance);

            return Mapper.Map<BuyerComplianceViewModel>(buyerCompliance);
        }

        public async Task<BuyerComplianceViewModel> ActivateAsync(ActivateBuyerComplianceViewModel model)
        {
            var buyerCompliance = await Repository.GetAsync(x => x.Id == model.Id, null, FullIncludeProperties);

            if (buyerCompliance == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", model.Id)} not found!");
            }

            model.MergeAgentAssignment();
            Mapper.Map(model, buyerCompliance);

            Repository.Update(buyerCompliance);
            await this.UnitOfWork.SaveChangesAsync();

            TriggerEmailProgressCheckJob(buyerCompliance);

            return Mapper.Map<BuyerComplianceViewModel>(buyerCompliance);
        }

        private void TriggerEmailProgressCheckJob(BuyerComplianceModel buyerCompliance)
        {
            if (buyerCompliance.Stage == BuyerComplianceStage.Activated && buyerCompliance.IsEmailNotificationToSupplier == true && buyerCompliance.EmailNotificationTime != null)
            {
                var hourMinute = buyerCompliance.EmailNotificationTime.Split(" ")[0];
                var beforeOrAfterMidday = buyerCompliance.EmailNotificationTime.Split(" ")[1];
                var hour = Convert.ToInt32(hourMinute.Split(":")[0]);
                var minute = Convert.ToInt32(hourMinute.Split(":")[1]);
                if (hour == 12)
                {
                    hour = 0;
                }
                hour = beforeOrAfterMidday == "PM" ? hour + 12 : hour;
                if (hour - (int)Timezone.UTC8 < 0)
                {
                    hour = 24 - ((int)Timezone.UTC8 - hour);
                }
                else
                {
                    hour = hour - (int)Timezone.UTC8;
                }

                RecurringJob.AddOrUpdate<EmailToSupplierJob>($"Email Progress Check Compliance#{buyerCompliance.Id}", c => c.ExecuteAsync(buyerCompliance.Id, buyerCompliance.AgentAssignmentMethod, buyerCompliance.OrganizationName), Cron.Daily(hour, minute));
            }
            else
            {
                RecurringJob.RemoveIfExists($"Email Progress Check Compliance#{buyerCompliance.Id}");
            }
        }

        public async Task<bool> IsOrganizationExists(long id, long organizationId)
        {
            var model = await Repository.GetAsync(x => x.OrganizationId == organizationId);
            return model != null && model.Id != id;
        }

        public async Task<IEnumerable<DropDownListItem>> GetPrincipalSelectionsForAgentRoleAsync(long organizationId)
        {
            var sql = $@"
                        SELECT bc.OrganizationId AS [Value], bc.OrganizationName AS [Text]
                        FROM BuyerCompliances bc WITH(NOLOCK)
                        WHERE 
	                        EXISTS (
		                        SELECT 1
		                        FROM  AgentAssignments aa WITH(NOLOCK) 
		                        WHERE aa.BuyerComplianceId = bc.Id AND aa.AgentOrganizationId = {organizationId}
	                        )
	                        AND bc.Status = {(int)BuyerComplianceStatus.Active} 
	                        AND bc.Stage = {(int)BuyerComplianceStage.Activated}
            ";

            Func<DbDataReader, IEnumerable<DropDownListItem>> mapping = (reader) =>
            {
                var mappedData = new List<DropDownListItem>();

                while (reader.Read())
                {
                    var newRow = new DropDownListItem
                    {
                        Value = reader[0]?.ToString() ?? "",
                        Text = reader[1]?.ToString() ?? ""
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = _dataQuery.GetDataBySql(sql, mapping);
            return result;
        }



    }
}
