using System;
using System.Net;

namespace SchedulerJobs.Common.Exceptions
{
#pragma warning disable S3925 // "ISerializable" should be implemented correctly
    public class ELinksApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        
        public ELinksApiException(string message, HttpStatusCode statusCode) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}