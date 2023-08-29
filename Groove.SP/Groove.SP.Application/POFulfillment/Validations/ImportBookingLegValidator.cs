using FluentValidation;
using Groove.SP.Application.POFulfillment.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using Groove.SP.Infrastructure.CSFE.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.POFulfillment.Validations
{
    public class ImportBookingLegValidator : AbstractValidator<ImportBookingLegViewModel>
    {
        protected ICSFEApiClient _csfeApiClient;
        protected IEnumerable<Carrier> carriers;
        protected readonly Func<ModeOfTransportType, bool> IsSeaOrAir = (m) => m.ToString().Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) || m.ToString().Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);

        public ImportBookingLegValidator(ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;

            RuleFor(a => a.LoadingPort).NotEmpty();

            RuleFor(a => a.LoadingPort)
                .MustAsync(async (loadingPort, cancellation) => (await csfeApiClient.GetLocationByCodeAsync(loadingPort)) != null)
                .WithMessage("Inputted data is not existing in system.")
                .When(x => !string.IsNullOrWhiteSpace(x.LoadingPort) && IsSeaOrAir(x.ModeOfTransport));

            RuleFor(a => a.DischargePort).NotEmpty();

            RuleFor(a => a.DischargePort)
               .MustAsync(async (dischargePort, cancellation) => (await csfeApiClient.GetLocationByCodeAsync(dischargePort)) != null)
               .WithMessage("Inputted data is not existing in system.")
               .When(x => !string.IsNullOrWhiteSpace(x.DischargePort) && IsSeaOrAir(x.ModeOfTransport));

            RuleFor(a => a.CarrierCode)
                .NotNull()
                .When(x => IsSeaOrAir(x.ModeOfTransport));

            RuleFor(a => a)
                .Must(a => CheckCarrierCodeValid(a))
                .WithMessage($"'{nameof(ImportBookingLegViewModel.CarrierCode)}' is not existing in system.")
                .WithName(nameof(ImportBookingLegViewModel.CarrierCode))
                .When(x => x.CarrierCode != null && IsSeaOrAir(x.ModeOfTransport));

            RuleFor(a => a.ModeOfTransport)
                .Must(x => !x.ToString().Equals(ModeOfTransport.MultiModal, StringComparison.InvariantCultureIgnoreCase))
                .WithMessage("Mode of Transport does not allow Multi-modal.")
                .When(x => !string.IsNullOrWhiteSpace(x.ModeOfTransport.ToString()));
        }

        protected bool CheckCarrierCodeValid(ImportBookingLegViewModel leg)
        {
            if (string.IsNullOrWhiteSpace(leg.CarrierCode))
            {
                return false;
            }

            if (carriers == null)
            {
                carriers = _csfeApiClient.GetAllCarriesAsync().Result;
            }

            return carriers.Any(c => c.CarrierCode == leg.CarrierCode && !string.IsNullOrEmpty(c.ModeOfTransport) && c.ModeOfTransport.Equals(leg.ModeOfTransport.ToString() ?? string.Empty, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
