using System;

namespace SchedulerJobs.Common.Models
{
    public class JudiciaryLeaverModel
    {
        public Guid? Id { get; set; }
        public bool Leaver { get; set; }
        public string LeftOn { get; set; }
    }
}
