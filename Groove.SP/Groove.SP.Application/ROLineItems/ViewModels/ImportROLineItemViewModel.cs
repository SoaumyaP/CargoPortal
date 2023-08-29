using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using System.Globalization;
using System.Xml.Serialization;

namespace Groove.SP.Application.ROLineItems.ViewModels
{
    public class ImportROLineItemViewModel : ViewModelBase<ROLineItemModel>
    {
        private readonly CultureInfo _culture = new("en") { NumberFormat = { NumberDecimalSeparator = "," } };

        [XmlElement("PONo")]
        public string PONo { get; set; }

        [XmlElement("ItemNo")]
        public string ItemNo { get; set; }

        [XmlElement("DescriptionOfGoods")]
        public string DescriptionOfGoods { get; set; }

        [XmlElement("ChineseDescription")]
        public string ChineseDescription { get; set; }

        [XmlElement("OrderedUnitQty")]
        public int OrderedUnitQty { get; set; }

        [XmlElement("UnitUOM")]
        public string UnitUOM { get; set; }

        [XmlElement("BookedPackage")]
        public int? BookedPackage { get; set; }

        [XmlElement("PackageUOM")]
        public string PackageUOM { get; set; }

        [XmlElement("GrossWeight")]
        public string GrossWeightString { get; set; }
        public decimal? GrossWeight => decimal.TryParse(GrossWeightString, NumberStyles.Any, _culture, out var result) ? result : null;

        [XmlElement("NetWeight")]
        public string NetWeightString { get; set; }
        public decimal? NetWeight => !string.IsNullOrWhiteSpace(NetWeightString) ? (decimal.TryParse(NetWeightString, NumberStyles.Any, _culture, out var result)
            ? result : throw new FormatException($"'{nameof(this.NetWeight)}' is not in the correct format"))
            : null;

        [XmlElement("Volume")]
        public string VolumeString { get; set; }
        public decimal? Volume => !string.IsNullOrWhiteSpace(VolumeString) ? (decimal.TryParse(VolumeString, NumberStyles.Any, _culture, out var result)
            ? result : throw new FormatException($"'{nameof(this.Volume)}' is not in the correct format"))
            : null;

        [XmlElement("HSCode")]
        public string HsCode { get; set; }

        [XmlElement("Commodity")]
        public string Commodity { get; set; }

        [XmlElement("ShippingMarks")]
        public string ShippingMarks { get; set; }

        [XmlElement("CountryCodeOfOrigin")]
        public string CountryCodeOfOrigin { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            throw new System.NotImplementedException();
        }
    }
}
