namespace SchedulerJobs.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static partial class JudiciaryPeopleLoggerAdapter
    {
        // Information messages
        
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
            EventId = 6015, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryPeople: Adding raw data to JudiciaryPersonStaging from page: {CurrentPage}, total records: {Records}")]
        public static partial void LogInformationImportJudiciaryAddingToJudiciaryPersonStaging(this ILogger logger, int currentPage, int records);

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
            EventId = 8003, 
            Level = LogLevel.Warning,
            Message = "ImportJudiciaryPeople: No results from api for page: {CurrentPage}")]
        public static partial void LogWarningImportJudiciaryPeopleCountFromApi(this ILogger logger, int currentPage);

        // Debug messages

        [LoggerMessage(
            EventId = 9000, 
            Level = LogLevel.Debug,
            Message = "ImportJudiciaryPeople: using stub")]
        public static partial void LogDebugImportJudiciaryPeopleUsingStub(this ILogger logger);

        // Error messages

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
        
    }
}