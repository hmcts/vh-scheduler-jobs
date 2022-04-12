using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SchedulerJobs.Services.Interfaces
{
    public interface IHearingNotificationService
    {
        public Task SendNotificationsAsync();
    }
}
