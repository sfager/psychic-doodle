using System.Text.Json.Serialization;

namespace EdiX.Schema.Serialization.Dto;

/// <summary>
/// JSON DTO for EdiElementSchema.
/// </summary>
public sealed class EdiElementSchemaDto
{
    /// <summary>1-based position of this element within its segment.</summary>
    [JsonPropertyName("position")]
    public int Position { get; set; }

    /// <summary>Element name (e.g., "Purchase Order Number").</summary>
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    /// <summary>Usage requirement: "Mandatory", "Optional", or "NotUsed".</summary>
    [JsonPropertyName("usage")]
    public string Usage { get; set; } = "Optional";

    /// <summary>Data type (e.g., "AN", "N0", "DT", "TM", "ID").</summary>
    [JsonPropertyName("dataType")]
    public string DataType { get; set; } = "AN";

    /// <summary>Minimum length.</summary>
    [JsonPropertyName("minLength")]
    public int MinLength { get; set; }

    /// <summary>Maximum length.</summary>
    [JsonPropertyName("maxLength")]
    public int MaxLength { get; set; }

    /// <summary>Allowed values, if restricted. Null means any value is permitted.</summary>
    [JsonPropertyName("allowedValues")]
    public List<string>? AllowedValues { get; set; }

    /// <summary>Conditional rules applying to this element.</summary>
    [JsonPropertyName("conditions")]
    public List<EdiConditionRuleDto>? Conditions { get; set; }
}
