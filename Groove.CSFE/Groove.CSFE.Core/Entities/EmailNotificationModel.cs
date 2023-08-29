namespace Groove.CSFE.Core.Entities
{
    public class EmailNotificationModel : Entity
    {
        public long Id { get; set; }
        public long OrganizationId { get; set; }
        public long CustomerId { get; set; }
        public long? CountryId { get; set; }
        public string PortSelectionIds { get; set; }
        public string Email { get; set; }
        public OrganizationModel Organization { get; set; }
        public OrganizationModel Customer { get; set; }
        public CountryModel Country { get; set; }
    }
}