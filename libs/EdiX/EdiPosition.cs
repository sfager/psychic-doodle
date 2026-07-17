namespace EdiX;

/// <summary>
/// Represents a position within the raw EDI source text.
/// Used in parse errors and validation errors.
/// </summary>
public readonly record struct EdiPosition
{
    /// <summary>Zero-based character offset from the start of the input.</summary>
    public long CharacterOffset { get; init; }

    /// <summary>Zero-based ordinal of the segment within the interchange.</summary>
    public int SegmentIndex { get; init; }

    /// <summary>Initializes a new <see cref="EdiPosition"/>.</summary>
    public EdiPosition(long characterOffset, int segmentIndex)
    {
        CharacterOffset = characterOffset;
        SegmentIndex    = segmentIndex;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        $"offset {CharacterOffset}, segment {SegmentIndex}";
}

/// <summary>
/// Uniquely identifies a segment's location within an <see cref="EdiDocument"/>.
/// Used as the targeting mechanism for surgical edits.
/// </summary>
/// <remarks>
/// All indices are zero-based.
/// Set <see cref="GroupIndex"/> to <c>-1</c> for EDIFACT transactions that
/// appear directly under the interchange without a UNG/UNE functional group.
/// </remarks>
public readonly record struct SegmentAddress
{
    /// <summary>Zero-based index of the functional group. Use <c>-1</c> for EDIFACT ungrouped transactions.</summary>
    public int GroupIndex { get; init; }

    /// <summary>Zero-based index of the transaction within the group.</summary>
    public int TransactionIndex { get; init; }

    /// <summary>Zero-based ordinal of the segment within the transaction's flat segment list.</summary>
    public int SegmentPosition { get; init; }

    /// <summary>Initializes a new <see cref="SegmentAddress"/>.</summary>
    public SegmentAddress(int groupIndex, int transactionIndex, int segmentPosition)
    {
        GroupIndex       = groupIndex;
        TransactionIndex = transactionIndex;
        SegmentPosition  = segmentPosition;
    }

    /// <inheritdoc/>
    public override string ToString() =>
        GroupIndex == -1
            ? $"tx[{TransactionIndex}]/seg[{SegmentPosition}]"
            : $"grp[{GroupIndex}]/tx[{TransactionIndex}]/seg[{SegmentPosition}]";
}

/// <summary>Controls where a segment is inserted relative to an address.</summary>
public enum InsertPosition
{
    /// <summary>Insert immediately before the addressed segment.</summary>
    Before,

    /// <summary>Insert immediately after the addressed segment.</summary>
    After
}