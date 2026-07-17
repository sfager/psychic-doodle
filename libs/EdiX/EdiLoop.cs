using System.Collections;
using System.Collections.Immutable;

namespace EdiX;

/// <summary>
/// Represents one iteration of a named loop within an EDI transaction.
/// Loops are logical groupings defined by schema — not encoded in the raw EDI file.
/// </summary>
public sealed class EdiLoop
{
    /// <summary>The loop identifier as defined in the transaction schema (e.g. <c>2000</c>, <c>SG1</c>).</summary>
    public string LoopId { get; }

    /// <summary>One-based iteration number of this loop instance within its parent.</summary>
    public int Iteration { get; }

    /// <summary>The segments contained directly within this loop iteration.</summary>
    public ImmutableArray<EdiSegment> Segments { get; }

    /// <summary>Child loops nested within this loop iteration.</summary>
    public EdiLoopCollection ChildLoops { get; }

    /// <summary>
    /// Finds the first segment with the given identifier within this loop's direct segment list.
    /// Does not search child loops. Case-insensitive.
    /// </summary>
    public EdiSegment? Segment(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var upper = id.ToUpperInvariant();
        foreach (var seg in Segments)
            if (seg.Id == upper) return seg;
        return null;
    }

    /// <summary>Returns all segments with the given identifier within this loop's direct segment list.</summary>
    public IEnumerable<EdiSegment> AllSegments(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var upper = id.ToUpperInvariant();
        return Segments.Where(s => s.Id == upper);
    }

    /// <exception cref="ArgumentException">Thrown when <paramref name="loopId"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="iteration"/> is less than 1.</exception>
    internal EdiLoop(string loopId, int iteration,
        ImmutableArray<EdiSegment> segments, EdiLoopCollection childLoops)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopId);
        ArgumentOutOfRangeException.ThrowIfLessThan(iteration, 1);
        LoopId     = loopId;
        Iteration  = iteration;
        Segments   = segments;
        ChildLoops = childLoops;
    }

    /// <inheritdoc/>
    public override string ToString() => $"Loop {LoopId} (iteration {Iteration})";
}

/// <summary>
/// An ordered, named collection of <see cref="EdiLoop"/> instances materialized from schema-aware parsing.
/// </summary>
public sealed class EdiLoopCollection : IEnumerable<EdiLoop>
{
    private readonly ImmutableArray<EdiLoop> _loops;

    internal EdiLoopCollection(ImmutableArray<EdiLoop> loops) => _loops = loops;

    /// <summary>Shared empty instance.</summary>
    internal static readonly EdiLoopCollection Empty = new(ImmutableArray<EdiLoop>.Empty);

    /// <summary>Returns all iterations of the loop with the given identifier. Empty array if not found.</summary>
    public ImmutableArray<EdiLoop> this[string loopId]
    {
        get
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(loopId);
            return _loops.Where(l => l.LoopId == loopId).ToImmutableArray();
        }
    }

    /// <summary>Returns the first iteration of the named loop.</summary>
    /// <exception cref="InvalidOperationException">Thrown when no loop with that ID exists.</exception>
    public EdiLoop First(string loopId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopId);
        return _loops.First(l => l.LoopId == loopId);
    }

    /// <summary>Returns the first iteration of the named loop, or <see langword="null"/> if not found.</summary>
    public EdiLoop? FirstOrDefault(string loopId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopId);
        return _loops.FirstOrDefault(l => l.LoopId == loopId);
    }

    /// <summary>Returns the first matching iteration, or <see langword="null"/> if none match.</summary>
    public EdiLoop? FirstOrDefault(string loopId, Func<EdiLoop, bool> predicate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(loopId);
        ArgumentNullException.ThrowIfNull(predicate);
        return _loops.FirstOrDefault(l => l.LoopId == loopId && predicate(l));
    }

    /// <summary>Total number of loop iterations across all loop IDs.</summary>
    public int Count => _loops.Length;

    /// <inheritdoc/>
    public IEnumerator<EdiLoop> GetEnumerator() =>
        ((IEnumerable<EdiLoop>)_loops).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}