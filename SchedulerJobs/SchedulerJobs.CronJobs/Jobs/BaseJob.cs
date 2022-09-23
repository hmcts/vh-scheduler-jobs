namespace SchedulerJobs.CronJobs.Jobs
{
    public abstract class BaseJob : BackgroundService
    {
        private readonly IHostApplicationLifetime _lifetime;

        protected BaseJob(IHostApplicationLifetime lifetime)
        {
            _lifetime = lifetime;
        }
        
        protected abstract Task DoWorkAsync();
        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await DoWorkAsync();
                
                _lifetime.StopApplication();
            }
            catch (Exception)
            {
                // Signal to the OS that this was an error condition
                // Indicates to Kubernetes that the job has failed
                Environment.ExitCode = 1;
                throw;
            }
        }
    }
}
