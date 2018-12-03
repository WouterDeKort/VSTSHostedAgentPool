using System;
using System.Configuration;

namespace AzureDevOps.Operations.Helpers
{
    public class GetTypedSetting
    {
        /// <summary>
        /// Got this example from https://dejanstojanovic.net/aspnet/2015/may/reading-config-value-to-a-proper-data-type/
        /// Returns proper data type from config 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <param name="defaultValue"></param>
        /// <returns></returns>
        public static T GetSetting<T>(string key, T defaultValue = default(T)) where T : IConvertible
        {
            var val = ConfigurationManager.AppSettings[key] ?? string.Empty;
            var result = defaultValue;
            if (string.IsNullOrEmpty(val))
            {
                return result;
            }
            var typeDefault = default(T);
            if (typeof(T) == typeof(string))
            {
                typeDefault = (T)(object)string.Empty;
            }
            result = (T)Convert.ChangeType(val, typeDefault.GetTypeCode());
            return result;
        }

    }
}