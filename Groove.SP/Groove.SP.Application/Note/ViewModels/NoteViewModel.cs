using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Core.Entities;
using Newtonsoft.Json;
using System;

namespace Groove.SP.Application.Note.ViewModels
{
    public class NoteViewModel : ViewModelBase<NoteModel>
    {
        public long Id { get; set; }

        public long? PurchaseOrderId { get; set; }

        public string Category { get; set; }

        public string NoteText { get; set; }

        public string ExtendedData { get; set; }

        public string Owner { get; set; }

        public override void ValidateAndThrow(bool isUpdating)
        {
        }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public new DateTime CreatedDate { get; set; }

        [JsonConverter(typeof(UtcDateTimeConverter))]
        public new DateTime? UpdatedDate { get; set; }
    }
}
