using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Models;
using System;
using System.Linq;

namespace Groove.SP.Application.CruiseOrders.Validations
{
    public class CruiseOrderItemViewModelValidator : BaseValidation<CruiseOrderItemViewModel>
    {
        public CruiseOrderItemViewModelValidator(System.Collections.Generic.IEnumerable<Infrastructure.CSFE.Models.Currency> currencies)
        {
            var currentCodes = currencies.Select(x => x.Code);
            RuleFor(x => x.POLine).NotNull();
            RuleFor(x => x.CurrencyCode)
                .Must(x => currentCodes.Contains(x))
                .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', currentCodes)}.")
                .When(x => x.CurrencyCode != null);
            RuleFor(x => x.UOM)
               .Must(x => Enum.TryParse(typeof(CruiseUOM), x, out var tmp))
               .When(x => x.UOM != null)
               .WithMessage($"Inputted data is not valid. Available values are: {string.Join(',', Enum.GetNames(typeof(CruiseUOM)))}.");

        }
    }
}
