using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.UnitOfWork;
using Groove.CSFE.Application.Organizations.Services.Interfaces;
using Groove.CSFE.Application.Organizations.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System;
using System.Collections.Generic;
using GGroove.CSFE.Application;
using Groove.CSFE.Application.Exceptions;
using Groove.CSFE.Core;
using Groove.CSFE.Application.SendMails.Services.Interfaces;
using Groove.CSFE.Application.SendMails.ViewModels;
using Microsoft.Extensions.Options;
using Groove.CSFE.Application.Interfaces.Repositories;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.Primitives;
using System.Net.Http.Headers;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Models;
using System.Data.Common;
using System.Data;
using System.Data.SqlClient;
using Groove.CSFE.Application.WarehouseLocations.ViewModels;
using Groove.CSFE.Application.WarehouseAssignments.ViewModels;
using System.Text.RegularExpressions;

namespace Groove.CSFE.Application.Organizations.Services
{
    public class OrganizationService : ServiceBase<OrganizationModel, OrganizationViewModel>, IOrganizationService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ISendMailService _sendMailService;
        private readonly IRepository<CustomerRelationshipModel> _customerRelationshipRepository;
        private readonly IRepository<WarehouseAssignmentModel> _warehouseAssignmentRepository;
        private readonly AppConfig _appConfig;
        private readonly IOrganizationRepository _organizationRepository;
        private readonly IDataQuery _dataQuery;


        public OrganizationService(IUnitOfWorkProvider unitOfWorkProvider, ISendMailService sendMailService, IOptions<AppConfig> appConfig,
            IRepository<CustomerRelationshipModel> customerRelationshipRepository, IHttpClientFactory httpClientFactory,
            IOrganizationRepository organizationRepository,
            IRepository<WarehouseAssignmentModel> warehouseAssignmentRepository,
            IDataQuery dataQuery)
            : base(unitOfWorkProvider)
        {
            _sendMailService = sendMailService;
            _appConfig = appConfig.Value;
            _customerRelationshipRepository = customerRelationshipRepository;
            _httpClientFactory = httpClientFactory;
            _organizationRepository = organizationRepository;
            _dataQuery = dataQuery;
            _warehouseAssignmentRepository = warehouseAssignmentRepository;
        }

        protected override IDictionary<string, string> SortMap { get; } = new Dictionary<string, string>()
        {
            { "statusName", "status" },
            { "organizationTypeName", "organizationType" }
        };

        public async Task<IEnumerable<OrganizationReferenceDataViewModel>> GetOrgReferenceDataSourceAsync(IEnumerable<long> ids)
        {
            List<OrganizationModel> models;
            if (ids != null && ids.Any())
            {
                models = await this.Repository.QueryAsNoTracking(x => ids.Contains(x.Id)).ToListAsync();
            }
            else
            {
                models = await this.Repository.QueryAsNoTracking().ToListAsync();
            }
            return Mapper.Map<IEnumerable<OrganizationReferenceDataViewModel>>(models);
        }

        public async Task<OrganizationReferenceDataViewModel> GetOrgReferenceDataSourceAsync(string code)
        {
            var model = await this.Repository.GetAsNoTrackingAsync(x => x.Code == code);
            return Mapper.Map<OrganizationReferenceDataViewModel>(model);
        }

        public async Task<IEnumerable<OrganizationViewModel>> GetByCodesAsync(IEnumerable<string> codes)
        {
            var models = await this.Repository.QueryAsNoTracking(x => codes.Contains(x.Code.Trim())).ToListAsync();
            return Mapper.Map<IEnumerable<OrganizationViewModel>>(models);
        }

        public async Task<IEnumerable<OrganizationViewModel>> GetByIdsAsync(IEnumerable<long> ids)
        {
            var models = await Repository.QueryAsNoTracking(x => ids.Contains(x.Id)).ToListAsync();
            return Mapper.Map<IEnumerable<OrganizationViewModel>>(models);
        }

        public async Task<IEnumerable<OrganizationViewModel>> GetByEdisonCompanyCodeIdsAsync(IEnumerable<string> edisonCompanyCodes)
        {
            var models = await this.Repository.QueryAsNoTracking(x => edisonCompanyCodes.Contains(x.EdisonCompanyCodeId)).ToListAsync();
            return Mapper.Map<IEnumerable<OrganizationViewModel>>(models);
        }

