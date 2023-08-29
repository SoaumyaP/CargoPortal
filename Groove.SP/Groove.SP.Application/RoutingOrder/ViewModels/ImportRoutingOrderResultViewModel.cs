using Groove.SP.Application.Utilities;
using Newtonsoft.Json.Converters;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Groove.SP.Application.RoutingOrder.ViewModels
{
    public class ImportRoutingOrderResultViewModel
    {
        public string Type { get; set;}

        public bool Success
        {
            get
            {
                return Type == ImportingRoutingOrderResult.Success;
            }
        }
        public Dictionary<string, object> Result { get; set; }

        public ImportRoutingOrderResultViewModel()
        {
            Type = ImportingRoutingOrderResult.Success;
            Result = new Dictionary<string, object>();
        }

        public void Log(string type, string logName, object logValue)
        {
            Type = type;
            if (Result.ContainsKey(logName))
            {
                Result[logName] = logValue;

            }
            else
            {
                Result.TryAdd(logName, logValue);
            }
        }
    }

    public class ImportingRoutingOrderResult
    {
        public const string ValidationFailed = "Validation Failed";
        public const string ErrorDuringImport = "Importing Error";
        public const string Success = "Success";
    }
}
