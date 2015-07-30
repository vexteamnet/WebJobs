using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Download.Models.VexDb
{
    public class Team
    {
        [JsonProperty("number")]
        public string Number { get; set; }

        [JsonProperty("team_name")]
        public string Name { get; set; }
        [JsonProperty("robot_name")]
        public string RobotName { get; set; }

        [JsonProperty("organisation")]
        public string Organization { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("region")]
        public string Region { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        
        [JsonProperty("is_registered"), JsonConverter(typeof(JsonBoolConverter), "1", "0")]
        public bool IsRegistered { get; set; }

        [JsonProperty("grade"), JsonConverter(typeof(JsonEnumConverter))]
        public Level Level { get; set; }
        [JsonProperty("program")]
        public Program Program { get; set; }
    }
}
