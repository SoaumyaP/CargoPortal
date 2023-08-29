namespace Groove.SP.Application.Reports.ViewModels
{
    public class TelerikAccessTokenModel
    {
        public string _expires { get; set; }
        public string _issued { get; set; }
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string token_type { get; set; }
        public string userName { get; set; }
    }
}
