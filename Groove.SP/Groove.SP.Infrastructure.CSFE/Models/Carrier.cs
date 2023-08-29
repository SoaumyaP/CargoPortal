namespace Groove.SP.Infrastructure.CSFE.Models
{
    public class Carrier
    {
        public long Id { get; set; }
        public string CarrierCode { get; set; }
        public string Name { get; set; }
        public string ModeOfTransport { get; set; }
        public CarrierStatus Status { get; set; }
    }
    public enum CarrierStatus
    {
        Inactive = 0,
        Active = 1
    }
}
