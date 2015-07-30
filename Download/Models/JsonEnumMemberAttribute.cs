using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Download.Models
{
    internal class JsonEnumMemberAttribute : Attribute
    {
        public string Name { get; set; }
    }

    internal class JsonEnumConverter : JsonConverter
    {

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsEnum;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            string value = reader.Value.ToString();

            foreach(Enum val in Enum.GetValues(objectType))
            {
                FieldInfo fi = objectType.GetField(val.ToString());
                JsonEnumMemberAttribute attribute = (JsonEnumMemberAttribute)fi.GetCustomAttribute(typeof(JsonEnumMemberAttribute), false);
                if (attribute.Name == value)
                    return val;
            }

            throw new ArgumentException($"The value '{value}' is not supported.");
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
