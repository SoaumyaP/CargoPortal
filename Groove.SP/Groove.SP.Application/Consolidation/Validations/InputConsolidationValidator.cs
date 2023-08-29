using Groove.SP.Application.Common;
using FluentValidation;
using System.Linq;
using Groove.SP.Core.Models;
using Groove.SP.Application.Consolidation.ViewModels;

namespace Groove.SP.Application.Consolidation.Validations
{
    public class InputConsolidationValidator : BaseValidation<InputConsolidationViewModel>
    {
        public InputConsolidationValidator()
        {
            RuleFor(a => a.ConsignmentId).NotEmpty();
            RuleFor(a => a.OriginCFS).NotEmpty();
            RuleFor(a => a.CFSCutoffDate).NotEmpty();
            RuleFor(a => a.EquipmentType).NotEmpty();
        }
    }
}
