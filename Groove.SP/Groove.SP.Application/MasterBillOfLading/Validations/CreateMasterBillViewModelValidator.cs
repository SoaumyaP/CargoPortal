using FluentValidation;
using Groove.SP.Application.Itinerary.Validations;
using Groove.SP.Application.MasterBillOfLading.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Infrastructure.CSFE;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Application.MasterBillOfLading.Validations
{
    public class CreateMasterBillViewModelValidator : MasterBillViewModelValidator<CreateMasterBillOfLadingViewModel>
    {
        public CreateMasterBillViewModelValidator(ICSFEApiClient csfeApiClient)
        {
            this.csfeApiClient = csfeApiClient;

            RuleFor(a => a.MasterBillOfLadingNo).NotEmpty();
            RuleFor(a => a.ExecutionAgentId).NotEmpty();
            RuleFor(a => a.IssueDate).NotEmpty();
            RuleFor(a => a.OnBoardDate).NotEmpty();

            // Check VesselName if "Sea"
            RuleFor(a => a)
                .Must(a => CheckVesselNameValid(a))
                .WithMessage("Vessel name is not existing on master data.")
                .When(a => !string.IsNullOrEmpty(a.Vessel)
                    && !string.IsNullOrEmpty(a.ModeOfTransport)
                    && a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            RegisterBussinessValidations();
        }

        protected bool CheckVesselNameValid(CreateMasterBillOfLadingViewModel masterbillViewModel)
        {
            var tryToCorrectVesselName = CorrectVesselName(masterbillViewModel);
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
