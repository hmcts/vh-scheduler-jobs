using System.Collections.Generic;

namespace SchedulerJobs.Common.Models
{
    public class LeaversResponse
    {
        public Pagination Pagination { get; set; }
        public IEnumerable<JudiciaryLeaverModel> Results { get; set; }
    }
}
