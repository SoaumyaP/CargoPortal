using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Core.Data;
using Groove.SP.Application.Exceptions;
using Groove.SP.Infrastructure.CSFE;
using System.Data.Common;
using System.Data.SqlClient;
using System.Data;
using Groove.SP.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using Groove.SP.Application.ContractMaster.ViewModels;

namespace Groove.SP.Application.MasterBillOfLading.Services
{
    public class ContractMasterService : ServiceBase<ContractMasterModel, ContractMasterViewModel>, IContractMasterService
    {
        private readonly IDataQuery _dataQuery;
        private readonly ICSFEApiClient _cSFEApiClient;

        public ContractMasterService(IUnitOfWorkProvider uow, ICSFEApiClient cSFEApiClient,
            IDataQuery dataQuery) : base(uow)
        {
            _dataQuery = dataQuery;
            _cSFEApiClient = cSFEApiClient;
        }

        public override async Task<DataSourceResult> ListAsync(DataSourceRequest request)
        {
            IQueryable<ContractMasterQueryModel> query;
            string sql;
            sql = @"
                    SELECT 
	                    CM.Id,
                        CM.CarrierContractNo,
                        CM.CarrierCode,
	                    C.CarrierName,
	                    CM.RealContractNo,
	                    CM.ContractType,
                        CM.ContractHolder,
                        CASE WHEN O.OrganizationName IS NULL THEN '' ELSE O.OrganizationName END OrganizationName,
                        CM.ColoaderCode,
	                    CM.ValidFrom ValidFromDate,
	                    CM.ValidTo ValidToDate,
	                    CM.Status,
                        CM.IsVip,
                        NULL AS CustomerContractType
                    FROM ContractMaster CM (NOLOCK)
                    OUTER APPLY 
                    (
	                    SELECT C.Name CarrierName
	                    FROM Carriers C
	                    WHERE C.CarrierCode = CM.CarrierCode
                    ) C
                    OUTER APPLY 
                    (
	                    SELECT O.Name OrganizationName
	                    FROM  Organizations O
	                    WHERE CAST(O.Id AS NVARCHAR(512)) = CM.ContractHolder
                    ) O
                ";

            query = _dataQuery.GetQueryable<ContractMasterQueryModel>(sql);
            ModifyFilters(request.Filters);
            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<ContractMasterQueryModel> GetByKeyAsync(long id)
        {
            string sql = @"
                    SELECT 
	                    CM.Id,
                        CM.CarrierContractNo,
                        CM.CarrierCode,
	                    C.CarrierName,
	                    CM.RealContractNo,
	                    CM.ContractType,
                        CM.ContractHolder,
                        O.OrganizationName,
                        CM.ColoaderCode,
	                    CM.ValidFrom ValidFromDate,
	                    CM.ValidTo ValidToDate,
	                    CM.Status,
                        CM.IsVip,
                        CM.CustomerContractType
                    FROM ContractMaster CM (NOLOCK)
                    OUTER APPLY 
                    (
	                    SELECT C.Name CarrierName
	                    FROM Carriers C
	                    WHERE C.CarrierCode = CM.CarrierCode
                    ) C
                    OUTER APPLY 
                    (
	                    SELECT O.Name OrganizationName
	                    FROM  Organizations O
	                    WHERE CAST(O.Id AS NVARCHAR(512)) = CM.ContractHolder
                    ) O
                    WHERE CM.Id = {0}
                ";

            var query = _dataQuery.GetQueryable<ContractMasterQueryModel>(sql, id).AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }

        public async Task<ContractMasterQueryModel> CreateAsync(ContractMasterQueryModel viewModel, string userName)
        {
            var model = Mapper.Map<ContractMasterModel>(viewModel);
            var currentContractKey = $"{AppConstant.ContractKey}{DateTime.UtcNow.ToString("yy")}{DateTime.UtcNow.ToString("MM")}";
            var latestContractByContractKey =
                    await Repository
                        .QueryAsNoTracking(c => c.CarrierContractNo.StartsWith(currentContractKey), a => a.OrderByDescending(s => s.CarrierContractNo))
                        .FirstOrDefaultAsync();
            var maxContractKey = Convert.ToInt64(latestContractByContractKey?.CarrierContractNo.Split(currentContractKey)[1]);
            model.CarrierContractNo = maxContractKey == 0 ? $"{currentContractKey}{1}" : $"{currentContractKey}{maxContractKey + 1}";
            model.Status = model.ValidTo.Date < DateTime.UtcNow.Date ? ContractMasterStatus.Inactive : ContractMasterStatus.Active;
            model.Audit(userName);
            await Repository.AddAsync(model);
            await UnitOfWork.SaveChangesAsync();
            viewModel.Id = model.Id;
            return viewModel;
        }

        public async Task<ContractMasterQueryModel> UpdateAsync(long id, ContractMasterQueryModel viewModel, string userName)
        {
            var model = await Repository.FindAsync(id);
            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }
            Mapper.Map(viewModel, model);
            model.Audit(userName);
            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();
            return viewModel;
        }

