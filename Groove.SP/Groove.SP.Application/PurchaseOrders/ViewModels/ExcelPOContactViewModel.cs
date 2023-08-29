
namespace Groove.SP.Application.PurchaseOrders.ViewModels
{
    /// <summary>
    /// Model to import Purchase Order via Excel file (PO Contact tab).
    /// </summary>
    /// <remarks>
    /// <b>Property order must be the same as the Excel file</b>
    /// </remarks>
    public class ExcelPOContactViewModel
    {
        public string PONumber { get; set; }
        public string OrganizationRole { get; set; }
        public string OrganizationCode { get; set; }
        public string Row { get; set; }
    }
}
