using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.CruiseOrders.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Infrastructure.CSFE;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.CruiseOrders.Validations
{
    public class CreateBulkCruiseOrderViewModelValidator : BaseValidation<IEnumerable<CreateCruiseOrderViewModel>>
    {
        public CreateBulkCruiseOrderViewModelValidator(ICSFEApiClient csfeApiClient, IRepository<CruiseOrderModel> cruiseOrderRepository)
        {
            var currencies = csfeApiClient.GetAllCurrenciesAsync().Result;

            RuleFor(x => x).NotNull()
                .WithMessage("There is no data.");
            RuleFor(x => x).Must(x => x != null && x.Any() && x.Count() <= 500)
                .WithMessage("Minimum 1 and maximum 500 elements to import once.");
            RuleFor(x => x).Must(x => !x.GroupBy(y => y.PONumber).Any(g => g.Count() > 1))
                .WithMessage($"Duplicate '{nameof(CreateCruiseOrderViewModel.PONumber)}' in inputted data.");
            RuleForEach(x => x).SetValidator(new CreateCruiseOrderViewModelValidator(csfeApiClient, cruiseOrderRepository));
        }
    }
}
