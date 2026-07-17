using System.Diagnostics;

namespace EdiX.Observability;

/// <summary>
/// Provides telemetry support for EDI operations via ActivitySource.
/// </summary>
public static class EdiTelemetry
{
    /// <summary>
    /// ActivitySource for distributed tracing of EDI operations.
    /// Name: "EdiLib", Version: "1.0.0"
    /// </summary>
    public static readonly ActivitySource Source = new("EdiLib", "1.0.0");

    /// <summary>
    /// Creates an activity for parsing operations.
    /// </summary>
    /// <param name="standard">EDI standard (X12, EDIFACT, etc.)</param>
    /// <returns>Activity or null if not enabled.</returns>
    public static Activity? StartParse(string standard)
    {
        var activity = Source.StartActivity("EDI.Parse", ActivityKind.Internal);
        activity?.SetTag("edi.standard", standard);
        return activity;
    }

    /// <summary>
    /// Creates an activity for validation operations.
    /// </summary>
    /// <param name="standard">EDI standard (X12, EDIFACT, etc.)</param>
    /// <returns>Activity or null if not enabled.</returns>
    public static Activity? StartValidate(string standard)
    {
        var activity = Source.StartActivity("EDI.Validate", ActivityKind.Internal);
        activity?.SetTag("edi.standard", standard);
        return activity;
    }

    /// <summary>
    /// Creates an activity for generation/writing operations.
    /// </summary>
    /// <param name="standard">EDI standard (X12, EDIFACT, etc.)</param>
    /// <returns>Activity or null if not enabled.</returns>
    public static Activity? StartGenerate(string standard)
    {
        var activity = Source.StartActivity("EDI.Generate", ActivityKind.Internal);
        activity?.SetTag("edi.standard", standard);
        return activity;
    }
}
