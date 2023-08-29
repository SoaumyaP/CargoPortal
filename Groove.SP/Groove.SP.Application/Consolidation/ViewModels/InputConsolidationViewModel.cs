using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Consolidation.Validations;
using Groove.SP.Core.Entities;
using System;

namespace Groove.SP.Application.Consolidation.ViewModels
{
    public class InputConsolidationViewModel : ViewModelBase<ConsolidationModel>
    {
        public long ConsignmentId { get; set; }

        public string EquipmentType { get; set; }

        public string OriginCFS { get; set; }

        public DateTime CFSCutoffDate { get; set; }

        public string CarrierSONo { get; set; }

        public DateTime? LoadingDate { get; set; }

        public string ContainerNo { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new InputConsolidationValidator().ValidateAndThrow(this);
        }
    }
}
