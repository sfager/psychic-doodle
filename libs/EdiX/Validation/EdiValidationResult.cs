namespace EdiX.Validation;

/// <summary>
/// Represents the result of validating an EDI document.
/// </summary>
public sealed class EdiValidationResult
{
    private readonly IReadOnlyList<EdiValidationError> _errors;
    private readonly IReadOnlyList<EdiValidationError> _warnings;
    private readonly IReadOnlyDictionary<EdiValidationLayer, IReadOnlyList<EdiValidationError>> _errorsByLayer;

    /// <summary>
    /// Initializes a new instance of the <see cref="EdiValidationResult"/> class.
    /// </summary>
    public EdiValidationResult(IEnumerable<EdiValidationError> errors)
    {
        ArgumentNullException.ThrowIfNull(errors);
        
        var allErrors = errors.ToList();
        _errors = allErrors.Where(e => e.Severity == EdiValidationSeverity.Error).ToList();
        _warnings = allErrors.Where(e => e.Severity == EdiValidationSeverity.Warning).ToList();
        
        _errorsByLayer = allErrors
            .GroupBy(e => e.Layer)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyList<EdiValidationError>)g.ToList());
    }

    /// <summary>Gets whether the document is valid (no errors, only warnings allowed).</summary>
    public bool IsValid => _errors.Count == 0;

    /// <summary>Gets all errors (severity = Error).</summary>
    public IReadOnlyList<EdiValidationError> Errors => _errors;

    /// <summary>Gets all warnings (severity = Warning).</summary>
    public IReadOnlyList<EdiValidationError> Warnings => _warnings;

    /// <summary>Gets errors grouped by validation layer.</summary>
    public IReadOnlyDictionary<EdiValidationLayer, IReadOnlyList<EdiValidationError>> ErrorsByLayer => _errorsByLayer;
}
