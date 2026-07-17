using System.Text.Json.Serialization;

namespace EdiX.Schema.Serialization.Dto;

/// <summary>
/// JSON DTO for EdiSchemaKey.
/// </summary>
public sealed class EdiSchemaKeyDto
{
    /// <summary>The EDI dialect ("X12" or "Edifact").</summary>
    [JsonPropertyName("dialect")]
    public string Dialect { get; set; } = string.Empty;

    /// <summary>Transaction type code (e.g., "850", "ORDERS").</summary>
    [JsonPropertyName("transactionType")]
    public string TransactionType { get; set; } = string.Empty;

    /// <summary>Version (e.g., "004" for X12, "D" for EDIFACT).</summary>
    [JsonPropertyName("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>Release (e.g., "010" for X12, "96A" for EDIFACT).</summary>
    [JsonPropertyName("release")]
    public string Release { get; set; } = string.Empty;

    /// <summary>Controlling agency code — EDIFACT only (e.g., "UN"). Null for X12.</summary>
    [JsonPropertyName("controllingAgency")]
    public string? ControllingAgency { get; set; }

    /// <summary>Association code — EDIFACT only (e.g., "EAN008"). Null if not applicable.</summary>
    [JsonPropertyName("associationCode")]
    public string? AssociationCode { get; set; }
}
