using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consolidation.ViewModels;
using Groove.SP.Core.Models;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.Consolidation.Validations
{
    public class ConsolidationValidation : BaseValidation<ConsolidationViewModel>
    {
        private List<int> StageEnumValues
        {
            get
            {
                return Enum.GetValues(typeof(ConsolidationStage))
                    .OfType<ConsolidationStage>()
                    .Select(s => (int)s).ToList();
            }

        }
        public ConsolidationValidation(bool isUpdating = false)
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
            RuleFor(a => a.CFSCutoffDate).NotNull();
            RuleFor(a => a.TotalGrossWeight).NotNull();
            RuleFor(a => a.TotalNetWeight).NotNull();
            RuleFor(a => a.TotalPackage).NotNull();
            RuleFor(a => a.TotalVolume).NotNull();
            RuleFor(a => a.ConsolidationNo).NotNull();
            RuleFor(a => a.Stage)
               .Must(x => StageEnumValues.Contains((int)x))
               .When(x => x.IsPropertyDirty(nameof(ConsolidationViewModel.Stage)))
               .WithMessage($"Inputted data is not valid. Available values are: {string.Join(", ", StageEnumValues)}.");
        }

        private void ValidateUpdate()
        {
            RuleFor(a => a.Id).NotEmpty();
            RuleFor(a => a.CFSCutoffDate).NotEmpty().When(x => x.IsPropertyDirty(nameof(ConsolidationViewModel.CFSCutoffDate)));
            RuleFor(a => a.TotalGrossWeight).NotNull().When(x => x.IsPropertyDirty(nameof(ConsolidationViewModel.TotalGrossWeight)));
            RuleFor(a => a.TotalNetWeight).NotNull().When(x => x.IsPropertyDirty(nameof(ConsolidationViewModel.TotalNetWeight)));
            RuleFor(a => a.TotalPackage).NotNull().When(x => x.IsPropertyDirty(nameof(ConsolidationViewModel.TotalPackage)));
            RuleFor(a => a.TotalVolume).NotNull().When(x => x.IsPropertyDirty(nameof(ConsolidationViewModel.TotalVolume)));
            RuleFor(a => a.ConsolidationNo).NotNull().When(x => x.IsPropertyDirty(nameof(ConsolidationViewModel.ConsolidationNo)));
            RuleFor(a => a.Stage)
               .Must(x => StageEnumValues.Contains((int)x))
               .When(x => x.IsPropertyDirty(nameof(ConsolidationViewModel.Stage)))
               .WithMessage($"Inputted data is not valid. Available values are: {string.Join(", ", StageEnumValues)}.");
        }
    }
}
