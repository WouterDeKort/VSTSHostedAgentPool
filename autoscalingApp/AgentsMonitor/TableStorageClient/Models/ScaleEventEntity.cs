using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace TableStorageClient.Models
{
    public class ScaleEventEntity : TableEntity
    {
        /// <summary>
        /// we  need empty parameterless constructor here
        /// </summary>
        public ScaleEventEntity()
        {
        }

        public ScaleEventEntity(string virtualMachinesScaleSetResourceGroupName, string virtualMachinesScaleSetName)
        {
            PartitionKey = virtualMachinesScaleSetResourceGroupName;
            RowKey = virtualMachinesScaleSetName;
        }

        /// <summary>
        /// Records, if we are starting more VMs at VMSS or deprovisining existing
        /// </summary>
        public bool IsProvisioningEvent { get; set; }
    }
}