using AzureDevOps.Operations.Classes;
using Microsoft.Azure.WebJobs;

namespace AutoScaler
{
    public static class Functions
    {
        /// <summary>
        /// Deprovisioning trigger shall run less frequently than provisioning one
        /// </summary>
        /// <param name="timer"></param>
        [Singleton]
        public static void DeprovisionTrigger([TimerTrigger("0 */15 * * * *", RunOnStartup = true)] 
            TimerInfo timer)
        {
            Checker.AgentsQueue(false);
        }

        /// <summary>
        /// Provision more agents shall be running more frequently to allow faster agents provisioning
        /// </summary>
        /// <param name="timer"></param>
        public static void ProvisionTrigger([TimerTrigger("0 */2 * * * *", RunOnStartup = true)]
            TimerInfo timer)
        {
            Checker.AgentsQueue(true);
        }
    }
}