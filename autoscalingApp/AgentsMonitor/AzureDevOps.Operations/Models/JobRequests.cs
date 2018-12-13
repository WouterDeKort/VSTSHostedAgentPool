namespace AzureDevOps.Operations.Models
{
    using AzureDevOps.Operations.Models.Partials;
    using Newtonsoft.Json;
    using System;

    /// <summary>
    /// List all jobs requests in current pool via request https://{instanceName}.visualstudio.com/_apis/distributedtask/pools/{poolId}/jobrequests?api-version=4.1
    /// or https://dev.azure.com/{instanceName}/_apis/distributedtask/pools/{poolId}/jobrequests?api-version=4.1
    /// Generated with help of https://app.quicktype.io/#l=cs&amp;r=json2csharp
    /// </summary>
    public partial class JobRequests
    {
        [JsonProperty("count", NullValueHandling = NullValueHandling.Ignore)]
        public long? Count { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public JobRequest[] AllJobRequests { get; set; }
    }

    public partial class JobRequest
    {
        [JsonProperty("requestId", NullValueHandling = NullValueHandling.Ignore)]
        public long? RequestId { get; set; }

        [JsonProperty("queueTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? QueueTime { get; set; }

        [JsonProperty("assignTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? AssignTime { get; set; }

        [JsonProperty("receiveTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? ReceiveTime { get; set; }

        [JsonProperty("finishTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? FinishTime { get; set; }

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public string Result { get; set; }

        [JsonProperty("serviceOwner", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ServiceOwner { get; set; }

        [JsonProperty("hostId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? HostId { get; set; }

        [JsonProperty("scopeId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? ScopeId { get; set; }

        [JsonProperty("planType", NullValueHandling = NullValueHandling.Ignore)]
        public string PlanType { get; set; }

        [JsonProperty("planId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? PlanId { get; set; }

        [JsonProperty("jobId", NullValueHandling = NullValueHandling.Ignore)]
        public Guid? JobId { get; set; }

        [JsonProperty("demands", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Demands { get; set; }

        [JsonProperty("reservedAgent", NullValueHandling = NullValueHandling.Ignore)]
        public ReservedAgent ReservedAgent { get; set; }

        [JsonProperty("definition", NullValueHandling = NullValueHandling.Ignore)]
        public Definition Definition { get; set; }

        [JsonProperty("owner", NullValueHandling = NullValueHandling.Ignore)]
        public Definition Owner { get; set; }

        [JsonProperty("data", NullValueHandling = NullValueHandling.Ignore)]
        public Data Data { get; set; }

        [JsonProperty("poolId", NullValueHandling = NullValueHandling.Ignore)]
        public long? PoolId { get; set; }

        [JsonProperty("agentDelays", NullValueHandling = NullValueHandling.Ignore)]
        public object[] AgentDelays { get; set; }

        [JsonProperty("orchestrationId", NullValueHandling = NullValueHandling.Ignore)]
        public string OrchestrationId { get; set; }
    }

    public partial class Data
    {
        [JsonProperty("ParallelismTag", NullValueHandling = NullValueHandling.Ignore)]
        public string ParallelismTag { get; set; }
    }

    public partial class Definition
    {
        [JsonProperty("_links", NullValueHandling = NullValueHandling.Ignore)]
        public Links Links { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

    public partial class ReservedAgent
    {
        [JsonProperty("_links", NullValueHandling = NullValueHandling.Ignore)]
        public Links Links { get; set; }

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
}
