using System.Collections.Generic;

namespace SchedulerJobs.Common.Models
{
    public class PeopleResponse
    {
        public Pagination Pagination { get; set; }
        public IEnumerable<JudiciaryPersonModel> Results { get; set; }
    }
}
