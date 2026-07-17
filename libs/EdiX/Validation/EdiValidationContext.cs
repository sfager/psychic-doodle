using EdiX.Schema;

namespace EdiX.Validation;

/// <summary>
/// Provides context for validation rules.
/// </summary>
public sealed class EdiValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EdiValidationContext"/> class.
    /// </summary>
    public EdiValidationContext(
        EdiSchemaKey? schemaKey,
        EdiInterchange interchange,
        EdiFunctionalGroup? group = null)
    {
        SchemaKey = schemaKey;
        Interchange = interchange ?? throw new ArgumentNullException(nameof(interchange));
        Group = group;
    }

    /// <summary>Gets the schema key for the transaction being validated.</summary>
    public EdiSchemaKey? SchemaKey { get; }

    /// <summary>Gets the interchange containing the transaction.</summary>
    public EdiInterchange Interchange { get; }

    /// <summary>Gets the functional group containing the transaction, if applicable.</summary>
    public EdiFunctionalGroup? Group { get; }
}
