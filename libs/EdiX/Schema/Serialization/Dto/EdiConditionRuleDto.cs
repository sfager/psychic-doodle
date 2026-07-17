using System.Text.Json.Serialization;

namespace EdiX.Schema.Serialization.Dto;

/// <summary>
/// JSON DTO for EdiConditionRule.
/// </summary>
public sealed class EdiConditionRuleDto
{
    /// <summary>The condition type (e.g., "Paired", "Required", "Exclusion").</summary>
    [JsonPropertyName("conditionType")]
    public string ConditionType { get; set; } = string.Empty;

    /// <summary>The 1-based positions of elements involved in this condition.</summary>
    [JsonPropertyName("positions")]
    public List<int> Positions { get; set; } = new();
}
