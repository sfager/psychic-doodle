using System.Collections.Immutable;
using EdiX.Schema;

namespace EdiX.Schemas.X12;

/// <summary>
/// X12 850 Purchase Order - Version 004010
/// </summary>
public static class X12_850_004010
{
    /// <summary>
    /// Gets the schema for X12 850 version 004010.
    /// </summary>
    public static EdiTransactionSchema Schema { get; } = BuildSchema();

    private static EdiTransactionSchema BuildSchema()
    {
        // N1 Loop - Party Identification
        var n1Loop = new EdiLoopSchema(
            loopId: "N1",
            triggerSegmentId: "N1",
            maxRepeat: 200,
            segmentIds: ImmutableArray.Create("N1", "N2", "N3", "N4", "REF", "PER"),
            childLoops: ImmutableArray<EdiLoopSchema>.Empty);

        // PO1 Loop - Baseline Item Data
        var po1Loop = new EdiLoopSchema(
            loopId: "PO1",
            triggerSegmentId: "PO1",
            maxRepeat: 100000,
            segmentIds: ImmutableArray.Create("PO1", "CUR", "PO3", "CTP", "PID", "MEA", "PWK", "PKG", "TD1", "TD5", "TD3", "TD4", "MAN", "TXI", "CTB", "QTY", "SCH", "PKG", "LS", "LDT", "QTY", "MSG", "REF", "PER", "SAC", "IT8", "CSH", "ITD", "DIS", "INC", "TAX", "FOB", "SDQ", "IT3", "N9", "MTX", "LM", "V1", "FA1", "FA2"),
            childLoops: ImmutableArray<EdiLoopSchema>.Empty);

        // Transaction-level loops
        var transactionLoops = ImmutableArray.Create(n1Loop, po1Loop);

        // For built-in schemas, we focus on loop structure
        // Detailed element validation can be added later
        var transactionSegments = ImmutableArray<EdiSegmentSchema>.Empty;

        return new EdiTransactionSchema(
            key: EdiSchemaKey.ForX12("850", "004", "010"),
            description: "Purchase Order",
            loops: transactionLoops,
            segments: transactionSegments
        );
    }
}
