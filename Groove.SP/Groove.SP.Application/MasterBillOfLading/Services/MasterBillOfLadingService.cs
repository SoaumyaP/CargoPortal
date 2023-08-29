using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

using Newtonsoft.Json;
using Groove.SP.Application.Common;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.MasterBillOfLading.Services.Interfaces;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
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

namespace Groove.SP.Application.MasterBillOfLading.Services
{
    public class MasterBillOfLadingService : ServiceBase<MasterBillOfLadingModel, MasterBillOfLadingViewModel>, IMasterBillOfLadingService
    {
        private readonly AppConfig _appConfig;
        private readonly IDataQuery _dataQuery;
        private readonly ICSFEApiClient _csfeApiClient;
        /// <summary>
        /// House of ladings
        /// </summary>
        private readonly IRepository<BillOfLadingModel> _billOfLadingRepository;
        private readonly IContractMasterRepository _contractMasterRepository;
        private readonly IRepository<ShipmentModel> _shipmentRepository;


        public MasterBillOfLadingService(IUnitOfWorkProvider uow,
            IOptions<AppConfig> appConfig,
            ICSFEApiClient csfeApiClient,
            IDataQuery dataQuery,
            IRepository<BillOfLadingModel> billOfLadingRepository,
            IContractMasterRepository contractMasterRepository,
            IRepository<ShipmentModel> shipmentRepository) : base(uow)
        {
            _appConfig = appConfig.Value;
            _dataQuery = dataQuery;
            _csfeApiClient = csfeApiClient;
            _billOfLadingRepository = billOfLadingRepository;
            _contractMasterRepository = contractMasterRepository;
            _shipmentRepository = shipmentRepository;
        }

        public async Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, long userRoleId, long organizationId, string affiliates)
        {
            IQueryable<MasterBillOfLadingQueryModel> query;
            string sql = "";
            if (isInternal)
            {
                sql =
                    @"SELECT MB.Id,
                        CASE MB.ModeOfTransport WHEN 'Air' THEN STUFF(MB.MasterBillOfLadingNo, 4, 0, '-')
						     ELSE MB.MasterBillOfLadingNo
                        END AS MasterBillOfLadingNo,
                        MB.CarrierName,
                        CM.RealContractNo,
                        CM.ContractType,
                        MB.OnBoardDate,
                        CM.ContractHolder
                     FROM MasterBills MB
                     OUTER APPLY (
	                                    SELECT TOP(1) RealContractNo, ContractType, ContractHolder
	                                    FROM ContractMaster CM WITH (NOLOCK)
	                                    WHERE CM.CarrierContractNo = MB.CarrierContractNo
                                    ) CM
                    ";
            }
            else
            {
                var listOfAffiliates = new List<long>();
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                var checkingRoles = new[] { (long)Role.Shipper, (long)Role.Principal, (long)Role.CruisePrincipal, (long)Role.Agent };
                if (checkingRoles.Contains(userRoleId))
                {
                    sql =
                     @"SELECT MB.Id,
                        CASE MB.ModeOfTransport WHEN 'Air' THEN STUFF(MB.MasterBillOfLadingNo, 4, 0, '-')
						     ELSE MB.MasterBillOfLadingNo
                        END AS MasterBillOfLadingNo,
                        MB.CarrierName,
                        IIF(ContractHolder = '" + organizationId + @"', CM.RealContractNo, '" + AppConstant.DefaultValue2Hyphens + @"') AS RealContractNo,
                        CM.ContractType,
                        MB.OnBoardDate,
                        CM.ContractHolder
                    FROM MasterBills MB
                    OUTER APPLY (
	                                    SELECT TOP(1) RealContractNo, ContractType, ContractHolder
	                                    FROM ContractMaster CM WITH (NOLOCK)
	                                    WHERE CM.CarrierContractNo = MB.CarrierContractNo
                                    ) CM
                    WHERE EXISTS 
	                (
		                SELECT 1
                        FROM MasterBillContacts MC" +
                    $"  WHERE MB.Id = MC.MasterBillOfLadingId AND MC.OrganizationId IN ({string.Join(",", listOfAffiliates)})" +
                    @")";
                }
                else
                {
                    sql =
                     @"SELECT MB.Id,
                        CASE MB.ModeOfTransport WHEN 'Air' THEN STUFF(MB.MasterBillOfLadingNo, 4, 0, '-')
						     ELSE MB.MasterBillOfLadingNo
                        END AS MasterBillOfLadingNo,
                        MB.CarrierName,
                        CM.RealContractNo,
                        CM.ContractType,
                        MB.OnBoardDate,
                        CM.ContractHolder
                    FROM MasterBills MB
                    OUTER APPLY (
	                                    SELECT TOP(1) RealContractNo, ContractType, ContractHolder
	                                    FROM ContractMaster CM WITH (NOLOCK)
	                                    WHERE CM.CarrierContractNo = MB.CarrierContractNo
                                    ) CM
                    WHERE EXISTS 
	                (
		                SELECT 1 
                        FROM MasterBillContacts MC" +
                    $"  WHERE MB.Id = MC.MasterBillOfLadingId AND MC.OrganizationId IN ({string.Join(",", listOfAffiliates)})" +
                    @")";
                }
            }

            query = _dataQuery.GetQueryable<MasterBillOfLadingQueryModel>(sql);

            return await query.ToDataSourceResultAsync(request);
        }

        public async Task<MasterBillOfLadingViewModel> GetAsync(long id, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();
            Task<MasterBillOfLadingModel> query;
            MasterBillOfLadingModel model;
            if (isInternal)
            {
                query = this.Repository.GetAsync(x => x.Id == id);
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                query = this.Repository.GetAsync(x => x.Id == id && x.Contacts.Any(y => listOfAffiliates.Contains(y.OrganizationId)));
            }
            // Get data for Master bill of lading
            model = await query;
            if (model != null) {
                // Fetch data for Contract Master to fulfill MasterBillOfLadingViewModel.CarrierContractNo
                model.ContractMaster = await _contractMasterRepository.GetAsNoTrackingAsync(x => x.CarrierContractNo == model.CarrierContractNo);
            }
            var viewModel = Mapper.Map<MasterBillOfLadingViewModel>(model);
            return viewModel;
        }

        public async Task<MasterBillOfLadingViewModel> CreateAsync(CreateMasterBillOfLadingViewModel viewModel, string userName)
        {
            var model = Mapper.Map<MasterBillOfLadingModel>(viewModel);
            var error = await ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            await MapLocationCodeToDescriptionAsync(viewModel);

            //Correct CarrierName from DB when user create/update MasterBillOfLading from api
            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) || viewModel.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
            {
                await CorrectCarrierNameAsync(viewModel);
            }

            Mapper.Map(viewModel, model);
            model.Audit(userName);

            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();

            var result = Mapper.Map<MasterBillOfLadingViewModel>(model);
            return result;
        }

        public async Task<MasterBillOfLadingViewModel> UpdateAsync(UpdateMasterBillOfLadingViewModel viewModel, string userName, long id)
        {
            var model = await this.Repository.FindAsync(id);
            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }
            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            await MapLocationCodeToDescriptionAsync(viewModel);

            //Correct CarrierName from DB when user create/update MasterBillOfLading from api
            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) || viewModel.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
            {
                await CorrectCarrierNameAsync(viewModel);
            }

