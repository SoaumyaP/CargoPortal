namespace Groove.CSFE.Core.Entities
{
    public class CarrierQueryModel
    {
        public long Id { get; set; }
        public string CarrierCode { get; set; }
        public string CarrierCodeNumber { get; set; }
        public string ModeOfTransport { get; set; }
        public string Name { get; set; }
        public int? CarrierNumber { get; set; }
        public byte Status { get; set; }
    }
}
