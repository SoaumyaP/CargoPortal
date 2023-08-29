using FluentValidation;
using Groove.SP.Application.Consolidation.ViewModels;

namespace Groove.SP.Application.Consolidation.Validations
{
    public class UpdateConsolidationValidator : AbstractValidator<UpdateConsolidationViewModel>
    {
        public UpdateConsolidationValidator()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.OriginCFS).NotEmpty();
            RuleFor(a => a.CFSCutoffDate).NotEmpty();
            RuleFor(a => a.EquipmentType).NotEmpty();
        }
    }
}
