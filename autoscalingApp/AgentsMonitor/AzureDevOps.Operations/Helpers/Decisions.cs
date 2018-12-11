using System;
using System.Collections.Generic;
using System.Linq;
using AzureDevOps.Operations.Models;

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
            return runningJobs > agentsCount;
        }

        public static int HowMuchAgents(int runningJobs, int agentsCount, int maxAgents)
        {
            if (agentsCount == maxAgents && runningJobs >= agentsCount)
            {
                //there is more jobs than we could have agents deployed
                return 0;
            }

            var amountOfAgents = Math.Abs(runningJobs - agentsCount);

            return amountOfAgents > maxAgents ? Math.Abs(maxAgents - agentsCount) : amountOfAgents;
        }

        public static string[] CollectInstanceIdsToDeallocate(IEnumerable<ScaleSetVirtualMachineStripped>vmScaleSetStripped, JobRequest[] jobRequests)
        {
            var instanceIdList = new List<string>();
            var busyAgentsNames = jobRequests.Select(job => job.ReservedAgent.Name).ToArray();

            foreach (var scaleSetVirtualMachineStripped in vmScaleSetStripped)
            {
                if (!busyAgentsNames.Contains(scaleSetVirtualMachineStripped.VmName))
                {
                    instanceIdList.Add(scaleSetVirtualMachineStripped.VmInstanceId);
                }
            }

            return instanceIdList.ToArray();
        }
    }
}