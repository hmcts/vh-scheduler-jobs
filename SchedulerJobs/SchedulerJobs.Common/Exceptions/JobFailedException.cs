using System;

namespace SchedulerJobs.Common.Exceptions;

public class JobFailedException(string message, Exception innerException) : Exception(message, innerException);