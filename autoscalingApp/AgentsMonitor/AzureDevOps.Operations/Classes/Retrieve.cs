﻿using AzureDevOps.Operations.Helpers;
using AzureDevOps.Operations.Models;
using System;
using System.Linq;
using System.Net.Http;

namespace AzureDevOps.Operations.Classes
{
    public class Retrieve
    {
        /// <summary>
        /// Organization agentsPoolName in Azure DevOps
        /// </summary>
        private string AzureDevOpsOrganizationName { get; }
        /// <summary>
        /// PAT (Personal Access Token) to access Azure DevOps
        /// </summary>
        private string AzureDevOpsPersonalAccessToken { get; }
        /// <summary>
        /// Needed for mocking and testing
        /// </summary>
        private readonly HttpClient _localHttpClient;


        public Retrieve(string orgName, string token, HttpClient httpClient)
        {
            AzureDevOpsOrganizationName = orgName;
            AzureDevOpsPersonalAccessToken = token;
            _localHttpClient = httpClient;
        }
        /// <summary>
        /// Starting string for an URL
        /// </summary>
        private const string AzureDevOpsUrl = "https://dev.azure.com";
        /// <summary>
        /// API used to retrieve running tasks
        /// </summary>
        private const string TasksBaseUrl = "_apis/distributedtask/pools";

        /// <summary>
        /// Retrieves pool id basing on Name
        /// </summary>
        /// <param name="agentsPoolName"></param>
        /// <returns></returns>
        public int? GetPoolId(string agentsPoolName)
        {
            var url = $"{AzureDevOpsUrl}/{AzureDevOpsOrganizationName}/{TasksBaseUrl}";

            var allPools = GetData.DownloadSerializedJsonData<AgentsPools>(url, AzureDevOpsPersonalAccessToken, _localHttpClient);

            if (allPools == null)
            {
                return null;
            }

            foreach (var agentPool in allPools.Pools)
            {
                if (!agentPool.Name.Equals(agentsPoolName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (agentPool.Id != null)
                {
                    return (int)agentPool.Id.Value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets all agents, which is online now
        /// </summary>
        /// <param name="agentsPoolId"></param>
        /// <returns></returns>
        public int? GetOnlineAgentsCount(int agentsPoolId)
        {
            const string requiredStatus = "online";

            var allAgents = GetAllAgentsRunningNow(agentsPoolId);

            return allAgents?.AllAgents.Count(agent => agent.Status.Equals(requiredStatus, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Gets all possible agents in current pool count (this shall be set on provisioning time)
        /// </summary>
        /// <param name="agentsPoolId"></param>
        /// <returns></returns>
        public int GetAllAccessibleAgents(int agentsPoolId)
        {
            //gets agents in all statuses assigned to pool; maybe need to check VMSS size instead??
            var allAgents = GetAllAgentsRunningNow(agentsPoolId);
            if (allAgents?.Count != null)
            {
                return (int)allAgents.Count.Value;
            }

            return 0;
        }

        public int GetCurrentJobsRunning(int agentsPoolId)
        {
            var url = $"{AzureDevOpsUrl}/{AzureDevOpsOrganizationName}/{TasksBaseUrl}/{agentsPoolId}/jobrequests";

            var allJobsRequests = GetData.DownloadSerializedJsonData<JobRequests>(url, AzureDevOpsPersonalAccessToken, _localHttpClient);

            //count amount of jobs without result - they are running
            return allJobsRequests?.AllJobRequests.Count(jobRequest => jobRequest.Result == null) ?? 0;
        }

        private Agents GetAllAgentsRunningNow(int agentsPoolId)
        {
            var url = $"{AzureDevOpsUrl}/{AzureDevOpsOrganizationName}/{TasksBaseUrl}/{agentsPoolId}/agents";
            return GetData.DownloadSerializedJsonData<Agents>(url, AzureDevOpsPersonalAccessToken, _localHttpClient);
        }
    }
}