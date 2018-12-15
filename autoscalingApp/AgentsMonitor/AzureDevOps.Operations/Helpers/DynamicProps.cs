using AzureDevOps.Operations.Helpers.Mockable;

namespace AzureDevOps.Operations.Helpers
{
    /// <summary>
    /// Checks dynamic properties
    /// </summary>
    public class DynamicProps
    {
        /// <summary>
        /// checks, if we are situated inside business times, defined in settings
        /// </summary>
        public bool WeAreInsideBusinessTime
        {
            get
            {
                if (!Properties.BusinessRuntimeDefined)
                {
                    //if business requirements is not defined - then we are not inside them, actually :D
                    return Properties.BusinessRuntimeDefined;
                }

                var currentTime = Clock.Now;
                //checks that current time falls in defined values
                return (currentTime.DayOfWeek >= Properties.BusinessDaysStartingDay 
                        && currentTime.DayOfWeek <= Properties.BusinessDaysLastDay
                        && currentTime.Hour >= Properties.BussinesDayStartHour
                        && currentTime.Hour <= Properties.BussinesDayEndHour);

            }
        }
    }
}