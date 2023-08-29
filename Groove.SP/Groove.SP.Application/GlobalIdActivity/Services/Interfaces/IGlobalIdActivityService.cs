using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.SP.Application.GlobalIdActivity.RequestModels;
using Groove.SP.Application.GlobalIdActivity.ViewModels;
using Groove.SP.Core.Models;

namespace Groove.SP.Application.GlobalIdActivity.Services.Interfaces
{
    public interface IGlobalIdActivityService : IServiceBase<GlobalIdActivityModel, GlobalIdActivityViewModel>
    {
        /// <summary>
        /// To get data for activity time line on booking module
        /// </summary>
        /// <param name="poffId"></param>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        Task<IEnumerable<GlobalIdActivityViewModel>> GetByPOFF(long poffId, GlobalIdActivityRequestModel requestModel);

        /// <summary>
        /// To get data for activity time line on purchase order module
        /// </summary>
        /// <param name="poId"></param>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        Task<IEnumerable<GlobalIdActivityViewModel>> GetByPO(long poId, GlobalIdActivityRequestModel requestModel);

        /// <summary>
        /// To get the cross module data for activity time line
        /// </summary>
        /// <param name="entityId"></param>
        /// <param name="entityType"></param>
        /// <param name="requestModel"></param>
        /// <returns></returns>
        Task<GlobalIdActivityPagingViewModel> GetActivityTimelineAsync(long entityId, string entityType, GetActivityTimelineRequestModel requestModel);

        Task<int> GetActivityTotalAsync(long entityId, string entityType);

        Task<IEnumerable<DropDownListItem>> GetFilterValueDropdownAsync(long entityId, string entityType, string filterBy);

        Task DeleteActivitiesByShipmentAndConsigmentAsync(long shipmentId, long consignmentId);
    }
}
