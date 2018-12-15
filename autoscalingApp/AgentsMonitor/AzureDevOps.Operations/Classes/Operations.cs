using AzureDevOps.Operations.Helpers;
using AzureDevOps.Operations.Models;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
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
        /// <param name="areWeCheckingToStartVmInVmss">Describes, which functions calls out - provisioning or deprovisioning</param>
        public static void WorkWithVmss(int onlineAgents, int maxAgentsInPool, bool areWeCheckingToStartVmInVmss)
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
            if (vmss == null)
            {
                Console.WriteLine($"Could not retrieve Virtual Machines Scale Set with name {vmssName} in resource group {resourceGroupName}. Exiting...");
                LeaveTheBuilding.Exit(Checker.DataRetriever);
                //return statement does not have any sense here, but it save
                return;
            }
            var virtualMachines = vmss.VirtualMachines.List()
                //there could be failed VMs during provisioning
                .Where(vm => !vm.Inner.ProvisioningState.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                .Select(vmssVm => new ScaleSetVirtualMachineStripped
                {
                    VmInstanceId = vmssVm.InstanceId,
                    VmName = vmssVm.ComputerName,
                    VmInstanceState = vmssVm.PowerState
                }).ToList();

            //get jobs again to check, if we could deallocate a VM in VMSS
            //(if it is running a job - it is not wise to deallocate it)
            //since getting VMMS is potentially lengthy operation - we could need this)
            var currentJobs = Checker.DataRetriever.GetRuningJobs(Properties.AgentsPoolId);
            var addMoreAgents = Decisions.AddMoreAgents(currentJobs.Length, onlineAgents);
            var amountOfAgents = Decisions.HowMuchAgents(currentJobs.Length, onlineAgents, maxAgentsInPool);

            if (amountOfAgents == 0)
            {
                //nevertheless - should we (de)provision agents: we are at boundaries
                Console.WriteLine("Should not add/remove more agents...");
                return;
            }

            if (addMoreAgents != areWeCheckingToStartVmInVmss)
            {
                //target event is not the same as source one
                return;
            }

            //I wish this record to be processed on it's own; it is just tracking
            RecordDataInTable(vmssName, addMoreAgents, amountOfAgents);

            if (!addMoreAgents)
            {
                Console.WriteLine("Deallocating VMs");
                //we need to downscale, only running VMs shall be selected here
                var instanceIdCollection = Decisions.CollectInstanceIdsToDeallocate(virtualMachines.Where(vm => vm.VmInstanceState.Equals(PowerState.Running)), currentJobs);

                foreach (var instanceId in instanceIdCollection)
                {
                    Console.WriteLine($"Deallocating VM with instance ID {instanceId}");
                    if (!Properties.IsDryRun)
                    {
                        vmss.VirtualMachines.Inner.BeginDeallocateWithHttpMessagesAsync(resourceGroupName, vmssName,
                            instanceId);
                    }
                }
                //if we are deprovisioning - it is some time to do some housekeeping as well
                if (!Properties.IsDryRun)
                {
                    var failedVms = vmss.VirtualMachines.List().Where(vm =>
                        vm.Inner.ProvisioningState.Equals("Failed", StringComparison.OrdinalIgnoreCase)).ToArray();

                    if (failedVms.Any())
                    {
                        Console.WriteLine("We have some failed VMs and will try to reimage them async");
                        ReimageFailedVm(failedVms);
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
                    Console.WriteLine($"Starting VM {scaleSetVirtualMachineStripped.VmName} with id {scaleSetVirtualMachineStripped.VmInstanceId}");
                    if (!Properties.IsDryRun)
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

        private static async void RecordDataInTable(string vmScaleSetName, bool isProvisioning, int agentsCount)
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

            var entity = new ScaleEventEntity(vmScaleSetName) { IsProvisioningEvent = isProvisioning, AmountOfVms = agentsCount };

            await Properties.ActionsTrackingOperations.InsertOrReplaceEntityAsync(entity);
        }

        /// <summary>
        /// If there is a failed VM in VMSS - we can reimage them
        /// </summary>
        /// <param name="failedVirtualMachines"></param>
        /// <returns></returns>
        private static async void ReimageFailedVm(IEnumerable<IVirtualMachineScaleSetVM> failedVirtualMachines)
        {
            foreach (var virtualMachineScaleSetVm in failedVirtualMachines)
            {
                await virtualMachineScaleSetVm.ReimageAsync();
            }
        }
    }
}