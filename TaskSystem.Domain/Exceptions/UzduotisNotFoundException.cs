namespace TaskSystem.Domain.Exceptions;

public class UzduotisNotFoundException : Exception
{
    public UzduotisNotFoundException(int id)
        : base($"Task with id {id} was not found.") { }

    public UzduotisNotFoundException(string message)
        : base(message) { }
}
