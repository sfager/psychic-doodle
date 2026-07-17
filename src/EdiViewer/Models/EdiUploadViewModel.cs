using System.ComponentModel.DataAnnotations;

namespace EdiViewer.Models;

/// <summary>
/// View model for the EDI upload form.
/// </summary>
public sealed class EdiUploadViewModel
{
    /// <summary>EDI text pasted directly into the form.</summary>
    [Display(Name = "EDI Content")]
    public string? EdiText { get; set; }

    /// <summary>Uploaded EDI file.</summary>
    [Display(Name = "EDI File")]
    public IFormFile? EdiFile { get; set; }
}