        public async Task<IEnumerable<DropDownListItem<string>>> GetMasterBOLContractMasterOptionsAsync(string searchTerm, string carrierCode, DateTime currentDate, IdentityInfo currentUser)
        {
            IQueryable<DropDownListItem<string>> query;

            // Checking Shipper/Principal/Cruise Principal/Agent can ONLY see the value of Contract No if their OrganizationId is matched with ContractHolder 
            var checkingRoles = new[] { (long)Role.Shipper, (long)Role.Principal, (long)Role.CruisePrincipal, (long)Role.Agent };
            if (checkingRoles.Contains(currentUser.UserRoleId))
            {
                query = Repository.QueryAsNoTracking(x => x.RealContractNo.Contains(searchTerm)
                                                && (string.IsNullOrWhiteSpace(x.CarrierCode) || x.CarrierCode == carrierCode)
                                                && x.Status == ContractMasterStatus.Active
                                                && x.ValidFrom <= currentDate && currentDate <= x.ValidTo
                                                && x.ContractHolder == currentUser.OrganizationId.ToString())
                            .Select(x => new DropDownListItem<string>
                            {
                                Value = x.CarrierContractNo,
                                Text = x.RealContractNo
                            });
            }
            else
            {
                query = Repository.QueryAsNoTracking(x => x.RealContractNo.Contains(searchTerm)
                                                && (string.IsNullOrWhiteSpace(x.CarrierCode) || x.CarrierCode == carrierCode)
                                                && x.Status == ContractMasterStatus.Active
                                                && x.ValidFrom <= currentDate && currentDate <= x.ValidTo)
                            .Select(x => new DropDownListItem<string>
                            {
                                Value = x.CarrierContractNo,
                                Text = x.RealContractNo
                            });
            }
            var result = await query.ToListAsync();
            return result;
        }