            Mapper.Map(viewModel, model);
            model.Audit(userName);

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            var result = Mapper.Map<MasterBillOfLadingViewModel>(model);
            return result;
        }

        public async Task<MasterBillOfLadingViewModel> UpdateAsync(MasterBillOfLadingViewModel viewModel, string userName, long id)
        {
            var result = await base.UpdateAsync(viewModel, id);

            // Update related Shipment.CarrierContractNo

            // [dbo].[spu_UpdateShipmentCarrierContractNo]
            // @masterBOLId BIGINT = NULL,
            // @shipmentId BIGINT = NULL,
            // @updatedBy NVARCHAR(512)	

            var sql = @"spu_UpdateShipmentCarrierContractNo 
                        @p0,
	                    @p1,
	                   	@p2";
            var parameters = new object[]
            {
                id,
                null,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            return result;

        }
        public Task<IEnumerable<MasterBillOfLadingViewModel>> GetMasterBillOfLadingListBySearchingNumberAsync(string searchTerm, bool isDirectMaster, bool isInternal, string affiliates)
        {
            var filterOnAffiliates = $@"
                AND EXISTS (SELECT 1 
                            FROM MasterBillContacts MBC WITH (NOLOCK) 
                            WHERE MBC.MasterBillOfLadingId = MB.Id AND MBC.OrganizationId IN (
                                                                                    SELECT value 
                                                                                    FROM [dbo].[fn_SplitStringToTable](@affiliates, ','))
                            )
            ";

            var sql = $@"
                SELECT MB.Id, MB.MasterBillOfLadingNo, CM.RealContractNo, MB.PlaceOfIssue, MB.IssueDate, MB.OnBoardDate, MB.CarrierName, MB.VesselFlight, MB.PortOfLoading, MB.PortOfDischarge
                FROM MasterBills MB WITH (NOLOCK)
                OUTER APPLY (
	                SELECT TOP(1) RealContractNo
	                FROM ContractMaster CM WITH (NOLOCK)
	                WHERE CM.CarrierContractNo = MB.CarrierContractNo
                ) CM
                WHERE MB.MasterBillOfLadingNo LIKE CONCAT(@searchTerm, '%')
                      AND MB.IsDirectMaster = @isDirectMaster
                      {(isInternal ? "" : filterOnAffiliates)}

                ORDER BY MB.MasterBillOfLadingNo ASC
            ";

            var filterParameters = new List<SqlParameter>
            {
                new SqlParameter
                {
                    ParameterName = "@isDirectMaster",
                    Value = isDirectMaster,
                    DbType = DbType.Boolean,
                    Direction = ParameterDirection.Input
                },
                new SqlParameter
                {
                    ParameterName = "@searchTerm",
                    Value = searchTerm,
                    DbType = DbType.String,
                    Direction = ParameterDirection.Input
                }
            };

            if (!isInternal)
            {
                filterParameters.Add(
                                    new SqlParameter
                                    {
                                        ParameterName = "@affiliates",
                                        Value = affiliates?.Replace("[", "").Replace("]", ""),
                                        DbType = DbType.String,
                                        Direction = ParameterDirection.Input
                                    });
            }

            IEnumerable<MasterBillOfLadingViewModel> mappingCallback(DbDataReader reader)
            {
                var mappedData = new List<MasterBillOfLadingViewModel>();

                while (reader.Read())
                {
                    var id = reader["Id"];
                    var masterBillOfLadingNo = reader["MasterBillOfLadingNo"];
                    var carrierContractNo = reader["RealContractNo"];
                    var placeOfIssue = reader["PlaceOfIssue"];
                    var issueDate = reader["IssueDate"];
                    var onBoardDate = reader["OnBoardDate"];
                    var carrierName = reader["CarrierName"];
                    var vesselFlight = reader["VesselFlight"];
                    var portOfLoading = reader["PortOfLoading"];
                    var portOfDischarge = reader["PortOfDischarge"];

                    var newRow = new MasterBillOfLadingViewModel
                    {
                        Id = (long)id,
                        MasterBillOfLadingNo = masterBillOfLadingNo.ToString(),
                        CarrierContractNo = carrierContractNo != DBNull.Value ? carrierContractNo.ToString() : null,
                        PlaceOfIssue = placeOfIssue != DBNull.Value ? placeOfIssue.ToString() : null,
                        IssueDate = (DateTime)issueDate,
                        OnBoardDate = (DateTime)onBoardDate,
                        CarrierName = carrierName != DBNull.Value ? carrierName.ToString() : null,
                        VesselFlight = vesselFlight != DBNull.Value ? vesselFlight.ToString() : null,
                        PortOfLoading = portOfLoading != DBNull.Value ? portOfLoading.ToString() : null,
                        PortOfDischarge = portOfDischarge != DBNull.Value ? portOfDischarge.ToString() : null
                    };
                    mappedData.Add(newRow);
                }

                return mappedData;
            }
            var result = _dataQuery.GetDataBySql(sql, mappingCallback, filterParameters.ToArray());
            return Task.FromResult(result);
        }

        

        public async Task<bool> AssignHouseBLToMasterBLAsync(long masterBOLId, long houseBOLId, string userName)
        {
            //   [dbo].[spu_AssignHouseBOLToMasterBOL]
            //   @masterBOLId BIGINT,
            //   @houseBOLId BIGINT,	
            //   @updatedBy NVARCHAR(512)

            var sql = @"spu_AssignHouseBOLToMasterBOL 
                        @p0,
	                    @p1,
	                   	@p2";
            var parameters = new object[]
            {
                masterBOLId,
                houseBOLId,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            // Update related Shipment.CarrierContractNo

            // [dbo].[spu_UpdateShipmentCarrierContractNo]
            // @masterBOLId BIGINT = NULL,
            // @shipmentId BIGINT = NULL,
            // @updatedBy NVARCHAR(512)	

            sql = @"spu_UpdateShipmentCarrierContractNo 
                        @p0,
	                    @p1,
	                   	@p2";
            parameters = new object[]
            {
                masterBOLId,
                null,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());

            return true;
        }

        public async Task<bool> RemoveHouseBLFromMasterBLAsync(long masterBOLId, long houseBOLId, string userName)
        {
            //   [dbo].[spu_RemoveHouseBOLFromMasterBOL]
            //   @masterBOLId BIGINT,
            //   @houseBOLId BIGINT,	
            //   @updatedBy NVARCHAR(512)

            var sql = @"spu_RemoveHouseBOLFromMasterBOL 
                        @p0,
	                    @p1,
	                   	@p2";
            var parameters = new object[]
            {
                masterBOLId,
                houseBOLId,
                userName
            };
            _dataQuery.ExecuteSqlCommand(sql, parameters.ToArray());
            return true;
        }
        public async Task<MasterBillOfLadingViewModel> CreateFromHouseBillOfLadingAsync(MasterBillOfLadingViewModel data, long houseBOLId, string userName)
        {
            var houseBL = await _billOfLadingRepository.GetAsNoTrackingAsync(x => x.Id == houseBOLId, includes: x => x.Include(y => y.BillOfLadingItineraries).ThenInclude(y => y.Itinerary).IgnoreQueryFilters().Include(z => z.Contacts));
            if (houseBL == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {houseBOLId} not found!");
            }

            // convert to master bill of lading model
            var masterBL = Mapper.Map<MasterBillOfLadingModel>(data);
            masterBL.Audit(userName);

            var masterBLItineraries = new List<MasterBillOfLadingItineraryModel>();
            var masterBLContacts = new List<MasterBillOfLadingContactModel>();

            // clone itineraries from house bl to master bl for Sea only
            foreach (var blItinerary in houseBL.BillOfLadingItineraries.Where(x => x.Itinerary.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.OrdinalIgnoreCase)))
            {
                var masterBLItinerary = new MasterBillOfLadingItineraryModel
                {
                    ItineraryId = blItinerary.ItineraryId
                };
                masterBLItinerary.Audit(userName);
                masterBLItineraries.Add(masterBLItinerary);
            }

            // only get distinct three organization roles
            // clone contacts from house bl to master bl
            var orgRoleFiltering = new[]
            {
                OrganizationRole.OriginAgent,
                OrganizationRole.DestinationAgent,
                OrganizationRole.Principal
            };

            // Distinct by organization id and organization role
            foreach (var blContact in houseBL.Contacts?.Where(x => orgRoleFiltering.Contains(x.OrganizationRole, StringComparer.OrdinalIgnoreCase)))
            {
                if (!masterBLContacts.Any(x => x.OrganizationId == blContact.OrganizationId && x.OrganizationRole.Equals(blContact.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
                {
                    var masterBLContact = new MasterBillOfLadingContactModel
                    {
                        OrganizationId = blContact.OrganizationId,
                        OrganizationRole = blContact.OrganizationRole,
                        CompanyName = blContact.CompanyName,
                        Address = blContact.Address,
                        ContactName = blContact.ContactName,
                        ContactNumber = blContact.ContactNumber,
                        ContactEmail = blContact.ContactEmail
                    };
                    masterBLContact.Audit(userName);
                    masterBLContacts.Add(masterBLContact);
                }
            }
            masterBL.MasterBillOfLadingItineraries = masterBLItineraries;
            masterBL.Contacts = masterBLContacts;

            await Repository.AddAsync(masterBL);
            await UnitOfWork.SaveChangesAsync();

            return Mapper.Map<MasterBillOfLadingViewModel>(masterBL);


        }

        public async Task<MasterBillOfLadingViewModel> CreateFromShipmentAsync(MasterBillOfLadingViewModel data, long shipmentId, string userName)
        {
            var shipment = await _shipmentRepository.GetAsNoTrackingAsync(x => x.Id == shipmentId, includes: x => x.Include(y => y.ConsignmentItineraries).ThenInclude(y => y.Itinerary).IgnoreQueryFilters().Include(z => z.Contacts));
            if (shipment == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {shipmentId} not found!");
            }

            // convert to master bill of lading model
            var masterBL = Mapper.Map<MasterBillOfLadingModel>(data);
            masterBL.Audit(userName);

            var masterBLItineraries = new List<MasterBillOfLadingItineraryModel>();
            var masterBLContacts = new List<MasterBillOfLadingContactModel>();

            // clone itineraries from house bl to master bl for Sea only
            foreach (var consignmentItinerary in shipment.ConsignmentItineraries.Where(x => x.Itinerary.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.OrdinalIgnoreCase)))
            {
                var masterBLItinerary = new MasterBillOfLadingItineraryModel
                {
                    ItineraryId = consignmentItinerary.ItineraryId
                };
                masterBLItinerary.Audit(userName);
                masterBLItineraries.Add(masterBLItinerary);
            }

            // only get distinct three organization roles
            // clone contacts from house bl to master bl
            var orgRoleFiltering = new[]
            {
                OrganizationRole.OriginAgent,
                OrganizationRole.DestinationAgent,
                OrganizationRole.Principal
            };

            // Distinct by organization id and organization role
            foreach (var blContact in shipment.Contacts?.Where(x => orgRoleFiltering.Contains(x.OrganizationRole, StringComparer.OrdinalIgnoreCase)).OrderBy(x => x.Id))
            {
                if (!masterBLContacts.Any(x => x.OrganizationId == blContact.OrganizationId && x.OrganizationRole.Equals(blContact.OrganizationRole, StringComparison.OrdinalIgnoreCase)))
                {
                    var masterBLContact = new MasterBillOfLadingContactModel
                    {
                        OrganizationId = blContact.OrganizationId,
                        OrganizationRole = blContact.OrganizationRole,
                        CompanyName = blContact.CompanyName,
                        Address = blContact.Address,
                        ContactName = blContact.ContactName,
                        ContactNumber = blContact.ContactNumber,
                        ContactEmail = blContact.ContactEmail
                    };
                    masterBLContact.Audit(userName);
                    masterBLContacts.Add(masterBLContact);
                }
            }
            masterBL.MasterBillOfLadingItineraries = masterBLItineraries;
            masterBL.Contacts = masterBLContacts;

            await Repository.AddAsync(masterBL);
            await UnitOfWork.SaveChangesAsync();

            return Mapper.Map<MasterBillOfLadingViewModel>(masterBL);


        }

        /// <summary>
        /// From MasterBillOfLading API Import/Update, view-model contains location code, then must to map location code to location description
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        private async Task MapLocationCodeToDescriptionAsync(MasterBillOfLadingViewModel viewModel)
        {
            var checkingModeOfTransports = new[] { ModeOfTransport.Sea, ModeOfTransport.Air };
            // MasterBillOfLading Import/Update API will pass location code
            // Then, need to get location description by location code if Sea or Air
            if (checkingModeOfTransports.Contains(viewModel.ModeOfTransport.ToLowerInvariant(), StringComparer.OrdinalIgnoreCase)
                & (
                    !string.IsNullOrEmpty(viewModel.PlaceOfReceipt)
                    || !string.IsNullOrEmpty(viewModel.PortOfLoading)
                    || !string.IsNullOrEmpty(viewModel.PortOfDischarge)
                    || !string.IsNullOrEmpty(viewModel.PlaceOfDelivery)
                  )
            )
            {
                var locations = await _csfeApiClient.GetAllLocationsAsync();

                viewModel.PlaceOfReceipt = string.IsNullOrEmpty(viewModel.PlaceOfReceipt)
                    ? viewModel.PlaceOfReceipt
                    : locations.First(x => x.Name == viewModel.PlaceOfReceipt).LocationDescription;

                viewModel.PortOfLoading = string.IsNullOrEmpty(viewModel.PortOfLoading)
                    ? viewModel.PortOfLoading
                    : locations.First(x => x.Name == viewModel.PortOfLoading).LocationDescription;

                viewModel.PortOfDischarge = string.IsNullOrEmpty(viewModel.PortOfDischarge)
                    ? viewModel.PortOfDischarge
                    : locations.First(x => x.Name == viewModel.PortOfDischarge).LocationDescription;

                viewModel.PlaceOfDelivery = string.IsNullOrEmpty(viewModel.PlaceOfDelivery)
                    ? viewModel.PlaceOfDelivery
                    : locations.First(x => x.Name == viewModel.PlaceOfDelivery).LocationDescription;
            }
        }

        private async Task CorrectCarrierNameAsync(MasterBillOfLadingViewModel viewModel)
        {
            var carriers = await _csfeApiClient.GetAllCarriesAsync();
            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase))
            {
                var carrierNameFromMasterData = carriers.SingleOrDefault(c =>
                    c.CarrierCode == viewModel.SCAC &&
                    c.ModeOfTransport.Equals(viewModel.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));

                viewModel.CarrierName = carrierNameFromMasterData?.Name;
            }

            if (viewModel.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase))
            {
                var carrierNameFromMasterData = carriers.SingleOrDefault(c =>
                    c.CarrierCode == viewModel.AirlineCode &&
                    c.ModeOfTransport.Equals(viewModel.ModeOfTransport, StringComparison.InvariantCultureIgnoreCase));

                viewModel.CarrierName = carrierNameFromMasterData?.Name;
            }
        }
    }
}
