using AzureDevOps.Operations.Classes;
using Microsoft.WindowsAzure.Storage.Table;
using System.Configuration;
using TableStorageClient.Classes;
using TableStorageClient.Models;

namespace AzureDevOps.Operations.Helpers
{
    public static class Properties
    {
        internal static string StorageTableName
        {
            get
            {
                var tableName = string.IsNullOrWhiteSpace(
                    ConfigurationManager.AppSettings[Constants.AzureStorageTrackingTableSettingName]) 
                    ? Constants.AzureStorageDefaultTrackingTableName 
                    : ConfigurationManager.AppSettings[Constants.AzureStorageTrackingTableSettingName];

                if (IsDryRun)
                {
                    //appending DryRun to table name, as dry run data could not be used to train any ML models
                    tableName = string.Concat(tableName, "DryRun");
                }
                //removing dashes (if user set them for table name)
                tableName = tableName.Replace("-", string.Empty);

                return tableName;
            }
        }

        internal static bool IsDryRun => GetTypedSetting.GetSetting<bool>(Constants.DryRunSettingName);

        private static string StorageConnectionString =>
            ConfigurationManager.AppSettings[Constants.AzureStorageConnectionStringName];

        private static TableOperations<ScaleEventEntity> actionsTrackingOperations;

        public static TableOperations<ScaleEventEntity> ActionsTrackingOperations
        {
            get
            {
                if (string.IsNullOrWhiteSpace(StorageConnectionString))
                {
                    //could not connect to Azure Storage, as there is no connection string defined
                    return null;
                }

                if (actionsTrackingOperations != null)
                {
                    return actionsTrackingOperations;
                }

                actionsTrackingOperations = new TableOperations<ScaleEventEntity>(StorageTableName, StorageConnectionString);
                return actionsTrackingOperations;
            }
        }



    }
}