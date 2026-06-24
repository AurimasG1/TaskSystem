namespace TaskSystem.Domain.Exceptions;

public class InvalidStatusException : Exception
{
    public InvalidStatusException(string status)
        : base($"Status '{status}' is invalid.") { }
}
