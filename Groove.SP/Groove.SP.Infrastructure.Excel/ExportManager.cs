using OfficeOpenXml;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace Groove.SP.Infrastructure.Excel
{
    public class ExportManager : IExportManager
    {
        public ExcelPackage ExportDataTableToXlsx(DataTable dataTable)
        {
            var properties = new List<PropertyByName<DataRow>>();

            foreach (DataColumn column in dataTable.Columns)
            {
                properties.Add(new PropertyByName<DataRow>(column.ColumnName, dataRow => dataRow[column.ColumnName]));
            }

            var manager = new PropertyManager<DataRow>(properties);

            var xlPackage = new ExcelPackage();
            var worksheet = xlPackage.Workbook.Worksheets.Add("sheet1");
            worksheet.DefaultColWidth = 25;
            manager.WriteCaption(worksheet);
            
            var row = 2;
            foreach (DataRow item in dataTable.Rows)
            {
                manager.WriteToXlsx(worksheet, row++, item);
            }
            
            return xlPackage;
                
        }
    }
}
