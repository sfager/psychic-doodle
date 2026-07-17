using System.Collections.Immutable;

namespace EdiX.Generation;

/// <summary>
/// Builder for constructing EDI loops with segments and nested child loops.
/// </summary>
public sealed class EdiLoopBuilder
{
    private readonly string _loopId;
    private readonly int _iteration;
    private readonly List<EdiSegment> _segments = new();
    private readonly List<EdiLoop> _childLoops = new();

    internal EdiLoopBuilder(string loopId, int iteration)
    {
        _loopId = loopId;
        _iteration = iteration;
    }

    /// <summary>
    /// Adds a segment with the specified ID and element values.
    /// </summary>
    /// <param name="id">The segment ID.</param>
    /// <param name="elements">The element values.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiLoopBuilder AddSegment(string id, params string[] elements)
    {
        var builder = new EdiSegmentBuilder();
        foreach (var elem in elements)
        {
            builder.AddElement(elem);
        }
        _segments.Add(builder.Build(id, _segments.Count));
        return this;
    }

    /// <summary>
    /// Adds a segment using a builder configuration action.
    /// </summary>
    /// <param name="id">The segment ID.</param>
    /// <param name="configure">Action to configure the segment builder.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiLoopBuilder AddSegment(string id, Action<EdiSegmentBuilder> configure)
    {
        var builder = new EdiSegmentBuilder();
        configure(builder);
        _segments.Add(builder.Build(id, _segments.Count));
        return this;
    }

    /// <summary>
    /// Adds a nested child loop using a builder configuration action.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="configure">Action to configure the loop builder.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiLoopBuilder AddLoop(string loopId, Action<EdiLoopBuilder> configure)
    {
        // Count existing iterations of this loop ID
        var iteration = _childLoops.Count(l => l.LoopId.Equals(loopId, StringComparison.OrdinalIgnoreCase)) + 1;
        
        var loopBuilder = new EdiLoopBuilder(loopId, iteration);
        configure(loopBuilder);
        _childLoops.Add(loopBuilder.Build());
        return this;
    }

    internal EdiLoop Build()
    {
        return new EdiLoop(_loopId, _iteration, _segments.ToImmutableArray(), 
            new EdiLoopCollection(_childLoops.ToImmutableArray()));
    }
}