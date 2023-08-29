using System.Collections.Generic;
using System.Linq;
using Groove.CSFE.Application.Converters;
using Groove.CSFE.Application.Converters.Interfaces;
using Groove.CSFE.Core;
using Newtonsoft.Json;

namespace Groove.CSFE.Application.Organizations.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class CustomerRelationshipViewModel : IHasFieldStatus
    {
        public long SupplierId { get; set; }

        public long CustomerId { get; set; }

        public bool IsApplyAffiliates { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public string CustomerRefId { get; set; }

        public Dictionary<string, FieldDeserializationStatus> FieldStatus { get; set; }

        public bool IsPropertyDirty(string name)
        {
            return FieldStatus != null &&
                   FieldStatus.ContainsKey(name) &&
                   FieldStatus.FirstOrDefault(x => x.Key == name).Value == FieldDeserializationStatus.HasValue;
        }
    }
}
