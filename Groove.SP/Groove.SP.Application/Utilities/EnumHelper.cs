using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Groove.SP.Application.Utilities
{
    public static class EnumHelper<T> where T : IConvertible
    {
        public static string GetDisplayName(T value)
        {
            return GetDisplayProperty(value, nameof(DisplayAttribute.Name));
        }

        public static string GetDisplayDescription(T value)
        {
            return GetDisplayProperty(value, nameof(DisplayAttribute.Description));
        }

        /// <summary>
        /// Gets the display property value
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>Property value of DisplayAttribute</returns>
        private static string GetDisplayProperty(T value, string propertyName)
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

            string propertyValue;
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

        /// <summary>
        /// Get description attribute value by short name
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string GetEnumDescriptionByShortName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var enumValues = Enum.GetValues(typeof(T));

            foreach (T item in enumValues)
            {
                var fieldInfo = item.GetType().GetField(item.ToString());
                var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DisplayAttribute), false) as DisplayAttribute[];

                if (descriptionAttributes == null || !descriptionAttributes.Any())
                {
                    continue;
                }
                if (descriptionAttributes[0].ShortName == value)
                {
                    return descriptionAttributes[0].Description;
                }
            }
            return string.Empty;
        }

        public static IList<T> ConvertToIds(T type)
        {
            var ids = new List<T>();
            var type_int = Convert.ToInt32(type);
            foreach (T item in Enum.GetValues(typeof(T)))
            {
                var item_int = Convert.ToInt32(item);
                if ((item_int & type_int) == item_int)
                {
                    ids.Add((T)Enum.Parse(typeof(T), item_int.ToString()));
                }
            }
            return ids;
        }

        public static T ConvertToInt(IList<T> ids)
        {
            int result = 0;
            foreach (var id in ids)
            {
                result |= (int)(object)id;
            }
            return (T)Enum.Parse(typeof(T), result.ToString());
        }
    }

    public static class EnumExtension
    {
        public static Expected GetAttributeValue<AttributeType, Expected>(this Enum enumeration, Func<AttributeType, Expected> propertyExpression) where AttributeType : Attribute
        {
            AttributeType attribute =
              enumeration
                .GetType()
                .GetMember(enumeration.ToString())
                .Where(member => member.MemberType == MemberTypes.Field)
                .FirstOrDefault()
                .GetCustomAttributes(typeof(AttributeType), false)
                .Cast<AttributeType>()
                .SingleOrDefault();

            if (attribute == null)
                return default(Expected);

            return propertyExpression(attribute);
        }

        public static T GetEnumValueFromDescription<T>(string description)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                            .SelectMany(f => f.GetCustomAttributes(
                                typeof(DisplayAttribute), false), (
                                    f, a) => new { Field = f, Att = a })
                            .Where(a => ((DisplayAttribute)a.Att)
                                .Description == description).SingleOrDefault();
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }

        public static T GetEnumValueFromMember<T>(string member)
        {
            var type = typeof(T);
            if (!type.IsEnum)
                throw new ArgumentException();
            FieldInfo[] fields = type.GetFields();
            var field = fields
                            .SelectMany(f => f.GetCustomAttributes(
                                typeof(EnumMemberAttribute), false), (
                                    f, a) => new { Field = f, Att = a })
                            .Where(a => ((EnumMemberAttribute)a.Att)
                                .Value == member).SingleOrDefault();
            return field == null ? default(T) : (T)field.Field.GetRawConstantValue();
        }

        public static string GetEnumMemberValue<T>(this T value) where T : Enum
        {
            return typeof(T)
                .GetTypeInfo()
                .DeclaredMembers
                .SingleOrDefault(x => x.Name == value.ToString())
                ?.GetCustomAttribute<EnumMemberAttribute>(false)
                ?.Value ?? string.Empty;
        }
    }
}
