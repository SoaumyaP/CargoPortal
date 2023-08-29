using System.Collections.Generic;
using System;
using System.Linq;
using AutoMapper.Internal;
using System.Collections;
using System.Reflection;
using Groove.SP.Application.ViewSetting.Interfaces;

namespace Groove.SP.Application.Utilities
{
    public static class PropertyToggler
    {
        /// <summary>
        /// Toggle properties display at the specific selector.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="result"></param>
        /// <param name="selector"></param>
        /// <param name="fieldToDisplay"></param>
        public static void ToggleDisplay<T1, T2>(T1 result, Func<T1, IEnumerable<T2>> selector, string[] fieldToDisplay)
        {
            var selectorObject = selector.Invoke(result);

            var typeOfDataSource = selectorObject.GetType().GetGenericArguments()[0];

            var properties = typeOfDataSource.GetProperties();

            foreach (var property in properties)
            {
                var propertyName = property.Name;
                if (!fieldToDisplay.Any(f => f.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))) // ignore case comparision
                {
                    foreach (var item in selectorObject)
                    {
                        ClearPropertyValue(item, propertyName);
                    }
                }
            }
        }

        /// <summary>
        /// Toggle properties display on the object (support nested object) which has inherited IHasViewSetting.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="fieldDictionary"></param>
        public static void ToggleDisplay<T>(T result, Dictionary<string, List<string>> fieldDictionary)
        {
            if (result is not IEnumerable)
            {
                //must implement IHasViewSetting
                if (!IsAssignableFrom<IHasViewSetting>(result.GetType()))
                {
                    return;
                }

                var properties = result.GetType().GetProperties(BindingFlags.DeclaredOnly |
                    BindingFlags.Instance | BindingFlags.Public
                    ).Where(x => !x.IsContainPropertyFrom<IHasViewSetting>()).ToList();

                //get configured module id
                var vwSettingId = (string)GetPropertyValue<T>(result, nameof(IHasViewSetting.ViewSettingModuleId));

                //get display fields by key
                var displayFields = fieldDictionary.FirstOrDefault(x => x.Key.ToLower() == vwSettingId?.ToLower()).Value;

                foreach (var property in properties)
                {

                    var propertyName = property.Name;
                    var currentPropertyValue = GetPropertyValue(result, propertyName);

                    if (currentPropertyValue == null)
                    {
                        continue;
                    }

                    if (IsAssignableFrom<IHasViewSetting>(property.PropertyType))
                    {
                        /*
                         * making callback (recursion) if property has implemented IHasViewSetting
                         */
                        ToggleDisplay(currentPropertyValue, fieldDictionary);

                        continue;
                    }

                    if (displayFields is null || !displayFields.Any()) continue;

                    if (!displayFields.Any(f => f.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))) // ignore case comparision
                    {

                        var currentPropertyType = currentPropertyValue.GetType();
                        // Do not reset if it is navigation property: class/array
                        if ((currentPropertyType.IsValueType || currentPropertyType.FullName == "System.String"))
                        {
                            ClearPropertyValue(result, propertyName);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in (IEnumerable)result)
                {
                    ToggleDisplay(item, fieldDictionary);
                }
            }
        }

        public static void ToggleDisplay<T>(T result, string[] fieldToDisplay)
        {
            if (result is not IEnumerable)
            {
                var properties = result.GetType().GetProperties().Where(x => x.IsPublic() && x.CanWrite).ToList();

                foreach (var property in properties)
                {
                    var propertyName = property.Name;
                    if (!fieldToDisplay.Any(f => f.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))) // ignore case comparision
                    {
                        var currentPropertyValue = GetPropertyValue(result, propertyName);
                        if (currentPropertyValue == null)
                        {
                            continue;
                        }
                        var currentPropertyType = currentPropertyValue.GetType();
                        // Do not reset if it is navigation property: class/array
                        if (currentPropertyType.IsValueType || currentPropertyType.FullName == "System.String")
                        {
                            ClearPropertyValue(result, propertyName);
                        }
                    }
                }
            }
            else
            {
                foreach (var item in (IEnumerable)result)
                {
                    ToggleDisplay(item, fieldToDisplay);
                }
            }
        }

        /// <summary>
        /// Remove current value/set null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="propertyName"></param>
        private static void ClearPropertyValue<T>(T result, string propertyName)
        {
            var pi = result.GetType().GetProperty(propertyName);
            if (pi.CanWrite)
                pi.SetValue(result, null);
        }

        /// <summary>
        /// Get current property value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="result"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static object GetPropertyValue<T>(T result, string propertyName)
        {
            return result.GetType().GetProperty(propertyName).GetValue(result);
        }

        private static bool IsContainPropertyFrom<T>(this PropertyInfo pi)
        {
            return typeof(T).GetProperties().Any(x => x.Name.ToLower() == pi.Name.ToLower());
        }

        private static bool IsAssignableFrom<T>(Type type)
        {
            if (IsGenericType(type))
            {
                type = type.GetGenericArguments()[0];
            }

            return typeof(T).IsAssignableFrom(type);
        }

        private static bool IsGenericType(Type type)
        {
            return type.IsGenericType && (
                type.GetGenericTypeDefinition().Equals(typeof(IEnumerable<>)) ||
                type.GetGenericTypeDefinition().Equals(typeof(ICollection<>)) ||
                type.GetGenericTypeDefinition().Equals(typeof(IList<>)));
        }
    }
}
