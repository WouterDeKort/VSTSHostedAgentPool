using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace AutoScaler
{
    class Program
    {
        private const string AgentsPoolNameSettingName = "AgentsPoolName";
        private const string AgentsPoolIdSettingName = "AgentsPoolId";

        static void Main()
        {
            if (string.IsNullOrWhiteSpace(
                    System.Configuration.ConfigurationManager.AppSettings[AgentsPoolNameSettingName]) ||
                string.IsNullOrWhiteSpace(
                    System.Configuration.ConfigurationManager.AppSettings[AgentsPoolIdSettingName]))
            {
                //log error and exit with non success exit code
                Environment.Exit(-1);
            }

            var config = new JobHostConfiguration();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);
            host.Call(typeof(Program).GetMethod("CheckQueue"));
        }

        [NoAutomaticTrigger]
        public static void CheckQueue()
        {

        }
    }
}
