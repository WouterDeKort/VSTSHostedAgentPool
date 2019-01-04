using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using AzureDevOps.Operations.Classes;
using AzureDevOps.Operations.Models;

namespace AzureDevOps.Operations.Helpers
{
    public static class Decisions
    {
        /// <summary>
        /// Decides how much agents must be added
        /// </summary>
        /// <param name="runningJobs">Amount of current jobs running</param>
        /// <param name="agentsCount">Amount of online agents now</param>
        /// <param name="maxAgents">Maximum accessible agents in current pool</param>
        /// <returns></returns>
        public static int HowMuchAgents(int runningJobs, int agentsCount, int maxAgents)
        {
            if (agentsCount == maxAgents && runningJobs >= agentsCount)
            {
                //there is more jobs than we could have agents deployed
                return 0;
            }

            var amountOfAgents = runningJobs - agentsCount;

            var dynamicProperties = new DynamicProps();

            if (dynamicProperties.WeAreInsideBusinessTime && amountOfAgents <= 0)
            {
                if (agentsCount <= Properties.AmountOfAgents)
                {
                    return Properties.AmountOfAgents - agentsCount;
                }

                if (amountOfAgents < Properties.AmountOfAgents)
                {
                    //we need to deprovision agents in business time
                    return amountOfAgents + Properties.AmountOfAgents;
                }
            }

            if (dynamicProperties.WeAreInsideBusinessTime && amountOfAgents > 0)
            {
                //we need to provision more agents
                if (agentsCount < Properties.AmountOfAgents)
                {
                    //if we need to give more agents - we need to decide on amount to be given
                    amountOfAgents = Properties.AmountOfAgents - agentsCount > amountOfAgents
                        ? Properties.AmountOfAgents - agentsCount
                        : amountOfAgents;
                }
            }

            return amountOfAgents > maxAgents ? Math.Abs(maxAgents - agentsCount) : amountOfAgents;
        }

        public static ScaleSetVirtualMachineStripped[] CollectInstanceIdsToDeallocate(IEnumerable<ScaleSetVirtualMachineStripped>vmScaleSetStripped, JobRequest[] jobRequests)
        {
            var busyAgentsNames = jobRequests.Select(job => job.ReservedAgent.Name).ToArray();

            return vmScaleSetStripped.Where(scaleSetVirtualMachineStripped => !busyAgentsNames.Contains(scaleSetVirtualMachineStripped.VmName)).ToArray();
        }

        /// <summary>
        /// Checks if VM got job assigned during 
        /// </summary>
        /// <param name="vmName"></param>
        /// <returns></returns>
        public static bool IsVmExecutingJob(string vmName)
        {
            var currentJobs = Checker.DataRetriever.GetRuningJobs(Properties.AgentsPoolId);
            return currentJobs.Select(job => job.ReservedAgent.Name).Contains(vmName);
        }
    }
}