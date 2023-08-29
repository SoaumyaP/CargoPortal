using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Groove.SP.Infrastructure.Excel
{
    public interface IExportManager
    {
        ExcelPackage ExportDataTableToXlsx(DataTable dataTable);
    }
}
