
namespace Groove.SP.Infrastructure.Excel
{
    public class ValidatorErrorInfo
    {
        public string SheetName { get; set; }
        public string ObjectName { get; set; }
        public string Row { get; set; }
        public string Column { get; set; }
        public string ErrorMsg { get; set; }
    }
}
