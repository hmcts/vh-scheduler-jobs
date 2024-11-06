using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SchedulerJobs.Common.ApiHelper;
using SchedulerJobs.Common.Exceptions;
using SchedulerJobs.Common.Models;
using SchedulerJobs.Services.HttpClients;

namespace SchedulerJobs.Services.UnitTests.HttpClients;

public class PeoplesClientTests : HttpClientTests
{
    private PeoplesClient _peoplesClient;
    
    [SetUp]
    public void Setup()
    {
        SetUp();
        _peoplesClient = new PeoplesClient(HttpClient) { BaseUrl = "https://test.com" };
    }

    [Test]
    public async Task GetPeopleJsonAsync_ReturnsResponse_WhenResponseIsSuccessful()
    {
        // Arrange
        var expectedResponse = new PeopleResponse
        {
            Pagination = new Pagination
            {
                CurrentPage = 1,
                ResultsPerPage = 100, 
                Results = 1
            },
            Results = new List<JudiciaryPersonModel>
            {
                new()
                {
                    Id = "123",
                    PersonalCode = "123456",
                    Title = "Title",
                    KnownAs = "KnownAs",
                    Surname = "Surname"
                }
            }
        };
        var expectedResponseJson = ApiRequestHelper.SerialiseRequestToSnakeCaseJson(expectedResponse);
        SetUpHttpMessage(HttpStatusCode.OK, expectedResponse);
        var updatedSince = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _peoplesClient.GetPeopleJsonAsync(updatedSince);

        // Assert
        result.Should().BeEquivalentTo(expectedResponseJson);
        VerifyApiCalled(updatedSince);
    }

    [Test]
    public void GetPeopleJsonAsync_ThrowsException_WhenResponseIsUnsuccessful()
    {
        // Arrange
        SetUpHttpMessage(HttpStatusCode.BadRequest);
        var updatedSince = new DateTime(2024, 01, 01, 0, 0, 0, DateTimeKind.Utc);

        // Act & Assert
        Assert.ThrowsAsync<ELinksApiException>(() => _peoplesClient.GetPeopleJsonAsync(updatedSince));
        VerifyApiCalled(updatedSince);
    }

    private void VerifyApiCalled(DateTime updatedSince)
    {
        var expectedUrl =
            $"{_peoplesClient.BaseUrl}/people?updated_since={updatedSince:yyyy-MM-dd}&page=1&per_page=100&include_previous_appointments=true";
        
        VerifyApiCalled(expectedUrl);
    }
}