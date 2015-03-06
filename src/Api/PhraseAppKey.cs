using System.Collections.Generic;
using Newtonsoft.Json;

namespace VercorsStudio.PhraseApp.Client.Window.Api
{
    public class PhraseAppKey
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "pluralized")]
        public bool Pluralized { get; set; }

        [JsonProperty(PropertyName = "data_type")]
        public PhraseAppDataType Type { get; set; }

        [JsonProperty(PropertyName = "tag_list")]
        public List<string> Tags { get; set; } 
    }
}