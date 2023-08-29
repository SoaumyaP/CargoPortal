using FluentValidation;
using Groove.SP.Application.Interfaces.Repositories;
using Groove.SP.Application.Itinerary.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace Groove.SP.Application.Itinerary.Validations
{
    public class UpdateItineraryViewModelValidator : ItineraryViewModelValidator<UpdateItineraryViewModel>
    {
        public UpdateItineraryViewModelValidator(ICSFEApiClient csfeApiClient, IFreightSchedulerRepository freightSchedulerRepository)
        {
            this.csfeApiClient = csfeApiClient;
            this._freightSchedulerRepository = freightSchedulerRepository;

            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.Sequence).NotNull().When(x => x.IsPropertyDirty("Sequence"));
            RuleFor(a => a.ModeOfTransport).NotNull().When(x => x.IsPropertyDirty("ModeOfTransport"));
            RuleFor(a => a.ETADate).NotEmpty().When(x => x.IsPropertyDirty("ETADate"));
            RuleFor(a => a.ETDDate).NotEmpty().When(x => x.IsPropertyDirty("ETDDate"));

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

        protected bool CheckVesselNameValid(UpdateItineraryViewModel itinerary)
        {
            var tryToCorrectVesselName = CorrectVesselName(itinerary);
            switch (tryToCorrectVesselName)
            {
                case "found":
                    return true;
                case "corrected":
                    // has to mark them as HasValue for AutoMapper working
                    itinerary.FieldStatus[nameof(UpdateItineraryViewModel.VesselName)] = FieldDeserializationStatus.HasValue;
                    itinerary.FieldStatus[nameof(UpdateItineraryViewModel.VesselFlight)] = FieldDeserializationStatus.HasValue;

                    return true;
                default:
                    return false;
            }
        }
    }
}
