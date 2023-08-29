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
    public class EdiSonConfirmPOFFLegValidator : AbstractValidator<Leg>
    {
        protected ICSFEApiClient _csfeApiClient;
        protected IEnumerable<Carrier> carriers;
        protected readonly Func<ModeOfTransportType, bool> IsSeaOrAir = (m) => m.ToString().Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase) || m.ToString().Equals(ModeOfTransport.Air, StringComparison.InvariantCultureIgnoreCase);

        public EdiSonConfirmPOFFLegValidator(ICSFEApiClient csfeApiClient)
        {
            _csfeApiClient = csfeApiClient;

            RuleFor(a => a.LoadingPortCode).NotEmpty().MaximumLength(10);

            RuleFor(a => a.LoadingPortCode)
                .MustAsync(async (loadingPortCode, cancellation) => (await csfeApiClient.GetLocationByCodeAsync(loadingPortCode)) != null)
                .WithMessage("Inputted data is not existing in system.")
                .When(x => !string.IsNullOrWhiteSpace(x.LoadingPortCode) && IsSeaOrAir(x.ModeOfTransport));

            RuleFor(a => a.DischargePortCode).NotEmpty().MaximumLength(10);

            RuleFor(a => a.DischargePortCode)
               .MustAsync(async (dischargePortCode, cancellation) => (await csfeApiClient.GetLocationByCodeAsync(dischargePortCode)) != null)
               .WithMessage("Inputted data is not existing in system.")
               .When(x => !string.IsNullOrWhiteSpace(x.DischargePortCode) && IsSeaOrAir(x.ModeOfTransport));

            RuleFor(a => a.CarrierCode)
                .NotNull()
                .When(x => IsSeaOrAir(x.ModeOfTransport));

            RuleFor(a => a)
                .Must(a => CheckCarrierCodeValid(a))
                .WithMessage("Carrier code is not existing in system.")
                .When(x => x.CarrierCode != null && IsSeaOrAir(x.ModeOfTransport));

            RuleFor(a => a.ModeOfTransport)
                .Must(x => !x.ToString().Equals(ModeOfTransport.MultiModal, StringComparison.InvariantCultureIgnoreCase))
                .WithMessage("Mode of Transport does not allow Multi-modal.")
                .When(x => !string.IsNullOrWhiteSpace(x.ModeOfTransport.ToString()));
        }

        protected bool CheckCarrierCodeValid(Leg leg)
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