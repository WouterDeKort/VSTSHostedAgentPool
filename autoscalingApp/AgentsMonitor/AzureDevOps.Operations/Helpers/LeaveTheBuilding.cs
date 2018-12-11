using System;
using AzureDevOps.Operations.Classes;

namespace AzureDevOps.Operations.Helpers
{
    public static class LeaveTheBuilding
    {
        public static void Exit(Retrieve dataRetriever)
        {
            dataRetriever.Dispose();
            Environment.Exit(Constants.ErrorExitCode);
        }
    }
}