namespace Groove.CSFE.Core.Entities
{
    public class WarehouseModel : Entity
    {
        public long Id { get; set; }
        public long LocationId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string Address { get; set; }

        public virtual LocationModel Location { get; set; }
    }
}