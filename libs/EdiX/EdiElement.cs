using System.Collections.Immutable;

namespace EdiX;

/// <summary>
/// Represents a data element within an <see cref="EdiSegment"/>.
/// An element is either simple (a single value) or composite (a collection of <see cref="EdiComponent"/> values).
/// </summary>
public sealed class EdiElement
{
    /// <summary>Shared sentinel returned by the safe <c>Element(n)</c> accessor for out-of-range positions.</summary>
    internal static readonly EdiElement Empty = new(0, null, ImmutableArray<EdiComponent>.Empty);

    /// <summary>One-based position of this element within its parent segment.</summary>
    public int Position { get; }

    /// <summary>The raw string value for a simple element, or <see langword="null"/> when composite or absent.</summary>
    public string? Value { get; }

    /// <summary>The components of a composite element. Empty when this element is simple.</summary>
    public ImmutableArray<EdiComponent> Components { get; }

    /// <summary>Returns <see langword="true"/> when this element contains sub-components.</summary>
    public bool IsComposite => !Components.IsEmpty;

    /// <summary>Returns <see langword="true"/> when this element has a non-empty value or any component has a value.</summary>
    public bool HasValue => IsComposite
        ? Components.Any(c => c.HasValue)
        : !string.IsNullOrEmpty(Value);

    /// <summary>
    /// Retrieves a component by its one-based position.
    /// Returns an empty component rather than throwing when the position is out of range.
    /// </summary>
    public EdiComponent Component(int position)
    {
        if (position < 1 || position > Components.Length)
            return new EdiComponent(position < 1 ? 1 : position, null);

        return Components[position - 1];
    }

    internal EdiElement(int position, string? value, ImmutableArray<EdiComponent> components)
    {
        Position   = position;
        Value      = value;
        Components = components;
    }

    internal static EdiElement Simple(int position, string? value) =>
        new(position, value, ImmutableArray<EdiComponent>.Empty);

    internal static EdiElement Composite(int position, ImmutableArray<EdiComponent> components) =>
        new(position, null, components);

    /// <inheritdoc/>
    public override string ToString() =>
        IsComposite
            ? string.Join(":", Components.Select(c => c.Value ?? string.Empty))
            : Value ?? string.Empty;
}