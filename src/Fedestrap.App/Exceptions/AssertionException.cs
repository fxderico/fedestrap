using System;

namespace Fedestrap.Exceptions;

internal class AssertionException(string message) : Exception(message + "\n\nThis is very likely just an off-chance error. Please report this first, and then start Fedestrap again.")
{
}
