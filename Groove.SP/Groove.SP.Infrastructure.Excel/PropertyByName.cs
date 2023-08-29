using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Groove.SP.Infrastructure.Excel
{
    public class PropertyByName<T>
    {
        public PropertyByName(string propertyName, Func<T, object> func = null)
        {
            this.ColumnName = propertyName;
            this.GetProperty = func;
            this.ColumnOrderPosition = 1;
        }

        public PropertyInfo Property { get; set; }

        public int ColumnOrderPosition { get; set; }

        /// <summary>
        /// Feature property access
        /// </summary>
        public Func<T, object> GetProperty { get; }

        public string ColumnName { get; set; }

        public object PropertyValue { get; set; }

        /// <summary>
        /// Converted property value to Int32
        /// </summary>
        public int? IntValue
        {
            get
            {
                if (PropertyValue == null || !int.TryParse(PropertyValue.ToString(), out int rez))
                    return null;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to Int64
        /// </summary>
        public long? LongValue
        {
            get
            {
                if (PropertyValue == null || !long.TryParse(PropertyValue.ToString(), out long rez))
                    return null;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to bool
        /// </summary>
        public bool? BooleanValue
        {
            get
            {
                if (PropertyValue == null || !bool.TryParse(PropertyValue.ToString(), out bool rez))
                    return null;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to string
        /// </summary>
        public string StringValue
        {
            get
            {
                return PropertyValue == null ? string.Empty : Convert.ToString(PropertyValue).Trim();
            }
        }

        /// <summary>
        /// Converted property value to decimal?
        /// </summary>
        public decimal? DecimalValue
        {
            get
            {
                if (PropertyValue == null || !decimal.TryParse(PropertyValue.ToString(), out decimal rez))
                    return null;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to double?
        /// </summary>
        public double? DoubleValue
        {
            get
            {
                if (PropertyValue == null || !double.TryParse(PropertyValue.ToString(), out double rez))
                    return null;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to DateTime?
        /// </summary>
        public DateTime? DateTimeValue
        {
            get
            {
                if (PropertyValue == null || !DateTime.TryParse(PropertyValue.ToString(), out DateTime rez))
                    return null;
                return rez;
            }
        }

        /// <summary>
        /// Converted property value to Enum
        /// </summary>
        public object EnumValue
        {
            get
            {
                var enumType = Nullable.GetUnderlyingType(Property.PropertyType);
                if (PropertyValue == null)
                {
                    return null;
                }

                bool isEnumConverted = Enum.TryParse(enumType, PropertyValue.ToString(), true, out var rez)
                    || TryParseSerializedValueToEnum(enumType, PropertyValue.ToString(), out rez);

                if (!isEnumConverted)
                {
                    return null;
                }

                return rez;
            }
        }

        /// <summary>
        /// Converts the serialized value associated with a enumeration member to an equivalent enumerated object.
        /// </summary>
        /// <param name="enumType"></param>
        /// <param name="value"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool TryParseSerializedValueToEnum(Type enumType, string value, out object result)
        {
            var isConverted = false;
            result = null;
            foreach (var name in Enum.GetNames(enumType))
            {
                var enumMemberAttribute = ((EnumMemberAttribute[])enumType.GetField(name).GetCustomAttributes(typeof(EnumMemberAttribute), true)).SingleOrDefault();
                if (enumMemberAttribute != null && enumMemberAttribute.Value.ToLower() == value.ToLower())
                {
                    result = Enum.Parse(enumType, name);
                    isConverted = true;
                    break;
                }
            }

            return isConverted;
        }
    }
}
