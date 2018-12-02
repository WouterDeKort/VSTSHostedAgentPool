using Microsoft.Azure.Management.Compute.Fluent;

namespace AzureDevOps.Operations.Models
{
    /// <summary>
    /// This class holds only required by this projects properties of Virtual Machine from Virtual Machines Scale Set <see cref="Microsoft.Azure.Management.Compute.Fluent.IVirtualMachineScaleSets"/>
    /// </summary>
    public class ScaleSetVirtualMachineStripped
    {
        /// <summary>
        /// Virtual Machine name
        /// </summary>
        public string VmName { get; set; }
        /// <summary>
        /// Virtual machine Instance Id
        /// </summary>
        public string VmInstanceId { get; set; }

        /// <summary>
        /// Holds marker if Instance is deallocated or not
        /// </summary>
        public PowerState VmInstanceState { get; set; }
    }
}