using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.FreightScheduler.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.FreightScheduler.Validations
{
    public class FreightSchedulerValidation : BaseValidation<FreightSchedulerViewModel>
    {
        public FreightSchedulerValidation(bool isUpdating = false)
        {
            if (isUpdating)
            {
                ValidateUpdate();
            }
            else
            {
                ValidateAdd();
            }
        }

        private void ValidateAdd()
        {
            RuleFor(a => a.ModeOfTransport).NotEmpty();
            RuleFor(a => a.LocationFromName).NotNull();
            RuleFor(a => a.LocationToName).NotNull();
            RuleFor(a => a.ETDDate).NotNull();
            RuleFor(a => a.ETADate).NotNull();
            RuleFor(a => a.CarrierCode).NotNull();
            RuleFor(a => a.CarrierName).NotNull();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
        }
    }
}
