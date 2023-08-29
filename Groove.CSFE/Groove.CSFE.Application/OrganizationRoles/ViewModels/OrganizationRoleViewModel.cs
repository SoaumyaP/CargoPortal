using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using Groove.CSFE.Application.Common;
using Groove.CSFE.Application.Converters;
using Groove.CSFE.Application.Converters.Interfaces;
using Groove.CSFE.Application.OrganizationRoles.Validations;
using Groove.CSFE.Core;
using Groove.CSFE.Core.Entities;
using Newtonsoft.Json;

namespace Groove.CSFE.Application.OrganizationRoles.ViewModels
{
    [JsonConverter(typeof(MyConverter))]
    public class OrganizationRoleViewModel : ViewModelBase<OrganizationRoleModel>, IHasFieldStatus
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }
        
        public OrganizationType OrganizationTypes { get; set; }

        public override void ValidateAndThrow(bool isUpdating = false)
        {
            new OrganizationRoleValidation(isUpdating).ValidateAndThrow(this);
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
