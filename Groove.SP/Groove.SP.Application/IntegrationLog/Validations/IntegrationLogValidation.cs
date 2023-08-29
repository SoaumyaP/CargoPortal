using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.IntegrationLog.ViewModels;

namespace Groove.SP.Application.IntegrationLog.Validations
{
    public class IntegrationLogValidation : BaseValidation<IntegrationLogViewModel>
    {
        public IntegrationLogValidation()
        {
        }
    }
}
