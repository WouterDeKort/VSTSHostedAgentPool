using AzureDevOps.Operations.Classes;
using System;
using System.Configuration;

namespace AutoScaler
{
    public static class DefinitionChecker
    {
        public static void CheckAllSettings()
        {
            if (string.IsNullOrWhiteSpace(
                    ConfigurationManager.AppSettings[Constants.AgentsPoolNameSettingName]) ||
                string.IsNullOrWhiteSpace(
                    ConfigurationManager.AppSettings[Constants.AgentsPoolIdSettingName]))
            {
                Console.WriteLine($"In AppSettings neither {Constants.AgentsPoolIdSettingName}, nor {Constants.AgentsPoolNameSettingName} is defined. Exiting...");
                //log error and exit with non success exit code
                Environment.Exit(Constants.ErrorExitCode);
            }

            ExitIfFail(ConfigurationManager.AppSettings[Constants.AzureDevOpsInstanceSettingName], "Azure DevOps instance name");
            ExitIfFail(ConfigurationManager.AppSettings[Constants.AzureDevOpsPatSettingName], "Azure DevOps PAT");
        }

        private static void ExitIfFail(string settingName, string errorMessage = "Setting is not defined")
        {
            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[settingName]))
            {
                return;
            }
            Console.WriteLine($"{errorMessage} in {settingName}. Exiting...");
            Environment.Exit(Constants.ErrorExitCode);
        }
    }
}