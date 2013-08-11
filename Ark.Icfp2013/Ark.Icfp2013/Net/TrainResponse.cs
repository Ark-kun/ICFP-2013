using Newtonsoft.Json;

namespace Ark.Icfp2013.Net {
    [JsonObject(MemberSerialization.OptIn)]
    class TrainResponse {
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
