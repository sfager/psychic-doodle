using System.Collections.Immutable;

namespace EdiX.Schema;

/// <summary>
/// Defines the structure of a segment within a transaction.
/// </summary>
public sealed class EdiSegmentSchema
{
    /// <summary>
    /// Gets the segment identifier (e.g., "BEG", "REF").
    /// </summary>
    public string Id { get; }
    
    /// <summary>
    /// Gets the human-readable description of this segment.
    /// </summary>
    public string Description { get; }
    
    /// <summary>
    /// Gets whether this segment is mandatory, optional, or not used.
    /// </summary>
    public EdiUsage Usage { get; }
    
    /// <summary>
    /// Gets the maximum number of times this segment can repeat.
    /// </summary>
    public int MaxRepeat { get; }
    
    /// <summary>
    /// Gets the element schemas for this segment.
    /// </summary>
    public ImmutableArray<EdiElementSchema> Elements { get; }

    /// <summary>
    /// Initializes a new segment schema.
    /// </summary>
    /// <param name="id">The segment ID.</param>
    /// <param name="description">The description.</param>
    /// <param name="usage">The usage requirement.</param>
    /// <param name="maxRepeat">The maximum repetitions.</param>
    /// <param name="elements">The element schemas.</param>
    public EdiSegmentSchema(
        string id,
        string description,
        EdiUsage usage,
        int maxRepeat,
        ImmutableArray<EdiElementSchema> elements)
    {
        Id = id;
        Description = description;
        Usage = usage;
        MaxRepeat = maxRepeat;
        Elements = elements;
    }
}