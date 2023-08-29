using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.MasterDialog.ViewModels;
using Groove.SP.Core.Entities;
using Newtonsoft.Json;
using System;

namespace Groove.SP.Application.GlobalIdMasterDialog.ViewModels
{
    public class GlobalIdMasterDialogViewModel : ViewModelBase<GlobalIdMasterDialogModel>
    {
        public string GlobalId { get; set; }

        public long MasterDialogId { get; set; }

        public string ExtendedData { get; set; }

        public GlobalIdModel ReferenceEntity { get; set; }

        public MasterDialogViewModel MasterDialog { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public new DateTime CreatedDate { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public new DateTime? UpdatedDate { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
        }
    }
}
