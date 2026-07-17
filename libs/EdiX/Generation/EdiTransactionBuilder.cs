using System.Collections.Immutable;

namespace EdiX.Generation;

/// <summary>
/// Builder for constructing EDI transaction sets with segments and loops.
/// </summary>
public sealed class EdiTransactionBuilder
{
    private readonly string _transactionType;
    private readonly List<EdiSegment> _segments = new();
    private readonly List<EdiLoop> _loops = new();

    internal EdiTransactionBuilder(string transactionType)
    {
        _transactionType = transactionType;
    }

    /// <summary>
    /// Adds a segment with the specified ID and element values.
    /// </summary>
    /// <param name="id">The segment ID.</param>
    /// <param name="elements">The element values.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiTransactionBuilder AddSegment(string id, params string[] elements)
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
    public EdiTransactionBuilder AddSegment(string id, Action<EdiSegmentBuilder> configure)
    {
        var builder = new EdiSegmentBuilder();
        configure(builder);
        _segments.Add(builder.Build(id, _segments.Count));
        return this;
    }

    /// <summary>
    /// Adds a loop using a builder configuration action.
    /// </summary>
    /// <param name="loopId">The loop ID.</param>
    /// <param name="configure">Action to configure the loop builder.</param>
    /// <returns>This builder for chaining.</returns>
    public EdiTransactionBuilder AddLoop(string loopId, Action<EdiLoopBuilder> configure)
    {
        // Count existing iterations of this loop ID
        var iteration = _loops.Count(l => l.LoopId.Equals(loopId, StringComparison.OrdinalIgnoreCase)) + 1;
        
        var loopBuilder = new EdiLoopBuilder(loopId, iteration);
        configure(loopBuilder);
        _loops.Add(loopBuilder.Build());
        return this;
    }

    internal EdiTransaction Build(EdiDialect dialect, string controlNumber)
    {
        // Build ST/SE or UNH/UNT envelope
        EdiSegment header, trailer;
        
        if (dialect == EdiDialect.X12)
        {
            // ST*850*0001
            header = new EdiSegmentBuilder()
                .AddElement(_transactionType)
                .AddElement(controlNumber)
                .Build("ST", 0);
            
            // SE includes ST and SE themselves in the count
            var segmentCount = _segments.Count + 2;
            trailer = new EdiSegmentBuilder()
                .AddElement(segmentCount.ToString())
                .AddElement(controlNumber)
                .Build("SE", segmentCount - 1);
        }
        else
        {
            // UNH+1+ORDERS:D:96A:UN
            header = new EdiSegmentBuilder()
                .AddElement(controlNumber)
                .AddCompositeElement(_transactionType.Split(':'))
                .Build("UNH", 0);
            
            var segmentCount = _segments.Count + 2;
            trailer = new EdiSegmentBuilder()
                .AddElement(segmentCount.ToString())
                .AddElement(controlNumber)
                .Build("UNT", segmentCount - 1);
        }

        return new EdiTransaction(
            _transactionType,
            header,
            trailer,
            _segments.ToImmutableArray(),
            new EdiLoopCollection(_loops.ToImmutableArray()),
            null  // SchemaKey
        );
    }
}