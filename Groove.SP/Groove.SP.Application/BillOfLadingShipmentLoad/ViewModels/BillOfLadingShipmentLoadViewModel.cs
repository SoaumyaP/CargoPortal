using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.BillOfLadingShipmentLoad.Validations;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.BillOfLadingShipmentLoad.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class BillOfLadingShipmentLoadViewModel : ViewModelBase<BillOfLadingShipmentLoadModel>, IHasFieldStatus
    {
        public long BillOfLadingId { get; set; }

        public long ShipmentLoadId { get; set; }

        public long? ContainerId { get; set; }

        public long? ConsolidationId { get; set; }

        public long? MasterBillOfLadingId { get; set; }

        public bool IsFCL { get; set; }

        public BillOfLadingModel BillOfLading { get; set; }

        public ShipmentLoadModel ShipmentLoad { get; set; }

        public ContainerModel Container { get; set; }

        public ConsolidationModel Consolidation { get; set; }

        public MasterBillOfLadingModel MasterBillOfLadingModel { get; set; }

        public BillOfLadingShipmentLoadViewModel()
            : base()
        { }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new BillOfLadingShipmentLoadValidation(isUpdating).ValidateAndThrow(this);
        }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
