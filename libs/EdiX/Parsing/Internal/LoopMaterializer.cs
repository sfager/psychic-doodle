using System.Collections.Immutable;
using EdiX.Schema;

namespace EdiX.Parsing.Internal;

/// <summary>
/// Materializes loop structures from flat segment lists using transaction schemas.
/// </summary>
internal static class LoopMaterializer
{
    /// <summary>
    /// Materializes loops from a flat segment list using the provided schema.
    /// </summary>
    /// <param name="segments">The flat list of transaction body segments (excluding ST/SE or UNH/UNT).</param>
    /// <param name="schema">The transaction schema defining loop structure.</param>
    /// <returns>The materialized loop collection, or null if schema is null.</returns>
    internal static EdiLoopCollection? Materialize(
        ImmutableArray<EdiSegment> segments,
        EdiTransactionSchema? schema)
    {
        if (schema == null || schema.Loops.Length == 0)
            return null;

        var loops = MaterializeLoops(segments, schema.Loops, 0, segments.Length);
        return new EdiLoopCollection(loops);
    }

    /// <summary>
    /// Recursively materializes loops from a segment range.
    /// </summary>
    private static ImmutableArray<EdiLoop> MaterializeLoops(
        ImmutableArray<EdiSegment> segments,
        ImmutableArray<EdiLoopSchema> loopSchemas,
        int startIndex,
        int endIndex)
    {
        if (loopSchemas.Length == 0 || startIndex >= endIndex)
            return ImmutableArray<EdiLoop>.Empty;

        var loops = ImmutableArray.CreateBuilder<EdiLoop>();
        int currentIndex = startIndex;

        while (currentIndex < endIndex)
        {
            // Find which loop schema (if any) matches the current segment
            EdiLoopSchema? matchedSchema = null;
            foreach (var loopSchema in loopSchemas)
            {
                if (segments[currentIndex].Id == loopSchema.TriggerSegmentId)
                {
                    matchedSchema = loopSchema;
                    break;
                }
            }

            if (matchedSchema == null)
            {
                // Segment doesn't trigger any loop - skip it
                currentIndex++;
                continue;
            }

            // Collect segments for this loop iteration
            int loopStartIndex = currentIndex;
            currentIndex++; // Move past trigger segment

            // Continue collecting segments until we hit:
            // 1. Another trigger for this loop (new iteration)
            // 2. A trigger for a different loop at this level
            // 3. A segment not in this loop's schema
            // 4. End of segment range
            while (currentIndex < endIndex)
            {
                var currentSegId = segments[currentIndex].Id;

                // Check if this segment triggers a new iteration of the same loop
                if (currentSegId == matchedSchema.TriggerSegmentId)
                    break;

                // Check if this segment triggers a different loop at this level
                bool triggersOtherLoop = false;
                foreach (var otherSchema in loopSchemas)
                {
                    if (otherSchema != matchedSchema && currentSegId == otherSchema.TriggerSegmentId)
                    {
                        triggersOtherLoop = true;
                        break;
                    }
                }
                if (triggersOtherLoop)
                    break;

                // Check if this segment is in the loop's allowed segment list
                if (!matchedSchema.SegmentIds.Contains(currentSegId))
                    break;

                currentIndex++;
            }

            // Extract segments for this loop iteration
            int loopLength = currentIndex - loopStartIndex;
            var loopSegments = segments.AsSpan().Slice(loopStartIndex, loopLength).ToImmutableArray();

            // Materialize child loops
            var childLoops = EdiLoopCollection.Empty;
            if (matchedSchema.ChildLoops.Length > 0)
            {
                var childLoopList = MaterializeLoops(
                    loopSegments,
                    matchedSchema.ChildLoops,
                    0,
                    loopSegments.Length);
                
                if (childLoopList.Length > 0)
                    childLoops = new EdiLoopCollection(childLoopList);
            }

            // Calculate iteration number (1-based)
            int iteration = loops.Count(l => l.LoopId == matchedSchema.LoopId) + 1;

            loops.Add(new EdiLoop(
                matchedSchema.LoopId,
                iteration,
                loopSegments,
                childLoops));
        }

        return loops.ToImmutable();
    }
}