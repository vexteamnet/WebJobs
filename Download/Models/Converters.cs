using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VexTeamNetwork.Models;

namespace Download.Models
{
    public static class Converters
    {
        public static bool ToBoolean(this string value, string @true, string @false)
        {
            if (value.Equals(@true))
                return true;
            if (value.Equals(@false))
                return false;
            throw new ArgumentOutOfRangeException(nameof(value), value, $"Value was not either {@true} or {@false}");
        }

        public static VexTeamNetwork.Models.Program ToProgram(this string value)
        {
            VexTeamNetwork.Models.Program program;
            if (Enum.TryParse(value, out program))
                return program;
            return VexTeamNetwork.Models.Program.Unknown;
        }

        public static Level ToLevel(this string value)
        {
            Dictionary<string, Level> map = new Dictionary<string, Level>
                {
                    { "Elementary", Level.ElementarySchool },
                    { "Middle School", Level.MiddleSchool },
                    { "High School", Level.HighSchool },
                    { "College", Level.University },
                    { "", Level.Unknown }
                };
            if (map.ContainsKey(value))
                return map[value];
            return Level.Unknown;
        }
    }
}
