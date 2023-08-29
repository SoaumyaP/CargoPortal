using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;

namespace Groove.CSFE.Application.Warehouses.ViewModels
{
    public class WarehouseViewModel : ViewModelBase<WarehouseModel>
    {
        public long Id { get; set; }
        public long LocationId { get; set; }
        public string WarehouseCode { get; set; }
        public string WarehouseName { get; set; }
        public string Address { get; set; }

        public LocationModel Location { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
