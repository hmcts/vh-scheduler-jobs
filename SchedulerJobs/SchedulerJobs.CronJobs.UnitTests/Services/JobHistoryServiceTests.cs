using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BookingsApi.Client;
using BookingsApi.Contract.Responses;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SchedulerJobs.Services;

namespace SchedulerJobs.CronJobs.UnitTests.Services
{
    public class JobHistoryServiceTests
    {
        private IJobHistoryService _jobHistoryService;
        private Mock<IBookingsApiClient> _bookingApiClient;
        private const string JobName = "testJobName";
        [SetUp]
        public void Setup()
        {
            _bookingApiClient = new Mock<IBookingsApiClient>();
            _jobHistoryService = new JobHistoryService(_bookingApiClient.Object);
        }

        [Test]
        public async Task Calls_update_job_history_end_point()
        {
            await _jobHistoryService.UpdateJobHistory(JobName, true);
            _bookingApiClient.Verify(e => e.UpdateJobHistoryAsync(JobName, true), Times.Once);
        }

        [Test] 
        public async Task Retrieves_job_history_and_return_latest_successful_run()
        {
            var mockDate = DateTime.UtcNow;
            var mockReturnedList = new List<JobHistoryResponse>()
            {
                new JobHistoryResponse(JobName, mockDate.AddDays(-1), false),
                new JobHistoryResponse(JobName, mockDate.AddDays(-3), false),
                new JobHistoryResponse(JobName, mockDate.AddDays(-4), true), //should return this one
                new JobHistoryResponse(JobName, mockDate.AddDays(-10), true),
                new JobHistoryResponse(JobName, null, true)
            };
            _bookingApiClient.Setup(e => e.GetJobHistoryAsync(JobName)).ReturnsAsync(mockReturnedList);
            var result = await _jobHistoryService.GetMostRecentSuccessfulRunDate(JobName);
            _bookingApiClient.Verify(e => e.GetJobHistoryAsync(JobName), Times.Once);
            result.Should().HaveValue();
            result.Should().Be(mockDate.AddDays(-4));
        }
    }
}