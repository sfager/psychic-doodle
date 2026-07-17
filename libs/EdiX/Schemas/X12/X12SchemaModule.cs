using System.Runtime.CompilerServices;
using EdiX.Schema;

namespace EdiX.Schemas.X12;

/// <summary>
/// Module initializer for X12 built-in schemas.
/// Registers schemas with the default registry before first access.
/// </summary>
internal static class X12SchemaModule
{
    [ModuleInitializer]
    internal static void Register()
    {
        EdiSchemaRegistry.RegisterBuiltIn(X12_850_004010.Schema);
    }
}
