using System;
using System.Collections.Generic;
using System.Text;

namespace Groove.SP.Infrastructure.CSFE.Configs
{
    public class CSFEApiSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string APIEndpoint { get; set; }
        public string TokenEndpoint { get; set; }
    }
}
