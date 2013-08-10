using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Icfp2013
{
    [JsonObject(MemberSerialization.OptIn)]
    class TrainResponse
    {
        [JsonProperty("challenge")]
        public string Program;

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("size")]
        public int Size;

        [JsonProperty("operators")]
        public string[] Operators;
    }
}
