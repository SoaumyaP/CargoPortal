using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class POFulfillmentCargoReceiveModel: Entity
    {
        public long Id { get; set; }
        public string CRNo { get; set; }
        public string PlantNo { get; set; }
        public string HouseNo { get; set; }
        public long POFulfillmentId { get; set; }
        public ICollection<POFulfillmentCargoReceiveItemModel> CargoReceiveItems { get; set; }
    }
}