namespace Groove.SP.Core.Entities
{
    public class OrgContactPreferenceModel : Entity
    {
        public long Id { set; get; }
        public long OrganizationId { get; set; }
        public string CompanyName { get; set; }
        public string Address { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string AddressLine4 { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }
        public string WeChatOrWhatsApp { get; set; }
    }
}
