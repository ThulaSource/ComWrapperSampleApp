﻿using Newtonsoft.Json;

namespace COMWrapperSampleApp.OpenIdConnect.DCR.Client
{

    public class ClientRequest : Client
    {
        [JsonProperty("secrets")]
        public Secret[] Secrets { get; set; }

        [JsonProperty("copy_dcr_client_claims")]
        public bool CopyDcrClientClaims { get; set; }
    }
}
