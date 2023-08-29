using System;
using Groove.SP.Application.Converters;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Groove.SP.Application.Utilities;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using System.Collections.Generic;

namespace Groove.SP.Application.Consolidation.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ConsolidationInternalViewModel
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public string ContainerId { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public string ConsolidationNo { get; set; }

        public string ModeOfTransport { get; set; }

        public string EquipmentType { get; set; }

        public string EquipmentTypeName => EnumHelper<EquipmentType>.GetEnumDescriptionByShortName(EquipmentType);

        public string OriginCFS { get; set; }

        public DateTime CFSCutoffDate { get; set; }

        public decimal TotalGrossWeight { get; set; }

        public string TotalGrossWeightUOM { get; set; }

        public decimal TotalNetWeight { get; set; }

        public string TotalNetWeightUOM { get; set; }

        public decimal TotalPackage { get; set; }

        public string TotalPackageUOM { get; set; }

        public decimal TotalVolume { get; set; }

        public string TotalVolumeUOM { get; set; }

        public string CarrierSONo { get; set; }

        public DateTime? LoadingDate { get; set; }

        public ConsolidationStage Stage { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public ConsolidationStage StageName => Stage;

        public IEnumerable<ShipmentLoadViewModel> ShipmentLoads { get; set; }
    }
}
