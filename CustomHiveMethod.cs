using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EpochHive
{
    public class CustomHiveMethod
    {
        public string MethodName { get; set; }
        public string SqlString { get; set; }
        public List<string> ReturnTypes { get; set; }

        [JsonIgnore]
        public string[] Parameters;
    }
}
