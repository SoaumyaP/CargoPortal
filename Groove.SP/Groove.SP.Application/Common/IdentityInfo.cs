using Groove.SP.Core.Models;

namespace Groove.SP.Application.Common
{
    public class IdentityInfo
    {
        public string Id { get; set; }

        /// <summary>
        /// It is an email
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The email of user stored on Azure
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// The name of user stored on Azure
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The phone number of user stored on Azure
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// The company name of user stored on Azure
        /// </summary>
        public string CompanyName { get; set; }

        /// <summary>
        /// The company address of user stored on Azure
        /// </summary>
        public string CompanyAddress { get; set; }

        /// <summary>
        /// Whether user is Internal
        /// </summary>
        public bool IsInternal { get; set; }

        //Business user information

        /// <summary>
        /// An organization Id of user stored in the Portal database
        /// </summary>
        public long OrganizationId { get; set; }

        /// <summary>
        /// The name of user stored in the Portal database, comparing to Name (on azure)
        /// </summary>
        public string PortalUserName { get; set; }

        /// <summary>
        /// An role Id of user stored in the Portal database
        /// </summary>
        public long UserRoleId { get; set; }

        /// <summary>
        /// An role name of user stored in the Portal database
        /// </summary>
        public string UserRoleName { get; set; }

        /// <summary>
        /// The OP contact email of user stored on Azure
        /// </summary>
        public string OPContactEmail { get; set; }

        /// <summary>
        /// The OP contact name of user stored on Azure
        /// </summary>
        public string OPContactName { get; set; }

        /// <summary>
        /// The OP Country ID of user stored on Azure
        /// </summary>
        public long? OPCountryId { get; set; }

        /// <summary>
        /// The OP location name of user stored on Azure
        /// </summary>
        public string OPLocation { get; set; }

        /// <summary>
        /// The Taxpayer Id of user stored on Azure
        /// </summary>
        public string TaxpayerId { get; set; }

        /// <summary>
        /// Whether user is Principal
        /// </summary>
        public bool IsPrincipal
        {
            get
            {
                return UserRoleId == (int)Role.Principal;
            }
        }
        /// <summary>
        /// Whether user is Agent
        /// </summary>
        public bool IsAgent
        {
            get
            {
                return UserRoleId == (int)Role.Agent;
            }
        }

        /// <summary>
        /// Whether user is Shipper
        /// </summary>
        public bool IsShipper
        {
            get
            {
                return UserRoleId == (int)Role.Shipper;
            }
        }

        /// <summary>
        /// Whether user is in pretend mode (user role switch) that internal user is playing as external user
        /// </summary>
        public bool IsUserRoleSwitch { get; set; }
    }
}
