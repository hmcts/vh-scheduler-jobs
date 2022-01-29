using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoApi.Client;

namespace SchedulerJobs.Services
{

    public interface IReconcileHearingAudioService
    {
        Task ReconcileAudiorecordingsWithConferencesAsync();
    }

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

            var audioMissingList = new List<string>();
            
           _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Started");

            try
            {
                var conferences = await _videoApiClient.GetConferencesHearingRoomsAsync(dateTimeStr);

                _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Conferences count " + conferences.Count.ToString());

                var audioConferenceItems = conferences.Select(x => x.HearingId).GroupBy(x => x).Select( g => new { Name = g.Key, Count = g.Count()});

                foreach (var item in audioConferenceItems)
                {
                    Console.WriteLine("{0} - {1}", item.Name, item.Count);
                    _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - " + item.Name + "" + item.Count.ToString());
                    
                    var filename_prefix = item.Name;

                    _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - filename prefix " + filename_prefix);

                    try
                    {
                        var result = await _videoApiClient.ReconcileAudioFilesInStorageAsync(filename_prefix, item.Count);

                        _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - result " + result.ToString());

                    }
                    catch (Exception ex)
                    {
                        audioMissingList.Add(item.Name.ToString());
                        _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - missing wowza audio or empty files for conferences - " + item.Name + "  Exception from Reconciliation - " + ex.Message);

                    }
                }
                _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Completed");
            }
            catch (Exception ex)
            {
                _logger.LogInformation("ReconcileAudiorecordingsWithConferencesAsync - Exception" + ex.StackTrace);
                throw;
            }

        }
    }
}
