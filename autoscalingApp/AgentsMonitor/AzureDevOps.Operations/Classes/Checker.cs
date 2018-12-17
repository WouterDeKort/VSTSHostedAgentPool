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
        /// <summary>
        /// Checks agent queue and decides, if we need to provision or not
        /// </summary>
        /// <param name="areWeCheckingToStartVm">describes function which called us</param>
        public static void AgentsQueue(bool areWeCheckingToStartVm)
        {
            var maxAgentsCount = DataRetriever.GetAllAccessibleAgents(Properties.AgentsPoolId);

            if (maxAgentsCount == 0)
            {
                Console.WriteLine($"There is 0 agents assigned to pool with id {Properties.AgentsPoolId}. Could not proceed, exiting...");
                LeaveTheBuilding.Exit(DataRetriever);
            }

            var onlineAgentsCount = 0;
            var countNullable = DataRetriever.GetOnlineAgentsCount(Properties.AgentsPoolId);
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

            var waitingJobsCount = DataRetriever.GetCurrentJobsRunningCount(Properties.AgentsPoolId);

            if (waitingJobsCount == onlineAgentsCount && !new DynamicProps().WeAreInsideBusinessTime)
            {
                //nothing to do here
                return;
            }

            Operations.WorkWithVmss(onlineAgentsCount, maxAgentsCount, areWeCheckingToStartVm);
        }
    }
}