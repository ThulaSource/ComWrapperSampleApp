using Newtonsoft.Json;

namespace COMWrapperSampleApp.OpenIdConnect.DCR.Client
{
    public class ClientClaim
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}
