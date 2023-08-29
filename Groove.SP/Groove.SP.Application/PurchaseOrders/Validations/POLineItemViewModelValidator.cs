using System;
using System.Linq;
using FluentValidation;

using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Infrastructure.CSFE;

namespace Groove.SP.Application.PurchaseOrders.Validations
{
    public class POLineItemViewModelValidator : BaseValidation<POLineItemViewModel>
    {
        public POLineItemViewModelValidator(ICSFEApiClient csfeApiClient)
        {
            var currencies = csfeApiClient.GetAllCurrenciesAsync().Result;
            var countries = csfeApiClient.GetAllCountriesAsync().Result;

            RuleFor(x => x.POLineKey).NotEmpty();
            RuleFor(a => a.OrderedUnitQty).NotEmpty();
            RuleFor(a => a.UnitUOM).NotEmpty();
            RuleFor(a => a.UnitPrice).NotEmpty();
            RuleFor(a => a.MinPackageQty).GreaterThanOrEqualTo(0).When(x => x.MinPackageQty.HasValue);
            RuleFor(a => a.MinOrderQty).GreaterThanOrEqualTo(0).When(x => x.MinOrderQty.HasValue);

            RuleFor(a => a.CurrencyCode)
                .Must(x => currencies.Any(r => r.Code.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Inputted data is not existing in system.")
                .When(x => !string.IsNullOrWhiteSpace(x.CurrencyCode));

            RuleFor(a => a.CountryCodeOfOrigin)
                .Must(x => countries.Any(r => r.Code.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("Inputted data is not existing in system.")
                .When(x => !string.IsNullOrWhiteSpace(x.CountryCodeOfOrigin));
        }
    }
}
