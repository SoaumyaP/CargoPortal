using OfficeOpenXml;

namespace Groove.SP.Infrastructure.Excel
{
    public static class EPPlusExtensions
    {
        public static bool IsRowEmpty(this ExcelWorksheet workSheet, int rowIndex)
        {
            var columnCount = workSheet.Dimension.Columns;
            for (int i = 1; i < columnCount; i++)
            {
                if (!string.IsNullOrEmpty(workSheet.GetValue<string>(rowIndex, i)))
                {
                    return false;
                }
            }
            return true;
        }


        public static bool IsRowEmpty(this ExcelWorksheet workSheet, int rowIndex, int numberOfColumns)
        {
            for (int i = 1; i <= numberOfColumns; i++)
            {
                if (!string.IsNullOrEmpty(workSheet.GetValue<string>(rowIndex, i)))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
