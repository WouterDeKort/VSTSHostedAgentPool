using System;
using AzureDevOps.Operations.Classes;
using Microsoft.Azure.WebJobs;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;

namespace AutoScaler
{
    class Program
    {
        static void Main()
        {
            //little bit of security
            //enabling TLS 1.2
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            //ban using extremely insecure SSL v3
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;

            //check all required settings
            DefinitionChecker.CheckAllSettings();

            var config = new JobHostConfiguration();
            config.UseTimers();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);
            host.RunAndBlock();
        }

        [Singleton]
        public static void QueueChecker([TimerTrigger("0 */5 * * * *")] TimerInfo timerInfo, TextWriter log) 
        {
            var poolIdSetting = ConfigurationManager.AppSettings[Constants.AgentsPoolIdSettingName];

            var organizationName = ConfigurationManager.AppSettings[Constants.AzureDevOpsInstanceSettingName];
            var accessToken = ConfigurationManager.AppSettings[Constants.AzureDevOpsPatSettingName];
            var httpClient = new HttpClient();

            var dataRetriever = new Retrieve(organizationName, accessToken, httpClient);
            int poolId;
            //if poolId is not defined in settings - we need to retrieve it
            if (string.IsNullOrWhiteSpace(poolIdSetting))
            {
                var agentsPoolName = ConfigurationManager.AppSettings[Constants.AgentsPoolNameSettingName];
                var poolIdNullable = dataRetriever.GetPoolId(agentsPoolName);
                if (poolIdNullable == null)
                {
                    //something went wrong 
                    Console.WriteLine($"Could not retrieve pool id for {agentsPoolName}, have to exit");
                    Environment.Exit(Constants.ErrorExitCode);
                }
                poolId = poolIdNullable.Value;
            }
            else
            {
                //poolId is defined in settings, we need to parse it now
                if (!int.TryParse(poolIdSetting, out poolId))
                {
                    Console.WriteLine($"Could not parse pool id from appSetting {Constants.AgentsPoolIdSettingName}. Exiting...");
                    Environment.Exit(Constants.ErrorExitCode);
                }
            }

            var maxAgentsCount = dataRetriever.GetAllAccessibleAgents(poolId);

            if (maxAgentsCount == 0)
            {
                Console.WriteLine($"There is 0 agents assigned to pool with id {poolId}. Could not proceed, exiting...");
                Environment.Exit(Constants.ErrorExitCode);
            }

            var onlineAgentsCount = 0;
            var countNullable = dataRetriever.GetOnlineAgentsCount(poolId);
            if (countNullable == null)
            {
                //something went wrong
                Console.WriteLine("Could not retrieve amount of agents online, exiting...");
                Environment.Exit(Constants.ErrorExitCode);
            }
            else
            {
                onlineAgentsCount = (int) countNullable.Value;
            }

            var waitingJobsCount = dataRetriever.GetCurrentJobsRunningCount(poolId);

            if (waitingJobsCount == onlineAgentsCount)
            {
                //nothing to do here
                return;
            }

            Operations.WorkWithVmss(onlineAgentsCount, maxAgentsCount, dataRetriever, poolId);
        }
    }
}
