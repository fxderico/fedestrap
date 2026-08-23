using System;

namespace Fedestrap.Exceptions;

internal class InvalidHTTPResponseException(string message) : Exception(message)
{
}
