namespace Groove.SP.Application.Users.ViewModels
{
    public class AccountCreationEmailTemplateViewModel
    {
        public string ActivationLink { get; set; }
        public string SigninLink { get; set; }
        public string ForgotPasswordLink { get; set; }
        public int ActivationExpireIn { get; set; }
    }
}
