using AzureDevOps.Operations.Helpers;
using AzureDevOps.Operations.Helpers.Mockable;
using AzureDevOps.Operations.Tests.Classes;
using NUnit.Framework;

namespace AzureDevOps.Operations.Tests.Helpers
{
    public class DynamicPropsTests
    {
        [Test]
        public void SettingsIsNotDefined()
        {
            var dynamicProp = new DynamicProps();
            Assert.IsFalse(dynamicProp.WeAreInsideBusinessTime);
        }

        //Monday
        [TestCase("10-Dec-2018 09:15", false)]
        [TestCase("10-Dec-2018 10:15", true)]
        [TestCase("10-Dec-2018 12:15", true)]
        [TestCase("10-Dec-2018 18:15", false)]
        //Tuesday
        [TestCase("11-Dec-2018 09:15", false)]
        [TestCase("11-Dec-2018 10:15", true)]
        [TestCase("11-Dec-2018 12:15", true)]
        [TestCase("11-Dec-2018 18:15", false)]
        //Wednesday
        [TestCase("12-Dec-2018 09:15", false)]
        [TestCase("12-Dec-2018 10:15", true)]
        [TestCase("12-Dec-2018 12:15", true)]
        [TestCase("12-Dec-2018 18:15", false)]
        //Thursday
        [TestCase("13-Dec-2018 09:15", false)]
        [TestCase("13-Dec-2018 10:15", true)]
        [TestCase("13-Dec-2018 12:15", true)]
        [TestCase("13-Dec-2018 18:15", false)]
        //Friday
        [TestCase("14-Dec-2018 09:15", false)]
        [TestCase("14-Dec-2018 10:15", true)]
        [TestCase("14-Dec-2018 12:15", true)]
        [TestCase("14-Dec-2018 18:15", false)]
        //Saturday
        [TestCase("15-Dec-2018 09:15", false)]
        [TestCase("15-Dec-2018 10:15", false)]
        [TestCase("15-Dec-2018 12:15", false)]
        [TestCase("15-Dec-2018 18:15", false)]
        //Sunday
        [TestCase("16-Dec-2018 09:15", false)]
        [TestCase("16-Dec-2018 10:15", false)]
        [TestCase("16-Dec-2018 12:15", false)]
        [TestCase("16-Dec-2018 18:15", false)]
        public void CheckingValuesDefinitions(string testDateTime, bool expectedResult)
        {
            TestInitilizers.InitAppSettingsForBusinessTimesTests();
            var dynamicProp = new DynamicProps();
            Clock.TestApi.Now = () => TestInitilizers.ParseDateTimeForTest(testDateTime);

            Assert.AreEqual(dynamicProp.WeAreInsideBusinessTime, expectedResult);

            Clock.TestApi.Reset();
        }
    }
}