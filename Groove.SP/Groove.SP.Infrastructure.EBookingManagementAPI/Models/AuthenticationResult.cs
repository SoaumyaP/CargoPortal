using Groove.Infrastructure.APIClientCore;

namespace Groove.SP.Infrastructure.EBookingManagementAPI
{
    public class AuthenticationResult : ResponseResultBase
    {
        public AuthenticationResultContent Result { get; set; }
    }

    public class AuthenticationResultContent
    {
        public string CurrSessionID { get; set; }
    }

}
