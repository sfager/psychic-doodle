using System.Collections.Immutable;

namespace EdiX.Editing;

/// <summary>
/// Provides surgical, named-method editing of individual elements within an <see cref="EdiSegment"/>.
/// Obtain via <see cref="EdiDocument.EditSegment"/>.
/// </summary>
public sealed class EdiSegmentEditor
{
    private readonly string _id;
    private readonly int _position;
    private readonly List<EdiElement> _elements;

    internal EdiSegmentEditor(EdiSegment original)
    {
        _id       = original.Id;
        _position = original.Position;
        _elements = original.Elements.ToList();
    }

    /// <summary>
    /// Sets the value of the element at the given one-based position.
    /// Extends the element list with null elements if position is beyond the current end.
    /// </summary>
    public EdiSegmentEditor SetElement(int position, string? value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 1);
        EnsureCapacity(position);
        _elements[position - 1] = EdiElement.Simple(position, value);
        return this;
    }

    /// <summary>
    /// Sets the value of a single component within a composite element.
    /// Promotes the element to composite if it was previously simple.
    /// </summary>
    public EdiSegmentEditor SetComponent(int elementPosition, int componentPosition, string? value)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(elementPosition, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(componentPosition, 1);
        EnsureCapacity(elementPosition);

        var existing = _elements[elementPosition - 1];
        var components = existing.IsComposite
            ? existing.Components.ToList()
            : new List<EdiComponent>();

        while (components.Count < componentPosition)
            components.Add(new EdiComponent(components.Count + 1, null));

        components[componentPosition - 1] = new EdiComponent(componentPosition, value);
        _elements[elementPosition - 1] = EdiElement.Composite(elementPosition, components.ToImmutableArray());
        return this;
    }

    /// <summary>Clears the element at the given one-based position, setting its value to empty string.</summary>
    public EdiSegmentEditor ClearElement(int position) => SetElement(position, string.Empty);

    internal EdiSegment Build()
    {
        var renumbered = _elements
            .Select((e, idx) => e.IsComposite
                ? EdiElement.Composite(idx + 1, e.Components)
                : EdiElement.Simple(idx + 1, e.Value))
            .ToImmutableArray();
        return new EdiSegment(_id, _position, renumbered);
    }

    private void EnsureCapacity(int position)
    {
        while (_elements.Count < position)
            _elements.Add(EdiElement.Simple(_elements.Count + 1, null));
    }
}