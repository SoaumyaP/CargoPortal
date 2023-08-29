using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Groove.SP.Application.Activity.Services.Interfaces
{
    public interface IActivityService : IServiceBase<ActivityModel, ActivityViewModel>
    {
        /// <summary>
        /// To get data for attachment grid by certain entity: shipment, container, consignment
        /// <br></br>Purchase order, POFF booking call APIs on <see cref="Groove.SP.API.Controllers.GlobalIdActivitiesController"/>
        /// </summary>
        /// <remarks><b>It applies logic sorting on Activity Date (date part only) DESC and Activity Code DESC, do not sort again on GUI</b></remarks>
        /// <param name="entityType">Entity type. See more values at <see cref="Groove.SP.Core.Models.EntityType"/></param>
        /// <param name="entityId">Entity Id</param>
        /// <returns></returns>
        Task<IEnumerable<ActivityViewModel>> GetActivities(string entityType, long entityId);
        Task<IEnumerable<ActivityViewModel>> GetActivityCrossModuleAsync(string entityType, long entityId);
        Task<ActivityViewModel> UpdateAsync(ActivityViewModel viewModel, long id, IdentityInfo currentUser = null);
        Task<ActivityViewModel> TriggerAnEvent(ActivityViewModel viewModel);

        /// <summary>
        /// Create activity from third party API (Agent)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ActivityViewModel> CreateAsync(AgentActivityCreateViewModel model);

        /// <summary>
        /// Update activity from third party API (Agent)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<ActivityViewModel> UpdateAsync(AgentActivityUpdateViewModel model);

        /// <summary>
        /// Delete activity from third party API (Agent)
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task DeleteAsync(AgentActivityDeleteViewModel model);

        /// <summary>
        /// To trigger list of events. All events have to be applied to same object PO/POFF/Shipment/...
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        Task TriggerEventList(IList<ActivityViewModel> events);
        Task<ActivityViewModel> GetActivitiesByCodeAsync(string code);
        Task DeleteAsync(long id);
        Task DeleteAsync(long id, bool IsDeleteATAViaFSApi);
    }
}
