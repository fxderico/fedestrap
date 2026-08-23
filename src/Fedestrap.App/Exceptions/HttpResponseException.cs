using System;
using System.Net.Http;

namespace Fedestrap.Exceptions;

internal class HttpResponseException(HttpResponseMessage responseMessage) : Exception($"Could not connect to {responseMessage.RequestMessage.RequestUri} because it returned HTTP {(int)responseMessage.StatusCode} ({responseMessage.ReasonPhrase})")
{
	public HttpResponseMessage ResponseMessage { get; } = responseMessage;
}
