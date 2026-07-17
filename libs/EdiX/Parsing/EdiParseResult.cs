namespace EdiX.Parsing;

/// <summary>
/// Represents the result of a TryParse operation.
/// </summary>
public sealed class EdiParseResult
{
    /// <summary>
    /// Gets the parsed EDI document (may be partial if errors occurred).
    /// </summary>
    public EdiDocument Document { get; }
    
    /// <summary>
    /// Gets the list of errors and warnings encountered during parsing.
    /// </summary>
    public IReadOnlyList<EdiParseError> Errors { get; }
    
    /// <summary>
    /// Gets a value indicating whether any errors (fatal or warnings) occurred.
    /// </summary>
    public bool HasErrors => Errors.Count > 0;
    
    /// <summary>
    /// Gets a value indicating whether any fatal errors occurred.
    /// </summary>
    public bool HasFatalErrors => Errors.Any(e => e.Severity == EdiParseErrorSeverity.Fatal);
    
    /// <summary>
    /// Gets the validation result, if validation was performed.
    /// </summary>
    public object? ValidationResult { get; }  // Will be EdiValidationResult in Phase 7

    /// <summary>
    /// Initializes a new parse result.
    /// </summary>
    /// <param name="document">The parsed document.</param>
    /// <param name="errors">The list of errors.</param>
    /// <param name="validationResult">Optional validation result.</param>
    public EdiParseResult(
        EdiDocument document,
        IReadOnlyList<EdiParseError> errors,
        object? validationResult = null)
    {
        Document = document;
        Errors = errors;
        ValidationResult = validationResult;
    }
}