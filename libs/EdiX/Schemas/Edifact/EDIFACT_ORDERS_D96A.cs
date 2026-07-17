using System.Collections.Immutable;
using EdiX.Schema;

namespace EdiX.Schemas.Edifact;

/// <summary>
/// EDIFACT ORDERS Purchase Order Message - Version D:96A
/// </summary>
public static class EDIFACT_ORDERS_D96A
{
    /// <summary>
    /// Gets the schema for EDIFACT ORDERS version D:96A.
    /// </summary>
    public static EdiTransactionSchema Schema { get; } = BuildSchema();

    private static EdiTransactionSchema BuildSchema()
    {
        // SG1 - Reference (RFF)
        var sg1Loop = new EdiLoopSchema(
            loopId: "SG1",
            triggerSegmentId: "RFF",
            maxRepeat: 10,
            segmentIds: ImmutableArray.Create("RFF", "DTM"),
            childLoops: ImmutableArray<EdiLoopSchema>.Empty);

        // SG2 - Name and Address (NAD)
        var sg3SubLoop = new EdiLoopSchema(
            loopId: "SG3",
            triggerSegmentId: "RFF",
            maxRepeat: 10,
            segmentIds: ImmutableArray.Create("RFF", "DTM"),
            childLoops: ImmutableArray<EdiLoopSchema>.Empty);

        var sg2Loop = new EdiLoopSchema(
            loopId: "SG2",
            triggerSegmentId: "NAD",
            maxRepeat: 99,
            segmentIds: ImmutableArray.Create("NAD", "LOC", "FII", "RFF", "DOC", "CTA"),
            childLoops: ImmutableArray.Create(sg3SubLoop));

        // SG25 - Line Item (LIN)
        var sg25Loop = new EdiLoopSchema(
            loopId: "SG25",
            triggerSegmentId: "LIN",
            maxRepeat: 9999,
            segmentIds: ImmutableArray.Create("LIN", "PIA", "IMD", "MEA", "QTY", "PCD", "ALI", "DTM", "GIN", "GIR", "QVR", "DOC", "PAI", "FTX", "MOA", "PAT", "PRI", "RFF", "PAC", "LOC", "TAX", "NAD", "ALC", "TDT", "TOD", "EQD", "SCC"),
            childLoops: ImmutableArray<EdiLoopSchema>.Empty);

        // Transaction-level loops
        var transactionLoops = ImmutableArray.Create(sg1Loop, sg2Loop, sg25Loop);

        // For built-in schemas, we focus on loop structure
        var transactionSegments = ImmutableArray<EdiSegmentSchema>.Empty;

        return new EdiTransactionSchema(
            key: EdiSchemaKey.ForEdifact("ORDERS", "D", "96A", "UN"),
            description: "Purchase Order Message",
            loops: transactionLoops,
            segments: transactionSegments
        );
    }
}
