using Groove.SP.Core.Models;
using System.Collections.Generic;

namespace Groove.SP.Core.Entities
{
    public class ViewSettingModel : Entity
    {
        public long ViewId { get; set; }

        public string Field { get; set; }

        public string Title { get; set; }

        public int? Sequence { get; set; }

        public string ModuleId { get; set; }

        public ViewSettingType ViewType { get; set; }

        public ICollection<ViewRoleSettingModel> ViewRoleSettings { get; set; }
    }
}