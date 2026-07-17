using System.Collections.Immutable;

namespace EdiX.Schema;

/// <summary>
/// Defines a repeatable loop structure within a transaction.
/// </summary>
public sealed class EdiLoopSchema
{
    /// <summary>
    /// Gets the loop identifier (e.g., "N1", "LX").
    /// </summary>
    public string LoopId { get; }
    
    /// <summary>
    /// Gets the segment ID whose appearance triggers a new loop iteration.
    /// </summary>
    public string TriggerSegmentId { get; }
    
    /// <summary>
    /// Gets the maximum number of times this loop can repeat.
    /// </summary>
    public int MaxRepeat { get; }
    
    /// <summary>
    /// Gets the segment IDs that may appear in this loop.
    /// </summary>
    public ImmutableArray<string> SegmentIds { get; }
    
    /// <summary>
    /// Gets the child loops that may be nested within this loop.
    /// </summary>
    public ImmutableArray<EdiLoopSchema> ChildLoops { get; }

    /// <summary>
    /// Initializes a new loop schema.
    /// </summary>
    /// <param name="loopId">The loop identifier.</param>
    /// <param name="triggerSegmentId">The trigger segment ID.</param>
    /// <param name="maxRepeat">The maximum repetitions.</param>
    /// <param name="segmentIds">The segment IDs.</param>
    /// <param name="childLoops">The child loops.</param>
    public EdiLoopSchema(
        string loopId,
        string triggerSegmentId,
        int maxRepeat,
        ImmutableArray<string> segmentIds,
        ImmutableArray<EdiLoopSchema> childLoops)
    {
        LoopId = loopId;
        TriggerSegmentId = triggerSegmentId;
        MaxRepeat = maxRepeat;
        SegmentIds = segmentIds;
        ChildLoops = childLoops;
    }
}