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
    public class UpdateMasterBillViewModelValidator : MasterBillViewModelValidator<UpdateMasterBillOfLadingViewModel>
    {
        public UpdateMasterBillViewModelValidator(ICSFEApiClient csfeApiClient)
        {
            this.csfeApiClient = csfeApiClient;

            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.MasterBillOfLadingNo).NotEmpty().When(x => x.IsPropertyDirty("MasterBillOfLadingNo"));
            RuleFor(a => a.ExecutionAgentId).NotEmpty().When(x => x.IsPropertyDirty("ExecutionAgentId"));
            RuleFor(a => a.IssueDate).NotEmpty().When(x => x.IsPropertyDirty("IssueDate"));
            RuleFor(a => a.OnBoardDate).NotEmpty().When(x => x.IsPropertyDirty("OnBoardDate"));

            // Check VesselName if "Sea"
            RuleFor(a => a)
                .Must(a => CheckVesselNameValid(a))
                .WithMessage("Vessel name is not existing on master data.")
                .When(a => !string.IsNullOrEmpty(a.Vessel)
                    && !string.IsNullOrEmpty(a.ModeOfTransport)
                    && a.ModeOfTransport.Equals(ModeOfTransport.Sea, StringComparison.InvariantCultureIgnoreCase));

            RegisterBussinessValidations();
        }

        protected bool CheckVesselNameValid(UpdateMasterBillOfLadingViewModel masterbillViewModel)
        {
            var tryToCorrectVesselName = CorrectVesselName(masterbillViewModel);
            switch (tryToCorrectVesselName)
            {
                case "found":
                    return true;
                case "corrected":
                    // has to mark them as HasValue for AutoMapper working
                    masterbillViewModel.FieldStatus[nameof(UpdateMasterBillOfLadingViewModel.Vessel)] = FieldDeserializationStatus.HasValue;
                    masterbillViewModel.FieldStatus[nameof(UpdateMasterBillOfLadingViewModel.VesselFlight)] = FieldDeserializationStatus.HasValue;

                    return true;
                default:
                    return false;
            }
        }
    }
}
