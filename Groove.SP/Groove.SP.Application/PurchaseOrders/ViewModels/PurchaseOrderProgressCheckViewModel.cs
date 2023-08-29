using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class PurchaseOrderProgressCheckViewModel: IHasFieldStatus
    {
        public long Id { get; set; }
        public string PONumber { get; set; }
        public DateTime? CargoReadyDate { get; set; }
        public bool ProductionStarted { set; get; }
        public bool QCRequired { set; get; }
        public bool ShortShip { set; get; }
        public bool SplitShipment { set; get; }
        public DateTime? ProposeDate { set; get; }
        public string Remark { set; get; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }
        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
