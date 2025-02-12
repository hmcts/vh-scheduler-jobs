using Microsoft.Extensions.Logging;
using SchedulerJobs.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using VideoApi.Client;

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

           _logger.LogDebug("ReconcileAudiorecordingsWithConferencesAsync - Started");

           var conferences = await _videoApiClient.GetConferencesHearingRoomsAsync(dateTimeStr);

           _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Conferences count {ConferenceCount}", + conferences.Count);

           var audioConferenceItems = conferences.Select(x => x.FileNamePrefix).GroupBy(x => x).Select( g => new { Name = g.Key, Count = g.Count()});

           foreach (var item in audioConferenceItems)
           {
               Console.WriteLine("{0} - {1}", item.Name, item.Count);
               _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - {ItemName} {ItemCount}", item.Name, item.Count);
                    
               var filenamePrefix = item.Name;

               _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - filename prefix {FilenamePrefix}", filenamePrefix);

               try
               {
                   var result = await _videoApiClient.ReconcileAudioFilesInStorageAsync(filenamePrefix, item.Count);

                   _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - result {Result}", result);

               }
               catch (Exception ex)
               {
                   _logger.LogError(ex, "ReconcileAudiorecordingsWithConferencesAsync - missing wowza audio or empty files for conferences - {ItemName} Exception from Reconciliation - {Message}", item.Name, ex.Message);

               }
           }
           _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Completed");
        }
    }
}
