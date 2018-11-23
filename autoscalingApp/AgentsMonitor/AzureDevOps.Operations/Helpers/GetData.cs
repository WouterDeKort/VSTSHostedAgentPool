using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Web;
using AzureDevOps.Operations.Classes;
using Newtonsoft.Json;

namespace AzureDevOps.Operations.Helpers
{
    /// <summary>
    /// Accesses data from Azure DevOps server
    /// </summary>
    public static class GetData
    {
        /// <summary>
        /// Get data from Azure DevOps server and deserialize it 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="accessToken"></param>
        /// <returns></returns>
        internal static T DownloadSerializedJsonData<T>(string url, string accessToken) where T : new()
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var encodedAuth = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{string.Empty}:{accessToken}"));
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", encodedAuth);

                var uriBuilder = new UriBuilder(url);
                //if we have query string in incoming URL - parse it
                var query = HttpUtility.ParseQueryString(uriBuilder.Query);
                //append api-version for which our models are built
                query["api-version"] = Constants.AzureDevOpsApiVersion;
                uriBuilder.Query = query.ToString();
                
                var response = client.GetAsync(uriBuilder.Uri).Result;
                string jsonData;

                if (response.IsSuccessStatusCode)
                {
                    jsonData = response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    //handle non error??
                    return new T();
                }

                return JsonConvert.DeserializeObject<T>(jsonData);
            }
        }
    }
}