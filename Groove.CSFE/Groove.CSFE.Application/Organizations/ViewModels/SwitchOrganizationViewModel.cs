using Groove.CSFE.Core;

namespace Groove.CSFE.Application.Organizations.ViewModels
{
    public class SwitchOrganizationViewModel
    {        
        public long Id { get; set; }
        public string Name { get; set; }
        public OrganizationType TypeId { get; set; }
        public Role RoleId { get; set; }
    }
}
