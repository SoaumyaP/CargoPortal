using FluentValidation;

namespace Groove.SP.Application.Common
{
    public static class CommonValidators
    {
        public static bool CheckValidEmails(string arg, char seperator = ',')
        {
            var isValid = true;

            if (arg == null) return isValid;

            var list = arg.Split(seperator);
            
            var emailValidator = new EmailValidator();

            foreach (var t in list)
            {
                isValid = emailValidator.Validate(new EmailModel { Email = t.Trim() }).IsValid;
                if (!isValid)
                    break;
            }

            return isValid;
        }
    }

    public class EmailValidator : AbstractValidator<EmailModel>
    {
        public EmailValidator()
        {
            RuleFor(x => x.Email).EmailAddress();
        }
    }

    public class EmailModel
    {
        public string Email { get; set; }
    }
}
