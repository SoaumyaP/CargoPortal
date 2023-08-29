using Groove.SP.Application.Common;
using Groove.SP.Application.FreightScheduler.ViewModels;
using Groove.SP.Application.Shipments.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.FreightScheduler.Services.Interfaces
{
    public interface IFreightSchedulerService : IServiceBase<FreightSchedulerModel, FreightSchedulerViewModel>
    {
        /// <summary>
        /// Return list of Freight Scheduler by searched parameters in kendoGrid
        /// </summary>
        /// <param name="request"></param>
        /// <param name="isInternal"></param>
        /// <param name="affiliates"></param>
        /// <param name="organizationId"></param>
        /// <returns></returns>
        Task<DataSourceResult> ListAsync(DataSourceRequest request, bool isInternal, string affiliates = "", long? organizationId = 0);

        /// <summary>
        /// Get Freight Scheduler detail by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<FreightSchedulerViewModel> GetByIdAsync(long id);

        /// <summary>
        /// Create new FreightScheduler not duplicated by fields (PSP-2215)
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<FreightSchedulerViewModel> CreateAsync(FreightSchedulerViewModel model, string userName);

        Task<IEnumerable<FreightSchedulerListViewModel>> FilterAsync(string jsonFilter);

        /// <summary>
        /// To update Freight Scheduler via UI.
        /// <br/>
        /// As current Scheduler is being used by Itinerary, it supports to update some fields (date mostly):
        /// <list type="bullet">IsAllowExternalUpdate</list>
        /// <list type="bullet">ETA</list>
        /// <list type="bullet">ETD</list>
        /// <list type="bullet">ATA</list>
        /// <list type="bullet">ATD</list>
        /// <list type="bullet">CY Open Date</list>
        /// <list type="bullet">CY Closing Date</list>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<FreightSchedulerViewModel> UpdateAsync(long id, UpdateFreightSchedulerViewModel model, string userName);

        /// <summary>
        /// To update Freight Scheduler via UI.
        /// <br/>
        /// As current Scheduler is not being used by any Itinerary, it supports to update most of fields.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<FreightSchedulerViewModel> EditAsync(long id, FreightSchedulerViewModel model, string userName);

        /// <summary>
        /// <b>Called via Freight Scheduler API</b> <br></br>To update ETA for all freight schedulers that map with the value of inputted fields in the model
        /// </summary>
        /// <param name="model"></param>
        /// <param name="userName"></param>
        /// <returns>List of updated freight schedulers</returns>
        Task<IEnumerable<FreightSchedulerViewModel>> UpdateAsync(UpdateFreightSchedulerApiViewModel model, string userName);

        /// <summary>
        /// Call to broadcast Schedule updates value (ETA/ETD) to related tabels (dbo.Shipments/ dbo.Consignments/ dbo.Itineraries/ dbo.Containers/ dbo.BillOfLadings)
        /// </summary>
        /// <param name="freightSchedulerIds"></param>
        /// <param name="updatedFromKeyword"></param>
        /// <param name="updateViaUI"></param>
        /// <param name="userName"></param>
        /// <returns></returns>
        Task<bool> BroadcastFreightScheduleUpdatesAsync(IEnumerable<long> freightSchedulerIds, string updatedFromKeyword, bool updateViaUI, string userName);

        /// <summary>
        /// To check if all shipments of a Freight Scheduler have Container# linked.
        /// </summary>
        /// <param name="freightSchedulerId"></param>
        /// <returns></returns>
        Task<Tuple<bool, List<long>>> IsReadyContainerManifestAsync(long freightSchedulerId);

        Task<int> CountVesselArrivalAsync(bool isInternal, string affiliates, long? delegatedOrgId, string customerRelationships, string statisticFilter);

        Task<DataSourceResult> GetListVesselArrivalAsync(DataSourceRequest request, bool isInternal, string affiliates, long? delegatedOrgId, string customerRelationships,
            string statisticKey = "", string statisticFilter = "", bool isExport = false);

        /// <summary>
        /// To broadcast CYClosingDate from Freight Scheduler to linked Itineraries and POFulfillments
        /// <br/>
        /// <b>Notes: This method will run sql script then it should be called after all data saved from EF.</b>
        /// </summary>
        /// <param name="freightSchedulerId">Id of Freight Scheduler</param>
        /// <returns></returns>
        Task<bool> BroadcastCYClosingDateAsync(long freightSchedulerId);
    }
}