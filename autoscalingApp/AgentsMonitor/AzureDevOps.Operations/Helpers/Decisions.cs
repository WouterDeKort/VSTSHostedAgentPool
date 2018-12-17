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

            }
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