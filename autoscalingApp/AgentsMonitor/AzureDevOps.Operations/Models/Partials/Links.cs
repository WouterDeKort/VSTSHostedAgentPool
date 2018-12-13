using Newtonsoft.Json;

namespace AzureDevOps.Operations.Models.Partials
{
    public partial class Links
    {
        [JsonProperty("self", NullValueHandling = NullValueHandling.Ignore)]
        public LinkSelf LinkSelf { get; set; }

        [JsonProperty("web", NullValueHandling = NullValueHandling.Ignore)]
        public LinkSelf Web { get; set; }
    }
}