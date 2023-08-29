using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Groove.CSFE.Core.Entities
{
    public class CountryModel : Entity
    {
        public long Id { get; set; }
        
        public string Code { get; set; }
        
        public string Name { get; set; }

        public ICollection<LocationModel> Locations { get; set; }

        public ICollection<EmailNotificationModel> EmailNotifications { get; set; }
    }
}