using FluentValidation;
using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Application.Note.Validations;
using Groove.SP.Core.Entities;
using Groove.SP.Core.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Groove.SP.Application.Note.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class CreateAndUpdateNoteViewModel : ViewModelBase<NoteModel>, IHasFieldStatus
    {
        public long? PurchaseOrderId { get; set; }

        public long? POFulfillmentId { get; set; }

        public long? ShipmentId { get; set; }

        public long? CruiseOrderId { get; set; }

        public long? CruiseOrderItemId { get; set; }

        public long? RoutingOrderId { get; set; }

        public string Owner { get; set; }

        public string Category { get; set; }

        public string NoteText { get; set; }

        public string ExtendedData { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new CreateAndUpdateNoteViewModelValidator(isUpdating).ValidateAndThrow(this);
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
