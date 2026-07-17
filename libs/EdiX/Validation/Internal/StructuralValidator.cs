using EdiX.Schema;

namespace EdiX.Validation.Internal;

/// <summary>
/// Validates structural conformance to registered schemas.
/// </summary>
internal static class StructuralValidator
{
    public static IEnumerable<EdiValidationError> Validate(
        EdiDocument document,
        EdiSchemaRegistry registry)
    {
        var schemaKey = document.Interchange.Transactions.FirstOrDefault()?.SchemaKey 
                     ?? document.Interchange.Groups.FirstOrDefault()?.Transactions.FirstOrDefault()?.SchemaKey;
        
        // SCHEMA-001: No schema registered - skip structural validation with warning
        if (schemaKey == null)
        {
            yield return new EdiValidationError(
                EdiValidationLayer.Structural,
                EdiValidationSeverity.Warning,
                "SCHEMA-001",
                "No schema registered - structural validation skipped",
                new EdiPosition { SegmentIndex = 0 },
                segmentId: document.Interchange.HeaderSegment.Id);
            yield break;
        }

        var schema = registry.GetSchema((EdiSchemaKey)schemaKey);
        if (schema == null)
        {
            yield return new EdiValidationError(
                EdiValidationLayer.Structural,
                EdiValidationSeverity.Warning,
                "SCHEMA-001",
                "No schema registered - structural validation skipped",
                new EdiPosition { SegmentIndex = 0 },
                segmentId: document.Interchange.HeaderSegment.Id);
            yield break;
        }

        // Validate each transaction
        foreach (var group in document.Interchange.Groups)
        {
            foreach (var transaction in group.Transactions)
            {
                foreach (var error in ValidateTransaction(transaction, schema))
                    yield return error;
            }
        }

        // Validate ungrouped transactions (EDIFACT)
        foreach (var transaction in document.Interchange.Transactions)
        {
            foreach (var error in ValidateTransaction(transaction, schema))
                yield return error;
        }
    }

    private static IEnumerable<EdiValidationError> ValidateTransaction(
        EdiTransaction transaction,
        EdiTransactionSchema schema)
    {
        var presentSegments = new HashSet<string>(
            transaction.Segments.Select(s => s.Id),
            StringComparer.OrdinalIgnoreCase);

        // Check mandatory segments
        foreach (var segmentSchema in schema.Segments)
        {
            if (segmentSchema.Usage == EdiUsage.Mandatory && !presentSegments.Contains(segmentSchema.Id))
            {
                // SCHEMA-002: Mandatory segment absent
                yield return new EdiValidationError(
                    EdiValidationLayer.Structural,
                    EdiValidationSeverity.Error,
                    "SCHEMA-002",
                    $"Mandatory segment '{segmentSchema.Id}' is missing",
                    new EdiPosition { SegmentIndex = 0 },
                    segmentId: segmentSchema.Id);
            }
        }

        // Additional structural validations can be added here:
        // - Segment order validation
        // - MaxOccurs validation
        // - Loop structure validation
    }
}
