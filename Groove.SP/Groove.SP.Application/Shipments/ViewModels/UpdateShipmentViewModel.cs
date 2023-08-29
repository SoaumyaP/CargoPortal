using System.Collections.Generic;
using Groove.SP.Application.Common;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using System.Linq;
using Groove.SP.Core.Entities;
using System;

namespace Groove.SP.Application.Shipments.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class UpdateShipmentViewModel: ViewModelBase<ShipmentModel>, IHasFieldStatus
    {
        public string CarrierContractNo { get; set; }

        public string AgentReferenceNo { get; set; }

        public string Movement { get; set; }

        public string CommercialInvoiceNo { get; set; }

        public DateTime? InvoiceDate { get; set; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                  FieldStatus.ContainsKey(name) &&
                  FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            
        }
    }
}
