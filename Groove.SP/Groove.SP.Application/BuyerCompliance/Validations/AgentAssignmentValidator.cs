using FluentValidation;
using Groove.SP.Application.BuyerCompliance.ViewModels;
using Groove.SP.Application.Common;

namespace Groove.SP.Application.BuyerCompliance.Validations
{
    public class AgentAssignmentValidator : BaseValidation<AgentAssignmentViewModel>
    {
        public AgentAssignmentValidator()
        {
            RuleFor(x => x.AgentOrganizationId).NotNull();
        }
    }
}