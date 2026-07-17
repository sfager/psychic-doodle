using EdiViewer.Models;
using Microsoft.AspNetCore.Mvc;
using EdiX;
using EdiX.Parsing;

namespace EdiViewer.Controllers;

/// <summary>
/// Handles EDI upload and human-readable presentation.
/// </summary>
public sealed class EdiController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View(new EdiUploadViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(EdiUploadViewModel model)
    {
        string? ediText = await ResolveEdiTextAsync(model);

        if (string.IsNullOrWhiteSpace(ediText))
        {
            ModelState.AddModelError(string.Empty, "Please paste EDI content or upload an EDI file.");
            return View(model);
        }

        EdiParseResult parseResult = EdiDocument.TryParse(ediText);
        EdiDocumentViewModel viewModel = MapToViewModel(parseResult, ediText);
        return View("Result", viewModel);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static async Task<string?> ResolveEdiTextAsync(EdiUploadViewModel model)
    {
        if (model.EdiFile is { Length: > 0 })
        {
            using var reader = new StreamReader(model.EdiFile.OpenReadStream());
            return await reader.ReadToEndAsync();
        }

        return model.EdiText;
    }

    private static EdiDocumentViewModel MapToViewModel(EdiParseResult parseResult, string rawEdi)
    {
        var vm = new EdiDocumentViewModel
        {
            RawEdi = rawEdi,
            Dialect = parseResult.Document.Interchange.Dialect.ToString(),
            ParseErrors = parseResult.Errors
                .Select(e => new EdiParseErrorViewModel
                {
                    Severity = e.Severity.ToString(),
                    Code = e.Code,
                    Message = e.Message
                })
                .ToList()
        };

        var interchange = parseResult.Document.Interchange;
        vm.Interchange = MapInterchange(interchange);
        return vm;
    }

    private static EdiInterchangeViewModel MapInterchange(EdiInterchange interchange)
    {
        var vm = new EdiInterchangeViewModel();

        if (interchange.Dialect == EdiDialect.X12)
        {
            var h = interchange.AsX12Header;
            vm.SenderId = h.SenderId?.Trim() ?? string.Empty;
            vm.ReceiverId = h.ReceiverId?.Trim() ?? string.Empty;
            vm.ControlNumber = h.ControlNumber;
            vm.Date = h.Date?.ToString("yyyy-MM-dd") ?? h.RawDate;
            vm.Time = h.Time?.ToString("HH:mm") ?? h.RawTime;
            vm.Version = h.VersionNumber;
            vm.UsageIndicator = h.UsageIndicator == "P" ? "Production" :
                                 h.UsageIndicator == "T" ? "Test" : h.UsageIndicator;
        }
        else
        {
            var h = interchange.AsEdifactHeader;
            vm.SenderId = h.SenderId?.Trim() ?? string.Empty;
            vm.ReceiverId = h.ReceiverId?.Trim() ?? string.Empty;
            vm.ControlNumber = h.ControlReference;
            vm.Date = h.RawDate;
        }

        vm.Groups = interchange.Groups
            .Select(MapGroup)
            .ToList();

        vm.UngroupedTransactions = interchange.Transactions
            .Select(MapTransaction)
            .ToList();

        return vm;
    }

    private static EdiGroupViewModel MapGroup(EdiFunctionalGroup group)
    {
        var vm = new EdiGroupViewModel();

        if (group.Dialect == EdiDialect.X12)
        {
            var h = group.AsX12GroupHeader;
            vm.FunctionalIdentifier = h.FunctionalIdentifier;
            vm.SenderId = h.SenderId?.Trim();
            vm.ReceiverId = h.ReceiverId?.Trim();
            vm.Date = h.Date?.ToString("yyyy-MM-dd") ?? h.RawDate;
            vm.ControlNumber = h.ControlNumber;
        }
        else
        {
            var h = group.AsEdifactGroupHeader;
            vm.FunctionalIdentifier = h.MessageGroupId;
            vm.SenderId = h.SenderId?.Trim();
            vm.ReceiverId = h.ReceiverId?.Trim();
            vm.Date = h.RawDate;
            vm.ControlNumber = h.ControlNumber;
        }

        vm.Transactions = group.Transactions
            .Select(MapTransaction)
            .ToList();

        return vm;
    }

    private static EdiTransactionViewModel MapTransaction(EdiTransaction tx)
    {
        return new EdiTransactionViewModel
        {
            TransactionType = tx.TransactionType,
            TransactionTypeName = GetTransactionTypeName(tx.TransactionType),
            Segments = tx.Segments
                .Select(s => new EdiSegmentViewModel
                {
                    Id = s.Id,
                    Description = GetSegmentDescription(s.Id),
                    Elements = s.Elements
                        .Select(e => e.Value ?? e.ToString() ?? string.Empty)
                        .ToList()
                })
                .ToList()
        };
    }

    // ── Segment / Transaction description lookup ──────────────────────────────

    private static string GetTransactionTypeName(string transactionType) =>
        transactionType switch
        {
            "810" => "Invoice",
            "820" => "Payment Order / Remittance Advice",
            "830" => "Planning Schedule",
            "840" => "Request for Quotation",
            "850" => "Purchase Order",
            "855" => "Purchase Order Acknowledgment",
            "856" => "Ship Notice / Manifest",
            "860" => "Purchase Order Change",
            "864" => "Text Message",
            "997" => "Functional Acknowledgment",
            "999" => "Implementation Acknowledgment",
            "204" => "Motor Carrier Load Tender",
            "214" => "Transportation Carrier Shipment Status",
            "270" => "Eligibility, Coverage or Benefit Inquiry",
            "271" => "Eligibility, Coverage or Benefit Information",
            "276" => "Health Care Claim Status Request",
            "277" => "Health Care Claim Status Notification",
            "278" => "Health Care Services Review",
            "835" => "Health Care Claim Payment / Advice",
            "837" => "Health Care Claim",
            "ORDERS" => "Order",
            "ORDRSP" => "Order Response",
            "INVOIC" => "Invoice",
            "DESADV" => "Despatch Advice",
            "APERAK" => "Application Error and Acknowledgment",
            "CONTRL" => "Syntax and Service Report",
            _ => string.Empty
        };

    private static string GetSegmentDescription(string segmentId) =>
        segmentId switch
        {
            "ISA" => "Interchange Control Header",
            "IEA" => "Interchange Control Trailer",
            "GS"  => "Functional Group Header",
            "GE"  => "Functional Group Trailer",
            "ST"  => "Transaction Set Header",
            "SE"  => "Transaction Set Trailer",
            "BEG" => "Beginning Segment for Purchase Order",
            "REF" => "Reference Identification",
            "DTM" => "Date/Time Reference",
            "N1"  => "Name",
            "N2"  => "Additional Name Information",
            "N3"  => "Address Information",
            "N4"  => "Geographic Location",
            "PO1" => "Baseline Item Data",
            "CTT" => "Transaction Totals",
            "AMT" => "Monetary Amount",
            "CUR" => "Currency",
            "PER" => "Administrative Communications Contact",
            "IT1" => "Baseline Item Data (Invoice)",
            "TDS" => "Total Monetary Value Summary",
            "TXI" => "Tax Information",
            "BAK" => "Beginning Segment for Purchase Order Acknowledgment",
            "ACK" => "Line Item Acknowledgment",
            "BSN" => "Beginning Segment for Ship Notice",
            "HL"  => "Hierarchical Level",
            "TD1" => "Carrier Details (Quantity and Weight)",
            "TD5" => "Carrier Details (Routing Sequence/Transit Time)",
            "MAN" => "Marks and Numbers",
            "UNH" => "Message Header",
            "UNT" => "Message Trailer",
            "UNB" => "Interchange Header",
            "UNZ" => "Interchange Trailer",
            "UNG" => "Group Header",
            "UNE" => "Group Trailer",
            "BGM" => "Beginning of Message",
            "LIN" => "Line Item",
            "QTY" => "Quantity",
            "PRI" => "Price Details",
            "TAX" => "Duty/Tax/Fee Details",
            "MOA" => "Monetary Amount",
            "NAD" => "Name and Address",
            "LOC" => "Place/Location Identification",
            "RFF" => "Reference",
            "STS" => "Status",
            _ => string.Empty
        };
}
