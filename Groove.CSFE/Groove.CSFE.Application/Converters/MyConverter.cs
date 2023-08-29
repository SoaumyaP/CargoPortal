using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Groove.CSFE.Application.Converters.Interfaces;
using Groove.CSFE.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Groove.CSFE.Application.Converters
{
    public class MyConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        // Always return False here since we don't need to implement WriteJson method.
        public override bool CanWrite => false;
        
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jsonObj = JObject.Load(reader);
            var targetObj = (IHasFieldStatus)Activator.CreateInstance(objectType);

            var dict = new Dictionary<string, FieldDeserializationStatus>();
            targetObj.FieldStatus = dict;

            foreach (var prop in objectType.GetProperties())
            {
                if (!prop.CanWrite || prop.Name == "FieldStatus") continue;

                if (jsonObj.TryGetValue(prop.Name, StringComparison.OrdinalIgnoreCase, out var value))
                {
                    // If the field is present on the JSON and set to string of empty or null. We will set it null.
                    if (value.Type == JTokenType.Array)
                    {
                        prop.SetValue(targetObj, value.ToObject(prop.PropertyType, serializer));
                    }
                    else
                    {
                        try
                        {
                            if (value.Value<string>() == "null")
                            {
                                prop.SetValue(targetObj, null);
                            }
                            else if (string.IsNullOrWhiteSpace(value.Value<string>()))
                            {
                                prop.SetValue(targetObj, null);
                            }
                            else
                            {
                                prop.SetValue(targetObj, value.ToObject(prop.PropertyType, serializer));
                            }
                        }
                        catch (Exception e)
                        {
                            prop.SetValue(targetObj, value.ToObject(prop.PropertyType, serializer));
                        }
                    }

                    dict.Add(prop.Name, FieldDeserializationStatus.HasValue);
                }
                else
                {
                    dict.Add(prop.Name, FieldDeserializationStatus.WasNotPresent);
                }
            }

            return targetObj;
        }

        public override bool CanConvert(Type objectType)
        {
            return (objectType.IsClass &&
                    objectType.GetInterfaces().Any(i => i == typeof(IHasFieldStatus)));
        }
    }
}