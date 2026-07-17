using EdiX.Editing;

namespace EdiX.Edifact;

/// <summary>
/// Provides named surgical editing of EDIFACT UNB interchange header fields.
/// Obtain via <see cref="EdiDocument.EditInterchangeHeader(Action{EdifactInterchangeHeaderEditor})"/>.
/// </summary>
public sealed class EdifactInterchangeHeaderEditor
{
    private readonly EdiSegmentEditor _editor;
    internal EdifactInterchangeHeaderEditor(EdiSegment unb) => _editor = new EdiSegmentEditor(unb);

    /// <summary>Sets UNB05 — Interchange Control Reference.</summary>
    public EdifactInterchangeHeaderEditor SetControlReference(string reference)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reference);
        _editor.SetElement(5, reference);
        return this;
    }

    /// <summary>Sets UNB04-1 formatted as yyMMdd.</summary>
    public EdifactInterchangeHeaderEditor SetDate(DateOnly date)
    {
        _editor.SetComponent(4, 1, date.ToString("yyMMdd"));
        return this;
    }

    /// <summary>Sets UNB04-2 formatted as HHmm.</summary>
    public EdifactInterchangeHeaderEditor SetTime(TimeOnly time)
    {
        _editor.SetComponent(4, 2, time.ToString("HHmm"));
        return this;
    }

    /// <summary>Sets UNB02-1 — Sender Identification.</summary>
    public EdifactInterchangeHeaderEditor SetSenderId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _editor.SetComponent(2, 1, id);
        return this;
    }

    /// <summary>Sets UNB03-1 — Recipient Identification.</summary>
    public EdifactInterchangeHeaderEditor SetReceiverId(string id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        _editor.SetComponent(3, 1, id);
        return this;
    }

    /// <summary>Sets UNB11 to "1" for test, or empty string for production.</summary>
    public EdifactInterchangeHeaderEditor SetTestIndicator(bool isTest)
    {
        _editor.SetElement(11, isTest ? "1" : string.Empty);
        return this;
    }

    internal EdiSegment Build() => _editor.Build();
}