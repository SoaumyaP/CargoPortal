using FluentValidation;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Groove.SP.Application.ContractMaster.ViewModels
{
    public class CreateContractMasterViewModel: ContractMasterViewModel, IValidatableObject, IHasFieldStatus
    {
        public IEnumerable<ValidationResult> Validate(System.ComponentModel.DataAnnotations.ValidationContext validationContext)
        {
            var validator = (IValidator<CreateContractMasterViewModel>)validationContext.GetService(typeof(IValidator<CreateContractMasterViewModel>));
            var result = validator.Validate(this);
            foreach (var error in result.Errors)
            {
                yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
            }
        }

        // Not applied for API call
        public new string CustomerContractType
        {
            get
            {
                return string.Empty;
            }
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { set; get; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
