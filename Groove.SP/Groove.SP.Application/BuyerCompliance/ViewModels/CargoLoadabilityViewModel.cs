using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Groove.SP.Application.BuyerCompliance.ViewModels
{
    public class CargoLoadabilityViewModel : ViewModelBase<CargoLoadabilityModel>
    {
        public string Name { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public EquipmentType EquipmentType { get; set; }

        public decimal CyMinimumCBM { get; set; }

        public decimal CyMaximumCBM { get; set; }

        public decimal? CfsMinimumCBM { get; set; }

        public decimal? CfsMaximumCBM { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
