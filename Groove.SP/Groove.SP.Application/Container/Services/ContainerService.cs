using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Groove.SP.Application.Activity.Services.Interfaces;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Application.Container.Services.Interfaces;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Application.Exceptions;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Interfaces.UnitOfWork;
using Groove.SP.Application.Provider.Report;
using Groove.SP.Application.QuickTrack;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Groove.SP.Application.Container.Services
{
    public class ContainerService : ServiceBase<ContainerModel, ContainerViewModel>, IContainerService
    {
        private readonly AppConfig _appConfig;
        private readonly ITelerikReportProvider _reportProvider;
        private readonly IActivityService _activityService;
        private readonly IActivityRepository _activityRepository;
        private readonly IItineraryRepository _itineraryRepository;
        private readonly IDataQuery _dataQuery;

        public ContainerService(IUnitOfWorkProvider unitOfWorkProvider,
                                IOptions<AppConfig> appConfig,
                                 IDataQuery dataQuery,
                                ITelerikReportProvider reportProvider,
                                IActivityService activityService,
                                IItineraryRepository itineraryRepository)
            : base(unitOfWorkProvider)
        {
            _appConfig = appConfig.Value;
            this._reportProvider = reportProvider;
            _activityService = activityService;
            _dataQuery = dataQuery;
            _activityRepository = (IActivityRepository)UnitOfWork.GetRepository<ActivityModel>();
            _itineraryRepository = itineraryRepository;
        }

        private async Task<IEnumerable<ContainerViewModel>> GetContainerViewModel(IEnumerable<ContainerModel> containers)
        {
            // key: containerId - value: confirmation status
            var confirmationCheckingDictionary = containers.Select(c => new { 
                id = c.Id,
                isConfirmed = !c.ShipmentLoads?.Any(a => a.Consolidation != null && a.Consolidation?.Stage != ConsolidationStage.Confirmed) ?? true
            }).ToDictionary(
                x => x.id,
                x => x.isConfirmed
            );

            var result = Mapper.Map<IEnumerable<ContainerViewModel>>(containers);

            // Populate activities
            var containerIdList = containers.Select(x => CommonHelper.GenerateGlobalId(x.Id, EntityType.Container));
            var containerActivities = await _activityRepository.Query(x => x.GlobalIdActivities.Any(y => containerIdList.Contains(y.GlobalId)),
                null,
                x => x.Include(a => a.GlobalIdActivities).ThenInclude(b => b.ReferenceEntity)).ToListAsync();

            foreach (var item in result)
            {
                var activities = containerActivities.Where(sa => sa.GlobalIdActivities.Any(ga => ga.ReferenceEntity.EntityId == item.Id));
                item.Activities = Mapper.Map<ICollection<ActivityViewModel>>(activities);
                item.IsConfirmed = confirmationCheckingDictionary[item.Id];
            }

            return result;
        }

        public async Task<ContainerViewModel> GetContainerAsync(string containerNoOrId, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();
            long.TryParse(containerNoOrId, out var containerId);

            ContainerModel model = null;

            if (isInternal)
            {
                model = await Repository.GetAsync(s => s.Id == containerId || s.ContainerNo == containerNoOrId, x => x.OrderByDescending(m => m.Id),
                                                    c => c.Include(m => m.ShipmentLoads).ThenInclude(m => m.Shipment).ThenInclude(m => m.Contacts)
                                                        .Include(m => m.Consolidation).ThenInclude(m => m.ShipmentLoads).ThenInclude(m => m.Shipment).ThenInclude(m => m.Contacts));
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                model = await Repository.GetAsync(c => (c.Id == containerId || c.ContainerNo == containerNoOrId)
                                           && ((c.IsFCL && c.ShipmentLoads.Any(cs => cs.Shipment.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId))))
                                                || (!c.IsFCL && c.Consolidation.ShipmentLoads.Any(cs => cs.Shipment.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId))))),
                                           c => c.OrderByDescending(m => m.Id),
                                           c => c.Include(m => m.ShipmentLoads).ThenInclude(m => m.Shipment).ThenInclude(m => m.Contacts)
                                                .Include(m => m.Consolidation).ThenInclude(m => m.ShipmentLoads).ThenInclude(m => m.Shipment).ThenInclude(m => m.Contacts));
            }

            return Mapper.Map<ContainerViewModel>(model);
        }

        public async Task<QuickTrackContainerViewModel> GetQuickTrackAsync(string containerNo)
        {
            var model = await Repository.GetAsync(s => s.ContainerNo == containerNo,
                                                    x => x.OrderByDescending(m => m.Id));

            if (model == null)
                return null;

            var result = Mapper.Map<QuickTrackContainerViewModel>(model);
            result.Activities = Mapper.Map<ICollection<QuickTrackActivityViewModel>>(await _activityService.GetActivities(EntityType.Container, model.Id));
            result.Milestones = Milestone.ContainerMileStones;

            foreach (var milestone in result.Milestones)
            {
                milestone.ActivityDate = result.Activities
                    .FirstOrDefault(a => a.ActivityCode == milestone.ActivityCode)?.ActivityDate;
            }

            return result;
        }

        public async Task<IEnumerable<ContainerViewModel>> GetContainersByBOLAsync(long billOfLadingId, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();
            IEnumerable<ContainerModel> result = null;

            if (isInternal)
            {
                result = await Repository.Query(c => (c.IsFCL && c.BillOfLadingShipmentLoads.Any(bsl => bsl.BillOfLadingId == billOfLadingId))
                                                            || (!c.IsFCL && c.Consolidation.BillOfLadingShipmentLoads.Any(bsl => bsl.BillOfLadingId == billOfLadingId)),
                                                        c => c.OrderBy(i => i.Id), x => x.Include(y => y.ShipmentLoads).ThenInclude(y => y.Consolidation)).ToListAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }
                result = await Repository.Query(c => (c.IsFCL && c.BillOfLadingShipmentLoads.Any(bsl => bsl.BillOfLadingId == billOfLadingId
                                                            && bsl.ShipmentLoad.Shipment.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId))))
                                                            || (!c.IsFCL && c.Consolidation.BillOfLadingShipmentLoads.Any(bsl => bsl.BillOfLadingId == billOfLadingId
                                                            && bsl.ShipmentLoad.Shipment.Contacts.Any(x => listOfAffiliates.Contains(x.OrganizationId)))),
                                                        c => c.OrderBy(i => i.Id), x => x.Include(y => y.ShipmentLoads).ThenInclude(y => y.Consolidation)).ToListAsync();
            }

            return await GetContainerViewModel(result);
        }

        public async Task<IEnumerable<ContainerViewModel>> GetContainersByShipmentAsync(long shipmentId, bool isInternal, string affiliates)
        {
            var listOfAffiliates = new List<long>();
            IEnumerable<ContainerModel> result = null;

            if (isInternal)
            {
                result = await Repository.Query(c => (c.IsFCL && c.ShipmentLoads.Any(x => x.ShipmentId == shipmentId))
                                                    || (!c.IsFCL && c.Consolidation.ShipmentLoads.Any(x => x.ShipmentId == shipmentId))).ToListAsync();
            }
            else
            {
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = JsonConvert.DeserializeObject<List<long>>(affiliates);
                }

                result = await Repository.Query(c => (c.IsFCL && c.ShipmentLoads.Any(x => x.ShipmentId == shipmentId)
                                                        && c.ShipmentLoads.Any(x => x.Shipment.Contacts.Any(sc => listOfAffiliates.Contains(sc.OrganizationId))))
                                                    || (!c.IsFCL && c.Consolidation.ShipmentLoads.Any(x => x.ShipmentId == shipmentId)
                                                        && c.Consolidation.ShipmentLoads.Any(x => x.Shipment.Contacts.Any(sc => listOfAffiliates.Contains(sc.OrganizationId))))).ToListAsync();
            }

            return await GetContainerViewModel(result);
        }

        public async Task<IEnumerable<ContainerViewModel>> GetContainersByMasterBOLAsync(long masterBOLId, bool isDirectMaster)
        {
            IEnumerable<ContainerModel> result;

            // Master BL links directly to Shipment
            if (isDirectMaster)
            {
                result = await Repository.QueryAsNoTracking(c => c.ShipmentLoads.Any(sm => sm.Consignment.MasterBillId == masterBOLId), orderBy: c => c.OrderBy(x => x.Id), includes: x => x.Include(y => y.ShipmentLoads).ThenInclude(y => y.Consolidation)).ToListAsync();

            }
            else
            {
                // Master BL links to Shipment by house BL
                result = await Repository.QueryAsNoTracking(s => (s.IsFCL && s.BillOfLadingShipmentLoads.Any(x => x.MasterBillOfLadingId == masterBOLId)
                                                       || (!s.IsFCL && s.Consolidation.BillOfLadingShipmentLoads.Any(x => x.MasterBillOfLadingId == masterBOLId))),
                                                         n => n.OrderBy(a => a.Id), x => x.Include(y => y.ShipmentLoads).ThenInclude(y => y.Consolidation)).ToListAsync();
            }
            return await GetContainerViewModel(result);
        }

        public async Task<Stream> TestReportAsync(ContainerReportRequest rq)
        {
            // Export file
            var reportRq = new ReportRequest
            {
                ReportFormat = rq.ReportFormat,
                ReportName = ReportNames.Container,
                ReportParameters = new { rq.ContainerId }
            };
            return await _reportProvider.ExportAsync(reportRq);
        }

        public override async Task<ContainerViewModel> UpdateAsync(ContainerViewModel viewModel, params object[] keys)
        {
            viewModel.ValidateAndThrow(true);

            ContainerModel model = await this.Repository.FindAsync(keys);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", keys)} not found!");
            }

            var itineraries = await _itineraryRepository.Query(s => s.ContainerItineraries.Any(x => x.ContainerId == model.Id) && s.ModeOfTransport == ModeOfTransport.Sea && s.ScheduleId != null,
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

            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }

            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            viewModel = Mapper.Map<ContainerViewModel>(model);
            return viewModel;
        }

        public async Task<DataSourceResult> GetListAsync(DataSourceRequest request, bool isInternal, string affiliates = "")
        {
            IQueryable<ContainerQueryModel> query;
            string sql;

            if (isInternal)
            {
                sql = @"SELECT 
                          Id, 
                          ContainerNo, 
                          ShipFrom, 
                          ShipTo, 
                          ShipFromETDDate, 
                          ShipToETADate, 
                          Movement 
                        FROM Containers WITH (NOLOCK) 
                        WHERE 
                          (
                            IsFCL = 1 AND Id IN 
                            (
                              SELECT 
                                ContainerId 
                              FROM 
                                ShipmentLoads WITH (NOLOCK) 
                                INNER JOIN Shipments ON ShipmentLoads.ShipmentId = Shipments.Id 
                                AND Shipments.Status = 'active' 
                              WHERE 
                                ShipmentLoads.ContainerId IS NOT NULL
                            )
                          ) 
                          OR 
                          (
                            IsFCL = 0 AND Id IN 
                            (
                              SELECT 
                                Consolidations.ContainerId 
                              FROM 
                                Consolidations WITH (NOLOCK) 
                                INNER JOIN ShipmentLoads WITH (NOLOCK) ON ShipmentLoads.ConsolidationId = Consolidations.Id 
                                INNER JOIN Shipments ON ShipmentLoads.ShipmentId = Shipments.Id 
                                AND Shipments.Status = 'active' 
                              WHERE 
                                Consolidations.ContainerId IS NOT NULL
                            )
                          )
                    ";
                query = _dataQuery.GetQueryable<ContainerQueryModel>(sql);
            }
            else
            {
                var listOfAffiliates = "";
                if (!string.IsNullOrEmpty(affiliates))
                {
                    listOfAffiliates = string.Join(",", JsonConvert.DeserializeObject<List<long>>(affiliates));
                }
                sql = @$"
                        SELECT  Id
	                            ,ContainerNo
                                ,ShipFrom
                                ,ShipTo
                                ,ShipFromETDDate
                                ,ShipToETADate
                                ,Movement

                        FROM Containers WITH (NOLOCK)

                        WHERE   IsFCL = 1 AND EXISTS
	                            (
                                    SELECT 1
                                    FROM ShipmentLoads WITH (NOLOCK)
                                    INNER JOIN Shipments ON ShipmentLoads.ShipmentId = Shipments.Id AND Shipments.Status = 'active'
                                    WHERE ShipmentLoads.ContainerId = Containers.Id
                                    AND EXISTS (
                                        SELECT 1
                                        FROM ShipmentContacts SC WITH (NOLOCK) 
                                        WHERE SC.ShipmentId = Shipments.Id AND SC.OrganizationId IN ({listOfAffiliates})
                                    )
                                )
                        UNION ALL
                        SELECT  Id
	                            ,ContainerNo
                                ,ShipFrom
                                ,ShipTo
                                ,ShipFromETDDate
                                ,ShipToETADate
                                ,Movement

                        FROM Containers WITH (NOLOCK)

                        WHERE   IsFCL = 0 AND EXISTS
	                            (
                                    SELECT 1
                                    FROM Consolidations WITH (NOLOCK)
                                    INNER JOIN ShipmentLoads WITH (NOLOCK) ON ShipmentLoads.ConsolidationId = Consolidations.Id
									INNER JOIN Shipments ON ShipmentLoads.ShipmentId = Shipments.Id AND Shipments.Status = 'active'
                                    WHERE Consolidations.ContainerId = Containers.Id
                                    AND EXISTS (
                                        SELECT 1
                                        FROM ShipmentContacts SC WITH (NOLOCK) 
                                        WHERE SC.ShipmentId = Shipments.Id AND SC.OrganizationId IN ({listOfAffiliates})
                                    ) 	  
                                )
                      ";
                query = _dataQuery.GetQueryable<ContainerQueryModel>(sql);
            }

            return await query.ToDataSourceResultAsync(request);
        }

        /// <summary>
        /// To handle Container Update via application UI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        public async Task<ContainerViewModel> UpdateAsync(long id, UpdateContainerViaUIViewModel viewModel, string userName)
        {
            viewModel.ValidateAndThrow();

            ContainerModel model = await this.Repository.FindAsync(id);

            if (model == null)
            {
                throw new AppEntityNotFoundException($"Object with the id {string.Join(", ", id)} not found!");
            }

            Mapper.Map(viewModel, model);

            var error = await this.ValidateDatabaseBeforeAddOrUpdateAsync(model);
            if (!string.IsNullOrEmpty(error))
            {
                throw new AppException(error);
            }
            model.Audit(userName);
            this.Repository.Update(model);
            await this.UnitOfWork.SaveChangesAsync();

            var result = Mapper.Map<ContainerViewModel>(model);
            return result;
        }

        public async Task<bool> IsDuplicatedContainerAsync(string containerNo, string carrierSONo, long currentContainerId = 0)
        {
            if (string.IsNullOrEmpty(containerNo) || string.IsNullOrEmpty(carrierSONo))
            {
                return false;
            }

            return await Repository.AnyAsync(x => x.Id != currentContainerId
                && !string.IsNullOrEmpty(x.CarrierSONo)
                && x.ContainerNo == containerNo
                && x.CarrierSONo == carrierSONo);
        }
    }
}
