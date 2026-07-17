namespace EdiX.Edifact;

/// <summary>Typed accessor for the EDIFACT UNB (Interchange Header) segment. Zero-allocation readonly struct.</summary>
public readonly struct EdifactInterchangeHeader
{
    private readonly EdiSegment _unb;
    internal EdifactInterchangeHeader(EdiSegment unb) => _unb = unb;

    /// <summary>UNB01-1 — Syntax Identifier.</summary>
    public string? SyntaxIdentifier    => _unb.Element(1).Component(1).Value;  // UNB01-1
    /// <summary>UNB01-2 — Syntax Version Number.</summary>
    public string? SyntaxVersion       => _unb.Element(1).Component(2).Value;  // UNB01-2
    /// <summary>UNB02-1 — Sender Identification.</summary>
    public string? SenderId            => _unb.Element(2).Component(1).Value;  // UNB02-1
    /// <summary>UNB02-2 — Sender Code Qualifier.</summary>
    public string? SenderQualifier     => _unb.Element(2).Component(2).Value;  // UNB02-2
    /// <summary>UNB03-1 — Recipient Identification.</summary>
    public string? ReceiverId          => _unb.Element(3).Component(1).Value;  // UNB03-1
    /// <summary>UNB03-2 — Recipient Code Qualifier.</summary>
    public string? ReceiverQualifier   => _unb.Element(3).Component(2).Value;  // UNB03-2
    /// <summary>UNB04-1 — Date (yyMMdd raw).</summary>
    public string? RawDate             => _unb.Element(4).Component(1).Value;  // UNB04-1
    /// <summary>UNB04-1 parsed to DateOnly. Null when absent or unparseable.</summary>
    public DateOnly? Date =>
        DateOnly.TryParseExact(RawDate, "yyMMdd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var d) ? d : null;
    /// <summary>UNB04-2 — Time (HHmm raw).</summary>
    public string? RawTime             => _unb.Element(4).Component(2).Value;  // UNB04-2
    /// <summary>UNB04-2 parsed to TimeOnly. Null when absent or unparseable.</summary>
    public TimeOnly? Time =>
        TimeOnly.TryParseExact(RawTime, "HHmm",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var t) ? t : null;
    /// <summary>UNB05 — Interchange Control Reference.</summary>
    public string? ControlReference    => _unb.ElementValue(5);               // UNB05
    /// <summary>UNB06-1 — Recipient Reference/Password.</summary>
    public string? RecipientReference  => _unb.Element(6).Component(1).Value; // UNB06-1
    /// <summary>UNB07 — Application Reference.</summary>
    public string? ApplicationReference => _unb.ElementValue(7);              // UNB07
    /// <summary>UNB08 — Processing Priority Code.</summary>
    public string? ProcessingPriority  => _unb.ElementValue(8);               // UNB08
    /// <summary>UNB09 — Acknowledgement Request.</summary>
    public string? AcknowledgementRequest => _unb.ElementValue(9);            // UNB09
    /// <summary>UNB10 — Interchange Agreement Identifier.</summary>
    public string? AgreementIdentifier => _unb.ElementValue(10);              // UNB10
    /// <summary>UNB11 — Test Indicator (true when value is "1").</summary>
    public bool IsTest                 => _unb.ElementValue(11) == "1";       // UNB11
}

/// <summary>Typed accessor for the EDIFACT UNZ (Interchange Trailer) segment.</summary>
public readonly struct EdifactInterchangeTrailer
{
    private readonly EdiSegment _unz;
    internal EdifactInterchangeTrailer(EdiSegment unz) => _unz = unz;
    /// <summary>UNZ01 — Interchange Control Count.</summary>
    public string? ControlCount     => _unz.ElementValue(1);
    /// <summary>UNZ02 — Interchange Control Reference.</summary>
    public string? ControlReference => _unz.ElementValue(2);
}

/// <summary>Typed accessor for the EDIFACT UNG (Functional Group Header) segment.</summary>
public readonly struct EdifactGroupHeader
{
    private readonly EdiSegment _ung;
    internal EdifactGroupHeader(EdiSegment ung) => _ung = ung;
    /// <summary>UNG01 — Message Group Identification.</summary>
    public string? MessageGroupId    => _ung.ElementValue(1);
    /// <summary>UNG02-1 — Application Sender Identification.</summary>
    public string? SenderId          => _ung.Element(2).Component(1).Value;
    /// <summary>UNG03-1 — Application Recipient Identification.</summary>
    public string? ReceiverId        => _ung.Element(3).Component(1).Value;
    /// <summary>UNG04-1 — Date (yyMMdd raw).</summary>
    public string? RawDate           => _ung.Element(4).Component(1).Value;
    /// <summary>UNG05 — Group Reference Number.</summary>
    public string? ControlNumber     => _ung.ElementValue(5);
    /// <summary>UNG06 — Controlling Agency.</summary>
    public string? ControllingAgency => _ung.ElementValue(6);
    /// <summary>UNG07-1 — Message Version Number.</summary>
    public string? MessageVersion    => _ung.Element(7).Component(1).Value;
    /// <summary>UNG07-2 — Message Release Number.</summary>
    public string? MessageRelease    => _ung.Element(7).Component(2).Value;
}

/// <summary>Typed accessor for the EDIFACT UNE (Functional Group Trailer) segment.</summary>
public readonly struct EdifactGroupTrailer
{
    private readonly EdiSegment _une;
    internal EdifactGroupTrailer(EdiSegment une) => _une = une;
    /// <summary>UNE01 — Number of Messages.</summary>
    public string? MessageCount  => _une.ElementValue(1);
    /// <summary>UNE02 — Group Reference Number.</summary>
    public string? ControlNumber => _une.ElementValue(2);
}

/// <summary>Typed accessor for the EDIFACT UNH (Message Header) segment.</summary>
public readonly struct EdifactMessageHeader
{
    private readonly EdiSegment _unh;
    internal EdifactMessageHeader(EdiSegment unh) => _unh = unh;
    /// <summary>UNH01 — Message Reference Number.</summary>
    public string? MessageReference    => _unh.ElementValue(1);
    /// <summary>UNH02-1 — Message Type.</summary>
    public string? MessageType         => _unh.Element(2).Component(1).Value;  // UNH02-1
    /// <summary>UNH02-2 — Message Version Number.</summary>
    public string? Version             => _unh.Element(2).Component(2).Value;  // UNH02-2
    /// <summary>UNH02-3 — Message Release Number.</summary>
    public string? Release             => _unh.Element(2).Component(3).Value;  // UNH02-3
    /// <summary>UNH02-4 — Controlling Agency.</summary>
    public string? ControllingAgency   => _unh.Element(2).Component(4).Value;  // UNH02-4
    /// <summary>UNH02-5 — Association Assigned Code.</summary>
    public string? AssociationCode     => _unh.Element(2).Component(5).Value;  // UNH02-5
    /// <summary>UNH03 — Common Access Reference.</summary>
    public string? CommonAccessReference => _unh.ElementValue(3);
}

/// <summary>Typed accessor for the EDIFACT UNT (Message Trailer) segment.</summary>
public readonly struct EdifactMessageTrailer
{
    private readonly EdiSegment _unt;
    internal EdifactMessageTrailer(EdiSegment unt) => _unt = unt;
    /// <summary>UNT01 — Number of Segments.</summary>
    public string? SegmentCount      => _unt.ElementValue(1);
    /// <summary>UNT02 — Message Reference Number.</summary>
    public string? MessageReference  => _unt.ElementValue(2);
}