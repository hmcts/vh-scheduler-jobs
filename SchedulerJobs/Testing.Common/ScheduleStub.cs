using System;
using Microsoft.Azure.WebJobs.Extensions.Timers;

namespace Testing.Common
{
    public class ScheduleStub : TimerSchedule
    {
        public override bool AdjustForDST => false;

        public override DateTime GetNextOccurrence(DateTime now)
        {
            return now;
        }
    }
}