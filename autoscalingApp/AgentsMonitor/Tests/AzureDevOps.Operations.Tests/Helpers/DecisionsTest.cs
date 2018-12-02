using System.Collections.Generic;
using AzureDevOps.Operations.Helpers;
using AzureDevOps.Operations.Models;
using AzureDevOps.Operations.Tests.Classes;
using AzureDevOps.Operations.Tests.Data;
using NUnit.Framework;

namespace AzureDevOps.Operations.Tests.Helpers
{
    public class DecisionsTest
    {
        [TestCase(2, 1, true, Description = "There is 2 jobs, and 1 agent - we shall upscale")]
        [TestCase(1, 2, false, Description = "There is 1 job, and 2 agent - we shall downscale")]
        public static void WhatToDoWithAgents(int jobsCount, int agentsCount, bool expectedResult)
        {
            var operation = Decisions.AddMoreAgents(jobsCount, agentsCount);

            Assert.AreEqual(operation, expectedResult);
        }

        [TestCase(3, 3, 7, 0, Description = "There is 3 jobs and 3 agents - do not need to add anything")]
        [TestCase(5, 3, 7, 2, Description = "There is 5 jobs and 3 agents - need 2 more agents")]
        [TestCase(1, 3, 7, 2, Description = "There is 1 job and 3 agents - could deprovision 2 agents")]
        [TestCase(10, 7, 7, 0, Description = "There is 10 jobs and 7 agents, with 7 agents max - could not do anything")]
        [TestCase(7, 7, 7, 0, Description = "There is 7 jobs and 7 agents, with 7 agents max - should not do anything")]
        public static void AmountOfAgents(int jobsCount, int agentsCount, int maxAgentsCount, int expectedAmount)
        {
            var amount = Decisions.HowMuchAgents(jobsCount, agentsCount, maxAgentsCount);
           
            Assert.AreEqual(amount, expectedAmount);
        }

        [Test]
        public static void TestInstanceIdRetrieval_agent_is_there()
        {
            var testValid = new ScaleSetVirtualMachineStripped
            {
                VmName = "Agent",
                VmInstanceId = "205"
            };

            var testArray = new ScaleSetVirtualMachineStripped[1];
            testArray[0] = testValid;

            var vmScaleSetData = GetTestData(10, testArray);
            var instanceIds = GetInstanceIds(vmScaleSetData);
            Assert.IsTrue(instanceIds.Length.Equals(1));
            Assert.IsTrue(instanceIds[0].Equals(testValid.VmInstanceId));
        }

        [Test]
        public static void TestInstanceIdRetrieval_agent_is_not_there()
        {
            var vmScaleSetData = GetTestData(10);
            
            var instanceIds = GetInstanceIds(vmScaleSetData);
            Assert.IsTrue(instanceIds.Length.Equals(0));
        }

        [Test]
        public static void TestInstanceIdRetrieval_no_jobs_retrieved()
        {
            var vmScaleSetData = GetTestData(10);
            
            var instanceIds = GetInstanceIds(vmScaleSetData, 1);
            Assert.IsTrue(instanceIds.Length.Equals(0));
        }

        private static string[] GetInstanceIds(List<ScaleSetVirtualMachineStripped> vmScaleSetData, int poolId = TestsConstants.TestPoolId)
        {
            var dataRetriever = RetrieveTests.CreateRetriever(TestsConstants.Json1JobIsRunning);
            return Decisions.CollectInstanceIdsToDeallocate(vmScaleSetData,
                dataRetriever.GetRuningJobs(poolId));
        }

        private static List<ScaleSetVirtualMachineStripped> GetTestData(int testListSize, ScaleSetVirtualMachineStripped[] addedData = null)
        {
            var vmScaleSetData = new List<ScaleSetVirtualMachineStripped>();
            if (addedData != null)
            {
                vmScaleSetData.AddRange(addedData);
            }
            
            for (var counter = 0; counter < testListSize; counter++)
            {
                vmScaleSetData.Add(new ScaleSetVirtualMachineStripped
                {
                    VmName = $"vm{counter}",
                    VmInstanceId = $"{counter}"
                });
            }

            return vmScaleSetData;

        }
    }
}