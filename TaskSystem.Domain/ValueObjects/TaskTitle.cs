namespace TaskSystem.Domain.ValueObjects;

public sealed class TaskTitle
{
    public string Value { get; }

    private TaskTitle(string value)
    {
        Value = value;
    }

    public static TaskTitle Create(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Task title cannot be empty");

        if (title.Length > 100)
            throw new ArgumentException("Task title is too long");

        return new TaskTitle(title.Trim());
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => obj is TaskTitle other && Value == other.Value;

    public override int GetHashCode() => Value.GetHashCode();
}
