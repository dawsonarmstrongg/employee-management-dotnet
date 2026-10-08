using System.Net;

namespace EmployeeManagement.Tests.Components;

// A small hand-written fake HttpMessageHandler (per the ground rules: no Moq/mocking libraries).
// Returns a fixed response (or runs a custom responder) for every request, so the Blazor
// component under test never makes a real network call.
public sealed class FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder) : HttpMessageHandler
{
    public HttpRequestMessage? LastRequest { get; private set; }

    public static FakeHttpMessageHandler ReturningJson(HttpStatusCode statusCode, string json) =>
        new(_ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
        });

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        return Task.FromResult(responder(request));
    }
}
