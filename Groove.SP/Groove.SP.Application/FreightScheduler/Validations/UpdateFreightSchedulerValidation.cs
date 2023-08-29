using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.FreightScheduler.ViewModels;
using Groove.SP.Core.Entities;

namespace Groove.SP.Application.FreightScheduler.Validations
{
    public class UpdateFreightSchedulerValidation : BaseValidation<UpdateFreightSchedulerViewModel>
    {
        public UpdateFreightSchedulerValidation(bool isUpdating = false)
        {
            if (isUpdating)
            {
                ValidateUpdate();
            }
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.ETDDate).NotEmpty();
            RuleFor(a => a.ETADate).NotEmpty();
        }
    }
}
