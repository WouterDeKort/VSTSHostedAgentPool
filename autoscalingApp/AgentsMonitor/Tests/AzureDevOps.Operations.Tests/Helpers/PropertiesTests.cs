using System;
using System.Configuration;
using AzureDevOps.Operations.Classes;
using AzureDevOps.Operations.Helpers;
using NUnit.Framework;

namespace AzureDevOps.Operations.Tests.Helpers
{
    public class PropertiesTests
    {
        [TestCase("Monday-Friday", DayOfWeek.Monday, DayOfWeek.Friday)]
        [TestCase("Monday-Monday", DayOfWeek.Monday, DayOfWeek.Monday)]
        [TestCase("Monday-Tuesday", DayOfWeek.Monday, DayOfWeek.Tuesday)]
        [TestCase("Monday-Wednesday", DayOfWeek.Monday, DayOfWeek.Wednesday)]
        [TestCase("Monday-Thursday", DayOfWeek.Monday, DayOfWeek.Thursday)]
        [TestCase("Monday-Saturday", DayOfWeek.Monday, DayOfWeek.Saturday)]
        [TestCase("Monday-Sunday", DayOfWeek.Monday, DayOfWeek.Sunday)]
        public static void BusinessDaysParserTests(string testString, DayOfWeek startingDayExpected,
            DayOfWeek endingDayExpected)
        {
            ConfigurationManager.AppSettings[Constants.BusinessHoursDaysSettingName] = testString;
            Assert.IsTrue(Properties.BusinessDaysStartingDay == startingDayExpected);
            Assert.IsTrue(Properties.BusinessDaysLastDay == endingDayExpected);
        }

        [TestCase("10-17", 10, 17)]
        [TestCase("22-23", 22, 23)]
        public static void BusinessHoursParserTests(string testString, int expectedStartingHour, int expectedEndHour)
        {
            ConfigurationManager.AppSettings[Constants.BusinessHoursRangeSettingName] = testString;
            Assert.AreEqual(Properties.BussinesDayStartHour, expectedStartingHour);
            Assert.AreEqual(Properties.BussinesDayEndHour, expectedEndHour);
        }
    }
}