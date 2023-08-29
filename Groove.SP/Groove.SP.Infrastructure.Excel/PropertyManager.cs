using FluentValidation;
using FluentValidation.Results;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Groove.SP.Infrastructure.Excel
{
    public class PropertyManager<T>
    {
        private readonly IDictionary<string, PropertyByName<T>> _columnPropertyInfos;

        public PropertyManager()
        {
            _columnPropertyInfos = new Dictionary<string, PropertyByName<T>>();
            var properties = typeof(T).GetProperties().ToList();

            foreach (var propInfo in properties)
            {
                int propertyOrderPosition = properties.IndexOf(propInfo) + 1;
                var displayNameAttribute = Attribute.GetCustomAttribute(propInfo, typeof(DisplayNameAttribute));
                var columnName = displayNameAttribute == null ? propInfo.Name : ((DisplayNameAttribute)displayNameAttribute).DisplayName.Trim();

                _columnPropertyInfos.Add(propInfo.Name, new PropertyByName<T>(columnName)
                {
                    Property = propInfo,
                    ColumnOrderPosition = propertyOrderPosition
                });
            }
        }

        public PropertyManager(IEnumerable<PropertyByName<T>> properties)
        {
            _columnPropertyInfos = new Dictionary<string, PropertyByName<T>>();

            var poz = 1;
            foreach (var propertyByName in properties)
            {
                propertyByName.ColumnOrderPosition = poz;
                poz++;
                _columnPropertyInfos.Add(propertyByName.ColumnName, propertyByName);
            }
        }

        public ICollection<PropertyByName<T>> Properties => _columnPropertyInfos.Values;

        private static string AddSpaceBeforeCapitalLetters(string value)
        {
            return Regex.Replace(value, "([A-Z])", " $0").Trim();
        }


        /// <summary>
        /// Read object data from XLSX worksheet
        /// </summary>
        /// <param name="worksheet">worksheet</param>
        /// <param name="row">Row index</param>
        public T ReadExcelByRow(ExcelWorksheet worksheet, int row, string objectName, string sheetName, AbstractValidator<T> validator, out IList<ValidatorErrorInfo> errorInfoItems) 
        {
            foreach (var prop in _columnPropertyInfos.Values)
            {
                prop.PropertyValue = worksheet.Cells[row, prop.ColumnOrderPosition].Value;
            }

            var tNew = (T)Activator.CreateInstance(typeof(T));

            errorInfoItems = new List<ValidatorErrorInfo>();

            foreach (var prop in this.Properties)
            {
                if (prop.Property.PropertyType == typeof(string))
                {
                    prop.Property.SetValue(tNew, prop.StringValue);
                }
                else if (prop.Property.PropertyType == typeof(DateTime))
                {
                    if (!string.IsNullOrEmpty(prop.StringValue) && !prop.DateTimeValue.HasValue)
                    {
                        errorInfoItems.Add(
                            new ValidatorErrorInfo
                            {
                                Row = row.ToString(),
                                SheetName = sheetName,
                                ObjectName = GetProperty(objectName).StringValue,
                                Column = prop.ColumnOrderPosition.ToString(),
                                ErrorMsg = string.Format("'{0}' is wrong format.", AddSpaceBeforeCapitalLetters(prop.ColumnName))
                            });
                    }
                    else
                    { 
                        prop.Property.SetValue(tNew, prop.DateTimeValue);
                    }
                }
                else if (prop.Property.PropertyType == typeof(DateTime?))
                {
                    if (!string.IsNullOrEmpty(prop.StringValue) && !prop.DateTimeValue.HasValue)
                    {
                        errorInfoItems.Add(
                            new ValidatorErrorInfo
                            {
                                Row = row.ToString(),
                                SheetName = sheetName,
                                ObjectName = GetProperty(objectName).StringValue,
                                Column = prop.ColumnOrderPosition.ToString(),
                                ErrorMsg = string.Format("'{0}' is wrong format.", AddSpaceBeforeCapitalLetters(prop.ColumnName))
                            });
                    }
                    else
                    {
                        prop.Property.SetValue(tNew, prop.DateTimeValue);
                    }
                }
                else if (prop.Property.PropertyType == typeof(decimal))
                {
                    prop.Property.SetValue(tNew, prop.DecimalValue);
                }
                else if (prop.Property.PropertyType == typeof(decimal?))
                {
                    prop.Property.SetValue(tNew, prop.DecimalValue);
                }
                else if (prop.Property.PropertyType == typeof(int))
                {
                    prop.Property.SetValue(tNew, prop.IntValue);
                }
                else if (prop.Property.PropertyType == typeof(int?))
                {
                    prop.Property.SetValue(tNew, prop.IntValue);
                }
                else if (prop.Property.PropertyType == typeof(long))
                {
                    prop.Property.SetValue(tNew, prop.LongValue);
                }
                else if (prop.Property.PropertyType == typeof(long?))
                {
                    prop.Property.SetValue(tNew, prop.LongValue);
                }
                else if (prop.Property.PropertyType == typeof(double))
                {
                    prop.Property.SetValue(tNew, prop.DoubleValue);
                }
                else if (prop.Property.PropertyType == typeof(double?))
                {
                    prop.Property.SetValue(tNew, prop.DoubleValue);
                }
                else if (prop.Property.PropertyType.IsEnum || prop.Property.PropertyType.IsNullableEnum())
                {
                    if (!string.IsNullOrEmpty(prop.StringValue) && prop.EnumValue == null)
                    {
                        errorInfoItems.Add(
                            new ValidatorErrorInfo
                            {
                                Row = row.ToString(),
                                SheetName = sheetName,
                                ObjectName = GetProperty(objectName).StringValue,
                                Column = prop.ColumnOrderPosition.ToString(),
                                ErrorMsg = string.Format("'{0}': Inputted data is not existing in system.", AddSpaceBeforeCapitalLetters(prop.ColumnName))
                            });
                    }
                    else
                    {
                        prop.Property.SetValue(tNew, prop.EnumValue);
                    }
                }
            }

            ValidationResult results = validator.Validate(tNew);
            if (!results.IsValid)
            {
                foreach (var errorInfo in results.Errors)
                {
                    errorInfoItems.Add(
                            new ValidatorErrorInfo
                            {
                                Row = row.ToString(),
                                SheetName = sheetName,
                                ObjectName = GetProperty(objectName).StringValue,
                                Column = GetProperty(errorInfo.PropertyName).ColumnOrderPosition.ToString(),
                                ErrorMsg = errorInfo.ErrorMessage
                            });
                }
            }

            return tNew;
        }

        /// <summary>
        /// Read object data from XLSX worksheet without validation
        /// </summary>
        /// <param name="worksheet">worksheet</param>
        /// <param name="row">row index</param>
        /// <returns></returns>
        public T ReadExcelByRowWithoutValidation(ExcelWorksheet worksheet, int row)
        {
            foreach (var prop in _columnPropertyInfos.Values)
            {
                prop.PropertyValue = worksheet.Cells[row, prop.ColumnOrderPosition].Value;
            }

            var tNew = (T)Activator.CreateInstance(typeof(T));


            foreach (var prop in this.Properties)
            {
                if (prop.Property.PropertyType == typeof(string))
                {
                    prop.Property.SetValue(tNew, prop.StringValue);
                }
                else if (prop.Property.PropertyType == typeof(DateTime))
                {
                    if (!string.IsNullOrEmpty(prop.StringValue) && !prop.DateTimeValue.HasValue)
                    {
                       
                    }
                    else
                    {
                        prop.Property.SetValue(tNew, prop.DateTimeValue);
                    }
                }
                else if (prop.Property.PropertyType == typeof(DateTime?))
                {
                    if (!string.IsNullOrEmpty(prop.StringValue) && !prop.DateTimeValue.HasValue)
                    {
                       
                    }
                    else
                    {
                        prop.Property.SetValue(tNew, prop.DateTimeValue);
                    }
                }
                else if (prop.Property.PropertyType == typeof(decimal))
                {
                    prop.Property.SetValue(tNew, prop.DecimalValue);
                }
                else if (prop.Property.PropertyType == typeof(decimal?))
                {
                    prop.Property.SetValue(tNew, prop.DecimalValue);
                }
                else if (prop.Property.PropertyType == typeof(int))
                {
                    prop.Property.SetValue(tNew, prop.IntValue);
                }
                else if (prop.Property.PropertyType == typeof(int?))
                {
                    prop.Property.SetValue(tNew, prop.IntValue);
                }
                else if (prop.Property.PropertyType == typeof(long))
                {
                    prop.Property.SetValue(tNew, prop.LongValue);
                }
                else if (prop.Property.PropertyType == typeof(long?))
                {
                    prop.Property.SetValue(tNew, prop.LongValue);
                }
                else if (prop.Property.PropertyType == typeof(double))
                {
                    prop.Property.SetValue(tNew, prop.DoubleValue);
                }
                else if (prop.Property.PropertyType == typeof(double?))
                {
                    prop.Property.SetValue(tNew, prop.DoubleValue);
                }
                else if (prop.Property.PropertyType.IsEnum || prop.Property.PropertyType.IsNullableEnum())
                {
                    if (!string.IsNullOrEmpty(prop.StringValue) && prop.EnumValue == null)
                    {
                    }
                    else
                    {
                        prop.Property.SetValue(tNew, prop.EnumValue);
                    }
                }
            }

            return tNew;
        }

        /// <summary>
        /// Get property by name
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public PropertyByName<T> GetProperty(string propertyName)
        {
            return _columnPropertyInfos.ContainsKey(propertyName) ? _columnPropertyInfos[propertyName] : null;
        }

        #region Write XLSX methods

        /// <summary>
        /// Write caption (first row) to XLSX worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="row"></param>
        public virtual void WriteCaption(ExcelWorksheet worksheet, int row = 1)
        {
            foreach (var caption in _columnPropertyInfos.Values)
            {
                var cell = worksheet.Cells[row, caption.ColumnOrderPosition];
                cell.Value = caption.ColumnName;

                SetCaptionStyle(cell);
                cell.Style.Hidden = false;
            }
        }

        /// <summary>
        /// Set caption style to excel cell
        /// </summary>
        /// <param name="cell">Excel cell</param>
        public void SetCaptionStyle(ExcelRange cell)
        {
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(184, 204, 228));
            cell.Style.Font.Bold = true;
        }

        /// <summary>
        /// Write object data to XLSX worksheet
        /// </summary>
        /// <param name="worksheet"></param>
        /// <param name="row"></param>
        /// <param name="item"></param>
        public virtual void WriteToXlsx(ExcelWorksheet worksheet, int row, T item)
        {
            if (item == null)
                return;

            foreach (var prop in _columnPropertyInfos.Values)
            {
                var cell = worksheet.Cells[row, prop.ColumnOrderPosition];
                cell.Value = prop.GetProperty(item);
            }
        }

        #endregion
    }
}
