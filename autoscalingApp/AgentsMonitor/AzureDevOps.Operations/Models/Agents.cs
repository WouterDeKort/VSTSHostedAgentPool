

namespace AzureDevOps.Operations.Models
{
    using AzureDevOps.Operations.Models.Partials;
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// Get all agents and theirs status via https://{instanceName}.visualstudio.com/_apis/distributedtask/pools/{poolId}/agents?api-version=4.1
    /// or https://dev.azure.com/{instanceName}/_apis/distributedtask/pools/{poolId}/agents?api-version=4.1
    /// Generated with help of https://app.quicktype.io/#l=cs&amp;r=json2csharp
    /// </summary>
    public partial class Agents
    {
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Agent[] AllAgents { get; set; }
    }

    public partial class Agent
    {
        [JsonProperty("_links", NullValueHandling = NullValueHandling.Ignore)]
        public Links Links { get; set; }

        [JsonProperty("maxParallelism", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxParallelism { get; set; }

        [JsonProperty("createdOn", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CreatedOn { get; set; }

        [JsonProperty("authorization", NullValueHandling = NullValueHandling.Ignore)]
        public Authorization Authorization { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }

        [JsonProperty("osDescription", NullValueHandling = NullValueHandling.Ignore)]
        public string OsDescription { get; set; }

        [JsonProperty("enabled", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Enabled { get; set; }

        [JsonProperty("status", NullValueHandling = NullValueHandling.Ignore)]
        public string Status { get; set; }

        [JsonProperty("provisioningState", NullValueHandling = NullValueHandling.Ignore)]
        public string ProvisioningState { get; set; }

        [JsonProperty("accessPoint", NullValueHandling = NullValueHandling.Ignore)]
        public string AccessPoint { get; set; }
    }

    public partial class Authorization
    {
        [JsonProperty("clientId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ClientId { get; set; }

        [JsonProperty("publicKey", NullValueHandling = NullValueHandling.Ignore)]
        public PublicKey PublicKey { get; set; }
    }

    public partial class PublicKey
    {
        [JsonProperty("exponent", NullValueHandling = NullValueHandling.Ignore)]
        public string Exponent { get; set; }

        [JsonProperty("modulus", NullValueHandling = NullValueHandling.Ignore)]
        public string Modulus { get; set; }
    }
}
