using System;

namespace AzureDevOps.Operations.Helpers
{
    public static class Decisions
    {
        /// <summary>
        /// If there is more jobs then agents - we need to add more; else - we need to downscale
        /// </summary>
        /// <param name="runningJobs"></param>
        /// <param name="agentsCount"></param>
        /// <returns></returns>
        public static bool AddMoreAgents(int runningJobs, int agentsCount)
        {
            //that's a stub, which shall change in future, to take into account situation, when there is just one agent running
            return runningJobs > agentsCount;
        }

        public static int HowMuchAgents(int runningJobs, int agentsCount, int maxAgents)
        {
            if (agentsCount == maxAgents && runningJobs >= agentsCount)
            {
                //there is more jobs than we could have agents deployed
                return 0;
            }
            return Math.Abs(runningJobs - agentsCount);
        }
    }
}