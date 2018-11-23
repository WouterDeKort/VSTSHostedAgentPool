using System;
using Newtonsoft.Json;

namespace AzureDevOps.Operations.Models.Partials
{
    public partial class LinkSelf
    {
        [JsonProperty("href", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Href { get; set; }
    }
}