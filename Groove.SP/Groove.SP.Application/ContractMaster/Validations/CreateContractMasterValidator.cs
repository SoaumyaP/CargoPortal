using FluentValidation;
using Groove.SP.Application.ContractMaster.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities;
using Groove.SP.Infrastructure.CSFE;

namespace Groove.SP.Application.ContractMaster.Validations
{
    public class CreateContractMasterValidator : ContractMasterValidator<CreateContractMasterViewModel>
    {
        public CreateContractMasterValidator(ICSFEApiClient csfeApiClient, IRepository<ContractMasterModel> repository)
        {
            RuleFor(a => a.CarrierContractNo).NotEmpty();
            RuleFor(a => a.CarrierContractNo)
                .MustAsync(async (viewModel, CarrierContractNo, cancellation) => !await repository.AnyAsync(c => c.CarrierContractNo == CarrierContractNo && viewModel.Id != c.Id))
                .WithMessage("Duplicate CarrierContractNo.")
                .When(x => !string.IsNullOrEmpty(x.CarrierContractNo));

            RuleFor(a => a.CarrierCode)
               .MustAsync(async (CarrierCode, cancellation) => await CheckExistCarrierAsync(CarrierCode, csfeApiClient))
               .WithMessage("Inputted data is not existing in system.")
               .When(x => !string.IsNullOrWhiteSpace(x.CarrierCode));
            RuleFor(a => a.RealContractNo).NotEmpty();
            RuleFor(a => a.ContractType).NotEmpty();
            RuleFor(a => a.ContractHolder).NotEmpty();
            RuleFor(a => a.ValidFrom).NotEmpty();
            RuleFor(a => a.ValidTo).NotEmpty();
            RuleFor(a => a.IsVIP).NotEmpty();
        }
    }
}
