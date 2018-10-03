using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Json;
using System.Text;
using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Launcher.Helper
{
    public static class JsonHelper
    {
        public static string Serialize<T>(object item, Formatting formatting = Formatting.Indented)
        {
            if (item == null)
                return JsonConvert.SerializeObject(null, formatting);

            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
            {
                using (var writer = new JsonTextWriter(sw))
                {
                    var converter = new CustomJsonConverter<T>();
                    var serializer = new JsonSerializer { Formatting = formatting };

                    converter.WriteJson(writer, item, serializer);
                }

                return sb.ToString();
            }
        }

        #region Nested

        private class CustomJsonConverter<T> : JsonConverter<T>
        {
            public override void WriteJson(JsonWriter writer, T value, JsonSerializer serializer)
            {
                writer.Formatting = serializer.Formatting;
                writer.StringEscapeHandling = serializer.StringEscapeHandling;

                var props = typeof(T).GetTypeInfo().DeclaredProperties.ToList();

                writer.WriteStartObject();

                //var resolver = serializer.ContractResolver;
                //var contract = resolver.ResolveContract(typeof(T));

                foreach (var prop in props)
                {
                    try
                    {
                        var propValue = prop.GetValue(value);

                        if (!CanWriteProperty(prop, serializer, propValue))
                            continue;

                        writer.WritePropertyName(prop.Name);
                        serializer.Serialize(writer, propValue);
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine(e.Message);
                    }
                }

                writer.WriteEndObject();
            }

            /// <summary>
            /// Можем ли записать поле
            /// </summary>
            /// <param name="prop">Информация о свойстве</param>
            /// <param name="serializer">Сериализатор</param>
            /// <param name="propValue">Значение свойства</param>
            /// <returns></returns>
            private bool CanWriteProperty(PropertyInfo prop, JsonSerializer serializer, object propValue)
            {
                if (serializer.NullValueHandling == NullValueHandling.Ignore && Equals(null, propValue))
                    return false;

                var ignoreAttr = prop.GetCustomAttribute<JsonIgnoreAttribute>();
                if (ignoreAttr != null)
                    return false;

                var attr = prop.GetCustomAttribute<JsonPropertyAttribute>();
                if (attr != null)
                {
                    if (attr.NullValueHandling == NullValueHandling.Ignore && Equals(null, propValue))
                        return false;
                }

                return true;
            }

            public override T ReadJson(JsonReader reader, Type objectType, T existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
