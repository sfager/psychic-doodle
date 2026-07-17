using EdiX.Schema;

namespace EdiX.Validation;

/// <summary>
/// Configuration options for EDI validation.
/// </summary>
public sealed class EdiValidatorOptions
{
    /// <summary>
    /// Gets or sets the validation layers to apply. Default is All.
    /// </summary>
    public EdiValidationLayer Layers { get; init; } = EdiValidationLayer.All;

    /// <summary>
    /// Gets or sets the schema registry for structural validation.
    /// </summary>
    public EdiSchemaRegistry? SchemaRegistry { get; init; }

    /// <summary>
    /// Gets or sets custom validation rules to apply.
    /// </summary>
    public IReadOnlyList<IEdiValidationRule>? Rules { get; init; }

    /// <summary>
    /// Gets or sets the maximum number of errors to collect before stopping. Default is 100.
    /// </summary>
    public int MaxErrors { get; init; } = 100;

    /// <summary>
    /// Gets or sets how to handle missing schemas. Default is Warning.
    /// </summary>
    public EdiValidationSeverity MissingSchemaHandling { get; init; } = EdiValidationSeverity.Warning;
}
