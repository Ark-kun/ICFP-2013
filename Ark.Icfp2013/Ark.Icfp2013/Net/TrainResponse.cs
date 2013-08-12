using Newtonsoft.Json;

namespace Ark.Icfp2013.Net {
    [JsonObject(MemberSerialization.OptIn)]
    public class TrainResponse {
        [JsonProperty("challenge")]
        public string Program;

        [JsonProperty("id")]
        public string Id;

        [JsonProperty("size")]
        public int Size;

        [JsonProperty("solved")]
        public bool? Solved;

        [JsonProperty("timeLeft")]
        public int? TimeLeft;

        [JsonProperty("operators")]
        public string[] Operators;
    }
}
