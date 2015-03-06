using Newtonsoft.Json;

namespace VercorsStudio.PhraseApp.Client.Window.Api
{
    public class PhraseAppAuthStatus
    {
        [JsonProperty(PropertyName = "success")]
        public bool Success { get; set; }

        [JsonProperty(PropertyName = "auth_token")]
        public string AuthToken { get; set; }
    }
}