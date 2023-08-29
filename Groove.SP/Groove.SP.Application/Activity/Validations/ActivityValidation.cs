using FluentValidation;
using Groove.SP.Application.Activity.ViewModels;
using Groove.SP.Application.Common;

namespace Groove.SP.Application.Activity.Validations
{
    public class ActivityValidation : BaseValidation<ActivityViewModel>
    {
        public ActivityValidation(bool isUpdating = false) 
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
            RuleFor(a => a.ActivityCode).NotEmpty();
            RuleFor(a => a.ActivityType).NotEmpty();
            RuleFor(a => a.ActivityDescription).NotEmpty();
            RuleFor(a => a.ActivityDate).NotEmpty();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.ActivityCode).NotEmpty().When(x => x.IsPropertyDirty("ActivityCode"));
            RuleFor(a => a.ActivityType).NotEmpty().When(x => x.IsPropertyDirty("ActivityType"));
            RuleFor(a => a.ActivityDescription).NotEmpty().When(x => x.IsPropertyDirty("ActivityDescription"));
            RuleFor(a => a.ActivityDate).NotEmpty().When(x => x.IsPropertyDirty("ActivityDate"));
        }
    }
}
