using Groove.SP.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Groove.SP.Core.Entities
{
    public class ConsolidationQueryModel
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

        public string ShipmentInfo { get; set; }

        public string ShipmentNo { get; set; }

        [NotMapped]
        public virtual IEnumerable<Tuple<long, string>> ShipmentNos
        {
            get
            {
                var shipmentNos = ShipmentNo?.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                var shipmentInfo = ShipmentInfo?.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
                Dictionary<string, string> shipmentInfoDictionary =
                  shipmentInfo?.ToDictionary(s => s.Split('~')[0], s => s.Split('~')[1]);

                return shipmentNos?.Select(x => new Tuple<long, string>(long.Parse(shipmentInfoDictionary[$"{x}"]), x));
            }
        }
    }
}
