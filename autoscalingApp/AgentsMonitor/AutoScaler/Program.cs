using AzureDevOps.Operations.Helpers;
using Microsoft.Azure.WebJobs;
using System.Net;

namespace AutoScaler
{
    internal static class Program
    {
        private static void Main()
        {
            //little bit of security
            //enabling TLS 1.2
            ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            //ban using extremely insecure SSL v3
            ServicePointManager.SecurityProtocol &= ~SecurityProtocolType.Ssl3;
            //added limits to connection amounts
            ServicePointManager.DefaultConnectionLimit = 50;

            //check all required settings
            SettingsChecker.CheckAllSettings();

            var config = new JobHostConfiguration();
            config.UseTimers();

            if (config.IsDevelopment)
            {
                config.UseDevelopmentSettings();
            }

            var host = new JobHost(config);
            host.RunAndBlock();
        }
    }
}
