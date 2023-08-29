using Groove.SP.Application.Common;
using Groove.SP.Application.Consolidation.Validations;
using Groove.SP.Application.ShipmentLoads.ViewModels;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.Consolidation.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ConsolidationViewModel : ViewModelBase<ConsolidationModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public long? ContainerId { get; set; }

        public string ConsolidationNo { get; set; }

        public string ModeOfTransport { get; set; }

        public string EquipmentType { get; set; }

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

        public ICollection<ShipmentLoadViewModel> ShipmentLoads { get; set; }
        
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ConsolidationValidation(isUpdating).ValidateAndThrow(this);
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
