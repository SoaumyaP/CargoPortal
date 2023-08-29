using FluentValidation;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Linq;

namespace Groove.SP.Application.Itinerary.Validations;

public class ImportItineraryValidator : ItineraryViewModelValidator<ImportItineraryViewModel>
{
    public ImportItineraryValidator(ICSFEApiClient csfeApiClient)
    {
        this.csfeApiClient = csfeApiClient;

        RuleFor(x => x.Sequence).NotEmpty();
        RuleFor(x => x.ModeOfTransport).NotEmpty();

        RuleFor(x => x.ModeOfTransport)
            .Must(x => ValidModeOfTransports.Contains(x, StringComparer.InvariantCultureIgnoreCase))
            .WithMessage($"'{nameof(ImportItineraryViewModel.ModeOfTransport)}' must be '{string.Join(", ", ValidModeOfTransports)}'")
            .When(x => !string.IsNullOrWhiteSpace(x.ModeOfTransport));

        RuleFor(x => x.BillOfLadingType)
            .Must(x => ValidHouseBillType.Contains(x))
            .WithMessage($"'{nameof(ImportItineraryViewModel.BillOfLadingType)}' must be '{string.Join(", ", ValidHouseBillType)}'")
            .When(x => !string.IsNullOrWhiteSpace(x.BillOfLadingType));

        RuleFor(x => x.CarrierCode).NotEmpty();
        RuleFor(x => x)
            .Must(x => CheckCarrierCodeValid(x.CarrierCode, x.ModeOfTransport))
            .WithName(nameof(ImportItineraryViewModel.CarrierCode))
            .WithMessage($"'{nameof(ImportItineraryViewModel.CarrierCode)}' is not existing on master data.")
            .When(x => !string.IsNullOrWhiteSpace(x.CarrierCode) && !string.IsNullOrWhiteSpace(x.ModeOfTransport));

        RuleFor(x => x.LoadingPort).NotEmpty();
        RuleFor(x => x.DischargePort).NotEmpty();
        RuleFor(x => x.ETDDate).NotEmpty();
        RuleFor(x => x.ETADate).NotEmpty();

        RuleFor(x => x.VesselName).NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.ModeOfTransport) && x.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

        RuleFor(x => x.VesselName)
            .Must(x => CheckVesselNameValid(x))
            .WithMessage($"'{nameof(ImportItineraryViewModel.VesselName)}' is not existing on master data.")
            .When(x => !string.IsNullOrEmpty(x.VesselName));

        RuleFor(x => x.Voyage).NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.ModeOfTransport) && x.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

        RuleFor(x => x.FlightNumber).NotEmpty()
            .When(x => !string.IsNullOrEmpty(x.ModeOfTransport) && x.ModeOfTransport.Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase));
    }
}

