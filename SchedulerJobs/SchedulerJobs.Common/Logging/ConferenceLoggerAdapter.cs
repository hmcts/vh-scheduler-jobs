namespace SchedulerJobs.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static partial class ConferenceLoggerAdapter
    {
        // Information messages

        [LoggerMessage(
            EventId = 6002,
            Level = LogLevel.Information,
            Message = "Cleared chat history for closed conferences")]
        public static partial void LogInformationClearedChatHistoryForClosedConferences(this ILogger logger);

        [LoggerMessage(
            EventId = 6006,
            Level = LogLevel.Information,
            Message = "Reconcile audio recording files with number of conferences for the day - Done")]
        public static partial void LogInformationReconcileAudioRecordingFilesForTheDay(this ILogger logger);

        [LoggerMessage(
            EventId = 6007,
            Level = LogLevel.Information,
            Message = "Removed heartbeats for conferences older than 14 days.")]
        public static partial void LogInformationRemovedHeartbeatsForConferencesOlderThan14Days(this ILogger logger);

        [LoggerMessage(
            EventId = 6027,
            Level = LogLevel.Information,
            Message = "ReconcileAudiorecordingsWithConferencesAsync - Conferences count {ConferenceCount}")]
        public static partial void LogInformationReconcileAudiorecordingsWithConferencesAsyncConferenceCount(this ILogger logger, int conferenceCount);

        [LoggerMessage(
            EventId = 6028,
            Level = LogLevel.Information,
            Message = "ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - {ItemName} {ItemCount}")]
        public static partial void LogInformationReconcileAudiorecordingsWithConferencesAsyncProcessingConferences(this ILogger logger, string itemName, int itemCount);

        [LoggerMessage(
            EventId = 6029,
            Level = LogLevel.Information,
            Message = "ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - filename prefix {FilenamePrefix}")]
        public static partial void LogInformationReconcileAudiorecordingsWithConferencesAsyncProcessingConferencesPrefix(this ILogger logger, string filenamePrefix);

        [LoggerMessage(
            EventId = 6030,
            Level = LogLevel.Information,
            Message = "ReconcileAudiorecordingsWithConferencesAsync - Processing conferences - result {Result}")]
        public static partial void LogInformationReconcileAudiorecordingsWithConferencesAsyncProcessingConferencesResult(this ILogger logger, bool result);

        [LoggerMessage(
            EventId = 6031,
            Level = LogLevel.Information,
            Message = "ReconcileAudiorecordingsWithConferencesAsync - Completed")]
        public static partial void LogInformationReconcileAudiorecordingsWithConferencesAsyncCompleted(this ILogger logger);

        [LoggerMessage(
            EventId = 6014, 
            Level = LogLevel.Information,
            Message = "Number of pagination results: {Results}")]
        public static partial void LogInformationNumberOfPaginationResults(this ILogger logger, int results);
        
        [LoggerMessage(
            EventId = 6017, 
            Level = LogLevel.Information,
            Message = "Data anonymised for hearings, conferences older than 3 months.")]
        public static partial void LogInformationDataAnonymisedForHearingsOlder3Months(this ILogger logger);

        // Debug messages

        [LoggerMessage(
            EventId = 9001,
            Level = LogLevel.Debug,
            Message = "ReconcileAudiorecordingsWithConferencesAsync - Started")]
        public static partial void LogDebugReconcileAudiorecordingsWithConferencesAsyncStarted(this ILogger logger);

        // Error messages

        [LoggerMessage(
           EventId = 7008,
           Level = LogLevel.Error,
           Message = "ReconcileAudiorecordingsWithConferencesAsync - missing wowza audio or empty files for conferences - {ItemName} Exception from Reconciliation - {Message}")]
        public static partial void LogErrorReconcileAudiorecordingsWithConferencesAsync(this ILogger logger, Exception exception, string itemName, string message);

    }
}