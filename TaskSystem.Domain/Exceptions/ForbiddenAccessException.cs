namespace TaskSystem.Domain.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Access denied.")
        : base(message) { }
}
