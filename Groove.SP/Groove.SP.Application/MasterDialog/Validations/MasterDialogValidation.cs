using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.MasterDialog.ViewModels;

namespace Groove.SP.Application.MasterDialog.Validations
{
    public class MasterDialogValidation : BaseValidation<MasterDialogViewModel>
    {
        public MasterDialogValidation(bool isUpdating = false) 
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
            RuleFor(a => a.DisplayOn).NotEmpty();
            RuleFor(a => a.FilterCriteria).NotEmpty();
            RuleFor(a => a.FilterValue).NotEmpty();
            RuleFor(a => a.Message).NotEmpty();
            RuleFor(a => a.Category).NotEmpty();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.DisplayOn).NotEmpty().When(x => x.IsPropertyDirty(nameof(MasterDialogViewModel.DisplayOn)));
            RuleFor(a => a.FilterCriteria).NotEmpty().When(x => x.IsPropertyDirty(nameof(MasterDialogViewModel.FilterCriteria)));
            RuleFor(a => a.FilterValue).NotEmpty().When(x => x.IsPropertyDirty(nameof(MasterDialogViewModel.FilterValue)));
            RuleFor(a => a.Message).NotEmpty().When(x => x.IsPropertyDirty(nameof(MasterDialogViewModel.Message)));
            RuleFor(a => a.Category).NotEmpty().When(x => x.IsPropertyDirty(nameof(MasterDialogViewModel.Category)));
        }
    }
}
