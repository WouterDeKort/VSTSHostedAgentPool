using AzureDevOps.Operations.Classes;
using System;
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

        private static TableOperations<ScaleEventEntity> _actionsTrackingOperations;

        public static TableOperations<ScaleEventEntity> ActionsTrackingOperations
        {
            get
            {
                if (string.IsNullOrWhiteSpace(StorageConnectionString))
                {
                    //could not connect to Azure Storage, as there is no connection string defined
                    return null;
                }

                if (_actionsTrackingOperations != null)
                {
                    return _actionsTrackingOperations;
                }

                _actionsTrackingOperations = new TableOperations<ScaleEventEntity>(StorageTableName, StorageConnectionString);
                return _actionsTrackingOperations;
            }
        }

        private static int _agentsPoolId;
        /// <summary>
        /// Stores in backing field agent pool id to minimize calls to Azure DevOps API
        /// </summary>
        internal static int AgentsPoolId
        {
            get
            {
                if (_agentsPoolId != 0)
                {
                    //we have correct value in backing field (this code assumes that it is not possible to have pool ID 0)
                    return _agentsPoolId;
                }

                _agentsPoolId = GetTypedSetting.GetSetting<int>(Constants.AgentsPoolIdSettingName);

                //if poolId is not defined in settings - we need to retrieve it
                if (_agentsPoolId != 0)
                {
                    return _agentsPoolId;
                }
                var agentsPoolName = ConfigurationManager.AppSettings[Constants.AgentsPoolNameSettingName];
                var poolIdNullable = Checker.DataRetriever.GetPoolId(agentsPoolName);
                if (poolIdNullable == null)
                {
                    //something went wrong 
                    Console.WriteLine($"Could not retrieve pool id for {agentsPoolName}, have to exit");
                    LeaveTheBuilding.Exit(Checker.DataRetriever);
                    //does not makes a sense here, as we are exiting - but it makes compiler happy :)
                    return 0;
                }
                _agentsPoolId = poolIdNullable.Value;

                return _agentsPoolId;
            }
        }

        /// <summary>
        /// Checks, if business runtime settings are defined 
        /// </summary>
        internal static bool BusinessRuntimeDefined => !string.IsNullOrWhiteSpace(
                                                          ConfigurationManager.AppSettings[Constants.BusinessHoursRangeSettingName])
                                                      && !string.IsNullOrWhiteSpace(
                                                          ConfigurationManager.AppSettings[Constants.BusinessHoursDaysSettingName])
                                                      && !string.IsNullOrWhiteSpace(
                                                          ConfigurationManager.AppSettings[Constants.BusinessHoursAgentsAmountSettingName]);
        /// <summary>
        /// Gets starting day for business days
        /// </summary>
        public static DayOfWeek BusinessDaysStartingDay
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[Constants.BusinessHoursDaysSettingName];
                if (!setting.Contains("-"))
                {
                    return DayOfWeek.Monday;
                }
                var startingDayAsString = setting.Split('-')[0];
                var possibleDay = DayParser(startingDayAsString);

                return possibleDay ?? DayOfWeek.Monday;
            }
        }
        /// <summary>
        /// Gets ending day for business days
        /// </summary>
        public static DayOfWeek BusinessDaysLastDay
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[Constants.BusinessHoursDaysSettingName];
                if (!setting.Contains("-"))
                {
                    return DayOfWeek.Friday;
                }
                var endingDayAsString = setting.Split('-')[1];
                var possibleDay = DayParser(endingDayAsString);

                return possibleDay ?? DayOfWeek.Friday;
            }
        }

        /// <summary>
        /// Gets starting hour of a business day
        /// </summary>
        public static int BussinesDayStartHour
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[Constants.BusinessHoursRangeSettingName];
                if (!setting.Contains("-"))
                {
                    return 0;
                }

                var hourAsString = setting.Split('-')[0];

                return int.TryParse(hourAsString, out var returnValue) ? returnValue : 0;
            }
        }

        /// <summary>
        /// Gets last hour of a business day
        /// </summary>
        public static int BussinesDayEndHour
        {
            get
            {
                var setting = ConfigurationManager.AppSettings[Constants.BusinessHoursRangeSettingName];
                if (!setting.Contains("-"))
                {
                    return 0;
                }

                var hourAsString = setting.Split('-')[1];

                return int.TryParse(hourAsString, out var returnValue) ? returnValue : 0;
            }
        }
        /// <summary>
        /// Parses amount of agents
        /// </summary>
        public static int AmountOfAgents => GetTypedSetting.GetSetting<int>(Constants.BusinessHoursAgentsAmountSettingName);

        private static DayOfWeek? DayParser(string day)
        {
            if (Enum.TryParse(day, true, out DayOfWeek returnValue))
            {
                return returnValue;
            }
            else
            {
                return null;
            }
        }
    }
}