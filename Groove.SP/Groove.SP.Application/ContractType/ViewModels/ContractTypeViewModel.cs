using Groove.SP.Application.Common;
using Groove.SP.Application.Converters;
using Groove.SP.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace Groove.SP.Application.ContractType.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class ContractTypeViewModel : ViewModelBase<ContractTypeModel>
    {
        public long Id { set; get; }
        public string Name { set; get; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {

        }
    }
}
