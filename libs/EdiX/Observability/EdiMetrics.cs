using System.Diagnostics.Metrics;

namespace EdiX.Observability;

/// <summary>
/// Provides metrics for EDI operations via System.Diagnostics.Metrics.
/// </summary>
public static class EdiMetrics
{
    private static readonly Meter _meter = new("EdiLib", "1.0.0");

    /// <summary>
    /// Counter for total EDI documents parsed.
    /// Tags: edi.standard, edi.version, edi.type
    /// </summary>
    public static readonly Counter<long> DocumentsParsed = _meter.CreateCounter<long>(
        "edi.documents.parsed",
        unit: "{document}",
        description: "Total number of EDI documents parsed");

    /// <summary>
    /// Counter for total EDI transactions parsed.
    /// Tags: edi.standard, edi.type
    /// </summary>
    public static readonly Counter<long> TransactionsParsed = _meter.CreateCounter<long>(
        "edi.transactions.parsed",
        unit: "{transaction}",
        description: "Total number of EDI transactions parsed");

    /// <summary>
    /// Counter for parse errors encountered.
    /// Tags: edi.standard, edi.error_code
    /// </summary>
    public static readonly Counter<long> ParseErrors = _meter.CreateCounter<long>(
        "edi.parse.errors",
        unit: "{error}",
        description: "Total number of parse errors");

    /// <summary>
    /// Counter for validation errors encountered.
    /// Tags: edi.standard, edi.error_code, edi.severity
    /// </summary>
    public static readonly Counter<long> ValidationErrors = _meter.CreateCounter<long>(
        "edi.validation.errors",
        unit: "{error}",
        description: "Total number of validation errors");

    /// <summary>
    /// Histogram for document parse duration in milliseconds.
    /// Tags: edi.standard
    /// </summary>
    public static readonly Histogram<double> ParseDuration = _meter.CreateHistogram<double>(
        "edi.parse.duration",
        unit: "ms",
        description: "Duration of document parsing operations");

    /// <summary>
    /// Histogram for validation duration in milliseconds.
    /// Tags: edi.standard
    /// </summary>
    public static readonly Histogram<double> ValidationDuration = _meter.CreateHistogram<double>(
        "edi.validation.duration",
        unit: "ms",
        description: "Duration of validation operations");

    /// <summary>
    /// Histogram for document generation duration in milliseconds.
    /// Tags: edi.standard
    /// </summary>
    public static readonly Histogram<double> GenerationDuration = _meter.CreateHistogram<double>(
        "edi.generation.duration",
        unit: "ms",
        description: "Duration of document generation operations");
}
