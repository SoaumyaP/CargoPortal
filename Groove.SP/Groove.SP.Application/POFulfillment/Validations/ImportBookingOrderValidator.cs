using FluentValidation;
using Groove.SP.Application.BuyerComplianceService.Services.Interfaces;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Application.Utilities;
using Groove.SP.Core.Entities;
using Groove.SP.Infrastructure.CSFE;
using System.Linq;
using System.Threading.Tasks;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class ImportBookingOrderValidator : AbstractValidator<ImportBookingOrderViewModel>
    {
        private readonly ICSFEApiClient _csfeApiClient;

        public ImportBookingOrderValidator(
            string principalOrgCode,
            string shipFrom,
            string shipTo,
            ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;

            RuleFor(a => a.CustomerPONumber).NotEmpty();

            RuleFor(a => a.ProductCode).NotEmpty();

            RuleFor(a => a.FulfillmentUnitQty).NotEmpty();

            RuleFor(a => a.UnitUOM).NotEmpty();

            RuleFor(a => a.PackageUOM).NotEmpty();

            RuleFor(a => a.HsCode)
                .Must(c => CommonHelper.CheckValidHSCode(c))
                .WithMessage($"{nameof(ImportBookingOrderViewModel.HsCode)} is invalid.");

            RuleFor(a => a.CountryCodeOfOrigin)
                .MustAsync(async (countryCodeOfOrigin, cancellation) => (await csfeApiClient.GetCountryByCodeAsync(countryCodeOfOrigin)) != null)
                .WithMessage($"'{nameof(ImportBookingOrderViewModel.CountryCodeOfOrigin)}' is not existing on master data, must be country code.")
                .When(x => !string.IsNullOrWhiteSpace(x.CountryCodeOfOrigin));

            RuleFor(a => a.BookedPackage).NotEmpty();
        }
    }
}