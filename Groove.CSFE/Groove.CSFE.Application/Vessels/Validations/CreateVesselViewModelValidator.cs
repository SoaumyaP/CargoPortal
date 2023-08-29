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
    public class CreateVesselViewModelValidator : VesselViewModelValidator<CreateVesselViewModel>
    {
        public CreateVesselViewModelValidator(IRepository<VesselModel> vesselRepository)
        {
            _vesselRepository = vesselRepository;

            RuleFor(a => a.Name).NotEmpty();
            RuleFor(a => a.Status).NotNull();
            RuleFor(a => a.IsRealVessel).NotNull();
            RuleFor(a => a.CreatedBy).NotNull();
            RuleFor(a => a.CreatedDate).NotEmpty();

            CheckDuplicateCode(false);
        }
    }
}
