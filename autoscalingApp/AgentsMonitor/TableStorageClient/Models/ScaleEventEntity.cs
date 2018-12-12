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

        public ScaleEventEntity(string virtualMachinesScaleSetName)
        {
            PartitionKey = virtualMachinesScaleSetName;
            //for now will set row key to emptry string
            RowKey = DateTime.UtcNow.ToString("dd-MM-yyyy|HH:mm:ss");
        }

        /// <summary>
        /// Records, if we are starting more VMs at VMSS or deprovisining existing
        /// </summary>
        public bool IsProvisioningEvent { get; set; }
        /// <summary>
        /// Records how much VMs we are (de)provisioning in given Virtual Machines Scale Set
        /// </summary>
        public int AmountOfVms { get; set; }
    }
}