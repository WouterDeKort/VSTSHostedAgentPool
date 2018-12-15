using System;
using System.Configuration;
using AzureDevOps.Operations.Classes;

namespace AzureDevOps.Operations.Tests.Classes
{
    /// <summary>
    /// Collection of test initializers
    /// </summary>
    public static class TestInitilizers
    {
        /// <summary>
        /// Sets business times to be Monday to Friday from 10 o'clock till 17 o'clock
        /// </summary>
        public static void InitAppSettingsForBusinessTimesTests()
        {
            ConfigurationManager.AppSettings[Constants.BusinessHoursRangeSettingName] = "10-17";
            ConfigurationManager.AppSettings[Constants.BusinessHoursDaysSettingName] = "Monday-Friday";
            ConfigurationManager.AppSettings[Constants.BusinessHoursAgentsAmountSettingName] = "3";
        }

        /// <summary>
        /// Parses string like 14-Dec-2018 15:15 to date time
        /// </summary>
        /// <param name="dateTimeAsString"></param>
        /// <returns></returns>
        public static DateTime ParseDateTimeForTest(string dateTimeAsString)
        {
            return DateTime.Parse(dateTimeAsString);
        }
    }
}