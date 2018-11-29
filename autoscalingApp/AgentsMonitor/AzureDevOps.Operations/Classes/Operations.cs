using AzureDevOps.Operations.Helpers;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Configuration;

namespace AzureDevOps.Operations.Classes
{
    public static class Operations
    {
        /// <summary>
        /// Here we will proceed working with VMSS ((de)provision additional agents, keep current agents count)
        /// </summary>
        /// <param name="waitingJobs"></param>
        /// <param name="onlineAgents"></param>
        /// <param name="maxAgentsInPool"></param>
        /// <param name="dataRetriever">Used to get data from Azure DevOps</param>
        /// <param name="agentsPoolId"></param>
        public static void WorkWithVmss(int onlineAgents, int maxAgentsInPool, Retrieve dataRetriever, int agentsPoolId)
        {
            var credentials = AzureCreds();

            var azure = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithSubscription(ConfigurationManager.AppSettings[Constants.AzureSubscriptionIdSettingName]);

            //working with VMSS
            var resourceGroupName = ConfigurationManager.AppSettings[Constants.AzureVmssResourceGroupSettingName];
            var vmssName = ConfigurationManager.AppSettings[Constants.AzureVmssNameSettingName];
            var vmss = azure.VirtualMachineScaleSets.GetByResourceGroup(resourceGroupName, vmssName);
            //get jobs again to check, if we could deallocate a VM in VMSS (if it is running a job - it is not wise to deallocate it)
            var currentJobs = dataRetriever.GetRuningJobs(agentsPoolId);
            var addMoreAgents = Decisions.AddMoreAgents(currentJobs.Length, onlineAgents);
            var amountOfAgents = Decisions.HowMuchAgents(currentJobs.Length, onlineAgents, maxAgentsInPool);

            if (amountOfAgents == 0)
            {
                //nevertheless - should we (de)provision agents: we are at boundaries
                Environment.Exit(Constants.SuccessExitCode);
            }


            //foreach (var vmssVm in vmss.VirtualMachines.List())
            //{
            //    vmssVm.Deallocate();
            //}
        }

        private static AzureCredentials AzureCreds()
        {
            var clientId = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleClientIdSettingName];
            var clientSecret = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleClientSecretSettingName];
            var tenantId = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleTenantIdSettingName];
            //maybe in future I'll need to extend this one to allow other then Global Azure environment
            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
        }
    }
}