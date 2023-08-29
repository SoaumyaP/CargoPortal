using System.Collections.Generic;
using System.Linq;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Core.Entities;
using FluentValidation;
using Groove.CSFE.Application.Converters;
using Groove.CSFE.Application.Converters.Interfaces;
using Groove.CSFE.Application.Countries.Validations;
using Groove.CSFE.Core;
using Newtonsoft.Json;

namespace Groove.CSFE.Application.Countries.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class CountryViewModel : ViewModelBase<CountryModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public string Code { get; set; }

        public string Name { get; set; }
        
        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new CountryValidation(isUpdating).ValidateAndThrow(this);
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
