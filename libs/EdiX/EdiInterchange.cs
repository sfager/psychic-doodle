using System.Collections.Immutable;

namespace EdiX;

/// <summary>
/// Represents the outermost envelope of an EDI interchange (X12 ISA/IEA or EDIFACT UNB/UNZ).
/// </summary>
public sealed class EdiInterchange
{
    /// <summary>The EDI dialect of this interchange.</summary>
    public EdiDialect Dialect { get; }

    /// <summary>The delimiters detected or configured for this interchange.</summary>
    public EdiDelimiters Delimiters { get; }

    /// <summary>The raw envelope header segment (X12: <c>ISA</c>, EDIFACT: <c>UNB</c>).</summary>
    public EdiSegment HeaderSegment { get; }

    /// <summary>The raw envelope trailer segment (X12: <c>IEA</c>, EDIFACT: <c>UNZ</c>).</summary>
    public EdiSegment TrailerSegment { get; }

    /// <summary>
    /// The functional groups in this interchange.
    /// For X12 always present; for EDIFACT empty when transactions are ungrouped.
    /// </summary>
    public ImmutableArray<EdiFunctionalGroup> Groups { get; }

    /// <summary>Transactions directly under the interchange without a group. EDIFACT only; always empty for X12.</summary>
    public ImmutableArray<EdiTransaction> Transactions { get; }

    /// <summary>Returns a typed accessor for the X12 ISA interchange header fields.</summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Dialect"/> is not X12.</exception>
    public X12.X12InterchangeHeader AsX12Header =>
        Dialect == EdiDialect.X12
            ? new X12.X12InterchangeHeader(HeaderSegment)
            : throw new InvalidOperationException(
                "Cannot access X12 interchange header: interchange dialect is EDIFACT.");

    /// <summary>Returns a typed accessor for the EDIFACT UNB interchange header fields.</summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Dialect"/> is not EDIFACT.</exception>
    public Edifact.EdifactInterchangeHeader AsEdifactHeader =>
        Dialect == EdiDialect.Edifact
            ? new Edifact.EdifactInterchangeHeader(HeaderSegment)
            : throw new InvalidOperationException(
                "Cannot access EDIFACT interchange header: interchange dialect is X12.");

    internal EdiInterchange(
        EdiDialect dialect,
        EdiDelimiters delimiters,
        EdiSegment headerSegment,
        EdiSegment trailerSegment,
        ImmutableArray<EdiFunctionalGroup> groups,
        ImmutableArray<EdiTransaction> transactions)
    {
        ArgumentNullException.ThrowIfNull(headerSegment);
        ArgumentNullException.ThrowIfNull(trailerSegment);
        Dialect        = dialect;
        Delimiters     = delimiters;
        HeaderSegment  = headerSegment;
        TrailerSegment = trailerSegment;
        Groups         = groups;
        Transactions   = transactions;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"{Dialect} Interchange ({Groups.Length} group(s), {Transactions.Length} ungrouped transaction(s))";
}
