using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SchedulerJobs.Common.ApiHelper;

namespace SchedulerJobs.Services.UnitTests.HttpClients;

public abstract class HttpClientTests
{
    protected HttpClient HttpClient;
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;

    [SetUp]
    public void SetUp()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        HttpClient = new HttpClient(_httpMessageHandlerMock.Object);
    }
    
    protected void SetUpHttpMessage(HttpStatusCode statusCode, object response = null)
    {
        var httpResponseMessage = new HttpResponseMessage(statusCode);
        if (response != null)
            httpResponseMessage.Content = new StringContent(ApiRequestHelper.SerialiseRequestToSnakeCaseJson(response));
        
        SetUpHttpMessageHandler(httpResponseMessage);
    }
    
    private void SetUpHttpMessageHandler(HttpResponseMessage httpResponseMessage)
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponseMessage);
    }
    
    protected void VerifyApiCalled(string expectedUrl)
    {
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get && req.RequestUri.ToString() == expectedUrl),
                ItExpr.IsAny<CancellationToken>()
            );
    }
}