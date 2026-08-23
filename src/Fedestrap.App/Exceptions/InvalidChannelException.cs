using System;
using System.Net;

namespace Fedestrap.Exceptions;

public class InvalidChannelException(HttpStatusCode? statusCode) : Exception
{
	public HttpStatusCode? StatusCode = statusCode;
}
