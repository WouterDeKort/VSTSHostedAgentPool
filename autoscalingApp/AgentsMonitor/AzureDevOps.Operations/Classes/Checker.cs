using System;
using System.Configuration;
using System.Net.Http;
using AzureDevOps.Operations.Helpers;

namespace AzureDevOps.Operations.Classes
{
    /// <summary>
    /// Class to hold all checks logic
    /// </summary>
    public static class Checker
    {
        private static Retrieve _dataRetrieveCache;

        internal static Retrieve DataRetriever
        {
            get
            {
                if (_dataRetrieveCache != null)
                {
                    return _dataRetrieveCache;
                }
                var organizationName = ConfigurationManager.AppSettings[Constants.AzureDevOpsInstanceSettingName];
                var accessToken = ConfigurationManager.AppSettings[Constants.AzureDevOpsPatSettingName];
                var httpClient = new HttpClient();

                _dataRetrieveCache = new Retrieve(organizationName, accessToken, httpClient);
                return _dataRetrieveCache;
            }
        }

        public static void AgentsQueue()
        {
            var poolIdSetting = ConfigurationManager.AppSettings[Constants.AgentsPoolIdSettingName];

            int poolId;
            //if poolId is not defined in settings - we need to retrieve it
            if (string.IsNullOrWhiteSpace(poolIdSetting))
            {
                var agentsPoolName = ConfigurationManager.AppSettings[Constants.AgentsPoolNameSettingName];
                var poolIdNullable = DataRetriever.GetPoolId(agentsPoolName);
                if (poolIdNullable == null)
                {
                    //something went wrong 
                    Console.WriteLine($"Could not retrieve pool id for {agentsPoolName}, have to exit");
                    LeaveTheBuilding.Exit(DataRetriever);
                }
                poolId = poolIdNullable.Value;
            }
            else
            {
                //poolId is defined in settings, we need to parse it now
                if (!int.TryParse(poolIdSetting, out poolId))
                {
                    Console.WriteLine($"Could not parse pool id from appSetting {Constants.AgentsPoolIdSettingName}. Exiting...");
                    LeaveTheBuilding.Exit(DataRetriever);
                }
            }

            var maxAgentsCount = DataRetriever.GetAllAccessibleAgents(poolId);

            if (maxAgentsCount == 0)
            {
                Console.WriteLine($"There is 0 agents assigned to pool with id {poolId}. Could not proceed, exiting...");
                LeaveTheBuilding.Exit(DataRetriever);
            }

            var onlineAgentsCount = 0;
            var countNullable = DataRetriever.GetOnlineAgentsCount(poolId);
            if (countNullable == null)
            {
                //something went wrong
                Console.WriteLine("Could not retrieve amount of agents online, exiting...");
                LeaveTheBuilding.Exit(DataRetriever);
            }
            else
            {
                onlineAgentsCount = countNullable.Value;
            }

            var waitingJobsCount = DataRetriever.GetCurrentJobsRunningCount(poolId);

            if (waitingJobsCount == onlineAgentsCount)
            {
                //nothing to do here
                return;
            }

            Operations.WorkWithVmss(onlineAgentsCount, maxAgentsCount, poolId);
        }
    }
}