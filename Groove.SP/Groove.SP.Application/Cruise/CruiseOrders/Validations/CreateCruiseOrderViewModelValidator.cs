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
    public class CreateCruiseOrderViewModelValidator : BaseValidation<CreateCruiseOrderViewModel>
    {
        private readonly IRepository<CruiseOrderModel> _cruiseOrderRepository;
        private IEnumerable<CruiseOrderModel> cruiseOrders;

        public CreateCruiseOrderViewModelValidator(ICSFEApiClient csfeApiClient, IRepository<CruiseOrderModel> cruiseOrderRepository)
        {
            _cruiseOrderRepository = cruiseOrderRepository;

            var currencies = csfeApiClient.GetAllCurrenciesAsync().Result;

            RuleFor(x => x.PONumber).NotEmpty();

            RuleFor(x => x)
                 .MustAsync(async (x, Cancellation) => !await CheckDuplicatePONumberAsync(x))
                 .WithMessage($"Duplicate '{nameof(CreateCruiseOrderViewModel.PONumber)}'.");

            RuleFor(x => x.POStatus)
                .IsInEnum()
                .When(x => x.POStatus.HasValue)
                .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', Enum.GetValues(typeof(CruiseOrderStatus)).Cast<int>())}.");

            RuleFor(x => x.Contacts)
                .Must(x => x != null && x.Any())
                .WithMessage($"'{nameof(CreateCruiseOrderViewModel.Contacts)}' is empty.");

            RuleForEach(x => x.Contacts)
                .SetValidator(new CruiseOrderContactViewModelValidator());

            RuleFor(x => x.Contacts)
              .Must(x => x.Any(c => !string.IsNullOrEmpty(c.OrganizationRole) ? c.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.InvariantCultureIgnoreCase) : false))
              .WithMessage("Principal is required in Contact.");

            RuleFor(x => x.Contacts)
              .Must(x => x.Any(c => !string.IsNullOrEmpty(c.OrganizationRole) ? c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.InvariantCultureIgnoreCase) : false))
              .WithMessage("Consignee is required in Contact.");

            RuleFor(x => x.Items)
                .Must(x => x != null && x.Any())
                .WithMessage($"'{nameof(CreateCruiseOrderViewModel.Items)}' is empty.");
            RuleFor(x => x.Items)
                .Must(x => !x.GroupBy(y => y.POLine).Any(g => g.Count() > 1))
                .When(x => x.Items != null && x.Items.Any())
                .WithMessage($"Duplicate '{nameof(CruiseOrderItemViewModel.POLine)}' in '{nameof(CreateCruiseOrderViewModel.Items)}'.");
            RuleForEach(x => x.Items)
                .SetValidator(new CruiseOrderItemViewModelValidator(currencies));

        }

        private async Task<bool> CheckDuplicatePONumberAsync(CreateCruiseOrderViewModel viewModel)
        {
            if (cruiseOrders == null)
            {
                cruiseOrders = await _cruiseOrderRepository.QueryAsNoTracking(c => c.PONumber == viewModel.PONumber, includes: c => c.Include(a => a.Contacts)).ToListAsync();
            }

            if (cruiseOrders?.Count() > 0)
            {
                var importPrincipalContacts = viewModel.Contacts.Where(x => x.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.InvariantCultureIgnoreCase));
                var importConsigneeContacts = viewModel.Contacts.Where(x => x.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.InvariantCultureIgnoreCase));

                foreach (var cruiseOrder in cruiseOrders)
                {
                    var cruiseOrderContacts = cruiseOrder.Contacts;
                    if (cruiseOrderContacts?.Count() > 0)
                    {
                        var isDuplicatedByPrincipal = cruiseOrderContacts.Any(c =>
                            importPrincipalContacts.Any(x => x.OrganizationId == c.OrganizationId) &&
                            c.OrganizationRole.Equals(OrganizationRole.Principal, StringComparison.InvariantCultureIgnoreCase)
                            );


                        var isDuplicatedByConsignee = cruiseOrderContacts.Any(c =>
                            importConsigneeContacts.Any(x => x.CompanyName.Equals(c.CompanyName, StringComparison.InvariantCultureIgnoreCase)) &&
                            c.OrganizationRole.Equals(OrganizationRole.Consignee, StringComparison.InvariantCultureIgnoreCase)
                            );

                        if (isDuplicatedByPrincipal && isDuplicatedByConsignee)
                        {
                            return true;
                        }
                    }
                }
                
            }
            return false;
        }
    }
}