        public async Task<OrganizationViewModel> GetAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Id == id, null, IncludeProperties);
            var organizationViewModel = Mapper.Map<OrganizationModel, OrganizationViewModel>(model);
            if (!string.IsNullOrEmpty(model?.ParentId))
            {
                var parentIds = model.ParentId.Split(".").Where(c => !string.IsNullOrEmpty(c));
                var parentOrg = await GetOrganizationCodeByParentIds(parentIds.Select(long.Parse).ToList());
                organizationViewModel.ParentOrgCode = parentOrg.Count() == 1 ? parentOrg.First().Code : parentOrg.Last().Code;
                organizationViewModel.ParentOrgName = parentOrg.Count() == 1 ? parentOrg.First().Name : parentOrg.Last().Name;
            }
            return organizationViewModel;
        }

        public async Task<OrganizationViewModel> GetSimpleAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Id == id);
            return Mapper.Map<OrganizationModel, OrganizationViewModel>(model);
        }

        public async Task<OrganizationViewModel> GetAsync(string code)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Code == code, null, IncludeProperties);
            return Mapper.Map<OrganizationModel, OrganizationViewModel>(model);
        }

        public async Task<OrganizationViewModel> GetByNameAsync(string name)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Name == name, null, IncludeProperties);
            return Mapper.Map<OrganizationModel, OrganizationViewModel>(model);
        }

        public async Task<IEnumerable<OrganizationViewModel>> GetByFulltextSearchNameAsync(string name)
        {
            string sql = @$"
                           SELECT 
	                            Id,
	                            Name,
	                            Address,
                                AddressLine2,
                                AddressLine3,
                                AddressLine4,
	                            ContactName,
                                ContactEmail,
                                ContactNumber,
                                WeChatOrWhatsApp
                            FROM Organizations O
                            INNER JOIN FREETEXTTABLE(Organizations,Name,'{name}') AS FT
                            ON O.Id = FT.[KEY]
                            ORDER BY FT.RANK DESC

                    ";

            Func<DbDataReader, IEnumerable<OrganizationViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<OrganizationViewModel>();

                while (reader.Read())
                {
                    var newRow = new OrganizationViewModel
                    {
                        Id = (long)reader[0],
                        Name = reader[1]?.ToString() ?? "",
                        Address = reader[2]?.ToString() ?? "",
                        AddressLine2 = reader[3]?.ToString() ?? "",
                        AddressLine3 = reader[4]?.ToString() ?? "",
                        AddressLine4 = reader[5]?.ToString() ?? "",
                        ContactName = reader[6]?.ToString() ?? "",
                        ContactEmail = reader[7]?.ToString() ?? "",
                        ContactNumber = reader[8]?.ToString() ?? "",
                        WeChatOrWhatsApp = reader[9]?.ToString() ?? ""
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = _dataQuery.GetDataBySql(sql, mapping);
            return result;
        }

        protected override Func<IQueryable<OrganizationModel>, IQueryable<OrganizationModel>> IncludeProperties
        {
            get
            {
                return x => x.Include(m => m.OrganizationInRoles).ThenInclude(m => m.OrganizationRole)
                            .Include(m => m.Location).ThenInclude(m => m.Country)
                            .Include(m => m.CustomerRelationship).ThenInclude(m => m.Customer)
                            .Include(m => m.EmailNotifications);
            }
        }

        public override async Task<IEnumerable<OrganizationViewModel>> GetAllAsync()
        {
            try
            {
                var models = await this.Repository.QueryAsNoTracking(null, null, IncludeProperties).ToListAsync();
                return Mapper.Map<IEnumerable<OrganizationViewModel>>(models);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<IEnumerable<DropDownModel>> OtherCodeDropDownAsync(long organizationId)
        {
            var organization = await Repository.GetAsNoTrackingAsync(x => x.Id == organizationId);
            organization.ParentId = organization.ParentId ?? string.Empty;
            if (organization == null)
            {
                throw new AppException("organizationNotFound");
            }
            var models = await Repository.QueryAsNoTracking(x =>
                x.Id != organization.Id &&
                x.Status == OrganizationStatus.Active &&
                string.IsNullOrEmpty(x.ParentId) &&
                organization.OrganizationType == x.OrganizationType &&
                !organization.ParentId.StartsWith(x.Id.ToString() + AppConstants.DELIMITER_PARENT_ID)
                , c => c.OrderBy(x => x.Code)
                )
                .ToListAsync();
            var result = models?.Select(x => new DropDownModel { Label = x.Code, Value = x.Id });
            return result;
        }

        public async Task<IEnumerable<DropDownModel>> GetAffiliateDropdownAsync(long organizationId)
        {
            var organization = await Repository.GetAsNoTrackingAsync(x => x.Id == organizationId);
            organization.ParentId = organization.ParentId ?? string.Empty;
            if (organization == null)
            {
                throw new AppException("organizationNotFound");
            }

            var models = await Repository.QueryAsNoTracking(x =>
                        x.Id != organization.Id &&
                        x.Status == OrganizationStatus.Active &&
                        string.IsNullOrEmpty(x.ParentId) &&
                        organization.OrganizationType == x.OrganizationType &&
                        !organization.ParentId.StartsWith(x.Id.ToString() + AppConstants.DELIMITER_PARENT_ID), c => c.OrderBy(x => x.Code)).ToListAsync();

            var result = models?.Select(x => new DropDownModel { Label = $"{x.Code} - {x.Name}", Value = x.Id });
            return result;
        }

        public async Task<IEnumerable<OrganizationReferenceDataViewModel>> GetActiveOrgReferenceDataSourceAsync()
        {
            var storedProcedureName = "spu_GetActiveOrganizations";

            Func<DbDataReader, IEnumerable<OrganizationReferenceDataViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<OrganizationReferenceDataViewModel>();

                // Map data for purchase order information
                while (reader.Read())
                {
                    var newRow = new OrganizationReferenceDataViewModel();
                    object tmpValue;

                    // Must be in order of data reader
                    newRow.Id = (long)reader[0];
                    tmpValue = reader[1];
                    newRow.Code = tmpValue.ToString();
                    tmpValue = reader[2];
                    newRow.Name = tmpValue.ToString();
                    tmpValue = reader[3];
                    newRow.Address = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[4];
                    newRow.AddressLine2 = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[5];
                    newRow.AddressLine3 = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[6];
                    newRow.AddressLine4 = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[7];
                    newRow.ContactName = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[8];
                    newRow.ContactNumber = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[9];
                    newRow.ContactEmail = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[10];
                    newRow.AgentType = Enum.Parse<AgentType>(tmpValue.ToString());
                    tmpValue = reader[11];
                    newRow.CustomerPrefix = DBNull.Value == tmpValue ? null : tmpValue.ToString();
                    tmpValue = reader[12];
                    newRow.OrganizationType = Enum.Parse<OrganizationType>(tmpValue.ToString());
                    tmpValue = reader[13];
                    newRow.IsBuyer = tmpValue.ToString() == "1";
                    tmpValue = reader[14];
                    newRow.Status = Enum.Parse<OrganizationStatus>(tmpValue.ToString());
                    tmpValue = reader[15];
                    newRow.WeChatOrWhatsApp = DBNull.Value == tmpValue ? null : tmpValue.ToString();

                    mappedData.Add(newRow);
                }

                return mappedData;
            };
            var result = await _dataQuery.GetDataByStoredProcedureAsync(storedProcedureName, mapping);
            return result;
        }

        public async Task<IEnumerable<OrganizationCodeViewModel>> GetActiveCodesListExcludeIdsAsync(IEnumerable<long> excludeOrgIds)
        {
            var organizations = await Repository.QueryAsNoTracking(x => x.Status == OrganizationStatus.Active && !excludeOrgIds.Contains(x.Id)).ToListAsync();
            var result = Mapper.Map<IEnumerable<OrganizationModel>, IEnumerable<OrganizationCodeViewModel>>(organizations);
            return result;
        }

        public async Task<IEnumerable<OrganizationCodeViewModel>> GetActiveCodesListAsync()
        {
            var organizations = await Repository.QueryAsNoTracking(x => x.Status == OrganizationStatus.Active).ToListAsync();
            var result = Mapper.Map<IEnumerable<OrganizationModel>, IEnumerable<OrganizationCodeViewModel>>(organizations);
            return result;
        }

        public async Task<bool> CheckCustomerPrefixNotTaken(long id, string customerPrefix)
        {
            if (string.IsNullOrEmpty(customerPrefix))
            {
                return true;
            }

            var organization = await Repository.GetAsNoTrackingAsync(o => o.CustomerPrefix.Equals(customerPrefix));

            if (organization == null)
            {
                return true;
            }

            if (organization.Id == id)
            {
                return true;
            }

            return false;
        }

        public async Task<bool> CheckContactEmailExists(long id)
        {
            var organization = await Repository.GetAsNoTrackingAsync(o => o.Id == id);

            if (organization == null)
            {
                return false;
            }

            if (String.IsNullOrEmpty(organization.ContactEmail))
            {
                return false;
            }

            return true;
        }

        public async Task<IEnumerable<OrganizationViewModel>> GetAffiliatesAsync(long organizationId)
        {
            var organization = await Repository.GetAsNoTrackingAsync(x => x.Id == organizationId);
            var parentIdMerge = organization.GetQueryParentIdString();
            var affiliates = await Repository.QueryAsNoTracking(x => x.ParentId.Equals(parentIdMerge),
                                    x => x.OrderBy(y => y.UpdatedDate).ThenBy(y => y.Name), IncludeProperties).ToListAsync();
            var results = Mapper.Map<IEnumerable<OrganizationModel>, IEnumerable<OrganizationViewModel>>(affiliates);
            return results;
        }

        public async Task<IEnumerable<OrganizationViewModel>> GetAffiliatesAsync(string organizationIds)
        {
            var ids = organizationIds.Split(";").Select(c => Int64.Parse(c)).ToList();
            var organizations = await Repository.QueryAsNoTracking(x => ids.Contains(x.Id)).ToListAsync();
            List<string> parentIdMerges = new List<string>();

            foreach (var item in organizations)
            {
                parentIdMerges.Add(item.GetQueryParentIdString());
            }

            var affiliates = await Repository.QueryAsNoTracking(x => parentIdMerges.Contains(x.ParentId)).ToListAsync();
            var results = Mapper.Map<IEnumerable<OrganizationModel>, IEnumerable<OrganizationViewModel>>(affiliates);
            return results;
        }

        public async Task<IEnumerable<long>> GetAffiliateCodesAsync(long organizationId)
        {
            var organization = await Repository.GetAsNoTrackingAsync(x => x.Id == organizationId);
            var parentIdMerge = organization.GetQueryParentIdString();
            var results = await Repository.QueryAsNoTracking(x => x.ParentId.StartsWith(parentIdMerge)).Select(x => x.Id).ToListAsync();
            results.Add(organization.Id);
            return results;
        }

        public async Task<OrganizationViewModel> AddAffiliateAsync(long organizationId, long affiliateId, string userName)
        {
            try
            {
                var organizationModel = await Repository.GetAsync(x => x.Id == organizationId);
                if (organizationModel == null)
                {
                    throw new AppException("organizationNotFound");
                }

                var affiliateModel = await Repository.GetAsync(x => x.Id == affiliateId && x.Status == OrganizationStatus.Active);
                if (affiliateModel == null)
                {
                    throw new AppException("affiliateNotFound");
                }

                // current node
                affiliateModel.ParentId = organizationModel.GetQueryParentIdString();
                affiliateModel.Audit(userName);

                // add parentId for all child node
                var childAffiliateList = await Repository.Query(x => x.ParentId.StartsWith(affiliateModel.Id + AppConstants.DELIMITER_PARENT_ID)).ToListAsync();
                foreach (var child in childAffiliateList)
                {
                    child.ParentId = affiliateModel.ParentId + child.ParentId;
                    child.Audit(userName);
                }

                await this.UnitOfWork.SaveChangesAsync();
                return Mapper.Map<OrganizationModel, OrganizationViewModel>(affiliateModel);
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

        public override async Task<OrganizationViewModel> CreateAsync(OrganizationViewModel viewModel)
        {
            viewModel.ValidateAndThrow();

            // Refer to link: https://en.wikipedia.org/wiki/CJK_Unified_Ideographs_(Unicode_block)
            // CJK Unified Ideographs is a Unicode block containing the most common CJK ideographs used in modern Chinese, Japanese, Korean and Vietnamese characters

            // Not allowed Chinese wording
            Regex cjkCharRegex = new Regex(@"\p{IsCJKUnifiedIdeographs}");

            //If Organization type is General then system will auto generate Organization code
            if (viewModel.OrganizationType == OrganizationType.General)
            {
                viewModel.Code = await _organizationRepository.GetNextNumberAsync();
            }
            else
            {
               
                if (cjkCharRegex.IsMatch(viewModel.Code))
                {
                    throw new AppValidationException($"#OrgCodeInvalidChinese# Organization Code {viewModel.Code} doesn't allow Chinese wording.");
                }

                var isOrganizationAlreadyExist = await _organizationRepository.AnyAsync(c => c.Code == viewModel.Code);
                if (isOrganizationAlreadyExist == true)
                {
                    throw new AppValidationException($"#OrgCodeDuplicated# Organization Code {viewModel.Code} already exists.");
                }

            }

            if (!string.IsNullOrEmpty(viewModel.CustomerPrefix))
            {
                if (cjkCharRegex.IsMatch(viewModel.CustomerPrefix))
                {
                    throw new AppValidationException($"#CustomerPrefixInvalidChinese# Customer Prefix {viewModel.CustomerPrefix} doesn't allow Chinese wording.");
                }
            }

            var model = Mapper.Map<OrganizationModel>(viewModel);

            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();

            OnEntityCreated(model);

            viewModel = Mapper.Map<OrganizationViewModel>(model);
            return viewModel;
        }

        public async Task<int> BulkInsertOrganizationsAsync(IEnumerable<BulkInsertOrganizationViewModel> viewModel)
        {

            var generalOrganizations = viewModel.Where(x => x.OrganizationType == (int)OrganizationType.General).ToList();
            var currentGeneralOrganizationCodeSequence = 0;
            // Auto fulfill organization code for General Orgs
            if (generalOrganizations != null && generalOrganizations.Any())
            {
                var generalOrgCount = generalOrganizations.Count();
                currentGeneralOrganizationCodeSequence = await _organizationRepository.ReserveSequenceNumberAsync(generalOrgCount);

                for (int i = 0; i < generalOrgCount; i++)
                {
                    var organizationViewModel = generalOrganizations[i];
                    organizationViewModel.Code = $"ORG{currentGeneralOrganizationCodeSequence + i:0000}";
                }
            }
            // Custom table data type
            var customDataType = "BulkInsertOrganizationTableType";
            var sql = @"
                    INSERT INTO Organizations (
                        [CreatedBy]
	                    ,[CreatedDate]
	                    ,[UpdatedBy]
	                    ,[UpdatedDate]
	                    ,[Code]
	                    ,[Name]
	                    ,[ContactEmail]
	                    ,[ContactName]
	                    ,[ContactNumber]
	                    ,[Address]
	                    ,[EdisonInstanceId]
	                    ,[EdisonCompanyCodeId]
	                    ,[CustomerPrefix]
	                    ,[LocationId]
	                    ,[OrganizationType]
	                    ,[Status]
	                    ,[AdminUser]
	                    ,[WebsiteDomain]
	                    ,[AddressLine2]
	                    ,[AddressLine3]
	                    ,[AddressLine4]
                        ,[TaxpayerId]
                        ,[WeChatOrWhatsApp]
                    )
    
                    SELECT [CreatedBy]
	                    ,[CreatedDate]
	                    ,[UpdatedBy]
	                    ,[UpdatedDate]
	                    ,[Code]
	                    ,[Name]
	                    ,[ContactEmail]
	                    ,[ContactName]
	                    ,[ContactNumber]
	                    ,[Address]
	                    ,[EdisonInstanceId]
	                    ,[EdisonCompanyCodeId]
	                    ,[CustomerPrefix]
	                    ,[LocationId]
	                    ,[OrganizationType]
	                    ,[Status]
	                    ,[AdminUser]
	                    ,[WebsiteDomain]
	                    ,[AddressLine2]
	                    ,[AddressLine3]
	                    ,[AddressLine4]
                        ,[TaxpayerId]
                        ,[WeChatOrWhatsApp]
                    from @importDataTable

                    SELECT @@ROWCOUNT";

            var dtTable = new DataTable();
            dtTable.Columns.Add("CreatedBy", typeof(string));
            dtTable.Columns.Add("CreatedDate", typeof(DateTime));
            dtTable.Columns.Add("UpdatedBy", typeof(string));
            dtTable.Columns.Add("UpdatedDate", typeof(DateTime));
            dtTable.Columns.Add("Code", typeof(string));
            dtTable.Columns.Add("Name", typeof(string));
            dtTable.Columns.Add("ContactEmail", typeof(string));
            dtTable.Columns.Add("ContactName", typeof(string));
            dtTable.Columns.Add("ContactNumber", typeof(string));
            dtTable.Columns.Add("Address", typeof(string));
            dtTable.Columns.Add("EdisonInstanceId", typeof(string));
            dtTable.Columns.Add("EdisonCompanyCodeId", typeof(string));
            dtTable.Columns.Add("CustomerPrefix", typeof(string));
            dtTable.Columns.Add("LocationId", typeof(long));
            dtTable.Columns.Add("OrganizationType", typeof(int));
            dtTable.Columns.Add("Status", typeof(int));
            dtTable.Columns.Add("AdminUser", typeof(string));
            dtTable.Columns.Add("WebsiteDomain", typeof(string));
            dtTable.Columns.Add("AddressLine2", typeof(string));
            dtTable.Columns.Add("AddressLine3", typeof(string));
            dtTable.Columns.Add("AddressLine4", typeof(string));
            dtTable.Columns.Add("TaxpayerId", typeof(string));
            dtTable.Columns.Add("WeChatOrWhatsApp", typeof(string));

            foreach (var organizationViewModel in viewModel)
            {
                dtTable.Rows.Add(organizationViewModel.CreatedBy,
                    organizationViewModel.CreatedDate,
                    organizationViewModel.CreatedBy,
                    organizationViewModel.CreatedDate,
                    organizationViewModel.Code,
                    organizationViewModel.Name,
                    organizationViewModel.ContactEmail,
                    organizationViewModel.ContactName,
                    organizationViewModel.ContactNumber,
                    organizationViewModel.Address,
                    organizationViewModel.EdisonInstanceId,
                    organizationViewModel.EdisonCompanyCodeId,
                    organizationViewModel.CustomerPrefix,
                    organizationViewModel.LocationId,
                    organizationViewModel.OrganizationType,
                    OrganizationStatus.Active,
                    organizationViewModel.ContactEmail,
                    organizationViewModel.WebDomain,
                    organizationViewModel.AddressLine2,
                    organizationViewModel.AddressLine3,
                    organizationViewModel.AddressLine4,
                    organizationViewModel.TaxpayerId,
                    organizationViewModel.WeChatOrWhatsApp
                    );
            }

            var result = await _dataQuery.BulkInsertDataWithCustomDataTypeAsync(sql, dtTable, customDataType);
            return int.Parse(result.ToString());
        }

        public async Task<OrganizationViewModel> UpdateAsync(long id, OrganizationViewModel viewModel, string userName)
        {
            viewModel.ValidateAndThrow(true);

            // get OrganizationInRoles to compare
            var model = await Repository.GetAsync(x => x.Id == id, null, IncludeProperties);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            if (viewModel.IsPropertyDirty("Code"))
            {
                model.Code = viewModel.Code;
            }

            if (viewModel.IsPropertyDirty("Name"))
            {
                model.Name = viewModel.Name;
            }

            if (viewModel.IsPropertyDirty("ContactEmail"))
            {
                model.ContactEmail = viewModel.ContactEmail;
            }

            if (viewModel.IsPropertyDirty("ContactName"))
            {
                model.ContactName = viewModel.ContactName;
            }

            if (viewModel.IsPropertyDirty("WebsiteDomain"))
            {
                model.WebsiteDomain = viewModel.WebsiteDomain;
            }

            if (viewModel.IsPropertyDirty("ContactNumber"))
            {
                model.ContactNumber = viewModel.ContactNumber;
            }

            if (viewModel.IsPropertyDirty("Address"))
            {
                model.Address = viewModel.Address;
            }

            if (viewModel.IsPropertyDirty("AddressLine2"))
            {
                model.AddressLine2 = viewModel.AddressLine2;
            }

            if (viewModel.IsPropertyDirty("AddressLine3"))
            {
                model.AddressLine3 = viewModel.AddressLine3;
            }

            if (viewModel.IsPropertyDirty("AddressLine4"))
            {
                model.AddressLine4 = viewModel.AddressLine4;
            }

            if (viewModel.IsPropertyDirty("EdisonInstanceId"))
            {
                model.EdisonInstanceId = viewModel.EdisonInstanceId;
            }

            if (viewModel.IsPropertyDirty("EdisonCompanyCodeId"))
            {
                model.EdisonCompanyCodeId = viewModel.EdisonCompanyCodeId;
            }

            if (viewModel.IsPropertyDirty("LocationId"))
            {
                model.LocationId = viewModel.LocationId;
            }

            if (viewModel.IsPropertyDirty("ParentId"))
            {
                model.ParentId = viewModel.ParentId;
            }

            if (viewModel.IsPropertyDirty("Status"))
            {
                model.Status = viewModel.Status;
            }

            if (viewModel.IsPropertyDirty("OrganizationType"))
            {
                model.OrganizationType = viewModel.OrganizationType;
            }

            if (viewModel.IsPropertyDirty("AgentType"))
            {
                model.AgentType = viewModel.AgentType;
            }

            if (viewModel.IsPropertyDirty("SOFormGenerationFileType"))
            {
                model.SOFormGenerationFileType = viewModel.SOFormGenerationFileType;
            }

            if (viewModel.IsPropertyDirty("CustomerPrefix"))
            {
                model.CustomerPrefix = viewModel.CustomerPrefix;
            }

            if (viewModel.IsPropertyDirty("IsBuyer"))
            {
                model.IsBuyer = viewModel.IsBuyer;
            }

            if (viewModel.IsPropertyDirty("OrganizationLogo"))
            {
                model.OrganizationLogo = viewModel.OrganizationLogo;
            }

            if (viewModel.IsPropertyDirty("RemoveCustomerIds"))
            {
                if (viewModel.RemoveCustomerIds.Any())
                {
                    var customerRelationshipList = model.CustomerRelationship.Where(x => viewModel.RemoveCustomerIds.Contains(x.CustomerId));
                    foreach (var customerRelationship in customerRelationshipList.ToList())
                    {
                        model.CustomerRelationship.Remove(customerRelationship);
                    }
                }
            }

            if (viewModel.IsPropertyDirty("PendingCustomerIds"))
            {
                if (viewModel.PendingCustomerIds.Any())
                {
                    var pendingList = model.CustomerRelationship.Where(x => x.ConnectionType == ConnectionType.Pending && x.IsConfirmConnectionType);
                    foreach (var item in pendingList)
                    {
                        item.ConnectionType = ConnectionType.Active;
                    }
                }
            }

            if (viewModel.IsPropertyDirty("TaxpayerId"))
            {
                model.TaxpayerId = viewModel.TaxpayerId;
            }

            if (viewModel.IsPropertyDirty("WeChatOrWhatsApp"))
            {
                model.WeChatOrWhatsApp = viewModel.WeChatOrWhatsApp;
            }

            model.Audit(userName);

            // affiliate
            if (viewModel.RemoveAffiliateIds?.Count() > 0)
            {
                var affiliateList = await Repository.Query(x => viewModel.RemoveAffiliateIds.Contains(x.Id) && !string.IsNullOrEmpty(x.ParentId)).ToListAsync();
                foreach (var affiliate in affiliateList)
                {
                    await RemoveAffiliate(affiliate, userName);
                }
            }

            await this.UnitOfWork.SaveChangesAsync();
            return Mapper.Map<OrganizationModel, OrganizationViewModel>(model);
        }

        public async Task<OrganizationViewModel> UpdateBuyerAsync(OrganizationViewModel viewModel, string userName)
        {
            // get OrganizationInRoles to compare
            var model = await Repository.GetAsync(x => x.Id == viewModel.Id);
            model.Audit(userName);
            model.IsBuyer = viewModel.IsBuyer;
            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();
            return Mapper.Map<OrganizationModel, OrganizationViewModel>(model);
        }

        public async Task<OrganizationViewModel> UpdateAdminUserAsync(OrganizationViewModel viewModel, string userName)
        {
            // get OrganizationInRoles to compare
            var model = await Repository.GetAsync(x => x.Id == viewModel.Id);
            model.Audit(userName);
            model.AdminUser = viewModel.AdminUser;
            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();
            return Mapper.Map<OrganizationModel, OrganizationViewModel>(model);
        }

        public async Task<OrganizationViewModel> RemoveAffiliateAsync(long affiliateId, string userName)
        {
            try
            {
                var affiliateModel = await Repository.GetAsync(x => x.Id == affiliateId && !string.IsNullOrEmpty(x.ParentId));
                if (affiliateModel == null)
                {
                    throw new AppException("organizationIdNotFound");
                }
                await RemoveAffiliate(affiliateModel, userName);
                await this.UnitOfWork.SaveChangesAsync();
                return Mapper.Map<OrganizationModel, OrganizationViewModel>(affiliateModel);
            }
            catch (Exception ex)
            {
                throw new AppException(ex.Message);
            }
        }

        private async Task RemoveAffiliate(OrganizationModel affiliate, string userName)
        {
            var parentIdLength = affiliate.ParentId.Length;
            var parentIdMerge = affiliate.GetQueryParentIdString();
            affiliate.ParentId = string.Empty;
            affiliate.Audit(userName);
            // remove parentId for all child node
            var childAffiliateList = await Repository.Query(x => x.ParentId.StartsWith(parentIdMerge)).ToListAsync();
            foreach (var child in childAffiliateList)
            {
                child.ParentId = child.ParentId.Remove(0, parentIdLength);
                child.Audit(userName);
            }
        }

        public async Task<bool> HasCustomerPrefix(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(x => x.Id == id);
            if (model != null)
            {
                return !string.IsNullOrEmpty(model.CustomerPrefix);
            }
            return false;
        }

        #region Customers

        public async Task<IEnumerable<OrganizationViewModel>> GetCustomersAsync(long organizationId)
        {
            var organization = await this.Repository.GetAsNoTrackingAsync(x => x.Id == organizationId, null, x => x.Include(m => m.CustomerRelationship));
            var customerIds = organization.CustomerRelationship.Select(x => x.CustomerId);
            var customers = await Repository.QueryAsNoTracking(x => customerIds.Contains(x.Id), null, x => x.Include(m => m.Location).ThenInclude(m => m.Country)).ToListAsync();

            return Mapper.Map<IEnumerable<OrganizationViewModel>>(customers);
        }

        public async Task AddCustomerAsync(long supplierId, long customerId, ConnectionType connectionType, string userName)
        {
            var organization = await this.Repository.GetAsync(x => x.Id == supplierId, null, IncludeProperties);

            organization.CustomerRelationship.Add(
                new CustomerRelationshipModel
                {
                    SupplierId = supplierId,
                    CustomerId = customerId,
                    ConnectionType = connectionType
                }
            );

            organization.Audit(userName);
            this.Repository.Update(organization);
            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task RemoveCustomerAsync(long supplierId, long customerId, string userName)
        {
            var organization = await this.Repository.GetAsync(x => x.Id == supplierId, null, IncludeProperties);
            var customerRelationship = organization.CustomerRelationship.FirstOrDefault(x => x.CustomerId == customerId);

            if (customerRelationship != null)
            {
                organization.CustomerRelationship.Remove(customerRelationship);
            }

            organization.Audit(userName);
            this.Repository.Update(organization);
            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<OrganizationViewModel>> GetBuyersAsync()
        {
            var buyers = await Repository.QueryAsNoTracking(x => x.IsBuyer,
                null,
                x => x.Include(m => m.Location).ThenInclude(m => m.Country)).ToListAsync();

            return Mapper.Map<IEnumerable<OrganizationViewModel>>(buyers);
        }

        public async Task<IEnumerable<OrganizationViewModel>> GetBuyersAsync(long organizationId)
        {
            var organization = await this.Repository.GetAsNoTrackingAsync(x => x.Id == organizationId, null, x => x.Include(m => m.CustomerRelationship));
            var customerIds = organization.CustomerRelationship.Where(x => x.ConnectionType == ConnectionType.Active).Select(x => x.CustomerId);
            var buyers = await Repository.QueryAsNoTracking(x => customerIds.Contains(x.Id) && x.IsBuyer,
                null,
                x => x.Include(m => m.Location).ThenInclude(m => m.Country)).ToListAsync();

            return Mapper.Map<IEnumerable<OrganizationViewModel>>(buyers);
        }

        public async Task ResendConnectionToCustomer(long supplierId, long customerId, string userName)
        {
            var supplier = await this.Repository.GetAsync(x => x.Id == supplierId, null, x => x.Include(m => m.CustomerRelationship).ThenInclude(m => m.Customer));
            var customerRelationship = supplier.CustomerRelationship.FirstOrDefault(x => x.CustomerId == customerId);

            if (customerRelationship != null)
            {
                customerRelationship.ConnectionType = ConnectionType.Pending;
                customerRelationship.IsConfirmConnectionType = false;
            }
            this.Repository.Update(supplier);
            await this.UnitOfWork.SaveChangesAsync();

            var emailModel = new OrganizationResendEmailViewModel()
            {
                Name = supplier.Name,
                ContactName = customerRelationship.Customer.ContactName,
                DetailPage = $"{_appConfig.ClientUrl}/login?type=ex",
                SupportEmail = _appConfig.SupportEmail
            };
            await this._sendMailService.SendMailAsync($"Connect to Organization #{supplier.Id}", "ResendConnectionEmail", emailModel, customerRelationship.Customer.ContactEmail, "Shipment Portal: Connect to Organization");
        }

        public async Task ResendConnectionToSupplier(long supplierId, long customerId, string userName)
        {
            var customer = await this.Repository.GetAsync(x => x.Id == customerId);
            var supplier = await this.Repository.GetAsync(x => x.Id == supplierId, null, x => x.Include(m => m.CustomerRelationship));
            var supplierRelationship = supplier.CustomerRelationship.Where(x => x.CustomerId == customerId).FirstOrDefault();
            if (supplierRelationship != null)
            {
                supplierRelationship.ConnectionType = ConnectionType.Pending;
                supplierRelationship.IsConfirmConnectionType = false;
            }
            await this.UnitOfWork.SaveChangesAsync();

            var emailModel = new OrganizationResendEmailViewModel()
            {
                Name = customer.Name,
                ContactName = supplier.ContactName,
                DetailPage = $"{_appConfig.ClientUrl}/login?type=ex",
                SupportEmail = _appConfig.SupportEmail
            };
            await this._sendMailService.SendMailAsync($"Connect to Organization #{supplier.Id}", "ResendConnectionEmail", emailModel, supplier.ContactEmail, "Shipment Portal: Connect to Organization");
        }
        #endregion

        #region Suppliers

        public async Task<IEnumerable<SupplierCustomerRelationshipViewModel>> GetCustomerRelationShips(IEnumerable<long> affiliateCodes, ConnectionType connectionType)
        {
            var result = await
                _customerRelationshipRepository.GetListQueryable()
                .Where(x => affiliateCodes.Contains(x.SupplierId) && x.ConnectionType.Equals(connectionType))
                .Select(
                x => new SupplierCustomerRelationshipViewModel
                {
                    CustomerId = x.CustomerId,
                    SupplierId = x.SupplierId
                })
                .ToListAsync();
            return result;

        }

        private async Task CreateShipperUser(StringValues authHeader, OrganizationModel organization)
        {
            var httpClient = _httpClientFactory.CreateClient();

            var viewModel = new
            {
                Username = organization.ContactEmail,
                Email = organization.ContactEmail,
                Name = organization.ContactName,
                Phone = organization.ContactNumber,
                CompanyName = organization.Name,
                OrganizationId = organization.Id,
                OrganizationName = organization.Name,
                OrganizationType = organization.OrganizationType,
                UserRoles = new List<dynamic> { new
                    {
                        // shipper
                        RoleId = 9
                    }
                }
            };
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _appConfig.CreateExternalUserEndpoint)
            {
                Content = new StringContent(JsonConvert.SerializeObject(viewModel), Encoding.UTF8, "application/json"),

            };
            httpRequestMessage.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);
            await httpClient.SendAsync(httpRequestMessage);
        }

        private async Task CreateSupplierRelationship(long customerId, OrganizationModel supplier, bool isApplyAffiliates)
        {
            var customer = await this.Repository.GetAsync(x => x.Id == customerId);
            supplier.CustomerRelationship.Add(
                new CustomerRelationshipModel
                {
                    CustomerId = customerId,
                    SupplierId = supplier.Id,
                    ConnectionType = ConnectionType.Active
                }
            );

            // Add relationship from Supplier to Customer's affiliates
            if (isApplyAffiliates)
            {
                var parentIdMerge = customer.GetQueryParentIdString();
                var affiliateIds = await Repository.Query(x => x.ParentId.StartsWith(parentIdMerge)).Select(x => x.Id).ToListAsync();
                var supplierRelationships = await _customerRelationshipRepository.Query(x => affiliateIds.Contains(x.CustomerId)).ToListAsync();

                foreach (var affilidateId in affiliateIds)
                {
                    var isExisted = supplierRelationships
                         .Any(e => e.SupplierId == supplier.Id && e.CustomerId == affilidateId);
                    if (!isExisted)
                    {
                        supplier.CustomerRelationship.Add(
                            new CustomerRelationshipModel
                            {
                                CustomerId = affilidateId,
                                SupplierId = supplier.Id,
                                ConnectionType = ConnectionType.Active
                            }
                        );
                    }
                }
            }
        }

        public async Task<OrganizationViewModel> AddSupplierAsync(long customerId,
            SupplierViewModel supplierViewModel,
            string userName,
            StringValues authHeader)
        {
            var model = Mapper.Map<OrganizationModel>(supplierViewModel);
            model.Code = await _organizationRepository.GetNextNumberAsync();
            model.OrganizationType = OrganizationType.General;
            model.Status = OrganizationStatus.Active;
            model.IsBuyer = false;
            model.AdminUser = model.ContactEmail;
            model.Audit(userName);
            model.CustomerRelationship = new List<CustomerRelationshipModel>();

            await CreateSupplierRelationship(customerId, model, supplierViewModel.IsApplyAffiliates);
            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();
            await CreateShipperUser(authHeader, model);

            return Mapper.Map<OrganizationModel, OrganizationViewModel>(model);
        }

        public async Task UpdateSupplierAsync(long customerId,
            long supplierId,
            SupplierViewModel supplierViewModel,
            string userName,
            StringValues authHeader)
        {
            var supplier = await this.Repository.GetAsync(x => x.Id == supplierId,
                null,
                i => i.Include(x => x.CustomerRelationship));

            bool isCreateShipperUser = string.IsNullOrWhiteSpace(supplier.AdminUser);

            Mapper.Map(supplierViewModel, supplier);
            supplier.AdminUser = supplier.ContactEmail;
            supplier.Audit(userName);

            await CreateSupplierRelationship(customerId, supplier, supplierViewModel.IsApplyAffiliates);
            await this.UnitOfWork.SaveChangesAsync();

            if (isCreateShipperUser)
            {
                await CreateShipperUser(authHeader, supplier);
            }
        }

        public async Task<List<OrganizationCodeViewModel>> UpdateSupplierStatusAsync(long supplierId, string userName)
        {
            var pendingList = new List<OrganizationCodeViewModel>();
            var supplier = await Repository.GetAsync(x => x.Id == supplierId, null, x => x.Include(y => y.CustomerRelationship).ThenInclude(y => y.Customer));
            if (userName != supplier.AdminUser)
            {
                return pendingList;
            }

            // If user's organization status is Pending, update status to Active.
            if (supplier.Status == OrganizationStatus.Pending)
            {
                supplier.Status = OrganizationStatus.Active;
            }
            await this.UnitOfWork.SaveChangesAsync();

            pendingList = supplier.CustomerRelationship.Where(x => x.ConnectionType == ConnectionType.Pending && !x.IsConfirmConnectionType)
                .Select(x => new OrganizationCodeViewModel
                {
                    Name = x.Customer.Name,
                    Id = x.CustomerId
                }).ToList();
            return pendingList;
        }

        public async Task ConfirmConnectionTypeAsync(long supplierId, long customerId, bool isConfirm, string username)
        {
            var supplier = await this.Repository.GetAsync(x => x.Id == supplierId,
                null,
                i => i.Include(x => x.CustomerRelationship));
            var customerRelationship = supplier.CustomerRelationship.Where(x => x.CustomerId == customerId).FirstOrDefault();

            if (isConfirm)
            {
                if (!string.IsNullOrEmpty(supplier.EdisonCompanyCodeId) && !string.IsNullOrEmpty(supplier.EdisonInstanceId))
                {
                    customerRelationship.ConnectionType = ConnectionType.Active;
                }
                else
                {
                    customerRelationship.ConnectionType = ConnectionType.Pending;
                }
            }
            else
            {
                customerRelationship.ConnectionType = ConnectionType.Inactive;
            }
            customerRelationship.IsConfirmConnectionType = true;
            supplier.Audit(username);
            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task RemoveSupplierAsync(long customerId, long supplierId, string userName)
        {
            var supplier = await this.Repository.GetAsync(x => x.Id == supplierId,
                null,
                i => i.Include(x => x.CustomerRelationship));

            var customerRelationship = supplier.CustomerRelationship.FirstOrDefault(x => x.CustomerId == customerId);

            if (customerRelationship != null)
            {
                supplier.CustomerRelationship.Remove(customerRelationship);
            }

            supplier.Audit(userName);
            await this.UnitOfWork.SaveChangesAsync();
        }

        #endregion

        public async Task<IEnumerable<DropDownListItem>> GetPrincipalSelectionsAsync(bool isInternal, long roleId, long organizationId, string affiliates, bool checkIsBuyer = true)
        {
            if (Enum.TryParse<Role>(roleId.ToString(), out var currentRole))
            {
                var sql = "";
                switch (currentRole)
                {
                    case Role.Agent:
                    case Role.CSR:
                    case Role.SystemAdmin:
                        sql = $@"
                            SELECT Id AS [Value], [Name] AS [Text]
                            FROM Organizations 
                            WHERE OrganizationType = 4 {(checkIsBuyer ? "AND IsBuyer = 1" : "")} AND [status] = 1
                            ORDER BY [Text] ASC 
                            ";
                        break;
                    case Role.Shipper:
                        sql = @"
                            SELECT Id AS [Value], [Name] AS [Text]
                            FROM Organizations o
                            INNER JOIN CustomerRelationship cr on cr.CustomerId = o.Id
                            WHERE o.OrganizationType = 4 AND o.[status] = 1 AND cr.ConnectionType = 1 AND cr.SupplierId = " + organizationId + @"
                            ORDER BY [Text] ASC 
                            ";
                        break;

                    case Role.Principal:
                        var listOfAffiliates = "";
                        if (!string.IsNullOrEmpty(affiliates))
                        {
                            listOfAffiliates = string.Join(",", JsonConvert.DeserializeObject<List<long>>(affiliates));
                        }

                        sql = @"
                            SELECT Id AS [Value], [Name] AS [Text]
                            FROM Organizations 
                            WHERE OrganizationType = 4 AND [status] = 1 AND Id IN (" + listOfAffiliates + @")
                            ORDER BY [Text] ASC
                            ";

                        break;
                    case Role.Warehouse:
                        sql = $@"
                            SELECT o.Id AS [Value], o.[Name] AS [Text]
                            FROM WarehouseAssignments wa
                            INNER JOIN WarehouseLocations wl ON wa.WarehouseLocationId = wl.Id
                            INNER JOIN Organizations o ON o.Id = wa.OrganizationId
                            WHERE o.OrganizationType = 4 {(checkIsBuyer ? "AND IsBuyer = 1" : "")} AND o.[status] = 1 AND wl.OrganizationId = " + organizationId + @"
                            ORDER BY [Text] ASC
                            ";
                        break;
                    default:
                        return new List<DropDownListItem>();
                }

                Func<DbDataReader, IEnumerable<DropDownListItem>> mapping = (reader) =>
                {
                    var mappedData = new List<DropDownListItem>();

                    while (reader.Read())
                    {
                        var newRow = new DropDownListItem
                        {
                            Value = reader[0]?.ToString() ?? "",
                            Text = reader[1]?.ToString() ?? "",
                            Disabled = false
                        };
                        mappedData.Add(newRow);
                    }

                    return mappedData;
                };
                var result = _dataQuery.GetDataBySql(sql, mapping);
                return result;
            }
            return new List<DropDownListItem>();
        }

        public async Task<IEnumerable<DropDownListItem>> GetSupplierSelectionsByPrincipalAsync(long customerId)
        {
            var sql = @"SELECT Id AS [Value], IIF(cr.[CustomerRefId] IS NULL OR cr.[CustomerRefId] = '', o.[Name], CONCAT(o.[Name], ' (', cr.[CustomerRefId], ')')) AS [TEXT]
                        FROM Organizations o
                        INNER JOIN CustomerRelationship cr on cr.SupplierId = o.Id
                        WHERE o.OrganizationType = 1 AND o.[status] = 1 AND cr.ConnectionType = 1 AND cr.CustomerId = " + customerId + @"
                        ORDER BY [Text] ASC";

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

        public async Task<IEnumerable<long>> GetAccessibleOrganizationIdsForReportAsync(long roleId, long organizationId, string affiliates)
        {
            if (Enum.TryParse<Role>(roleId.ToString(), out var currentRole))
            {
                var sql = "";
                var listOfOrganizations = organizationId.ToString();
                switch (currentRole)
                {
                    case Role.CSR:
                    case Role.SystemAdmin:
                        // No need to return all organization as handled by role first
                        return new List<long>();
                    case Role.Shipper:
                        if (!string.IsNullOrEmpty(affiliates))
                        {
                            listOfOrganizations = string.Join(",", JsonConvert.DeserializeObject<List<long>>(affiliates));
                        }
                        sql = @"
                            SELECT Id AS [Value]
                            FROM Organizations o
                            WHERE o.[status] = 1 AND o.Id IN (
                                SELECT cr.CustomerId
                                FROM CustomerRelationship cr 
                                WHERE cr.ConnectionType = 1 AND cr.SupplierId IN (" + listOfOrganizations + @")
                            )
                            ORDER BY [Value] ASC 
                            ";
                        break;

                    case Role.Agent:
                    case Role.Principal:
                        if (!string.IsNullOrEmpty(affiliates))
                        {
                            listOfOrganizations = string.Join(",", JsonConvert.DeserializeObject<List<long>>(affiliates));
                        }

                        sql = @"
                            SELECT Id AS [Value]
                            FROM Organizations 
                            WHERE [status] = 1 AND Id IN (" + listOfOrganizations + @")
                            ORDER BY [Value] ASC
                            ";

                        break;

                    default:
                        return new List<long>();
                }

                Func<DbDataReader, IEnumerable<long>> mapping = (reader) =>
                {
                    var mappedData = new List<long>();

                    while (reader.Read())
                    {
                        var value = Convert.ToInt64(reader[0].ToString());
                        mappedData.Add(value);
                    }

                    return mappedData;
                };
                var result = _dataQuery.GetDataBySql(sql, mapping).ToList();
                // Add organization Id of current login
                if (!string.IsNullOrEmpty(affiliates))
                {
                    result.AddRange(JsonConvert.DeserializeObject<List<long>>(affiliates));
                }
                else
                {
                    result.Add(organizationId);
                }

                return result.Distinct();
            }
            return new List<long>();
        }

        public async Task<bool> ValidateReportPrincipal(long customerId, Role roleId, long requestingOrganizationId)
        {
            var sql = @"
                -- Default is invalid
                SET @result = 0;
            ";
            var filterParameter = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@customerId",
                    Value = customerId,
                    DbType = DbType.Int64,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@supplierId",
                    Value = requestingOrganizationId,
                    DbType = DbType.Int64,
                    Direction = ParameterDirection.Input
                }
            };

            switch (roleId)
            {
                case Role.Agent:
                    // Do not support on Agent role as it must be checking on SP AgentAssignments
                    return false;
                case Role.CSR:
                case Role.SystemAdmin:
                    sql = @"
                        SELECT @result = 1
                        FROM Organizations 
                        WHERE OrganizationType = 4 AND IsBuyer = 1 AND Status = 1 AND Id = @customerId";
                    break;
                case Role.Shipper:
                    sql = @"
                            SELECT @result = 1
                            FROM Organizations o
                            INNER JOIN CustomerRelationship cr on cr.CustomerId = o.Id
                            WHERE o.OrganizationType = 4 AND o.Status = 1 AND cr.ConnectionType = 1 
                            AND cr.CustomerId = @customerId AND cr.SupplierId = @supplierId";
                    break;

                case Role.Principal:
                    var affiliateIds = await GetAffiliateCodesAsync(requestingOrganizationId);

                    if (affiliateIds != null && affiliateIds.Any(affiliateId => affiliateId == customerId))
                    {
                        sql = @"
                            SELECT @result = 1
                            FROM Organizations 
                            WHERE OrganizationType = 4 AND Status = 1 AND Id = @customerId";
                    }
                    else
                    {
                        return false;
                    }

                    break;
                case Role.Warehouse:
                   
                        sql = @"
                            SELECT @result = 1
                            FROM WarehouseAssignments wa
                            INNER JOIN WarehouseLocations wl ON wa.WarehouseLocationId = wl.Id
                            WHERE wl.OrganizationId = @supplierId AND wa.OrganizationId = @customerId";
                    break;
                default:
                    return false;
            }

            Func<DbDataReader, IEnumerable<int>> mapping = (reader) =>
            {
                var mappedData = new List<int>();

                while (reader.Read())
                {
                    mappedData.Add(1);
                }

                return mappedData;
            };

            var result = _dataQuery.GetValueFromVariable(sql, filterParameter.ToArray()).Equals("1");
            return result;
        }

        private async Task<IEnumerable<OrganizationModel>> GetOrganizationCodeByParentIds(IEnumerable<long> parentIds)
        {
            var models = await Repository.QueryAsNoTracking(x => x.Status == OrganizationStatus.Active && parentIds.Contains(x.Id)).ToListAsync();
            var organizationOrderByParentIds = models.OrderBy(d => parentIds.ToList().IndexOf(d.Id)).ToList();
            return organizationOrderByParentIds;
        }


        public async Task AssignWarehouseLocationAsync(long customerOrgId, WarehouseAssignmentViewModel model, string userName)
        {
            var newWarehouseAssignment = new WarehouseAssignmentModel
            {
                OrganizationId = customerOrgId,
                WarehouseLocationId = model.WarehouseLocationId,
                ContactEmail = model.ContactEmail,
                ContactPerson = model.ContactPerson,
                ContactPhone = model.ContactPhone
            };
            newWarehouseAssignment.Audit(userName);
            await _warehouseAssignmentRepository.AddAsync(newWarehouseAssignment);

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task DeleteWarehouseLocationAsync(long customerOrgId, long warehouseLocationId)
        {
            var storedWarehouseAssignment = await _warehouseAssignmentRepository.GetAsync(x => x.OrganizationId == customerOrgId && x.WarehouseLocationId == warehouseLocationId);
            if (storedWarehouseAssignment == null)
            {
                throw new AppEntityNotFoundException($"Object not found!");
            }
            _warehouseAssignmentRepository.Remove(storedWarehouseAssignment);

            await UnitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<DropDownListItem<long>>> GetAgentOrganizationDropDownListAsync()
        {
            var result = Repository.QueryAsNoTracking(x => x.OrganizationType == OrganizationType.Agent && x.Status == OrganizationStatus.Active)
                .Select(
                    x => new DropDownListItem<long>
                    {
                        Text = x.Name,
                        Value = x.Id
                    }
                );
            return result;
        }

        public async Task<IEnumerable<DropDownModel<long>>> GetSelectionsAsync(OrganizationType? type = null)
        {
            var query = Repository.Query(x => x.Status == OrganizationStatus.Active);

            if (type.HasValue)
            {
                query = query.Where(x => x.OrganizationType == type);
            }

            var result = await query.Select(
                x => new DropDownModel<long>
                {
                    Label = $"{x.Code} - {x.Name}",
                    Value = x.Id

                }).ToListAsync();

            return result ?? new List<DropDownModel<long>>();
        }

        public async Task<bool> CheckAgentDomainAsync(string emailDomain)
        {
            if (string.IsNullOrEmpty(emailDomain))
            {
                return false;
            }
            if (!emailDomain.StartsWith('@'))
            {
                emailDomain = '@' + emailDomain;
            }
            var result = await Repository.AnyAsync(x => x.OrganizationType == OrganizationType.Agent && x.ContactEmail.EndsWith(emailDomain));
            return result;
        }

        public async Task<IEnumerable<SwitchOrganizationViewModel>> GetSwitchOrganizationSelectionsAsync(string organizationName)
        {
            var sql = @"
                        SELECT ORG.Id, ORG.Name, ORG.OrganizationType, WA.WHAssignment
                        FROM Organizations ORG
                        OUTER APPLY
                        (
	                        SELECT TOP(1) 1 AS [WHAssignment]
	                        FROM WarehouseAssignments WA WITH(NOLOCK)
	                        WHERE WA.OrganizationId = ORG.Id
                        ) WA


                        WHERE ORG.[Status] = 1 AND ORG.[Name] LIKE '%' + @organizationName + '%'
                        ORDER BY ORG.Name ";

            var filterParameters = new List<SqlParameter>
                {
                    new SqlParameter
                    {
                        ParameterName = "@organizationName",
                        Value = organizationName,
                        DbType = DbType.String,
                        Direction = ParameterDirection.Input
                    },
                };

            Func<DbDataReader, IEnumerable<SwitchOrganizationViewModel>> mapping = (reader) =>
            {
                var mappedData = new List<SwitchOrganizationViewModel>();

                while (reader.Read())
                {

                    var orgnizationId = (long)reader[0];
                    var orgnizationName = (string)reader[1];
                    var organizationType = (int)reader[2];
                    var isWarehouseAssignment = reader[3];

                    var newRow = new SwitchOrganizationViewModel
                    {
                        Id = orgnizationId,
                        Name = orgnizationName,
                        TypeId = (OrganizationType)organizationType,
                    };
                    switch (newRow.TypeId)
                    {
                        case OrganizationType.Principal:
                            newRow.RoleId = Role.Principal;
                            break;
                        case OrganizationType.General:
                            newRow.RoleId = Role.Shipper;
                            break;
                        case OrganizationType.Agent:
                            if (isWarehouseAssignment != DBNull.Value)
                            {
                                newRow.RoleId = Role.Warehouse;
                            }
                            else
                            {
                                newRow.RoleId = Role.Agent;
                            }
                            break;
                        default:
                            newRow.RoleId = Role.Guest;
                            break;
                    }
                    mappedData.Add(newRow);

                }

                return mappedData;
            };
            var result = _dataQuery.GetDataBySql(sql, mapping, filterParameters.ToArray());

            return result;

        }


        public async Task<bool> CheckParentInCustomerRelationshipAsync(long organizationId)
        {
            var organization = await Repository.GetAsNoTrackingAsync(x => x.Id == organizationId);

            if (organization != null)
            {
                // Seperated by dot (e.g. "546.545.537.")
                var parentId = organization.ParentId;

                if (!string.IsNullOrEmpty(parentId))
                {
                    var splitParentId = parentId.Trim().Split('.', StringSplitOptions.RemoveEmptyEntries).Select(id => long.Parse(id)).ToList();

                    var inCustomerRelationship = await _customerRelationshipRepository.AnyAsync(x => splitParentId.Contains(x.SupplierId));

                    return inCustomerRelationship;
                }
            }

            return false;
        }
    }
}