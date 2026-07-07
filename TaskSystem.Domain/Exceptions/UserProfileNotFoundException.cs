namespace TaskSystem.Domain.Exceptions;

public class UserProfileNotFoundException : Exception
{
    public UserProfileNotFoundException(int id)
        : base($"User profile with id {id} was not found.") { }

    public UserProfileNotFoundException(string message)
        : base(message) { }
}
