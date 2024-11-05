using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SchedulerJobs.Common.Exceptions;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services.HttpClients;

namespace SchedulerJobs.Services.UnitTests.HttpClients;

public class LeaversClientTests : HttpClientTests
{
    private LeaversClient _leaversClient;
    
    [SetUp]
    public void Setup()
    {
        SetUp();
        _leaversClient = new LeaversClient(HttpClient) { BaseUrl = "https://test.com" };
    }

    [Test]
    public async Task GetLeaversAsync_ReturnsLeaversResponse_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedResponse = new LeaversResponse
        {
            Pagination = new Pagination
            {
                CurrentPage = 1,
                ResultsPerPage = 100, 
                Results = 1
            },
            Results = new List<JudiciaryLeaverModel>
            {
                new()
                {
                    Id = "123",
                    PersonalCode = "123456",
                    LeftOn = "2024-01-01",
                    Leaver = true
                }
            }
        };
        SetUpHttpMessage(HttpStatusCode.OK, expectedResponse);
        var updatedSince = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _leaversClient.GetLeaversAsync(updatedSince);

        // Assert
        result.Should().BeEquivalentTo(expectedResponse);
        VerifyApiCalled(updatedSince);
    }

    [Test]
    public void GetLeaversAsync_ThrowsException_WhenResponseIsUnsuccessful()
    {
        // Arrange
        SetUpHttpMessage(HttpStatusCode.BadRequest);
        var updatedSince = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.ThrowsAsync<ELinksApiException>(() => _leaversClient.GetLeaversAsync(updatedSince));
        VerifyApiCalled(updatedSince);
    }

    private void VerifyApiCalled(DateTime updatedSince)
    {
        var expectedUrl = $"{_leaversClient.BaseUrl}/leavers?left_since={updatedSince:yyyy-MM-dd}&page=1&per_page=100";
        
        VerifyApiCalled(expectedUrl);
    }
}