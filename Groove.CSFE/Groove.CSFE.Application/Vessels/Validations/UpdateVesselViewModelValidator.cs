using FluentValidation;
using Groove.CSFE.Application.Interfaces.Repositories;
using Groove.CSFE.Application.Vessels.ViewModels;
using Groove.CSFE.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Groove.CSFE.Application.Vessels.Validations
{
    public class UpdateVesselViewModelValidator : VesselViewModelValidator<UpdateVesselViewModel>
    {
        public UpdateVesselViewModelValidator(IRepository<VesselModel> vesselRepository)
        {
            _vesselRepository = vesselRepository;

            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.Name).NotEmpty().When(x => x.IsPropertyDirty(nameof(x.Name)));
            RuleFor(a => a.Status).NotNull().When(x => x.IsPropertyDirty(nameof(x.Status))); 
            RuleFor(a => a.IsRealVessel).NotNull().When(x => x.IsPropertyDirty(nameof(x.IsRealVessel)));
            
            CheckDuplicateCode(true);
        }
    }
}
