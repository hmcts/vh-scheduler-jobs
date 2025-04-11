namespace SchedulerJobs.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static partial class JobLoggerAdapter
    {
        // Information messages

        [LoggerMessage(
            EventId = 6000,
            Level = LogLevel.Information,
            Message = "Job {jobName} already running")]
        public static partial void LogInformationJobNameAlreadyRunning(this ILogger logger, string jobName);

        [LoggerMessage(
            EventId = 6001,
            Level = LogLevel.Information,
            Message = "Close hearings function executed")]
        public static partial void LogInformationCloseHearingsFunctionExecuted(this ILogger logger);

        [LoggerMessage(
             EventId = 6003,
             Level = LogLevel.Information,
             Message = "Close hearings job executed and {conferencesCount} hearings closed")]
        public static partial void LogInformationCloseHearingsJobExecutedHearinsClosed(this ILogger logger, int conferencesCount);

        [LoggerMessage(
            EventId = 6004, 
            Level = LogLevel.Information,
            Message = "Started GetJudiciaryUsers job at: {Now} - param UpdatedSince: {UpdatedSince}")]
        public static partial void LogInformationStartedGetJudiciaryUsersJob(this ILogger logger, DateTime now, string updatedSince);
        
        [LoggerMessage(
            EventId = 6005, 
            Level = LogLevel.Information,
            Message = "Finished GetJudiciaryUsers job at: {Now} - param UpdatedSince: {UpdatedSince}")]
        public static partial void LogInformationFinishedGetJudiciaryUsersJob(this ILogger logger, DateTime now, string updatedSince);

        [LoggerMessage(
            EventId = 6008, 
            Level = LogLevel.Information,
            Message = "Send hearing notifications - Completed at: {DateTime}")]
        public static partial void LogInformationSendHearingNotifications(this ILogger logger, DateTime dateTime);

        // Error messages

        [LoggerMessage(
            EventId = 7000, 
            Level = LogLevel.Error,
            Message = "Job failed: {jobName}")]
        public static partial void LogErrorJobFailed(this ILogger logger, string jobName);
        

    }
}