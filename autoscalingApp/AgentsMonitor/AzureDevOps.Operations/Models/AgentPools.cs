namespace AzureDevOps.Operations.Models
{
    using AzureDevOps.Operations.Models.Partials;
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Get all pools https://{instanceName}.visualstudio.com/_apis/distributedtask/pools?api-version=4.1
    /// or https://dev.azure.com/{instanceName}/_apis/distributedtask/pools?api-version=4.1
    /// Generated with help of https://app.quicktype.io/#l=cs&amp;r=json2csharp
    /// </summary>
    public partial class AgentsPools
    {
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Pool[] Pools { get; set; }
    }

    public partial class Pool
    {
        [JsonProperty("createdOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("autoProvision", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AutoProvision { get; set; }

        [JsonProperty("autoSize", NullValueHandling = NullValueHandling.Ignore)]
        public bool? AutoSize { get; set; }

        [JsonProperty("agentCloudId")]
        public object AgentCloudId { get; set; }

        [JsonProperty("createdBy", NullValueHandling = NullValueHandling.Ignore)]
        public CreatedBy CreatedBy { get; set; }

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public CreatedBy Owner { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("scope", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Scope { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("isHosted", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsHosted { get; set; }

        [JsonProperty("poolType", NullValueHandling = NullValueHandling.Ignore)]
        public string PoolType { get; set; }

        [JsonProperty("size", NullValueHandling = NullValueHandling.Ignore)]
        public long? Size { get; set; }
    }

    public partial class CreatedBy
    {
        [JsonProperty("displayName", NullValueHandling = NullValueHandling.Ignore)]
        public string DisplayName { get; set; }

        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public Uri Url { get; set; }

        [JsonProperty("_links", NullValueHandling = NullValueHandling.Ignore)]
        public AvatarLinks Links { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? Id { get; set; }

        [JsonProperty("uniqueName", NullValueHandling = NullValueHandling.Ignore)]
        public string UniqueName { get; set; }

        [JsonProperty("imageUrl", NullValueHandling = NullValueHandling.Ignore)]
        public Uri ImageUrl { get; set; }

        [JsonProperty("isContainer", NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsContainer { get; set; }

        [JsonProperty("descriptor", NullValueHandling = NullValueHandling.Ignore)]
        public string Descriptor { get; set; }
    }

    public partial class AvatarLinks
    {
        [JsonProperty("avatar", NullValueHandling = NullValueHandling.Ignore)]
        public LinkSelf Avatar { get; set; }
    }
}
