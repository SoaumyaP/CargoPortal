using Newtonsoft.Json;

namespace Groove.SP.Infrastructure.EBookingManagementAPI.eSI
{
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class Container
    {
        public EquipmentType Type { get; set; }

        public string ContainerNo { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public string CarrierSoNo { get; set; }

        public string LoadingDate { get; set; }

        public string GateInDate { get; set; }

        public string PackType { get; set; }

        public string Temperature { get; set; }

        public BooleanOption? HeavyFlag { get; set; }

        public BooleanOption? FumigatedFlag { get; set; }

        public string Remarks { get; set; }
    }
}