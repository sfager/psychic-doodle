using EdiX.Schema;
using System.Collections.Concurrent;

namespace EdiX.Validation;

/// <summary>
/// Main validator orchestrating syntactic, structural, and semantic validation.
/// Thread-safe and suitable for DI singleton registration.
/// </summary>
public sealed class EdiValidator
{
    private readonly EdiValidatorOptions _options;
    private readonly EdiSchemaRegistry _registry;
    private readonly ConcurrentBag<IEdiValidationRule> _customRules;

    private EdiValidator(EdiValidatorOptions options)
    {
        _options = options;
        _registry = options.SchemaRegistry ?? new EdiSchemaRegistry();
        _customRules = new ConcurrentBag<IEdiValidationRule>(options.Rules ?? Array.Empty<IEdiValidationRule>());
    }

    /// <summary>
    /// Creates a new validator instance with the specified options.
    /// </summary>
    /// <param name="options">Validation options.</param>
    /// <returns>Configured validator instance.</returns>
    public static EdiValidator Create(EdiValidatorOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return new EdiValidator(options);
    }

    /// <summary>
    /// Validates an EDI document synchronously.
    /// </summary>
    /// <param name="document">Document to validate.</param>
    /// <returns>Validation result with errors and warnings.</returns>
    public EdiValidationResult Validate(EdiDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var errors = new List<EdiValidationError>();

        // Syntactic validation
        if (_options.Layers.HasFlag(EdiValidationLayer.Syntactic))
        {
            errors.AddRange(Internal.SyntacticValidator.Validate(document));
        }

        // Structural validation
        if (_options.Layers.HasFlag(EdiValidationLayer.Structural))
        {
            errors.AddRange(Internal.StructuralValidator.Validate(document, _registry));
        }

        // Semantic validation (custom rules)
        if (_options.Layers.HasFlag(EdiValidationLayer.Semantic))
        {
            // Get schema key from first transaction for context
            var schemaKey = document.Interchange.Transactions.FirstOrDefault()?.SchemaKey 
                         ?? document.Interchange.Groups.FirstOrDefault()?.Transactions.FirstOrDefault()?.SchemaKey;

            var context = new EdiValidationContext(
                (EdiSchemaKey?)schemaKey,
                document.Interchange,
                null);

            // Apply rules to each transaction
            foreach (var group in document.Interchange.Groups)
            {
                foreach (var transaction in group.Transactions)
                {
                    foreach (var rule in _customRules)
                    {
                        // Filter by dialect and transaction type
                        if (rule.Dialect.HasValue && rule.Dialect.Value != document.Interchange.Dialect)
                            continue;
                        if (rule.TransactionType != null && !transaction.TransactionType.Equals(rule.TransactionType, StringComparison.OrdinalIgnoreCase))
                            continue;

                        errors.AddRange(rule.Validate(transaction, context));
                    }
                }
            }

            foreach (var transaction in document.Interchange.Transactions)
            {
                foreach (var rule in _customRules)
                {
                    // Filter by dialect and transaction type
                    if (rule.Dialect.HasValue && rule.Dialect.Value != document.Interchange.Dialect)
                        continue;
                    if (rule.TransactionType != null && !transaction.TransactionType.Equals(rule.TransactionType, StringComparison.OrdinalIgnoreCase))
                        continue;

                    errors.AddRange(rule.Validate(transaction, context));
                }
            }
        }

        // Apply MaxErrors limit
        if (errors.Count > _options.MaxErrors)
        {
            errors = errors.Take(_options.MaxErrors).ToList();
        }

        return new EdiValidationResult(errors);
    }

    /// <summary>
    /// Validates an EDI document asynchronously.
    /// </summary>
    /// <param name="document">Document to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with errors and warnings.</returns>
    public Task<EdiValidationResult> ValidateAsync(
        EdiDocument document,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.Run(() => Validate(document), cancellationToken);
    }

    /// <summary>
    /// Validates a single transaction against a schema.
    /// </summary>
    /// <param name="transaction">Transaction to validate.</param>
    /// <param name="schemaKey">Schema key for structural validation.</param>
    /// <returns>Validation result.</returns>
    public EdiValidationResult ValidateTransaction(
        EdiTransaction transaction,
        EdiSchemaKey schemaKey)
    {
        ArgumentNullException.ThrowIfNull(transaction);
        ArgumentNullException.ThrowIfNull(schemaKey);

        var errors = new List<EdiValidationError>();

        // Structural validation only for single transaction
        if (_options.Layers.HasFlag(EdiValidationLayer.Structural))
        {
            var schemaResult = _registry.GetSchema(schemaKey);
            if (schemaResult != null)
            {
                var schema = schemaResult;
                var presentSegments = new HashSet<string>(
                    transaction.Segments.Select(s => s.Id),
                    StringComparer.OrdinalIgnoreCase);

                foreach (var segmentSchema in schema.Segments)
                {
                    if (segmentSchema.Usage == EdiUsage.Mandatory && !presentSegments.Contains(segmentSchema.Id))
                    {
                        errors.Add(new EdiValidationError(
                            EdiValidationLayer.Structural,
                            EdiValidationSeverity.Error,
                            "SCHEMA-002",
                            $"Mandatory segment '{segmentSchema.Id}' is missing",
                            new EdiPosition { SegmentIndex = 0 },
                            segmentId: segmentSchema.Id));
                    }
                }
            }
        }

        return new EdiValidationResult(errors);
    }

    /// <summary>
    /// Validates a single segment against a schema.
    /// </summary>
    /// <param name="segment">Segment to validate.</param>
    /// <param name="schema">Segment schema for validation.</param>
    /// <returns>Validation result.</returns>
    public EdiValidationResult ValidateSegment(
        EdiSegment segment,
        EdiSegmentSchema schema)
    {
        ArgumentNullException.ThrowIfNull(segment);
        ArgumentNullException.ThrowIfNull(schema);

        var errors = new List<EdiValidationError>();

        // Validate segment ID matches
        if (!segment.Id.Equals(schema.Id, StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(new EdiValidationError(
                EdiValidationLayer.Structural,
                EdiValidationSeverity.Error,
                "SEG-001",
                $"Segment ID mismatch: expected '{schema.Id}', got '{segment.Id}'",
                new EdiPosition { SegmentIndex = segment.Position },
                segmentId: segment.Id));
        }

        // Validate element count
        if (segment.Elements.Length < schema.Elements.Length)
        {
            errors.Add(new EdiValidationError(
                EdiValidationLayer.Structural,
                EdiValidationSeverity.Error,
                "SEG-002",
                $"Segment '{segment.Id}' has fewer elements than schema requires",
                new EdiPosition { SegmentIndex = segment.Position },
                segmentId: segment.Id));
        }

        return new EdiValidationResult(errors);
    }
}
