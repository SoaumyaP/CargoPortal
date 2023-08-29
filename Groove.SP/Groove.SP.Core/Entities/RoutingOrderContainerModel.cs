using Groove.SP.Core.Models;

namespace Groove.SP.Core.Entities
{
    public class RoutingOrderContainerModel : Entity
    {
        public long Id { get; set; }
        public long RoutingOrderId { get; set; }
        public EquipmentType ContainerType { get; set; }
        public int? Quantity { get; set; }
        public decimal? Volume { get; set; }

        public virtual RoutingOrderModel RoutingOrder { get; set; }
    }
}
