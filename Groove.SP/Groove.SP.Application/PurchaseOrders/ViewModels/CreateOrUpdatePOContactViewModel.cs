using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using System.Collections.Generic;
using Groove.SP.Core.Models;
using System.Linq;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Newtonsoft.Json;

namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class CreateOrUpdatePOContactViewModel : ViewModelBase<PurchaseOrderContactModel>, IHasFieldStatus
    {
        public string OrganizationRole { get; set; }
        public string OrganizationCode { get; set; }
        public string CompanyName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
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
