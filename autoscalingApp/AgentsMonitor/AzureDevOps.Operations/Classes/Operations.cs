namespace AzureDevOps.Operations.Classes
{
    public static class Operations
    {
        /// <summary>
        /// Here we will proceed working with VMSS (provision/deprovision additional agents, keep current agents count)
        /// </summary>
        /// <param name="waitingJobs"></param>
        /// <param name="onlineAgents"></param>
        /// <param name="maxAgentsInPool"></param>
        public static void WorkWithVmss(int waitingJobs, int onlineAgents, int maxAgentsInPool)
        {
            if (waitingJobs < onlineAgents)
            {
                //downscale agent pool, but keep 1 agent running for 1 hour more
            }

            if (waitingJobs > onlineAgents)
            {
                //add more agents, but not more than allowed
            }
        }
    }
}