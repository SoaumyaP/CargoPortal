using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.ShipmentContact.ViewModels;
using Groove.SP.Core.Models;
using Groove.SP.Application.Converters;
using Newtonsoft.Json;

namespace Groove.SP.Application.ShipmentLoads.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class SimpleShipmentViewModel
    {
        public SimpleShipmentViewModel()
           : base()
        {
        }

        public long Id { get; set; }

        public string ShipmentNo { get; set; }

        public string OriginAgent => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.OriginAgent)?.CompanyName ?? string.Empty;

        public string DestinationAgent => Contacts?.FirstOrDefault(c => c.OrganizationRole == OrganizationRole.DestinationAgent)?.CompanyName ?? string.Empty;

        public ICollection<ShipmentContactViewModel> Contacts { get; set; }
    }
}
