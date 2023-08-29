using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Groove.SP.Application.RoutingOrderContainer.ViewModels
{
    public class ImportRoutingOrderContainerViewModel : ViewModelBase<RoutingOrderContainerModel>
    {
        private readonly CultureInfo _culture = new("en") { NumberFormat = { NumberDecimalSeparator = "," } };

        [XmlElement("ContainerType")]
        public string ContainerType { get; set; }

        [XmlElement("Quantity")]
        public string QuantityString { get; set; }
        public int? Quantity => !string.IsNullOrWhiteSpace(QuantityString) ? (int.TryParse(QuantityString, out var result)
            ? result : throw new FormatException($"'{nameof(this.Quantity)}' is not in the correct format."))
            : null;

        [XmlElement("Volume")]
        public string VolumeString { get; set; }
        public decimal? Volume => !string.IsNullOrWhiteSpace(VolumeString) ? (decimal.TryParse(VolumeString, NumberStyles.Any, _culture, out var result)
            ? result : throw new FormatException($"'{nameof(this.Volume)}' is not in the correct format"))
            : null;

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
