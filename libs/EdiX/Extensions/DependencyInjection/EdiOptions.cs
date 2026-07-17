using EdiX.Parsing;
using EdiX.Schema;

namespace EdiX.Extensions.DependencyInjection;

/// <summary>
/// Configuration options for EDI services.
/// </summary>
public sealed class EdiOptions
{
    /// <summary>
    /// Gets or sets the default parse options.
    /// </summary>
    public EdiParseOptions ParseOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets the schema registry to use.
    /// Defaults to EdiSchemaRegistry.Default which includes built-in schemas.
    /// </summary>
    public EdiSchemaRegistry SchemaRegistry { get; set; } = EdiSchemaRegistry.Default;
}
