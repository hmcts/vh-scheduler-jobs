namespace SchedulerJobs.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static partial class HearingLoggerAdapter
    {
        // Information messages
        
        [LoggerMessage(
            EventId = 6009, 
            Level = LogLevel.Information,
            Message = "Hearing ids being processed: {HearingIds}")]
        public static partial void LogInformationHearingIds(this ILogger logger, IList<Guid> hearingIds);
        
        [LoggerMessage(
            EventId = 6021, 
            Level = LogLevel.Information,
            Message = "AllocateHearings: Starting to allocate hearings")]
        public static partial void LogInformationAllocateHearingsStartToAllocate(this ILogger logger);
        
        [LoggerMessage(
            EventId = 6022, 
            Level = LogLevel.Information,
            Message = "AllocateHearings: Allocated user {allocatedUsername} to hearing {hearingId}")]
        public static partial void LogInformationAllocateHearingsUserAndHearing(this ILogger logger, string allocatedUsername, Guid hearingId);

        [LoggerMessage(
            EventId = 6023, 
            Level = LogLevel.Information,
            Message = "AllocateHearings: Completed allocation of hearings, {hearingsAllocated} of {hearingsToAllocate} hearings allocated")]
        public static partial void LogInformationAllocateHearingCompleteAllocation(this ILogger logger, int hearingsAllocated, int hearingsToAllocate);

        // Warning messages

        [LoggerMessage(
            EventId = 8004, 
            Level = LogLevel.Warning,
            Message = "AllocateHearings: Error allocating hearing {hearingId}")]
        public static partial void LogWarningAllocateHearings(this ILogger logger, Exception exception, Guid hearingId);

        // Error messages

        [LoggerMessage(
            EventId = 7005, 
            Level = LogLevel.Error,
            Message = "AllocateHearings: Error allocating hearing {hearingId}")]
        public static partial void LogErrorAllocateHearings(this ILogger logger, Exception exception, Guid hearingId);

        [LoggerMessage(
            EventId = 7001, 
            Level = LogLevel.Error,
            Message = "Unknown exception when processing {username}")]
        public static partial void LogErrorUnknownExceptionForUser(this ILogger logger, Exception exception, string username);
    }
}