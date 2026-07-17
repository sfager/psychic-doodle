using System.Collections.Immutable;

namespace EdiX;

/// <summary>
/// Represents a functional group envelope within an EDI interchange (X12 GS/GE or EDIFACT UNG/UNE).
/// </summary>
public sealed class EdiFunctionalGroup
{
    /// <summary>The EDI dialect of the parent interchange.</summary>
    public EdiDialect Dialect { get; }

    /// <summary>The raw envelope header segment (X12: <c>GS</c>, EDIFACT: <c>UNG</c>).</summary>
    public EdiSegment HeaderSegment { get; }

    /// <summary>The raw envelope trailer segment (X12: <c>GE</c>, EDIFACT: <c>UNE</c>).</summary>
    public EdiSegment TrailerSegment { get; }

    /// <summary>The transactions contained within this functional group.</summary>
    public ImmutableArray<EdiTransaction> Transactions { get; }

    /// <summary>Returns a typed accessor for the X12 GS group header fields.</summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Dialect"/> is not X12.</exception>
    public X12.X12GroupHeader AsX12GroupHeader =>
        Dialect == EdiDialect.X12
            ? new X12.X12GroupHeader(HeaderSegment)
            : throw new InvalidOperationException(
                "Cannot access X12 group header: interchange dialect is EDIFACT.");

    /// <summary>Returns a typed accessor for the EDIFACT UNG group header fields.</summary>
    /// <exception cref="InvalidOperationException">Thrown when <see cref="Dialect"/> is not EDIFACT.</exception>
    public Edifact.EdifactGroupHeader AsEdifactGroupHeader =>
        Dialect == EdiDialect.Edifact
            ? new Edifact.EdifactGroupHeader(HeaderSegment)
            : throw new InvalidOperationException(
                "Cannot access EDIFACT group header: interchange dialect is X12.");

    internal EdiFunctionalGroup(
        EdiDialect dialect,
        EdiSegment headerSegment,
        EdiSegment trailerSegment,
        ImmutableArray<EdiTransaction> transactions)
    {
        ArgumentNullException.ThrowIfNull(headerSegment);
        ArgumentNullException.ThrowIfNull(trailerSegment);
        Dialect        = dialect;
        HeaderSegment  = headerSegment;
        TrailerSegment = trailerSegment;
        Transactions   = transactions;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"FunctionalGroup ({Transactions.Length} transaction(s))";
}