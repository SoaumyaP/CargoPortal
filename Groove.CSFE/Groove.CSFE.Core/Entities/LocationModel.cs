using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.CSFE.Core.Entities
{
    public class LocationModel : Entity
    {
        public long Id { get; set; }
        
        public string Name { get; set; }
        
        public long CountryId { get; set; }

        public string LocationDescription { get; set; }

        public string EdiSonPortCode { get; set; }

        public virtual CountryModel Country { get; set; }

        public virtual UserOfficeModel UserOffices { get; set; }

        public ICollection<OrganizationModel> Organizations { get; set; }

        public ICollection<WarehouseLocationModel> WarehouseLocations { get; set; }

        public ICollection<WarehouseModel> Warehouses { get; set; }

        public ICollection<TerminalModel> Terminals { get; set; }

        public ICollection<AlternativeLocationModel> AlternativeLocations { get; set; }
    }
}
