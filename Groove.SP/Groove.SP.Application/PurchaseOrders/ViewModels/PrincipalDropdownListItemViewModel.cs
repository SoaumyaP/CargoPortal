using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    public class PrincipalDropdownListItemViewModel : DropDownListItem
    {
        // From dbo.BuyerCompliances
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsAllowMissingPO { get; set; }
    }
}
