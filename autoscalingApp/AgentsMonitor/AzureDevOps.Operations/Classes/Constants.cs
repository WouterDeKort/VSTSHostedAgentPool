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
        public const string AgentsPoolNameSettingName = "Agents_PoolName";
        /// <summary>
        /// Agents Pool ID 
        /// </summary>
        public const string AgentsPoolIdSettingName = "Agents_PoolId";
        /// <summary>
        /// Azure DevOps instance to authenticate against
        /// </summary>
        public const string AzureDevOpsInstanceSettingName = "Azure_DevOpsInstance";
        /// <summary>
        /// Public Access Token for Azure DevOps instance
        /// </summary>
        public const string AzureDevOpsPatSettingName = "Azure_DevOpsPAT";
        //azure service principle
        /// <summary>
        /// Client ID of Azure Service Principle
        /// </summary>
        public const string AzureServicePrincipleClientIdSettingName = "Azure_ServicePrincipleClientId";
        /// <summary>
        /// Client Secret of Azure Service Principle
        /// </summary>
        public const string AzureServicePrincipleClientSecretSettingName = "Azure_ServicePrincipleClientSecret";
        /// <summary>
        /// Tenant ID for Azure Service Principle
        /// </summary>
        public const string AzureServicePrincipleTenantIdSettingName = "Azure_ServicePrincipleTenantId";
        //vmss data
        /// <summary>
        /// Defines Azure subscription ID where VMSS resides
        /// </summary>
        public const string AzureSubscriptionIdSettingName = "Azure_SubscriptionId";
        /// <summary>
        /// Defines resource group name in which VMSS with agents resides
        /// </summary>
        public const string AzureVmssResourceGroupSettingName = "Azure_VMSS_resourceGroupName";
        /// <summary>
        /// Defines VMSS name
        /// </summary>
        public const string AzureVmssNameSettingName = "Azure_VMSS_Name";
        /// <summary>
        /// Defines if we are executing test run (so, no actual changes will be done to VMSS agents
        /// </summary>
        public const string DryRunSettingName = "DryRunExecution";
        /// <summary>
        /// Holds connection string for Azure Storage for logging of (de)provisioning
        /// </summary>
        public const string AzureStorageConnectionStringName = "Azure_Storage_ConnectionString";
        /// <summary>
        /// Holds pointer to Azure Storage Table for tracking actions
        /// </summary>
        public const string AzureStorageTrackingTableSettingName = "Azure_Storage_ActionsTracking_TableName";
        /// <summary>
        /// if tracking table name is not set in appSettings - it will default to this
        /// </summary>
        public const string AzureStorageDefaultTrackingTableName = "DefaultTrackingTable";
    }
}