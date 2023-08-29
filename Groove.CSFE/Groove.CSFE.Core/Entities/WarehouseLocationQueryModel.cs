namespace Groove.CSFE.Core.Entities
{
    public class WarehouseLocationQueryModel
    {
        public long Id { set; get; }
        public string Code { get; set; }
        public string Name { get; set; }
        /// <summary>
        /// Location description - Country name
        /// </summary>
        public string Location { get; set; }
        public string ContactName { get; set; }
        public string Provider { get; set; }
        public long OrganizationId { get; set; }
    }
}
