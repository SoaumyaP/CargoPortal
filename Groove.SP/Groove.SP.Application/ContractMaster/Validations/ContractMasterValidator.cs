using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ContractMaster.ViewModels;
using Groove.SP.Infrastructure.CSFE;
using System.Threading.Tasks;

namespace Groove.SP.Application.ContractMaster.Validations
{
    public class ContractMasterValidator<T> : BaseValidation<T> where T : ContractMasterViewModel
    {
        protected async Task<bool> CheckExistCarrierAsync(string carrierCode, ICSFEApiClient csfeApiClient)
        {
            var carrier = await csfeApiClient.GetCarrierByCodeAsync(carrierCode);
            return carrier != null;
        }
    }
}
