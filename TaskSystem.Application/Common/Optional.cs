public readonly struct Optional<T>
{
    public bool HasValue { get; }
    public T? Value { get; }

    private Optional(T? value, bool hasValue)
    {
        Value = value;
        HasValue = hasValue;
    }

    public static Optional<T> Some(T? value) => new Optional<T>(value, true);

    public static Optional<T> None() => new Optional<T>(default!, false);

    public static Optional<T> FromNullable(T? value) => value is null ? None() : Some(value);

    public static Optional<T> From(T? value) => Some(value);

    public static implicit operator Optional<T>(T value) => Some(value);
}
