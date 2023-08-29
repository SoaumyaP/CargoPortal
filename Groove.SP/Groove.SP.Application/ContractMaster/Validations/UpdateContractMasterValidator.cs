using FluentValidation;
using Groove.SP.Application.ContractMaster.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Infrastructure.CSFE;

namespace Groove.SP.Application.ContractMaster.Validations
{
    public class UpdateContractMasterValidator : ContractMasterValidator<UpdateContractMasterViewModel>
    {
        public UpdateContractMasterValidator(ICSFEApiClient csfeApiClient, IRepository<ContractMasterModel> repository)
        {
            RuleFor(a => a.CarrierContractNo)
                .NotEmpty()
                .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.CarrierContractNo)));

            RuleFor(a => a.CarrierContractNo)
                .MustAsync(async (viewModel, CarrierContractNo, cancellation) => !await repository.AnyAsync(c => c.CarrierContractNo == CarrierContractNo && viewModel.Id != c.Id))
                .WithMessage("Duplicate CarrierContractNo.")
                .When(x => !string.IsNullOrEmpty(x.CarrierContractNo))
                .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.CarrierContractNo)));

            RuleFor(a => a.CarrierCode)
               .MustAsync(async (CarrierCode, cancellation) => await CheckExistCarrierAsync(CarrierCode, csfeApiClient))
               .WithMessage("Inputted data is not existing in system.")
               .When(x => !string.IsNullOrWhiteSpace(x.CarrierCode))
               .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.CarrierCode)));

            RuleFor(a => a.RealContractNo)
                .NotEmpty()
                .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.RealContractNo)));

            RuleFor(a => a.ContractType)
                .NotEmpty()
                .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.ContractType)));

            RuleFor(a => a.ContractHolder)
                .NotEmpty()
                .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.ContractHolder)));

            RuleFor(a => a.ValidFrom)
                .NotEmpty()
                .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.ValidFrom)));

            RuleFor(a => a.ValidTo)
                .NotEmpty()
                .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.ValidTo)));

            RuleFor(a => a.IsVIP)
                .NotEmpty()
                .When(c => c.IsPropertyDirty(nameof(ContractMasterViewModel.IsVIP)));
        }
    }
}
