using System.Collections.Immutable;

namespace EdiX;

/// <summary>
/// Represents a single EDI segment — the atomic unit of EDI structure.
/// Example: <c>BEG*00*SA*4500093444**20231015~</c>
/// </summary>
public sealed class EdiSegment
{
    /// <summary>The segment identifier (e.g. <c>BEG</c>, <c>N1</c>, <c>DTM</c>). Always uppercase.</summary>
    public string Id { get; }

    /// <summary>Zero-based ordinal position within the flat segment list of the parent <see cref="EdiTransaction"/>.</summary>
    public int Position { get; }

    /// <summary>The data elements contained in this segment.</summary>
    public ImmutableArray<EdiElement> Elements { get; }

    /// <summary>
    /// Retrieves an element by its one-based position.
    /// Returns <see cref="EdiElement.Empty"/> rather than throwing when the position is out of range.
    /// </summary>
    public EdiElement Element(int position)
    {
        if (position < 1 || position > Elements.Length)
            return EdiElement.Empty;

        return Elements[position - 1];
    }

    /// <summary>
    /// Returns the raw string value of the element at the given one-based position,
    /// or <see langword="null"/> if absent, empty, or composite.
    /// </summary>
    public string? ElementValue(int position) => Element(position).Value;

    /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is null or whitespace.</exception>
    internal EdiSegment(string id, int position, ImmutableArray<EdiElement> elements)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        Id       = id.ToUpperInvariant();
        Position = position;
        Elements = elements;
    }

    /// <inheritdoc/>
    public override string ToString() => Id;
}