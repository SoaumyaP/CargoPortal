using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.SP.Application.BillOfLading.ViewModels;
using Groove.SP.Application.BillOfLadingContact.Validations;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.BillOfLadingContact.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class BillOfLadingContactViewModel : ViewModelBase<BillOfLadingContactModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public long BillOfLadingId { get; set; }

        public long OrganizationId { get; set; }

        public string OrganizationRole { get; set; }

        public string CompanyName { get; set; }

        public string Address { get; set; }

        public string ContactName { get; set; }

        public string ContactNumber { get; set; }

        public string ContactEmail { get; set; }

        public BillOfLadingViewModel BillOfLading { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new BillOfLadingContactValidation(isUpdating).ValidateAndThrow(this);
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
