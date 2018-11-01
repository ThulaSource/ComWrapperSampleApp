using Newtonsoft.Json;

namespace COMWrapperSampleApp.OpenIdConnect.DCR.Api
{
    public class ApiResourceRequest : ApiResource
    {
        [JsonProperty("secrets")]
        public Secret[] Secrets { get; set; }
    }
}