        public async Task<IEnumerable<ContractMasterViewModel>> GetShipmentContractMasterOptionsAsync(string searchTerm, long shipmentId, DateTime currentDate, IdentityInfo currentUser)
        {
            var sql = $@"
                        -- SET NOCOUNT ON added to prevent extra result sets from interfering with SELECT statements.
                        SET NOCOUNT ON;

                        DECLARE @itineraryTbl AS TABLE (
	                        CarrierCode NVARCHAR(128)
                        )

                        -- Store related itineraries (the same ModeOfTransport with shipment) to a variable
                        -- [Carriers] is from view on local, external table on cloud
                        INSERT INTO @itineraryTbl (CarrierCode)
                        SELECT C.CarrierCode
                        FROM Shipments SM (NOLOCK) JOIN ConsignmentItineraries CI (NOLOCK) ON SM.Id = CI.ShipmentId 
                        JOIN Itineraries I ON CI.ItineraryId = I.Id AND SM.ModeOfTransport = I.ModeOfTransport
                        JOIN Carriers C ON I.CarrierName = C.[Name]
                        WHERE SM.Id = @shipmentId

                        SELECT CM.Id, CM.CarrierContractNo, CM.RealContractNo, CM.ContractType
                        FROM ContractMaster CM WITH(NOLOCK)
                        WHERE CM.[Status] = 1 -- Active
	                        AND CM.ValidFrom <= @currentDate AND @currentDate <= CM.ValidTo
                            AND (CM.CarrierCode = '' OR NOT EXISTS(SELECT 1 FROM @itineraryTbl) OR EXISTS (SELECT 1 FROM @itineraryTbl WHERE CarrierCode = CM.CarrierCode))
	                        AND CM.RealContractNo LIKE CONCAT('%', @searchTerm, '%')
            ";

            // Checking Shipper/Principal/Cruise Principal/Agent can ONLY see the value of Contract No if their OrganizationId is matched with ContractHolder 
            var checkingRoles = new[] { (long)Role.Shipper, (long)Role.Principal, (long)Role.CruisePrincipal, (long)Role.Agent };
            if (checkingRoles.Contains(currentUser.UserRoleId))
            {
                sql += @"
                            AND CM.ContractHolder = CAST (@organizationId AS NVARCHAR(512))
                        ";
            }


            var filterParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@searchTerm",
                    Value = searchTerm,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@currentDate",
                    Value = currentDate,
                    DbType = DbType.DateTime2,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@shipmentId",
                    Value = shipmentId,
                    DbType = DbType.Int64,
                    Direction = ParameterDirection.Input
                },
                 new SqlParameter
                {
                    ParameterName = "@organizationId",
                    Value = currentUser.OrganizationId,
                    DbType = DbType.Int64,
                    Direction = ParameterDirection.Input
                }
            };

            IEnumerable<ContractMasterViewModel> mappingCallback(DbDataReader reader)
            {
                var mappedData = new List<ContractMasterViewModel>();

                // Map data for bill of lading information
                while (reader.Read())
                {
                    var id = reader["Id"];
                    var carrierContractNo = reader["CarrierContractNo"];
                    var realContractNo = reader["RealContractNo"];
                    var contractType = reader["ContractType"];

                    var newRow = new ContractMasterViewModel
                    {
                        Id = (long)id,
                        CarrierContractNo = carrierContractNo != DBNull.Value ? carrierContractNo.ToString() : null,
                        RealContractNo = realContractNo != DBNull.Value ? realContractNo.ToString() : null,
                        ContractType = contractType != DBNull.Value ? contractType.ToString() : null,

                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            }
            var result = _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
            return result;
        }

        public async Task<bool> CheckContractAlreadyExistsAsync(string contractNo)
        {
            return await Repository.AnyAsync(c => c.RealContractNo == contractNo);
        }

        public async Task UpdateStatusAsync(long id, ContractMasterStatus status, string userName)
        {
            var model = await Repository.FindAsync(id);
            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }
            model.Status = status;
            model.Audit(userName);
            Repository.Update(model);
            await UnitOfWork.SaveChangesAsync();
        }

        public async Task AutoExpireStatusAsync()
        {
            var contracts = await Repository.Query(c => c.Status == ContractMasterStatus.Active && c.ValidTo < DateTime.Today).ToListAsync();
            if (contracts?.Count > 0)
            {
                foreach (var contract in contracts)
                {
                    contract.Status = ContractMasterStatus.Inactive;
                }

                Repository.UpdateRange(contracts.ToArray());
                await UnitOfWork.SaveChangesAsync();
            }
        }

        public async Task RemapContractHolderAsync(ContractMasterViewModel viewModel)
        {
            var orgs = await _cSFEApiClient.GetOrganizationsByEdisonCompanyIdCodesAsync(viewModel.ContractHolder);

            if (orgs.Count() > 0)
            {
                viewModel.ContractHolder = orgs.First().Id.ToString();
            }
        }
    }
}
