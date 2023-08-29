using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Groove.SP.Application.Utilities
{
    public static class Extensions
    {
        public static List<T> GetAllPublicConstantValues<T>(this Type type)
        {
            return type
                .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                .Where(fi => fi.IsLiteral && !fi.IsInitOnly && fi.FieldType == typeof(T))
                .Select(x => (T)x.GetRawConstantValue())
                .ToList();
        }

        public static byte[] GetAllBytes(this Stream stream)
        {
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                return memoryStream.ToArray();
            }
        }

        public static byte[] GetAllBytes(this IFormFile file)
        {
            using (var stream = file.OpenReadStream())
            {
                return stream.GetAllBytes();
            }
        }

        public static DateTime WeekStartDate(this DateTime date)
        {
            return date.AddDays(-7).Date;
        }

        public static DateTime MonthStartDate(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime LastMonthStartDate(this DateTime date)
        {
            return date.Month == 1
                ? new DateTime(date.Year - 1, 12, 1)
                : new DateTime(date.Year, date.Month - 1, 1);
        }

        public static DateTime NextMonthStartDate(this DateTime date)
        {
            return date.Month == 12
                ? new DateTime(date.Year + 1, 1, 1)
                : new DateTime(date.Year, date.Month + 1, 1);
        }

        public static DataTable ToDataTable<T>(this List<T> items)
        {
            DataTable dataTable = new(typeof(T).Name);

            //Get all the properties
            PropertyInfo[] Props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prop in Props)
            {
                //Setting column name as Display name
                var displayAttribute = (DisplayAttribute)prop.GetCustomAttribute(typeof(DisplayAttribute));
                if (displayAttribute != null)
                {
                    dataTable.Columns.Add(displayAttribute.Name);
                }
                //If Display name is empty, set property name as default column name.
                else
                {
                    
                    dataTable.Columns.Add(prop.Name);
                }
            }

            foreach (T item in items)
            {
                var values = new object[Props.Length];
                for (int i = 0; i < Props.Length; i++)
                {
                    //inserting property values to datatable rows
                    values[i] = Props[i].GetValue(item, null);
                }
                dataTable.Rows.Add(values);
            }

            //put a breakpoint here and check datatable
            return dataTable;
        }
    }
}
