using AzureDevOps.Operations.Helpers;
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
    }
}