namespace AzureDevOps.Operations.Classes
{
    public class Constants
    {
        /// <summary>
        /// Defines error exit code
        /// </summary>
        public const int ErrorExitCode = -1;
        public const int SuccessExitCode = 0;
        public const string AzureDevOpsApiVersion = "4.1";
        /// <summary>
        /// Agents Pool Name
        /// </summary>
        public const string AgentsPoolNameSettingName = "Agents.PoolName";
        /// <summary>
        /// Agents Pool ID 
        /// </summary>
        public const string AgentsPoolIdSettingName = "Agents.PoolId";
        /// <summary>
        /// Azure DevOps instance to authenticate against
        /// </summary>
        public const string AzureDevOpsInstanceSettingName = "Azure.DevOpsInstance";
        /// <summary>
        /// Public Access Token for Azure DevOps instance
        /// </summary>
        public const string AzureDevOpsPatSettingName = "Azure.DevOpsPAT";
        //azure service principle
        /// <summary>
        /// Client ID of Azure Service Principle
        /// </summary>
        public const string AzureServicePrincipleClientIdSettingName = "Azure.ServicePrincipleClientId";
        /// <summary>
        /// Client Secret of Azure Service Principle
        /// </summary>
        public const string AzureServicePrincipleClientSecretSettingName = "Azure.ServicePrincipleClientSecret";
        /// <summary>
        /// Tenant ID for Azure Service Principle
        /// </summary>
        public const string AzureServicePrincipleTenantIdSettingName = "Azure.ServicePrincipleTenantId";
        //vmss data
        /// <summary>
        /// Defines Azure subscription ID where VMSS resides
        /// </summary>
        public const string AzureSubscriptionIdSettingName = "Azure.SubscriptionId";
        /// <summary>
        /// Defines resource group name in which VMSS with agents resides
        /// </summary>
        public const string AzureVmssResourceGroupSettingName = "Azure.VMSS.resourceGroupName";
        /// <summary>
        /// Defines VMSS name
        /// </summary>
        public const string AzureVmssNameSettingName = "Azure.VMSS.Name";
        /// <summary>
        /// Defines if we are executing test run (so, no actual changes will be done to VMSS agents
        /// </summary>
        public const string DryRunSettingName = "DryRunExecution";
    }
}