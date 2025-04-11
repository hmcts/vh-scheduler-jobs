namespace SchedulerJobs.Common.Logging
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Extensions.Logging;

    public static partial class JudiciaryLeaversLoggerAdapter
    {
        // Information messages
        
        [LoggerMessage(
            EventId = 6016, 
            Level = LogLevel.Information,
            Message = "ImportJudiciaryLeavers: Calling bookings API with '{leaversResultCount}' leavers")]
        public static partial void LogInformationImportJudiciaryLeaversCallingApi(this ILogger logger, int leaversResultCount);

        [LoggerMessage(
            EventId = 6020, 
            Level = LogLevel.Information,
            Message = "No judiciary persons leaving: using stub")]
        public static partial void LogInformationJudiciaryPeopleLeavingStub(this ILogger logger);

        // Warning messages

        [LoggerMessage(
            EventId = 8002, 
            Level = LogLevel.Warning,
            Message = "ImportJudiciaryLeavers: No of leavers who are invalid '{invalidCount}' in page '{currentPage}'.")]
        public static partial void LogWarningImportJudiciaryPeopleInvalidPeopleCount(this ILogger logger, int invalidCount, int currentPage);
        
        // Error messages

        [LoggerMessage(
            EventId = 7002, 
            Level = LogLevel.Error,
            Message = "There was a problem importing judiciary leavers in page: '{currentPage}'")]
        public static partial void LogErrorImportJudiciaryLeaversException(this ILogger logger, Exception exception, int currentPage);

    }
}