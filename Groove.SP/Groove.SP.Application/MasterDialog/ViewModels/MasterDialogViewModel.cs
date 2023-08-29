using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using FluentValidation;
using System.Collections.Generic;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System.Linq;
using Groove.SP.Application.MasterDialog.Validations;
using System;

namespace Groove.SP.Application.MasterDialog.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class MasterDialogViewModel : ViewModelBase<MasterDialogModel>, IHasFieldStatus
    {
        public long Id { set; get; }

        public string DisplayOn { get; set; }

        public string FilterCriteria { get; set; }

        public string FilterValue { get; set; }

        public string Message { get; set; }

        public string Category { get; set; }

        public string SelectedItems { get; set; }

        public string Owner { get; set; }

        public long? OrganizationId { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public new DateTime CreatedDate { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public new DateTime? UpdatedDate { get; set; }


        public virtual ICollection<GlobalIdMasterDialogModel> GlobalIdMasterDialogs { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new MasterDialogValidation(isUpdating).ValidateAndThrow(this);
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
