namespace SchedulerJobs.CronJobs.Jobs
{
    public abstract class BaseJob : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;
        private readonly ILogger _logger;

        protected BaseJob(IHostApplicationLifetime lifetime, ILogger logger)
        {
            _lifetime = lifetime;
            _logger = logger;
        }
        
        public abstract Task DoWorkAsync();
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await DoWorkAsync();
                
                _lifetime.StopApplication();
            }
            catch (Exception ex)
            {
                // Signal to the OS that this was an error condition
                // Indicates to Kubernetes that the job has failed
                Environment.ExitCode = 1;
                
                var jobName = GetType().Name;
                _logger.LogError(ex, $"Job failed: {jobName}");
                throw;
            }
        }
    }
}
