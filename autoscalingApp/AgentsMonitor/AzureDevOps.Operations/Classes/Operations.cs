using AzureDevOps.Operations.Helpers;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using AzureDevOps.Operations.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using TableStorageClient.Models;

namespace AzureDevOps.Operations.Classes
{
    public static class Operations
    {
        /// <summary>
        /// Here we will proceed working with VMSS ((de)provision additional agents, keep current agents count)
        /// </summary>
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
            var virtualMachines = vmss.VirtualMachines.List()
                .Select(vmssVm => new ScaleSetVirtualMachineStripped
                {
                    VmInstanceId = vmssVm.InstanceId, 
                    VmName = vmssVm.ComputerName,
                    VmInstanceState = vmssVm.PowerState
                }).ToList();
            //get jobs again to check, if we could deallocate a VM in VMSS (if it is running a job - it is not wise to deallocate it)
            var currentJobs = dataRetriever.GetRuningJobs(agentsPoolId);
            var addMoreAgents = Decisions.AddMoreAgents(currentJobs.Length, onlineAgents);
            var amountOfAgents = Decisions.HowMuchAgents(currentJobs.Length, onlineAgents, maxAgentsInPool);

            if (amountOfAgents == 0)
            {
                //nevertheless - should we (de)provision agents: we are at boundaries
                Console.WriteLine("Could not add/remove more agents, exiting...");
                Environment.Exit(Constants.SuccessExitCode);
            }

            var isDryRun = GetTypedSetting.GetSetting<bool>(Constants.DryRunSettingName);

#pragma warning disable 4014
            //I wish this record to be processed on it's own; it is just tracking
            RecordDataInTable(resourceGroupName, vmssName, addMoreAgents);
#pragma warning restore 4014

            if (!addMoreAgents)
            {
                //TODO: Record deallocation
                Console.WriteLine("Deallocating VMs");
                //we need to downscale
                var instanceIdCollection = Decisions.CollectInstanceIdsToDeallocate(virtualMachines, currentJobs);

                foreach (var instanceId in instanceIdCollection)
                {
                    Console.WriteLine($"Deallocating VM with instance ID {instanceId}");
                    if (!isDryRun)
                    {
                        vmss.VirtualMachines.Inner.BeginDeallocateWithHttpMessagesAsync(resourceGroupName, vmssName,
                            instanceId);
                    }
                }
            }
            else
            {
                var virtualMachinesCounter = 0;
                Console.WriteLine("Starting more VMs");
                foreach (var scaleSetVirtualMachineStripped in virtualMachines.Where(vm => vm.VmInstanceState.Equals(PowerState.Deallocated)))
                {
                    if (virtualMachinesCounter >= amountOfAgents)
                    {
                        break;
                    }
                    //TODO: Record starting VM
                    Console.WriteLine($"Starting VM {scaleSetVirtualMachineStripped.VmName} with id {scaleSetVirtualMachineStripped.VmInstanceId}");
                    if (!isDryRun)
                    {
                        vmss.VirtualMachines.Inner.BeginStartWithHttpMessagesAsync(resourceGroupName, vmssName,
                            scaleSetVirtualMachineStripped.VmInstanceId);
                    }
                    virtualMachinesCounter++;
                }
            }
            Console.WriteLine("Finished execution");
        }

        private static AzureCredentials AzureCreds()
        {
            var clientId = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleClientIdSettingName];
            var clientSecret = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleClientSecretSettingName];
            var tenantId = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleTenantIdSettingName];
            //maybe in future I'll need to extend this one to allow other then Global Azure environment
            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
        }

        private static async Task RecordDataInTable(string rgName, string vmScaleSetName, bool isProvisioning)
        {
            var storageConnectionString = ConfigurationManager.AppSettings[Constants.AzureStorageConnectionStringName];

            if (string.IsNullOrWhiteSpace(storageConnectionString))
            {
                Console.WriteLine("Connection string is not defined for Azure Storage");
                //connection string for Azure Storage is not defined
                return;
            }

            if (Properties.ActionsTrackingOperations == null)
            {
                Console.WriteLine($"Could not connect to Azure Storage Table {Properties.StorageTableName}");
                return;
            }

            var entity = new ScaleEventEntity(rgName, vmScaleSetName) {IsProvisioningEvent = isProvisioning};

            await Properties.ActionsTrackingOperations.InsertOrReplaceEntityAsync(entity);
        }
    }
}