using Groove.SP.Application.ArticleMaster.Validations;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.ArticleMaster.ViewModels;
public class UpdateArticleMasterViewModel : ArticleMasterViewModel, IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var result = new ArticleMasterValidation(true).Validate(this); ;
        foreach (var error in result.Errors)
        {
            yield return new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
        }
    }
}