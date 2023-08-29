using System;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.PurchaseOrders.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;

namespace Groove.SP.Application.PurchaseOrders.Validations
{
    public class ExcelPOViewModelValidator : AbstractValidator<ExcelPOViewModel>
    {
        public ExcelPOViewModelValidator(ICSFEApiClient csfeApiClient)
        {
            var ports = csfeApiClient.GetAllPortsAsync().Result;
            var currencies = csfeApiClient.GetAllCurrenciesAsync().Result;
            var carries = csfeApiClient.GetAllCarriesAsync().Result;

            RuleFor(a => a.PONumber).NotEmpty();
            RuleFor(a => a.POIssueDate).NotNull();
            RuleFor(a => a.NumberOfLineItems).GreaterThan(0).When(x => x.NumberOfLineItems.HasValue);

            RuleFor(a => a.GatewayCode)
                .Must(x => ports.Any(r => r.AirportCode.Equals(x, StringComparison.OrdinalIgnoreCase) ||
                                          r.SeaportCode.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("'GatewayCode':;importLog.inputtedDataNotExisting")
                .When(x => !string.IsNullOrWhiteSpace(x.GatewayCode));

            RuleFor(a => a.CarrierCode)
                .Must(x => carries.Any(r => r.CarrierCode.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("'CarrierCode':;importLog.inputtedDataNotExisting")
                .When(x => !string.IsNullOrWhiteSpace(x.CarrierCode));

            RuleFor(a => a.PaymentCurrencyCode)
                .Must(x => currencies.Any(r => r.Code.Equals(x, StringComparison.OrdinalIgnoreCase)))
                .WithMessage("'PaymentCurrencyCode':;importLog.inputtedDataNotExisting")
                .When(x => !string.IsNullOrWhiteSpace(x.PaymentCurrencyCode));

            RuleFor(a => a.CargoReadyDate)
                .Must((viewModel, property) => viewModel.CargoReadyDate <= viewModel.ExpectedShipDate)
                .When(x => !string.IsNullOrWhiteSpace(x.CargoReadyDate?.ToString()) && !string.IsNullOrWhiteSpace(x.ExpectedShipDate?.ToString()))
                .WithMessage($"importLog.cargoReadyDateLaterThanExpectedShipDate");

            RuleFor(a => a.ExpectedShipDate)
                .Must((viewModel, property) => viewModel.ExpectedShipDate >= viewModel.CargoReadyDate)
                .When(x => !string.IsNullOrWhiteSpace(x.ExpectedShipDate?.ToString()) && !string.IsNullOrWhiteSpace(x.CargoReadyDate?.ToString()))
                .WithMessage($"importLog.expectedShipDateEarlierThanCargoReadyDate");

            RuleFor(a => a.ExpectedShipDate)
                .Must((viewModel, property) => viewModel.ExpectedShipDate <= viewModel.ExpectedDeliveryDate)
                .When(x => !string.IsNullOrWhiteSpace(x.ExpectedShipDate?.ToString()) && !string.IsNullOrWhiteSpace(x.ExpectedDeliveryDate?.ToString()))
                .WithMessage($"importLog.expectedShipDateLaterThanExpectedDeliveryDate");

            RuleFor(a => a.ExpectedDeliveryDate)
                .Must((viewModel, property) => viewModel.ExpectedDeliveryDate >= viewModel.ExpectedShipDate)
                .When(x => !string.IsNullOrWhiteSpace(x.ExpectedDeliveryDate?.ToString()) && !string.IsNullOrWhiteSpace(x.ExpectedShipDate?.ToString()))
                .WithMessage($"importLog.expectedDeliveryDateEarlierThanExpectedShipDate");
        }
    }
}
