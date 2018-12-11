using System;
using AzureDevOps.Operations.Classes;
using Microsoft.Azure.WebJobs;

namespace AutoScaler
{
    public class Functions
    {
        [Singleton]
        public static void TimerTrig([TimerTrigger("0 */5 * * * *", RunOnStartup = true)] TimerInfo timer)
        {
            Checker.AgentsQueue();
        }
    }
}