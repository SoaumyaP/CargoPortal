using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Vessels.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.CSFE.Application.Vessels.Validations
{
    public class VesselValidation : BaseValidation<VesselViewModel>
    {
        public VesselValidation(bool isUpdating = false)
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
            RuleFor(a => a.Name).NotEmpty();
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Name).NotEmpty().When(x => x.IsPropertyDirty("Name"));
        }
    }
}
