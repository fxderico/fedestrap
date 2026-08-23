using System;

namespace Fedestrap.Exceptions;

internal class ChecksumFailedException(string message) : Exception(message)
{
}
