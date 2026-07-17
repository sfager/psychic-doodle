namespace EdiViewer.Models;

/// <summary>
/// View model representing a human-readable EDI document.
/// </summary>
public sealed class EdiDocumentViewModel
{
    public string Dialect { get; set; } = string.Empty;
    public string RawEdi { get; set; } = string.Empty;
    public EdiInterchangeViewModel Interchange { get; set; } = new();
    public List<EdiParseErrorViewModel> ParseErrors { get; set; } = new();
    public bool HasParseErrors => ParseErrors.Count > 0;
}

public sealed class EdiInterchangeViewModel
{
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string? ControlNumber { get; set; }
    public string? Date { get; set; }
    public string? Time { get; set; }
    public string? Version { get; set; }
    public string? UsageIndicator { get; set; }
    public List<EdiGroupViewModel> Groups { get; set; } = new();
    public List<EdiTransactionViewModel> UngroupedTransactions { get; set; } = new();
}

public sealed class EdiGroupViewModel
{
    public string? FunctionalIdentifier { get; set; }
    public string? SenderId { get; set; }
    public string? ReceiverId { get; set; }
    public string? Date { get; set; }
    public string? ControlNumber { get; set; }
    public List<EdiTransactionViewModel> Transactions { get; set; } = new();
}

public sealed class EdiTransactionViewModel
{
    public string TransactionType { get; set; } = string.Empty;
    public string TransactionTypeName { get; set; } = string.Empty;
    public List<EdiSegmentViewModel> Segments { get; set; } = new();
}

public sealed class EdiSegmentViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> Elements { get; set; } = new();
}

public sealed class EdiParseErrorViewModel
{
    public string Severity { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
