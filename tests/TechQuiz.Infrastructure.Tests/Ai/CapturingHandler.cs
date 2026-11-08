using System.Net;
using System.Text;

namespace TechQuiz.Infrastructure.Tests.Ai;

/// <summary>
/// Fake <see cref="HttpMessageHandler"/> for provider tests: records the last request (and its
/// body) and answers every call with a canned body and status.
/// </summary>
internal sealed class CapturingHandler : HttpMessageHandler
{
    private string _body = "{}";
    private HttpStatusCode _status = HttpStatusCode.OK;

    public HttpRequestMessage? LastRequest { get; private set; }
    public string? LastRequestBody { get; private set; }

    public void RespondWith(string body, HttpStatusCode status = HttpStatusCode.OK)
    {
        _body = body;
        _status = status;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        LastRequestBody = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        return new HttpResponseMessage(_status)
        {
            Content = new StringContent(_body, Encoding.UTF8, "application/json"),
        };
    }
}
