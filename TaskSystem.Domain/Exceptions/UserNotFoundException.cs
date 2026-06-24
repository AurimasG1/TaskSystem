namespace TaskSystem.Domain.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(int id)
        : base($"User with id '{id}' was not found.") { }

    public UserNotFoundException(string email)
        : base($"User with email '{email}' was not found.") { }
}
