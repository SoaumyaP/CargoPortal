namespace Groove.CSFE.Core.Entities
{
    public class CarrierModel : Entity
    {
        public long Id { get; set; }
        public string CarrierCode { get; set; }
        public int? CarrierNumber { get; set; }
        public string ModeOfTransport { get; set; }
        public string Name { get; set; }
        public CarrierStatus Status { get; set; }
    }
}
