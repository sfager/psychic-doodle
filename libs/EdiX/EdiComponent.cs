namespace EdiX;

/// <summary>
/// Represents a single component within a composite data element.
/// </summary>
public sealed class EdiComponent
{
    /// <summary>One-based position of this component within its parent element.</summary>
    public int Position { get; }

    /// <summary>The raw string value, or <see langword="null"/> if the component was not present.</summary>
    public string? Value { get; }

    /// <summary>Returns <see langword="true"/> when <see cref="Value"/> is non-null and non-empty.</summary>
    public bool HasValue => !string.IsNullOrEmpty(Value);

    /// <param name="position">One-based position within the parent element.</param>
    /// <param name="value">Raw string value, or <see langword="null"/> if absent.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="position"/> is less than 1.</exception>
    internal EdiComponent(int position, string? value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 1);
        Position = position;
        Value    = value;
    }

    /// <inheritdoc/>
    public override string ToString() => Value ?? string.Empty;
}