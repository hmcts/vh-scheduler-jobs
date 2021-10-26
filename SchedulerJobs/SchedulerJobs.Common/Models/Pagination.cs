namespace SchedulerJobs.Common.Models
{
    public class Pagination
    {
        public bool MorePages { get; set; }

        public int CurrentPage { get; set; }

        public int ResultsPerPage { get; set; }

        public int Results { get; set; }

        public int Pages { get; set; }
    }
}