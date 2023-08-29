namespace Groove.SP.Core.Entities
{
    /// <summary>
    /// To grant available document types for each role
    /// </summary>
    public class AttachmentTypePermissionModel : Entity
    {
        public long Id { get; set; }

        /// <summary>
        /// Value of option. Ex: Shipping Order Form, Commercial Invoice
        /// </summary>
        public string AttachmentType { get; set; }
        /// <summary>
        /// Role id.
        /// <br></br>See more values as <see cref="Groove.SP.Core.Models.Role"/>
        /// </summary>
        public long RoleId { get; set; }

        /// <summary>
        /// If the flag is True, apply checking the user organization against ContractHolder of the Contract# in MasterBL
        /// </summary>
        public bool CheckContractHolder { get; set; }

        public string Alias { set; get; }
    }
}