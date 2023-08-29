using System;
using Groove.SP.Application.Consolidation.Validations;
using FluentValidation;

namespace Groove.SP.Application.Consolidation.ViewModels
{
    public class UpdateConsolidationViewModel
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public string SealNo { get; set; }

        public string SealNo2 { get; set; }

        public string EquipmentType { get; set; }

        public string OriginCFS { get; set; }

        public DateTime CFSCutoffDate { get; set; }

        public string CarrierSONo { get; set; }

        public DateTime? LoadingDate { get; set; }

        public void ValidateAndThrow()
        {
            new UpdateConsolidationValidator().ValidateAndThrow(this);
        }
    }
}
