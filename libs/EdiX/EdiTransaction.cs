using System.Collections.Immutable;

namespace EdiX;

/// <summary>
/// Represents a single EDI transaction set (X12 ST/SE) or message (EDIFACT UNH/UNT).
/// </summary>
public sealed class EdiTransaction
{
    /// <summary>
    /// The transaction type identifier.
    /// X12: the transaction set ID from ST01 (e.g. <c>850</c>).
    /// EDIFACT: the message type from UNH02-1 (e.g. <c>ORDERS</c>).
    /// </summary>
    public string TransactionType { get; }

    /// <summary>The schema key resolved during parsing. <see langword="null"/> when no schema was matched.</summary>
    public object? SchemaKey { get; }  // typed as object? until Phase 2 adds EdiSchemaKey

    /// <summary>The raw envelope header segment (X12 ST, EDIFACT UNH).</summary>
    public EdiSegment HeaderSegment { get; }

    /// <summary>The raw envelope trailer segment (X12 SE, EDIFACT UNT).</summary>
    public EdiSegment TrailerSegment { get; }

    /// <summary>
    /// The flat, ordered list of body segments, excluding the envelope header and trailer.
    /// Always populated regardless of whether a schema was provided.
    /// </summary>
    public ImmutableArray<EdiSegment> Segments { get; }

    /// <summary>The schema-materialized loop tree. <see langword="null"/> when no schema was provided.</summary>
    public EdiLoopCollection? Loops { get; }

    /// <summary>Finds the first segment with the given identifier. Case-insensitive.</summary>
    public EdiSegment? Segment(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var upper = id.ToUpperInvariant();
        foreach (var seg in Segments)
            if (seg.Id == upper) return seg;
        return null;
    }

    /// <summary>Finds the <paramref name="occurrence"/>-th (one-based) segment with the given identifier.</summary>
    public EdiSegment? Segment(string id, int occurrence)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentOutOfRangeException.ThrowIfLessThan(occurrence, 1);
        var upper = id.ToUpperInvariant();
        int count = 0;
        foreach (var seg in Segments)
            if (seg.Id == upper && ++count == occurrence) return seg;
        return null;
    }

    /// <summary>Returns all segments with the given identifier.</summary>
    public IEnumerable<EdiSegment> AllSegments(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        var upper = id.ToUpperInvariant();
        return Segments.Where(s => s.Id == upper);
    }

    internal EdiTransaction(
        string transactionType,
        EdiSegment headerSegment,
        EdiSegment trailerSegment,
        ImmutableArray<EdiSegment> segments,
        EdiLoopCollection? loops,
        object? schemaKey = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionType);
        ArgumentNullException.ThrowIfNull(headerSegment);
        ArgumentNullException.ThrowIfNull(trailerSegment);
        TransactionType = transactionType;
        HeaderSegment   = headerSegment;
        TrailerSegment  = trailerSegment;
        Segments        = segments;
        Loops           = loops;
        SchemaKey       = schemaKey;
    }

    /// <inheritdoc/>
    public override string ToString() => $"Transaction {TransactionType}";
}