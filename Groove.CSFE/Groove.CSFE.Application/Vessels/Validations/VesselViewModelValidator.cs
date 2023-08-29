using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Application.Vessels.ViewModels;
using Groove.CSFE.Core.Entities;
using System.Threading.Tasks;

namespace Groove.CSFE.Application.Vessels.Validations
{
    public class VesselViewModelValidator<T> : BaseValidation<T> where T : VesselViewModel
    {
        protected IRepository<VesselModel> _vesselRepository;

        protected void CheckDuplicateCode(bool isUpdate)
        {
            RuleFor(a => a.Code)
                .MustAsync(async (t, p, c) => !await CheckDuplicateCodeAsync(t, isUpdate))
                .When(a => !string.IsNullOrWhiteSpace(a.Code))
                .WithMessage("Vessel Code cannot be duplicated");
        }

        /// <summary>
        /// To check unique code of Vessel
        /// </summary>
        /// <param name="viewModel"></param>
        /// <param name="isUpdate"></param>
        /// <returns>Return true if VesselCode from api already exist in system</returns>
        private async Task<bool> CheckDuplicateCodeAsync(VesselViewModel viewModel, bool isUpdate)
        {
            if (string.IsNullOrWhiteSpace(viewModel.Code))
            {
                return false;
            }

            if (isUpdate == false)
            {
                return await _vesselRepository.AnyAsync(s => s.Code == viewModel.Code);
            }
            else
            {
                if (viewModel.IsPropertyDirty(nameof(VesselViewModel.Code)))
                {
                    return await _vesselRepository.AnyAsync(s => s.Code == viewModel.Code && s.Id != viewModel.Id);
                } 
            }

            return false;
        }
    }
}
