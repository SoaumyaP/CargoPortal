using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.RoutingOrderContainer.ViewModels
{
    public class RoutingOrderContainerViewModel : ViewModelBase<RoutingOrderContainerModel>
    {
        public long Id { get; set; }
        public long RoutingOrderId { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EquipmentType ContainerType { get; set; }

        public int? Quantity { get; set; }
        public decimal? Volume { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
