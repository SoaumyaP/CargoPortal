using FluentValidation;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Groove.SP.Application.Itinerary.Validations
{
    public class CreateItineraryViewModelValidator : ItineraryViewModelValidator<CreateItineraryViewModel>
    {
        public CreateItineraryViewModelValidator(ICSFEApiClient csfeApiClient, IFreightSchedulerRepository freightSchedulerRepository)
        {
            this.csfeApiClient = csfeApiClient;
            this._freightSchedulerRepository = freightSchedulerRepository;

            RuleFor(a => a.Id).NotNull();
            RuleFor(a => a.Sequence).NotNull();
            RuleFor(a => a.ModeOfTransport).NotNull();
            RuleFor(a => a.ETADate).NotNull();
            RuleFor(a => a.ETDDate).NotNull();

            // Check VesselName if "Sea"
            RuleFor(a => a)
                .Must(a => CheckVesselNameValid(a))
                .WithMessage("Vessel name is not existing on master data.")
                .When(a => !string.IsNullOrEmpty(a.VesselName)
                    && !string.IsNullOrEmpty(a.ModeOfTransport)
                    && a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            RegisterBussinessValidations();

            ValidateATDATA();
        }

        protected bool CheckVesselNameValid(CreateItineraryViewModel itinerary)
        {
            var tryToCorrectVesselName = CorrectVesselName(itinerary);
            switch (tryToCorrectVesselName)
            {
                case "found":
                    return true;
                case "corrected":
                    return true;
                default:
                    return false;
            }
        }
    }
}
