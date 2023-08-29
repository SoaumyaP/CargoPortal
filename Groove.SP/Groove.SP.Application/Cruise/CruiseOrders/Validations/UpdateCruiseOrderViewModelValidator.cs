using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.CruiseOrders.ViewModels;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Entities.Cruise;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.CruiseOrders.Validations
{
    public class UpdateCruiseOrderViewModelValidator : BaseValidation<UpdateCruiseOrderViewModel>
    {
        private readonly IRepository<CruiseOrderModel> _cruiseOrderRepository;
        private IEnumerable<CruiseOrderModel> cruiseOrders;

        public UpdateCruiseOrderViewModelValidator(ICSFEApiClient csfeApiClient, IRepository<CruiseOrderModel> cruiseOrderRepository)
        {
            _cruiseOrderRepository = cruiseOrderRepository;
            var currencies = csfeApiClient.GetAllCurrenciesAsync().Result;

            RuleFor(x => x.PONumber)
                .NotEmpty()
                .When(x => x.IsPropertyDirty(nameof(x.PONumber)));

            RuleFor(x => x.POStatus)
                .NotEmpty()
                .IsInEnum()
                .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', Enum.GetValues(typeof(CruiseOrderStatus)).Cast<int>())}.")
                .When(x => x.IsPropertyDirty(nameof(x.POStatus)));

            RuleFor(x => x.Contacts)
              .Must(x => x != null && x.Any(c => !string.IsNullOrEmpty(c.OrganizationRole) ? c.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.InvariantCultureIgnoreCase) : false))
              .WithMessage("Principal is required in Contact.");

            RuleFor(x => x.Contacts)
             .Must(x => x != null && x.Any(c => !string.IsNullOrEmpty(c.OrganizationRole) ? c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.InvariantCultureIgnoreCase) : false))
             .WithMessage("Consignee is required in Contact.");

            RuleFor(x => x.Contacts)
                .Must(x => x != null && x.Any())
                .WithMessage($"'{nameof(UpdateCruiseOrderViewModel.Contacts)}' is empty.")
                .When(x => x.IsPropertyDirty(nameof(x.Contacts)));
            RuleForEach(x => x.Contacts)
                .SetValidator(new CruiseOrderContactViewModelValidator())
                .When(x => x.IsPropertyDirty(nameof(x.Contacts)));

            RuleFor(x => x.Items)
                .Must(x => x != null && x.Any())
                .WithMessage($"'{nameof(UpdateCruiseOrderViewModel.Items)}' is empty.")
                .When(x => x.IsPropertyDirty(nameof(x.Items)));
            RuleFor(x => x.Items)
                .Must(x => !x.GroupBy(y => y.POLine).Any(g => g.Count() > 1))
                .WithMessage($"Duplicate '{nameof(CruiseOrderItemViewModel.POLine)}' in '{nameof(UpdateCruiseOrderViewModel.Items)}'.")
                .When(x => x.IsPropertyDirty(nameof(x.Items))); ;
            RuleForEach(x => x.Items)
                .SetValidator(new CruiseOrderItemViewModelValidator(currencies))
                .When(x => x.IsPropertyDirty(nameof(x.Items)));

        }
    }
}
