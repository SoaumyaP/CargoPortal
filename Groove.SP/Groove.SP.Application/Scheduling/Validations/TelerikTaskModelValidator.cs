using FluentValidation;
using Groove.SP.Application.Scheduling.ViewModels;

namespace Groove.SP.Application.Scheduling.Validations
{
    public class TelerikTaskModelValidator : AbstractValidator<TelerikTaskModel>
    {
        public TelerikTaskModelValidator()
        {
            RuleFor(a => a.Category).NotEmpty().NotNull();
            RuleFor(a => a.CategoryId).NotEmpty().NotNull();
            RuleFor(a => a.DocumentFormat).NotEmpty().NotNull();
            RuleFor(a => a.DocumentFormatDescr).NotEmpty().NotNull();
            RuleFor(a => a.MailTemplateBody).NotEmpty().NotNull();
            RuleFor(a => a.MailTemplateSubject).NotEmpty().NotNull();
            RuleFor(a => a.Name).NotEmpty().NotNull();
            RuleFor(a => a.Report).NotEmpty().NotNull();
            RuleFor(a => a.ReportId).NotEmpty().NotNull();
        }
    }
}
