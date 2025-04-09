namespace SchedulerJobs.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static partial class LoggerAdapter
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
            EventId = 6002, 
            Level = LogLevel.Information,
            Message = "Cleared chat history for closed conferences")]
        public static partial void LogInformationClearedChatHistoryForClosedConferences(this ILogger logger);
        
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
            EventId = 6008, 
            Level = LogLevel.Information,
            Message = "Send hearing notifications - Completed at: {DateTime}")]
        public static partial void LogInformationSendHearingNotifications(this ILogger logger, DateTime dateTime);
        
        [LoggerMessage(
            EventId = 6009, 
            Level = LogLevel.Information,
            Message = "Hearing ids being processed: {HearingIds}")]
        public static partial void LogInformationHearingIds(this ILogger logger, IList<Guid> hearingIds);
        
        [LoggerMessage(
            EventId = 6010, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryPeople: Removing all records from JudiciaryPersonsStaging")]
        public static partial void LogInformationImportJudiciaryPeopleRemoving(this ILogger logger);
        
        [LoggerMessage(
            EventId = 6011, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryPeople: Executing page {CurrentPage}")]
        public static partial void LogInformationImportJudiciaryPeopleExecutingPage(this ILogger logger, int currentPage);
        
        [LoggerMessage(
            EventId = 6012, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryPeople: Create people json file page-'{currentPage}'")]
        public static partial void LogInformationImportJudiciaryPeopleCreatePeopleJsonFile(this ILogger logger, int currentPage);
        
        [LoggerMessage(
            EventId = 6013, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryPeople: Calling bookings API with '{Count}' people")]
        public static partial void LogInformationImportJudiciaryPeopleCallingApiWith(this ILogger logger, int count);
        
        [LoggerMessage(
            EventId = 6014, 
            Level = LogLevel.Information,
            Message = "Number of pagination results: {Results}")]
        public static partial void LogInformationNumberOfPaginationResults(this ILogger logger, int results);
        
        [LoggerMessage(
            EventId = 6015, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryPeople: Adding raw data to JudiciaryPersonStaging from page: {CurrentPage}, total records: {Records}")]
        public static partial void LogInformationImportJudiciaryAddingToJudiciaryPersonStaging(this ILogger logger, int currentPage, int records);
        
        [LoggerMessage(
            EventId = 6016, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryLeavers: Calling bookings API with '{leaversResultCount}' leavers")]
        public static partial void LogInformationImportJudiciaryLeaversCallingApi(this ILogger logger, int leaversResultCount);
        
        [LoggerMessage(
            EventId = 6017, 
            Level = LogLevel.Information,
            Message = "Data anonymised for hearings, conferences older than 3 months.")]
        public static partial void LogInformationDataAnonymisedForHearingsOlder3Months(this ILogger logger);
        
        [LoggerMessage(
            EventId = 6018, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryPeople: Removing all records from JudiciaryPersonsStaging")]
        public static partial void LogInformationImportJudiciaryPeopleFromJudiciaryPersonsStaging(this ILogger logger);
        
        [LoggerMessage(
            EventId = 6019, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryPeople: Calling bookings API with {PeopleResultCount} people")]
        public static partial void LogInformationImportJudiciaryPeopleTotalPeople(this ILogger logger, int peopleResultCount);
        
        [LoggerMessage(
            EventId = 6020, 
            Level = LogLevel.Information,
            Message = "No judiciary persons leaving: using stub")]
        public static partial void LogInformationJudiciaryPeopleLeavingStub(this ILogger logger);
        
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

        
        // Warning messages

        [LoggerMessage(
            EventId = 8000, 
            Level = LogLevel.Warning,
            Message = "ImportJudiciaryPeople: List of Personal code which are failed to insert '{invalidPeople}'")]
        public static partial void LogWarningImportJudiciaryPeopleListOfPersonalCode(this ILogger logger, string invalidPeople);
        
        [LoggerMessage(
            EventId = 8001, 
            Level = LogLevel.Warning,
            Message = "ImportJudiciaryPeople: No of people who are invalid '{Count}' in page '{CurrentPage}'. Pages: {Pages}")]
        public static partial void LogWarningImportJudiciaryPeopleInvalidPeopleCount(this ILogger logger, int count, int currentPage, int pages);
        
        [LoggerMessage(
            EventId = 8002, 
            Level = LogLevel.Warning,
            Message = "ImportJudiciaryLeavers: No of leavers who are invalid '{invalidCount}' in page '{currentPage}'.")]
        public static partial void LogWarningImportJudiciaryPeopleInvalidPeopleCount(this ILogger logger, int invalidCount, int currentPage);
        
        [LoggerMessage(
            EventId = 8003, 
            Level = LogLevel.Warning,
            Message = "ImportJudiciaryPeople: No results from api for page: {CurrentPage}")]
        public static partial void LogWarningImportJudiciaryPeopleCountFromApi(this ILogger logger, int currentPage);
        
        [LoggerMessage(
            EventId = 8004, 
            Level = LogLevel.Warning,
            Message = "AllocateHearings: Error allocating hearing {hearingId}")]
        public static partial void LogWarningAllocateHearings(this ILogger logger, Exception exception, Guid hearingId);
        

        // Debug messages

        [LoggerMessage(
            EventId = 9000, 
            Level = LogLevel.Debug,
            Message = "ImportJudiciaryPeople: using stub")]
        public static partial void LogDebugImportJudiciaryPeopleUsingStub(this ILogger logger);
        
        [LoggerMessage(
            EventId = 9001, 
            Level = LogLevel.Debug,
            Message = "ReconcileAudiorecordingsWithConferencesAsync - Started")]
        public static partial void LogDebugReconcileAudiorecordingsWithConferencesAsyncStarted(this ILogger logger);
        
        // Error messages

        [LoggerMessage(
            EventId = 7000, 
            Level = LogLevel.Error,
            Message = "Job failed: {jobName}")]
        public static partial void LogErrorJobFailed(this ILogger logger, string jobName);
        
        [LoggerMessage(
            EventId = 7001, 
            Level = LogLevel.Error,
            Message = "Unknown exception when processing {username}")]
        public static partial void LogErrorUnknownExceptionForUser(this ILogger logger, Exception exception, string username);
        
        [LoggerMessage(
            EventId = 7002, 
            Level = LogLevel.Error,
            Message = "There was a problem importing judiciary leavers in page: '{currentPage}'")]
        public static partial void LogErrorImportJudiciaryLeaversException(this ILogger logger, Exception exception, int currentPage);
        
        [LoggerMessage(
            EventId = 7003, 
            Level = LogLevel.Error,
            Message = "ImportJudiciaryLeavers: {ErrorResponseMessage}")]
        public static partial void LogErrorImportJudiciaryLeaversMessage(this ILogger logger, string errorResponseMessage);
        
        [LoggerMessage(
            EventId = 7004, 
            Level = LogLevel.Error,
            Message = "ImportJudiciaryPeople: {ErrorResponseMessage}")]
        public static partial void LogErrorImportJudiciaryPeopleMessage(this ILogger logger, string errorResponseMessage);
        
        [LoggerMessage(
            EventId = 7005, 
            Level = LogLevel.Error,
            Message = "AllocateHearings: Error allocating hearing {hearingId}")]
        public static partial void LogErrorAllocateHearings(this ILogger logger, Exception exception, Guid hearingId);
        
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
        
        [LoggerMessage(
            EventId = 7008, 
            Level = LogLevel.Error,
            Message = "ReconcileAudiorecordingsWithConferencesAsync - missing wowza audio or empty files for conferences - {ItemName} Exception from Reconciliation - {Message}")]
        public static partial void LogErrorReconcileAudiorecordingsWithConferencesAsync(this ILogger logger, Exception exception, string itemName, string message);
        

    }
}