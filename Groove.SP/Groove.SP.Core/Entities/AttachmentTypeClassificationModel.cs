namespace Groove.SP.Core.Entities
{
    /// <summary>
    /// To classify available document types for each entity type: 
    /// </summary>
    public class AttachmentTypeClassificationModel : Entity
    {
        public long Id { get; set; }

        /// <summary>
        /// Value of option. Ex: Shipping Order Form, Commercial Invoice
        /// </summary>
        public string AttachmentType { get; set; }

        /// <summary>
        /// Entity type key
        /// <br></br>See more values as <see cref="Groove.SP.Core.Models.EntityType"/>
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// Order to display option, smaller to the top
        /// </summary>
        public int Order { get; set; }
    }
}
