using EdiViewer.Models;
using Microsoft.AspNetCore.Mvc;
using EdiX;
using EdiX.Parsing;
using EdiX.Validation;

namespace EdiViewer.Controllers;

/// <summary>
/// Handles EDI validation.
/// </summary>
public sealed class ValidateController : Controller
{
    private readonly EdiValidator _validator;

    public ValidateController(EdiValidator validator)
    {
        _validator = validator;
    }

    [HttpGet]
    public IActionResult Index()
    {
        return View(new EdiValidateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EdiValidateViewModel model)
    {
        string? ediText = await ResolveEdiTextAsync(model);

        if (string.IsNullOrWhiteSpace(ediText))
        {
            ModelState.AddModelError(string.Empty, "Please paste EDI content or upload an EDI file.");
            return View(model);
        }

        EdiParseResult parseResult = EdiDocument.TryParse(ediText);
        EdiValidationResult validationResult = await _validator.ValidateAsync(parseResult.Document);

        model.EdiText = ediText;
        model.Result = MapToResultViewModel(parseResult, validationResult);
        return View(model);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<string?> ResolveEdiTextAsync(EdiValidateViewModel model)
    {
        if (model.EdiFile is { Length: > 0 })
        {
            using var reader = new StreamReader(model.EdiFile.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        return model.EdiText;
    }

    private static EdiValidationResultViewModel MapToResultViewModel(
        EdiParseResult parseResult,
        EdiValidationResult validationResult)
    {
        return new EdiValidationResultViewModel
        {
            IsValid = !parseResult.HasFatalErrors && validationResult.IsValid,
            Dialect = parseResult.Document.Interchange.Dialect.ToString(),
            Errors = validationResult.Errors
                .Select(e => new EdiValidationErrorViewModel
                {
                    Severity = e.Severity.ToString(),
                    Layer = e.Layer.ToString(),
                    Code = e.Code,
                    Message = e.Message,
                    SegmentId = e.SegmentId
                })
                .ToList(),
            Warnings = validationResult.Warnings
                .Select(e => new EdiValidationErrorViewModel
                {
                    Severity = e.Severity.ToString(),
                    Layer = e.Layer.ToString(),
                    Code = e.Code,
                    Message = e.Message,
                    SegmentId = e.SegmentId
                })
                .ToList(),
            ParseErrors = parseResult.Errors
                .Select(e => new EdiParseErrorViewModel
                {
                    Severity = e.Severity.ToString(),
                    Code = e.Code,
                    Message = e.Message
                })
                .ToList()
        };
    }
}
