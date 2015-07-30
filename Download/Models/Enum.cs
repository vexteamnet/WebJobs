using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Download.Models
{
    public enum Program
    {
        VEXU,
        VRC,
        VIQC,
        Unknown
    }

    public enum Level
    {
        [JsonEnumMember(Name = "")]
        Unknown,
        [JsonEnumMember(Name = "Elementary")]
        ElementarySchool,
        [JsonEnumMember(Name = "Middle School")]
        MiddleSchool,
        [JsonEnumMember(Name = "High School")]
        HighSchool,
        [JsonEnumMember(Name = "College")]
        University
    }
}
