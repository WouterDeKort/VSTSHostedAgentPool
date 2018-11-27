using AzureDevOps.Operations.Classes;
using System;
using System.Configuration;

namespace AutoScaler
{
    public static class DefinitionChecker
    {
        /// <summary>
        /// Checks that all required settings are defined; if check fails - job will exit
        /// </summary>
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

            ExitIfSettingEmpty(ConfigurationManager.AppSettings[Constants.AzureDevOpsInstanceSettingName], "Azure DevOps instance name");
            ExitIfSettingEmpty(ConfigurationManager.AppSettings[Constants.AzureDevOpsPatSettingName], "Azure DevOps PAT");
        }

        /// <summary>
        /// Checks setting, that it is not empty
        /// </summary>
        /// <param name="settingName"></param>
        /// <param name="errorMessage"></param>
        private static void ExitIfSettingEmpty(string settingName, string errorMessage = "Setting")
        {
            if (!string.IsNullOrWhiteSpace(ConfigurationManager.AppSettings[settingName]))
            {
                return;
            }
            Console.WriteLine($"{errorMessage} is not defined in {settingName}. Exiting...");
            Environment.Exit(Constants.ErrorExitCode);
        }
    }
}