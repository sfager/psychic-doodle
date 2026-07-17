using System.Collections.Immutable;

namespace EdiX.Generation;

/// <summary>
/// Builder for constructing EDI segments with elements and composite elements.
/// </summary>
public sealed class EdiSegmentBuilder
{
    private readonly List<EdiElement> _elements = new();

    /// <summary>
    /// Adds a simple element with the specified value.
    /// </summary>
    /// <param name="value">The element value (null for empty element).</param>
    /// <returns>This builder for chaining.</returns>
    public EdiSegmentBuilder AddElement(string? value)
    {
        var position = _elements.Count + 1;
        _elements.Add(new EdiElement(position, value, ImmutableArray<EdiComponent>.Empty));
        return this;
    }

    /// <summary>
    /// Adds a composite element with multiple components.
    /// </summary>
    /// <param name="components">The component values.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiSegmentBuilder AddCompositeElement(params string?[] components)
    {
        var position = _elements.Count + 1;
        var comps = components.Select((c, i) => new EdiComponent(i + 1, c)).ToImmutableArray();
        _elements.Add(new EdiElement(position, null, comps));
        return this;
    }

    /// <summary>
    /// Adds an empty element (null value).
    /// </summary>
    /// <returns>This builder for chaining.</returns>
    public EdiSegmentBuilder AddEmpty()
    {
        return AddElement(null);
    }

    /// <summary>
    /// Builds the segment.
    /// </summary>
    internal EdiSegment Build(string id, int position)
    {
        return new EdiSegment(id, position, _elements.ToImmutableArray());
    }
}