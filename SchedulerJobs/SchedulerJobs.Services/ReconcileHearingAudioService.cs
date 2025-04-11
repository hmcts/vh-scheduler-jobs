using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using VideoApi.Client;
using SchedulerJobs.Common.Logging;

namespace SchedulerJobs.Services
{
    public class ReconcileHearingAudioService : IReconcileHearingAudioService
    {
        private readonly IVideoApiClient _videoApiClient;
        private readonly ILogger<ReconcileHearingAudioService> _logger;
        public ReconcileHearingAudioService(IVideoApiClient videoApiClient, ILogger<ReconcileHearingAudioService> logger)
        {
            
            _videoApiClient = videoApiClient;
            _logger = logger;
        }

        public async Task ReconcileAudiorecordingsWithConferencesAsync()
        {
            var dateTimeStr = DateTime.Today.ToString("yyyy-MM-dd");

           _logger.LogDebugReconcileAudiorecordingsWithConferencesAsyncStarted();

           var conferences = await _videoApiClient.GetConferencesHearingRoomsAsync(dateTimeStr);

           _logger.LogInformationReconcileAudiorecordingsWithConferencesAsyncConferenceCount(conferences.Count);

           var audioConferenceItems = conferences.Select(x => x.FileNamePrefix).GroupBy(x => x).Select( g => new { Name = g.Key, Count = g.Count()});

           foreach (var item in audioConferenceItems)
           {
               Console.WriteLine("{0} - {1}", item.Name, item.Count);
               _logger.LogInformationReconcileAudiorecordingsWithConferencesAsyncProcessingConferences(item.Name, item.Count);
                    
               var filenamePrefix = item.Name;

               _logger.LogInformationReconcileAudiorecordingsWithConferencesAsyncProcessingConferencesPrefix(filenamePrefix);

               try
               {
                   var result = await _videoApiClient.ReconcileAudioFilesInStorageAsync(filenamePrefix, item.Count);

                   _logger.LogInformationReconcileAudiorecordingsWithConferencesAsyncProcessingConferencesResult(result);

               }
               catch (Exception ex)
               {
                   _logger.LogErrorReconcileAudiorecordingsWithConferencesAsync(ex, item.Name, ex.Message);

               }
           }
           _logger.LogInformationReconcileAudiorecordingsWithConferencesAsyncCompleted();
        }
    }
}
