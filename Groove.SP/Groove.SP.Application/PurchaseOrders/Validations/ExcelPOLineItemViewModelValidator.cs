using FluentValidation;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Linq;

namespace Groove.SP.Application.PurchaseOrders.Validations
{
    public class ExcelPOLineItemViewModelValidator : AbstractValidator<ExcelPOLineItemViewModel>
    {
        public ExcelPOLineItemViewModelValidator(ICSFEApiClient csfeApiClient)
        {
            var currencies = csfeApiClient.GetAllCurrenciesAsync().Result;
            var countries = csfeApiClient.GetAllCountriesAsync().Result;

            RuleFor(a => a.PONumber).NotEmpty();
            RuleFor(a => a.ProductCode).NotNull();
            RuleFor(a => a.OrderedUnitQty).NotNull().GreaterThan(0);
            RuleFor(a => a.UnitUOM).NotEmpty();
            RuleFor(a => a.UnitPrice).NotNull();
            RuleFor(a => a.CurrencyCode).NotEmpty();
            RuleFor(a => a.LineOrder).GreaterThan(0).NotEmpty();
            RuleFor(a => a.MinPackageQty).GreaterThanOrEqualTo(0).When(x => x.MinPackageQty.HasValue);
            RuleFor(a => a.MinOrderQty).GreaterThanOrEqualTo(0).When(x => x.MinOrderQty.HasValue);

            RuleFor(a => a.UnitUOM).Must(s => Enum.TryParse(s.Trim(), true, out UnitUOMType result))
                .WithMessage("'UnitUOM':;importLog.inputtedDataNotExisting");

            RuleFor(a => a.PackageUOM).Must(s => Enum.TryParse(s.Trim(), true, out PackageUOMType result))
                .WithMessage("'PackageUOM':;importLog.inputtedDataNotExisting")
                .When(s => !string.IsNullOrWhiteSpace(s.PackageUOM));

            RuleFor(a => a.CurrencyCode)
                .Must(x => currencies.Any(r => r.Code.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("'CurrencyCode':;importLog.inputtedDataNotExisting")
                .When(x => !string.IsNullOrWhiteSpace(x.CurrencyCode));

            RuleFor(a => a.CountryCodeOfOrigin)
                .Must(x => countries.Any(r => r.Code.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("'CountryCodeOfOrigin':;importLog.inputtedDataNotExisting")
                .When(x => !string.IsNullOrWhiteSpace(x.CountryCodeOfOrigin));
        }
    }
}
