using System.Collections.Generic;

namespace Download.Models.VexDb
{
    public class RootObject<T>
    {
        public int status { get; set; }

        public int error_code { get; set; }
        public string error_text { get; set; }

        public int size { get; set; }

        public ICollection<T> result { get; set; }
    }
}
