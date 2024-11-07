using System;
using System.Linq;

namespace SchedulerJobs.Services.Extensions
{
    public static class StringExtensions
    {
        public static string Capitalise(this string input) =>
            input switch
            {
                null => throw new ArgumentNullException(nameof(input)),
                "" => throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input)),
                _ => string.Concat(input.First().ToString().ToUpper(), input.AsSpan(1))
            };
    }
}