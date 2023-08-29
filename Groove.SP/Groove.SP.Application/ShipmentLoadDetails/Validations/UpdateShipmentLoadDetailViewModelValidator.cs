using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.ShipmentLoadDetails.ViewModels;

namespace Groove.SP.Application.ShipmentLoadDetails.Validations
{
    public class UpdateShipmentLoadDetailViewModelValidator : BaseValidation<UpdateShipmentLoadDetailViewModel>
    {
        public UpdateShipmentLoadDetailViewModelValidator()
        {
            RuleFor(a => a.Id).NotNull();
            RuleFor(a => a.Unit).NotNull();
            RuleFor(a => a.Package).NotNull();
            RuleFor(a => a.GrossWeight).NotNull();
            RuleFor(a => a.Volume).NotNull();
            RuleFor(a => a.Sequence).NotNull();
        }
    }
}
