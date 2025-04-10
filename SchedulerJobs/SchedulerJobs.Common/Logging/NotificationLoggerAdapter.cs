namespace SchedulerJobs.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static partial class NotificationLoggerAdapter
    {
        // Information messages
        
        [LoggerMessage(
            EventId = 6024, 
            Level = LogLevel.Information,
            Message = "SendNotificationsAsync - Started")]
        public static partial void LogInformationSendNotificationsAsyncStarted(this ILogger logger);

        
        [LoggerMessage(
            EventId = 6025, 
            Level = LogLevel.Information,
            Message = "SendNotificationsAsync - No hearings to send notifications")]
        public static partial void LogInformationSendNotificationsAsyncNoHearingsToNotify(this ILogger logger);

        
        [LoggerMessage(
            EventId = 6026, 
            Level = LogLevel.Information,
            Message = "SendNotificationsAsync - Ignored Participant: {ParticipantId} has role {RoleName} which is not supported for notification in the hearing {HearingId}")]
        public static partial void LogInformationSendNotificationsAsyncIgnoreParticipants(this ILogger logger, Guid participantId, string roleName, Guid hearingId);

        // Error messages

         [LoggerMessage(
            EventId = 7006, 
            Level = LogLevel.Error,
            Message = "Error sending multi day hearing reminder email for hearing {HearingId} and case number {CaseNumber} to participant {ParticipantId}")]
        public static partial void LogErrorSendingMultiDayEmail(this ILogger logger, Exception exception, Guid hearingId, string caseNumber, Guid participantId);
        
        [LoggerMessage(
            EventId = 7007, 
            Level = LogLevel.Error,
            Message = "Error sending single day hearing reminder email for hearing {HearingId} and case number {CaseNumber} to participant {ParticipantId}")]
        public static partial void LogErrorSendingSingleDayEmail(this ILogger logger, Exception exception, Guid hearingId, string caseNumber, Guid participantId);
    }
}