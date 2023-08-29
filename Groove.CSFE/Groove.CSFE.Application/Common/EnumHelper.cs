using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Groove.CSFE.Application.Common
{
    public static class EnumHelper<T> where T : IConvertible
    {
        public static string GetDisplayValue(T value)
        {
            return GetDisplayValue(value, nameof(DisplayAttribute.Name));
        }

        /// <summary>
        /// Gets the display value (default property: DisplayAttribute.Name).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Property value of DisplayAttribute</returns>
        public static string GetDisplayValue(T value, string propertyName)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());

            if (fieldInfo == null || string.IsNullOrEmpty(propertyName))
            {
                return string.Empty;
            }

            var displayValue = value.ToString();

            var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

            if (descriptionAttributes == null || !descriptionAttributes.Any())
            {
                return displayValue;
            }

            var propertyValue = string.Empty;

            switch (propertyName)
            {
                case nameof(DisplayAttribute.Description):
                    propertyValue = descriptionAttributes[0].Description;
                    break;
                case nameof(DisplayAttribute.Name):
                    propertyValue = descriptionAttributes[0].Name;
                    break;
                case nameof(DisplayAttribute.ShortName):
                    propertyValue = descriptionAttributes[0].ShortName;
                    break;
                case nameof(DisplayAttribute.GroupName):
                    propertyValue = descriptionAttributes[0].GroupName;
                    break;
                default:
                    propertyValue = string.Empty;
                    break;
            }
            
            return propertyValue;
        }
    }
}
