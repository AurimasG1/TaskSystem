namespace TaskSystem.Domain.Exceptions;

public class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string message)
        : base(message) { }
}
