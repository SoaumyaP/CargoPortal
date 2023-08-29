using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Collections.Generic;

namespace Groove.SP.Application.POFulfillmentCargoReceive.ViewModels
{
    public class POFulfillmentCargoReceiveViewModel : ViewModelBase<POFulfillmentCargoReceiveModel>
    {
        public long Id { get; set; }
        public string CRNo { get; set; }
        public string PlantNo { get; set; }
        public string HouseNo { get; set; }
        public long POFulfillmentId { get; set; }
        public ICollection<POFulfillmentCargoReceiveItemModel> CargoReceiveItems { get; set; }
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
