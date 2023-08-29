using System.Collections.Generic;
using System.Threading.Tasks;
using Groove.CSFE.Application.Carriers.ViewModels;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Data;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Carriers.Services
{
    public interface ICarrierService : IServiceBase<CarrierModel, CarrierViewModel>
    {
        Task<CarrierViewModel> GetByCodeAsync(string code);
        Task<CarrierViewModel> GetByIdAsync(long id);
        IEnumerable<CarrierViewModel> GetAllCarriers(string code);
        Task<bool> CheckDuplicateCarrierCodeAsync(CarrierViewModel model);
        Task<bool> CheckDuplicateCarrierNameAsync(CarrierViewModel model);
        Task<bool> CheckDuplicateCarrierNumberAsync(CarrierViewModel model);
        Task<CarrierViewModel> UpdateStatusAsync(long id, CarrierStatus status, string userName);
    }
}
