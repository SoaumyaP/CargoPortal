namespace Groove.CSFE.Core.Entities
{
    public class UserOfficeModel: Entity
    {
        public long Id { get; set; }
        public long LocationId { set; get; }
        public string CorpMarketingContactName { set; get; }
        public string CorpMarketingContactEmail { set; get; }
        public string OPManagementContactName { set; get; }
        public string OPManagementContactEmail { set; get; }

        public virtual LocationModel Location { get; set; }
    }
}
