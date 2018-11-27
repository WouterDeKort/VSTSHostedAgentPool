using System.IO;
using System.Net.Http;
using AzureDevOps.Operations.Classes;
using AzureDevOps.Operations.Tests.Data;
using NUnit.Framework;
using RichardSzalay.MockHttp;

namespace AzureDevOps.Operations.Tests.Classes
{
    public class RetrieveTests
    {
        [TestCase(@"..\..\Data\TestData\GetPoolId\pools-success.json")]
        public void GetPoolIdTest_Pool_Present(string jsonPath)
        {
            var client = SetupHttpClient(jsonPath);

            var dataRetriever = new Retrieve(TestsConstants.TestOrganizationName, TestsConstants.TestToken,
                client);

            var poolId = dataRetriever.GetPoolId(TestsConstants.TestPoolName);

            Assert.IsNotNull(poolId);
            Assert.AreEqual(poolId.Value, 12);
        }

        [TestCase(@"..\..\Data\TestData\GetPoolId\pools-fail.json")]
        public void GetPoolIdTest_Pool_Not_Present(string jsonPath)
        {
            var client = SetupHttpClient(jsonPath);

            var dataRetriever = new Retrieve(TestsConstants.TestOrganizationName, TestsConstants.TestToken,
                client);

            var poolId = dataRetriever.GetPoolId(TestsConstants.TestPoolName);

            Assert.IsNull(poolId);
        }

        private static HttpClient SetupHttpClient(string jsonPathResponse)
        {
            var mockHttp = new MockHttpMessageHandler();

            var jsonPathCombined = Path.Combine(System.AppContext.BaseDirectory, jsonPathResponse);
            
            var response = File.Exists(jsonPathCombined) ? File.ReadAllText(jsonPathCombined) : string.Empty;

            var request = mockHttp.When("*").Respond("application/json", response);
            return mockHttp.ToHttpClient();
        }
    }
}