using System.Collections.Generic;
using System.Linq;
using Groove.SP.Application.Common;
using Groove.SP.Core.Entities;
using Groove.SP.Application.Container.Validations;
using FluentValidation;
using Groove.SP.Application.Converters;
using Groove.SP.Application.Converters.Interfaces;
using Groove.SP.Core.Models;
using Newtonsoft.Json;

namespace Groove.SP.Application.Container.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ContainerItineraryViewModel : ViewModelBase<ContainerItineraryModel>, IHasFieldStatus
    {
        public long ItineraryId { get; set; }

        public long ContainerId { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new ContainerItineraryValidation(isUpdating).ValidateAndThrow(this);
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
