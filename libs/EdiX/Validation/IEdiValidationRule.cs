namespace EdiX.Validation;

/// <summary>
/// Defines a validation rule that can be applied to EDI transactions.
/// </summary>
public interface IEdiValidationRule
{
    /// <summary>
    /// Gets the transaction type this rule applies to, or null for all transaction types.
    /// </summary>
    string? TransactionType { get; }

    /// <summary>
    /// Gets the dialect this rule applies to, or null for both dialects.
    /// </summary>
    EdiDialect? Dialect { get; }

    /// <summary>
    /// Validates a transaction and returns any errors found.
    /// </summary>
    /// <param name="transaction">The transaction to validate.</param>
    /// <param name="context">The validation context.</param>
    /// <returns>A sequence of validation errors.</returns>
    IEnumerable<EdiValidationError> Validate(EdiTransaction transaction, EdiValidationContext context);
}
