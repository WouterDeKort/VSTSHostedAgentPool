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
            //working with VMSS
            var vmss = GetVirtualMachinesScaleSet(Properties.VmScaleSetResourceGroupName, Properties.VmScaleSetName);
            var virtualMachines = vmss.VirtualMachines.List()
                //there could be failed VMs during provisioning
                .Where(vm => !vm.Inner.ProvisioningState.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                .Select(vmssVm => new ScaleSetVirtualMachineStripped
                {
                    VmInstanceId = vmssVm.InstanceId,
                    VmName = vmssVm.ComputerName,
                    VmInstanceState = vmssVm.PowerState
                }).ToArray();

            //get jobs again to check, if we could deallocate a VM in VMSS
            //(if it is running a job - it is not wise to deallocate it)
            //since getting VMMS is potentially lengthy operation - we could need this)
            var currentJobs = Checker.DataRetriever.GetRuningJobs(Properties.AgentsPoolId);
            var amountOfAgents = Decisions.HowMuchAgents(currentJobs.Length, onlineAgents, maxAgentsInPool);
            var addMoreAgents = amountOfAgents > 0;

            if (amountOfAgents == 0)
            {
                //nevertheless - should we (de)provision agents: we are at boundaries
                Console.WriteLine("Should not add/remove more agents...");
                return;
            }

            //further I need to work with positive numbers only
            amountOfAgents = Math.Abs(amountOfAgents);

            if (addMoreAgents != areWeCheckingToStartVmInVmss)
            {
                //target event is not the same as source one
                return;
            }

            //I wish this record to be processed on it's own; it is just tracking
            RecordDataInTable(addMoreAgents, amountOfAgents);

            if (addMoreAgents)
            {
                AllocateVms(virtualMachines, amountOfAgents, vmss);
            }
            else
            {
                DeallocationWorkWithScaleSet(virtualMachines, currentJobs, vmss, amountOfAgents);
            }           
        }

        private static AzureCredentials AzureCreds()
        {
            var clientId = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleClientIdSettingName];
            var clientSecret = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleClientSecretSettingName];
            var tenantId = ConfigurationManager.AppSettings[Constants.AzureServicePrincipleTenantIdSettingName];
            //maybe in future I'll need to extend this one to allow other then Global Azure environment
            return SdkContext.AzureCredentialsFactory.FromServicePrincipal(clientId, clientSecret, tenantId, AzureEnvironment.AzureGlobalCloud);
        }

        private static IVirtualMachineScaleSet GetVirtualMachinesScaleSet(string rgName,
            string virtualMachinesScaleSetName)
        {
            var credentials = AzureCreds();

            var azure = Azure
                .Configure()
                .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                .Authenticate(credentials)
                .WithSubscription(ConfigurationManager.AppSettings[Constants.AzureSubscriptionIdSettingName]);
            var virtualMachineScaleSet = azure.VirtualMachineScaleSets.GetByResourceGroup(rgName, virtualMachinesScaleSetName);
            if (virtualMachineScaleSet != null)
            {
                return virtualMachineScaleSet;
            }
            Console.WriteLine($"Could not retrieve Virtual Machines Scale Set with name {virtualMachinesScaleSetName} in resource group {rgName}. Exiting...");
            LeaveTheBuilding.Exit(Checker.DataRetriever);

            return null;
        }

        /// <summary>
        /// This method will perform all changes to scale set
        /// </summary>
        private static void DeallocationWorkWithScaleSet(
            ScaleSetVirtualMachineStripped[] virtualMachinesStripped,
            JobRequest[] executingJobs,
            IVirtualMachineScaleSet scaleSet, int agentsLimit)
        {
            Console.WriteLine("Deallocating VMs");
            //we need to downscale, only running VMs shall be selected here
            var instanceIdCollection = Decisions.CollectInstanceIdsToDeallocate(virtualMachinesStripped.Where(vm => vm.VmInstanceState.Equals(PowerState.Running)), executingJobs);
            DeallocateVms(instanceIdCollection, scaleSet, agentsLimit);

            //if we are deprovisioning - it is some time to do some housekeeping as well
            if (Properties.IsDryRun)
            {
                return;
            }

            HouseKeeping(scaleSet);
        }

        private static void DeallocateVms(IEnumerable<string> instanceIdCollection, IVirtualMachineScaleSet scaleSet, int agentsCountToDeallocate)
        {
            var virtualMachinesCounter = 0;
            foreach (var instanceId in instanceIdCollection)
            {
                if (virtualMachinesCounter >= agentsCountToDeallocate)
                {
                    break;
                }
                Console.WriteLine($"Deallocating VM with instance ID {instanceId}");
                if (!Properties.IsDryRun)
                {
                    scaleSet.VirtualMachines.Inner.BeginDeallocateWithHttpMessagesAsync(Properties.VmScaleSetResourceGroupName, Properties.VmScaleSetName,
                        instanceId);
                }
                virtualMachinesCounter++;
            }
        }

        /// <summary>
        /// If there is a failed VM in VMSS - we can reimage them
        /// </summary>
        /// <param name="scaleSet"></param>
        private static async void HouseKeeping(IVirtualMachineScaleSet scaleSet)
        {
            var failedVms = scaleSet.VirtualMachines.List().Where(vm =>
                vm.Inner.ProvisioningState.Equals("Failed", StringComparison.OrdinalIgnoreCase)).ToArray();

            if (!failedVms.Any())
            {
                return;
            }
            Console.WriteLine("We have some failed VMs and will try to reimage them async");
            foreach (var virtualMachineScaleSetVm in failedVms)
            {
                await virtualMachineScaleSetVm.ReimageAsync();
            }
        }

        private static void AllocateVms(ScaleSetVirtualMachineStripped[] virtualMachinesStripped, int agentsLimit, IVirtualMachineScaleSet scaleSet)
        {
            var virtualMachinesCounter = 0;
            try
            {
                virtualMachinesCounter = virtualMachinesStripped.Count(vm => vm.VmInstanceState.Equals(PowerState.Starting));
            }
            catch
            {
                Console.WriteLine("Could not get VMs in starting state");
            }

            Console.WriteLine("Starting more VMs");
            foreach (var virtualMachineStripped in virtualMachinesStripped.Where(vm => vm.VmInstanceState.Equals(PowerState.Deallocated)))
            {
                if (virtualMachinesCounter >= agentsLimit)
                {
                    break;
                }
                Console.WriteLine($"Starting VM {virtualMachineStripped.VmName} with id {virtualMachineStripped.VmInstanceId}");
                if (!Properties.IsDryRun)
                {
                    scaleSet.VirtualMachines.Inner.BeginStartWithHttpMessagesAsync(Properties.VmScaleSetResourceGroupName, Properties.VmScaleSetName,
                        virtualMachineStripped.VmInstanceId);
                }
                virtualMachinesCounter++;
            }
        }

        private static async void RecordDataInTable(bool isProvisioning, int agentsCount)
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

            var entity = new ScaleEventEntity(Properties.VmScaleSetName) { IsProvisioningEvent = isProvisioning, AmountOfVms = agentsCount };

            await Properties.ActionsTrackingOperations.InsertOrReplaceEntityAsync(entity);
        }
    }
}