using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
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

        [JsonProperty("grade"), JsonConverter(typeof(LevelConverter))]
        public VexTeamNetwork.Models.Level Level { get; set; }
        [JsonProperty("program")]
        public VexTeamNetwork.Models.Program Program { get; set; }

        public static Task<ICollection<Team>> Download(
            string teamNumber = null,
            VexTeamNetwork.Models.Program? program = null, 
            string organization = null,
            string city = null,
            string region = null,
            string country = null,
            VexTeamNetwork.Models.Level? level = null,
            bool? isRegistered = null,
            string sku = null)
        {
            string request = "http://api.vex.us.nallen.me/get_teams?";
            if (!string.IsNullOrWhiteSpace(teamNumber))
                request += $"team={teamNumber}&";
            if (program.HasValue)
                request += $"program={program.Value}&";
            if (!string.IsNullOrWhiteSpace(organization))
                request += $"organisation={organization}&";
            if (!string.IsNullOrWhiteSpace(city))
                request += $"city={city}&";
            if (!string.IsNullOrWhiteSpace(region))
                request += $"region={region}&";
            if (!string.IsNullOrEmpty(country))
                request += $"country={country}&";
            if (level.HasValue)
                request += $"level={LevelConverter.map.FirstOrDefault(e => e.Value == level).Key}&";
            if (isRegistered.HasValue)
                request += $"is_registered=" + (isRegistered.Value ? "1" : "0") + "&";
            if (!string.IsNullOrEmpty(sku))
                request += $"sku={sku}&";
            
            return Downloader.Download<Team>(request);
        }

        public VexTeamNetwork.Models.Team ToDbTeam()
        {
            return new VexTeamNetwork.Models.Team()
            {
                Number = Number,
                Name = Name,
                RobotName = RobotName,
                Organization = Organization,
                City = City,
                Region = Region,
                Country = Country,
                IsRegistered = IsRegistered, 
                Level = Level,
                Program = Program
            };
        }
    }
}
