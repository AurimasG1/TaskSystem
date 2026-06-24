namespace TaskSystem.Domain.Exceptions;

public class UserAlreadyExistsException : Exception
{
    public UserAlreadyExistsException(string email)
        : base($"User with email '{email}' already exists.") { }

    public UserAlreadyExistsException(string message, Exception? inner = null)
        : base(message, inner) { }
}
