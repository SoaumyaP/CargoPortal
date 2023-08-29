using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Groove.SP.Application.Common;
using Groove.SP.Application.Container.ViewModels;
using Groove.SP.Core.Data;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.Container.Services.Interfaces
{
    public interface IContainerService : IServiceBase<ContainerModel, ContainerViewModel>
    {
        Task<IEnumerable<ContainerViewModel>> GetContainersByBOLAsync(long billOfLadingId, bool isInternal, string affiliates = "");

        Task<IEnumerable<ContainerViewModel>> GetContainersByShipmentAsync(long shipmentId, bool isInternal, string affiliates = "");

        /// <summary>
        /// To load data grid of containers on master bill of lading details page
        /// </summary>
        /// <param name="masterBOLId"></param>
        /// <param name="isDirectMaster"></param>
        /// <returns></returns>
        Task<IEnumerable<ContainerViewModel>> GetContainersByMasterBOLAsync(long masterBOLId, bool isDirectMaster);

        Task<ContainerViewModel> GetContainerAsync(string containerNoOrId, bool isInternal, string affiliates = "");

        Task<QuickTrackContainerViewModel> GetQuickTrackAsync(string containerNo);

        Task<DataSourceResult> GetListAsync(DataSourceRequest request, bool isInternal, string affiliates = "");

        Task<Stream> TestReportAsync(ContainerReportRequest rq);

        /// <summary>
        /// To handle Container Update via application UI
        /// </summary>
        /// <param name="id"></param>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        Task<ContainerViewModel> UpdateAsync(long id, UpdateContainerViaUIViewModel viewModel, string userName);

        /// <summary>
        /// To check duplicate on a pair of container number and carrier so number.
        /// <br>Please provide [currentContainerId] if checking container is already exists.</br>
        /// </summary>
        /// <param name="containerNo"></param>
        /// <param name="carrierSONo"></param>
        /// <param name="currentContainerId"></param>
        /// <returns></returns>
        Task<bool> IsDuplicatedContainerAsync(string containerNo, string carrierSONo, long currentContainerId = 0);
    }
}
