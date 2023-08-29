using System.ComponentModel.DataAnnotations;

namespace Groove.SP.Application.ImportData.ViewModels
{
    public class ImportErrorExcelModel
    {
        [Display(Name = "label.number")]
        public int Order { get; set; }

        [Display(Name = "label.row")]
        public string Row { get; set; }

        [Display(Name = "label.column")]
        public string Column { get; set; }

        [Display(Name = "label.error")]
        public string ErrorMsg { get; set; }
    }
}