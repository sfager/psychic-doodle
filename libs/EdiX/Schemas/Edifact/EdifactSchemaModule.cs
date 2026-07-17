using System.Runtime.CompilerServices;
using EdiX.Schema;

namespace EdiX.Schemas.Edifact;

/// <summary>
/// Module initializer for EDIFACT built-in schemas.
/// Registers schemas with the default registry before first access.
/// </summary>
internal static class EdifactSchemaModule
{
    [ModuleInitializer]
    internal static void Register()
    {
        EdiSchemaRegistry.RegisterBuiltIn(EDIFACT_ORDERS_D96A.Schema);
    }
}
