using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.ImportData.ViewModels
{
    public class ImportPOErrorExcelModel
    {
        [Display(Name = "label.number")]
        public int Order { get; set; }

        [Display(Name = "label.purchaseOrder")]
        public string ObjectName { get; set; }

        [Display(Name = "label.sheetName")]
        public string SheetName { get; set; }

        [Display(Name = "label.row")]
        public string Row { get; set; }

        [Display(Name = "label.error")]
        public string ErrorMsg { get; set; }
    }
}
