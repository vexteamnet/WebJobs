using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VexTeamNetwork.Models;

namespace Download.Models.VexDb
{
    public class LevelConverter : JsonConverter
    {
        public static Dictionary<string, Level> map = new Dictionary<string, Level>
        {
            { "Elementary", Level.ElementarySchool },
            { "Middle School", Level.MiddleSchool },
            { "High School", Level.HighSchool },
            { "College", Level.University },
            { "", Level.Unknown }
        };
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(VexTeamNetwork.Models.Level);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Level _return;
            if (map.TryGetValue((string)reader.Value, out _return))
                return _return;
            return Level.Unknown;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
