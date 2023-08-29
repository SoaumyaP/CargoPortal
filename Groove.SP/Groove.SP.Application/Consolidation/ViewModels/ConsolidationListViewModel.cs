using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Groove.SP.Application.Consolidation.ViewModels
{
    public class ConsolidationListViewModel: ViewModelBase<ConsolidationModel>
    {
        public long Id { get; set; }

        public string ContainerNo { get; set; }

        public long? ContainerId { get; set; }

        public string ConsolidationNo { get; set; }

        public string EquipmentType { get; set; }

        public string OriginCFS { get; set; }

        public DateTime CFSCutoffDate { get; set; }

        public DateTime CFSCutoffDateOnly { get; set; }

        public string Stage { get; set; }

        public DateTime? LoadingDate { get; set; }

        public DateTime? LoadingDateOnly { get; set; }

        public string ShipmentNo { get; set; }

        [NotMapped]
        public IEnumerable<Tuple<long, string>> ShipmentNos { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}