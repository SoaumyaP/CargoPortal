using Groove.SP.Application.Common;
using Groove.SP.Application.Consignment.Services.Interfaces;
using Groove.SP.Application.Consignment.ViewModels;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.GlobalIdActivity.Services.Interfaces;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Users.Services.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.Consignment.Services
{
    public class ConsignmentService : ServiceBase<ConsignmentModel, ConsignmentViewModel>, IConsignmentService
    {
        private readonly ICSFEApiClient _csfeApiClient;
        private readonly IUserProfileService _userProfileService;
        private readonly AppConfig _appConfig;
        private readonly IGlobalIdActivityService _globalIdActivityService;
        private readonly IItineraryRepository _itineraryRepository;

        public ConsignmentService(IUnitOfWorkProvider unitOfWorkProvider,
            ICSFEApiClient csfeApiClient,
            IUserProfileService userProfileService,
            IOptions<AppConfig> appConfig,
            IGlobalIdActivityService globalIdActivityService,
            IItineraryRepository itineraryRepository)
            : base(unitOfWorkProvider)
        {
            _csfeApiClient = csfeApiClient;
            _userProfileService = userProfileService;
            _appConfig = appConfig.Value;
            _globalIdActivityService = globalIdActivityService;
            _itineraryRepository = itineraryRepository;
        }

        public async Task<IEnumerable<ConsignmentViewModel>> GetConsignmentsByShipmentAsync(long shipmentId)
        {
            var result = await Repository.Query(s => s.ShipmentId == shipmentId,
                                                    s => s.OrderBy(r => r.Sequence)).ToListAsync();

            return Mapper.Map<IEnumerable<ConsignmentViewModel>>(result);
        }

        public async Task<ConsignmentViewModel> GetAsync(long id, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();

            var query = Repository.GetListQueryable(i => i
                .Include(s => s.Shipment).ThenInclude(c => c.Contacts)
                .Include(s => s.Shipment).ThenInclude(c => c.Consignments).ThenInclude(m => m.MasterBill)
                .Include(s => s.Shipment).ThenInclude(c => c.ShipmentBillOfLadings).ThenInclude(sbl => sbl.BillOfLading)
                .Include(s => s.Shipment).ThenInclude(c => c.ContractMaster)
                .Where(s => s.Id == id));

            if (!isInternal)
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }
                query = query.Where(x => x.Shipment.Contacts.Any(c => listOfAffiliates.Contains(c.OrganizationId)));
            }

            var model = await query.FirstOrDefaultAsync();
            return Mapper.Map<ConsignmentViewModel>(model);
        }


        public override async Task<ConsignmentViewModel> CreateAsync(ConsignmentViewModel viewModel)
        {
            viewModel.ValidateAndThrow();

            ConsignmentModel model = Mapper.Map<ConsignmentModel>(viewModel);

            var executionAgent = await _csfeApiClient.GetOrganizationByIdAsync(model.ExecutionAgentId);
            model.ExecutionAgentName = executionAgent?.Name;

            await this.Repository.AddAsync(model);
            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<ConsignmentViewModel>(model);
            return viewModel;
        }

        public async Task<ConsignmentViewModel> UpdateAsync(ConsignmentViewModel viewModel, long id)
        {
            viewModel.ValidateAndThrow(true);

            ConsignmentModel model = await this.Repository.FindAsync(id);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Consignment with the id {id} not found!");
            }

            var itineraries = await _itineraryRepository.Query(s => s.ConsignmentItineraries.Any(x => x.ConsignmentId == model.Id) && s.ModeOfTransport == ModeOfTransport.Sea && s.ScheduleId != null,
                                                    n => n.OrderBy(a => a.Sequence), x => x.Include(y => y.FreightScheduler)).ToListAsync();

            // The API should skip the updates of the dates if the linked Freight Scheduler already LOCKED
            if (itineraries.Count() > 0 && !itineraries.First().FreightScheduler.IsAllowExternalUpdate)
            {
                viewModel.ShipFromETDDate = model.ShipFromETDDate;
            }
            if (itineraries.Count() > 0 && !itineraries.Last().FreightScheduler.IsAllowExternalUpdate)
            {
                viewModel.ShipToETADate = model.ShipToETADate;
            }

            Mapper.Map(viewModel, model);

            var executionAgent = await _csfeApiClient.GetOrganizationByIdAsync(model.ExecutionAgentId);
            model.ExecutionAgentName = executionAgent?.Name;

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<ConsignmentViewModel>(model);
            return viewModel;
        }

        public async Task MoveToTrashAsync(long id, string userName)
        {
            var user = await _userProfileService.GetAsync(userName);
            bool isInternal = user.IsInternal;
            var affiliateIds = user.OrganizationId.HasValue ?
                await _csfeApiClient.GetAffiliateIdsAsync(user.OrganizationId.Value) : null;

            var query = Repository.GetListQueryable()
                .Where(s => s.Id == id)
                .Include(s => s.ConsignmentItineraries)
                .ThenInclude(ci => ci.Itinerary)
                .IgnoreQueryFilters();


            if (!isInternal)
            {
                query = query.Where(x => affiliateIds.Contains(x.ExecutionAgentId));
            }

            var model = await query.FirstOrDefaultAsync();

            model.IsDeleted = true;

            var itineraryList = model.ConsignmentItineraries.Select(ci => ci.Itinerary)
                .Where(i => i.Status != StatusType.INACTIVE).ToList();

            itineraryList.ForEach(i => i.Status = StatusType.INACTIVE);

            await _globalIdActivityService.DeleteActivitiesByShipmentAndConsigmentAsync(model.ShipmentId, model.Id);

            await this.UnitOfWork.SaveChangesAsync();
        }

        public async Task<IEnumerable<ConsignmentViewModel>> GetByConsolidationAsync(long consolidationId, bool isInternal, string affiliates = "")
        {
            Func<IQueryable<ConsignmentModel>, IQueryable<ConsignmentModel>> includeProperties = x 
                => x.Include(y => y.ShipmentLoads)
                .Include(y => y.Shipment)
                .Include(y => y.ShipmentLoadDetails);

            var model = await Repository.QueryAsNoTracking(x => x.ShipmentLoads.Any(y => y.ConsolidationId == consolidationId), null, includeProperties).ToListAsync();

            // TODO: Data access right for principal & supplier
            //if (!isInternal)
            //{
            //    var listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
            //    model = model.Where(c => listOfAffiliates.Any(orgId => c.Shipment.Contacts.Select(x => x.OrganizationId).Contains(orgId))).ToList();
            //}
            foreach (var item in model)
            {
                var loadDetails = item.ShipmentLoadDetails?.Where(x => x.ConsolidationId == consolidationId);
                item.Package = loadDetails?.Sum(x => x.Package) ?? 0;
                item.PackageUOM = PackageUOMType.Carton.ToString();
                item.Volume = loadDetails?.Sum(x => x.Volume) ?? 0;
                item.VolumeUOM = AppConstant.CUBIC_METER;
                item.GrossWeight = loadDetails?.Sum(x => x.GrossWeight) ?? 0;
                item.GrossWeightUOM = AppConstant.KILOGGRAMS;
            }
            return Mapper.Map<List<ConsignmentViewModel>>(model);
        }

        public async Task<IEnumerable<ConsignmentDropdownItemViewModel>> GetDropdownByShipmentAsync(long shipmentId)
        {
            var seaOrAir = new[] { ModeOfTransport.Sea, ModeOfTransport.Air };
            var model = await Repository.QueryAsNoTracking(c => c.ShipmentId == shipmentId && seaOrAir.Any(x => x == c.ModeOfTransport)).ToListAsync();
            var viewModel = Mapper.Map<List<ConsignmentViewModel>>(model);
            return viewModel.Select(x => new ConsignmentDropdownItemViewModel { 
                Value = x.Id,
                Text = x.ExecutionAgentName,
                ExecutionAgentId = x.ExecutionAgentId
            });
        }

        public async Task<DropDownListItem<string>> GetDropdownOriginCFSAsync(long id)
        {
            var model = await Repository.GetAsNoTrackingAsync(c => c.Id == id, null, x => x.Include(y => y.Shipment));
            if (model == null)
            {
                throw new AppEntityNotFoundException($"Consignment with the id {id} not found!");
            }
            return new DropDownListItem<string>
            {
                Text = model.Shipment.ShipFrom,
                Value = model.Shipment.ShipFrom // TODO: system should save LocationCode into DB (next phase)
            };
        }
    }
}
