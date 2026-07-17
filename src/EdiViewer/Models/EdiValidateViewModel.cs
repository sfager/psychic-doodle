using System.ComponentModel.DataAnnotations;

namespace EdiViewer.Models;

/// <summary>
/// View model for the EDI validation page (shared for input and result).
/// </summary>
public sealed class EdiValidateViewModel
{
    /// <summary>EDI text to validate.</summary>
    [Display(Name = "EDI Content")]
    public string? EdiText { get; set; }

    /// <summary>Uploaded EDI file to validate.</summary>
    [Display(Name = "EDI File")]
    public IFormFile? EdiFile { get; set; }

    /// <summary>Set after validation runs.</summary>
    public EdiValidationResultViewModel? Result { get; set; }
}

public sealed class EdiValidationResultViewModel
{
    public bool IsValid { get; set; }
    public string Dialect { get; set; } = string.Empty;
    public List<EdiValidationErrorViewModel> Errors { get; set; } = new();
    public List<EdiValidationErrorViewModel> Warnings { get; set; } = new();
    public List<EdiParseErrorViewModel> ParseErrors { get; set; } = new();
    public bool HasParseErrors => ParseErrors.Count > 0;
}

public sealed class EdiValidationErrorViewModel
{
    public string Severity { get; set; } = string.Empty;
    public string Layer { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? SegmentId { get; set; }
}
