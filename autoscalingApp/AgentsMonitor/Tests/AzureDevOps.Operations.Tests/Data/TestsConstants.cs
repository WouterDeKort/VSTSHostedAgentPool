namespace AzureDevOps.Operations.Tests.Data
{
    public static class TestsConstants
    {
        internal const string TestOrganizationName = "testOrganization";
        internal const string TestToken = "testToken";
        internal const string TestPoolName = "testPool";

        internal const int TestPoolId = 12;
        internal const int AllAgentsCount = 4;
        internal const int OnlineAgentsCount = 3;

        internal const string FileNotExistPointer = @"\fileNotExists";
        /// <summary>
        /// Points to Json with 1 running job
        /// </summary>
        internal const string Json1JobIsRunning = @"..\..\Data\TestData\JobRequests\jobs-1-running.json";
    }
}