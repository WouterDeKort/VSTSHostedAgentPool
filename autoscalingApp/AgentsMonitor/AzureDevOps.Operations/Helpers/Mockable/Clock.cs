using System;

namespace AzureDevOps.Operations.Helpers.Mockable
{
    /// <summary>
    /// Allows to mock DateTime - thus, test code, which is relying on DateTime objects
    /// </summary>
    public static class Clock
    {
        public static DateTime Now => _nowImplementation();

        private static Func<DateTime> _nowImplementation = () => DateTime.Now;

        /// <summary>
        /// Provides indirect access to NowImplementation of <see cref="Clock._nowImplementation"/>
        /// </summary>
        public static class TestApi
        {
            // ReSharper disable once MemberHidesStaticFromOuterClass
            public static Func<DateTime> Now
            {
                set => _nowImplementation = value;
            }

            public static void Reset()
            {
                _nowImplementation = () => DateTime.Now;
            }
        }

    }
}