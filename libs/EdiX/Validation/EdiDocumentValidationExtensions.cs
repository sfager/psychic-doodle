
namespace EdiX.Validation;

/// <summary>
/// Extension methods for validating EDI documents.
/// </summary>
public static class EdiDocumentValidationExtensions
{
    /// <summary>
    /// Validates an EDI document using the specified options.
    /// </summary>
    /// <param name="document">Document to validate.</param>
    /// <param name="options">Validation options. If null, uses default options.</param>
    /// <returns>Validation result with errors and warnings.</returns>
    public static EdiValidationResult Validate(
        this EdiDocument document,
        EdiValidatorOptions? options = null)
    {
        options ??= new EdiValidatorOptions();
        var validator = EdiValidator.Create(options);
        return validator.Validate(document);
    }
}
