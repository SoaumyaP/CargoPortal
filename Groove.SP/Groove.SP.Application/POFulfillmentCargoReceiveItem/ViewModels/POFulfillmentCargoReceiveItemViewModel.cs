using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using System;

namespace Groove.SP.Application.POFulfillmentCargoReceiveItem.ViewModels
{
    public class POFulfillmentCargoReceiveItemViewModel : ViewModelBase<POFulfillmentCargoReceiveItemModel>
    {
        public long Id { get; set; }
        public string StyleNo { get; set; }
        public string ColourCode { get; set; }
        public string SizeCode { get; set; }
        public int? Length { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public decimal? Volume { get; set; }
        public decimal? NetWeight { get; set; }
        public decimal? GrossWeight { get; set; }
        public string DGFlag { get; set; }
        public string Reason { get; set; }
        public int? InnerPacakageQty { get; set; }
        public int? OuterPackageQty { get; set; }
        public int Quantity { get; set; }
        public UnitUOMType UnitUOM { get; set; }
        public DateTime InDate { get; set; }

        public POFulfillmentCargoReceiveModel POFulfillmentCargoReceive { get; set; }
        public POFulfillmentOrderModel POFulfillmentOrder { get; set; }

        #region Foreign keys
        public long POFulfillmentOrderId { get; set; }
        public long POFulfillmentCargoReceiveId { get; set; }
        #endregion
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new NotImplementedException();
        }
    }
}
