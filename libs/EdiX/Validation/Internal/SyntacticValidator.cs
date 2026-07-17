using EdiX;

namespace EdiX.Validation.Internal;

/// <summary>
/// Validates syntactic correctness of EDI documents (control numbers, segment counts, etc.).
/// </summary>
internal static class SyntacticValidator
{
    public static IEnumerable<EdiValidationError> Validate(EdiDocument document)
    {
        var interchange = document.Interchange;

        if (interchange.Dialect == EdiDialect.X12)
        {
            foreach (var error in ValidateX12Interchange(interchange))
                yield return error;
        }
        else if (interchange.Dialect == EdiDialect.Edifact)
        {
            foreach (var error in ValidateEdifactInterchange(interchange))
                yield return error;
        }
    }

    private static IEnumerable<EdiValidationError> ValidateX12Interchange(EdiInterchange interchange)
    {
        var isa = interchange.HeaderSegment;
        var iea = interchange.TrailerSegment;

        // X12-ISA-001: ISA does not contain exactly 16 elements
        if (isa.Elements.Length != 16)
        {
            yield return new EdiValidationError(
                EdiValidationLayer.Syntactic,
                EdiValidationSeverity.Error,
                "X12-ISA-001",
                "ISA segment must contain exactly 16 elements",
                new EdiPosition { SegmentIndex = 0 },
                segmentId: "ISA");
        }

        // X12-ISA-003: ISA13 (control number) is not numeric
        if (isa.Elements.Length > 13)
        {
            var controlNumber = isa.Element(13).Value;
            if (!string.IsNullOrEmpty(controlNumber) && !controlNumber.All(char.IsDigit))
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "X12-ISA-003",
                    "ISA13 control number must be numeric",
                    new EdiPosition { SegmentIndex = 0 },
                    segmentId: "ISA",
                    elementPosition: 13);
            }
        }

        // X12-ISA-004: IEA02 does not match ISA13
        if (isa.Elements.Length >= 13 && iea.Elements.Length >= 2)
        {
            var isaControl = isa.Element(13).Value?.TrimStart('0');
            var ieaControl = iea.Element(2).Value?.TrimStart('0');
            
            if (isaControl != ieaControl)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "X12-ISA-004",
                    $"IEA02 control number '{ieaControl}' does not match ISA13 '{isaControl}'",
                    new EdiPosition { SegmentIndex = -1 },
                    segmentId: "IEA",
                    elementPosition: 2);
            }
        }

        // X12-ISA-005: IEA01 does not equal number of functional groups
        if (iea.Elements.Length > 1)
        {
            var declaredCount = iea.Element(1).Value;
            var actualCount = interchange.Groups.Length.ToString();
            
            if (declaredCount != actualCount)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "X12-ISA-005",
                    $"IEA01 declares {declaredCount} groups but found {actualCount}",
                    new EdiPosition { SegmentIndex = -1 },
                    segmentId: "IEA",
                    elementPosition: 1);
            }
        }

        // Validate each group
        foreach (var group in interchange.Groups)
        {
            foreach (var error in ValidateX12Group(group))
                yield return error;
        }
    }

    private static IEnumerable<EdiValidationError> ValidateX12Group(EdiFunctionalGroup group)
    {
        var gs = group.HeaderSegment;
        var ge = group.TrailerSegment;

        // X12-GS-001: GS does not contain exactly 8 elements
        if (gs.Elements.Length != 8)
        {
            yield return new EdiValidationError(
                EdiValidationLayer.Syntactic,
                EdiValidationSeverity.Error,
                "X12-GS-001",
                "GS segment must contain exactly 8 elements",
                new EdiPosition { SegmentIndex = gs.Position },
                segmentId: "GS");
        }

        // X12-GS-002: GE02 does not match GS06
        if (gs.Elements.Length >= 6 && ge.Elements.Length >= 2)
        {
            var gsControl = gs.Element(6).Value;
            var geControl = ge.Element(2).Value;
            
            if (gsControl != geControl)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "X12-GS-002",
                    $"GE02 control number '{geControl}' does not match GS06 '{gsControl}'",
                    new EdiPosition { SegmentIndex = ge.Position },
                    segmentId: "GE",
                    elementPosition: 2);
            }
        }

        // X12-GS-003: GE01 does not equal number of transactions
        if (ge.Elements.Length > 1)
        {
            var declaredCount = ge.Element(1).Value;
            var actualCount = group.Transactions.Length.ToString();
            
            if (declaredCount != actualCount)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "X12-GS-003",
                    $"GE01 declares {declaredCount} transactions but found {actualCount}",
                    new EdiPosition { SegmentIndex = ge.Position },
                    segmentId: "GE",
                    elementPosition: 1);
            }
        }

        // Validate each transaction
        foreach (var transaction in group.Transactions)
        {
            foreach (var error in ValidateX12Transaction(transaction))
                yield return error;
        }
    }

    private static IEnumerable<EdiValidationError> ValidateX12Transaction(EdiTransaction transaction)
    {
        var st = transaction.HeaderSegment;
        var se = transaction.TrailerSegment;

        // X12-ST-001: ST does not contain at least 2 elements
        if (st.Elements.Length < 2)
        {
            yield return new EdiValidationError(
                EdiValidationLayer.Syntactic,
                EdiValidationSeverity.Error,
                "X12-ST-001",
                "ST segment must contain at least 2 elements",
                new EdiPosition { SegmentIndex = st.Position },
                segmentId: "ST");
        }

        // X12-ST-002: SE02 does not match ST02
        if (st.Elements.Length >= 2 && se.Elements.Length >= 2)
        {
            var stControl = st.Element(2).Value;
            var seControl = se.Element(2).Value;
            
            if (stControl != seControl)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "X12-ST-002",
                    $"SE02 control number '{seControl}' does not match ST02 '{stControl}'",
                    new EdiPosition { SegmentIndex = se.Position },
                    segmentId: "SE",
                    elementPosition: 2);
            }
        }

        // X12-ST-003: SE01 does not equal segment count (ST + body + SE)
        if (se.Elements.Length > 1)
        {
            var declaredCount = se.Element(1).Value;
            var actualCount = (transaction.Segments.Length + 2).ToString(); // ST + body + SE
            
            if (declaredCount != actualCount)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "X12-ST-003",
                    $"SE01 declares {declaredCount} segments but found {actualCount}",
                    new EdiPosition { SegmentIndex = se.Position },
                    segmentId: "SE",
                    elementPosition: 1);
            }
        }
    }

    private static IEnumerable<EdiValidationError> ValidateEdifactInterchange(EdiInterchange interchange)
    {
        var unb = interchange.HeaderSegment;
        var unz = interchange.TrailerSegment;

        // EDI-UNB-001: UNB missing sender or recipient identification
        if (unb.Elements.Length < 4)
        {
            yield return new EdiValidationError(
                EdiValidationLayer.Syntactic,
                EdiValidationSeverity.Error,
                "EDI-UNB-001",
                "UNB segment missing sender or recipient identification",
                new EdiPosition { SegmentIndex = 0 },
                segmentId: "UNB");
        }

        // EDI-UNB-002: UNZ02 does not match UNB05
        if (unb.Elements.Length >= 5 && unz.Elements.Length >= 2)
        {
            var unbControl = unb.Element(5).Value;
            var unzControl = unz.Element(2).Value;
            
            if (unbControl != unzControl)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "EDI-UNB-002",
                    $"UNZ02 control reference '{unzControl}' does not match UNB05 '{unbControl}'",
                    new EdiPosition { SegmentIndex = -1 },
                    segmentId: "UNZ",
                    elementPosition: 2);
            }
        }

        // EDI-UNB-003: UNZ01 does not equal number of messages/groups
        if (unz.Elements.Length > 1)
        {
            var declaredCount = unz.Element(1).Value;
            var actualCount = interchange.Groups.Length > 0
                ? interchange.Groups.Length.ToString()
                : interchange.Transactions.Length.ToString();
            
            if (declaredCount != actualCount)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "EDI-UNB-003",
                    $"UNZ01 declares {declaredCount} but found {actualCount}",
                    new EdiPosition { SegmentIndex = -1 },
                    segmentId: "UNZ",
                    elementPosition: 1);
            }
        }

        // Validate groups if present
        foreach (var group in interchange.Groups)
        {
            foreach (var error in ValidateEdifactGroup(group))
                yield return error;
        }

        // Validate ungrouped transactions
        foreach (var transaction in interchange.Transactions)
        {
            foreach (var error in ValidateEdifactTransaction(transaction))
                yield return error;
        }
    }

    private static IEnumerable<EdiValidationError> ValidateEdifactGroup(EdiFunctionalGroup group)
    {
        var ung = group.HeaderSegment;
        var une = group.TrailerSegment;

        // Similar validations for UNG/UNE as X12 GS/GE
        if (ung.Elements.Length > 5 && une.Elements.Length > 2)
        {
            var ungControl = ung.Element(5).Value;
            var uneControl = une.Element(2).Value;
            
            if (ungControl != uneControl)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "EDI-GROUP-002",
                    $"UNE02 control reference '{uneControl}' does not match UNG05 '{ungControl}'",
                    new EdiPosition { SegmentIndex = une.Position },
                    segmentId: "UNE",
                    elementPosition: 2);
            }
        }

        // Validate each transaction
        foreach (var transaction in group.Transactions)
        {
            foreach (var error in ValidateEdifactTransaction(transaction))
                yield return error;
        }
    }

    private static IEnumerable<EdiValidationError> ValidateEdifactTransaction(EdiTransaction transaction)
    {
        var unh = transaction.HeaderSegment;
        var unt = transaction.TrailerSegment;

        // EDI-UNH-001: UNH missing message reference or identifier
        if (unh.Elements.Length < 2)
        {
            yield return new EdiValidationError(
                EdiValidationLayer.Syntactic,
                EdiValidationSeverity.Error,
                "EDI-UNH-001",
                "UNH segment missing message reference or identifier",
                new EdiPosition { SegmentIndex = unh.Position },
                segmentId: "UNH");
        }

        // EDI-UNT-002: UNT02 does not match UNH01
        if (unh.Elements.Length >= 1 && unt.Elements.Length >= 2)
        {
            var unhRef = unh.Element(1).Value;
            var untRef = unt.Element(2).Value;
            
            if (unhRef != untRef)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "EDI-UNT-002",
                    $"UNT02 message reference '{untRef}' does not match UNH01 '{unhRef}'",
                    new EdiPosition { SegmentIndex = unt.Position },
                    segmentId: "UNT",
                    elementPosition: 2);
            }
        }

        // EDI-UNT-001: UNT01 does not equal segment count
        if (unt.Elements.Length > 1)
        {
            var declaredCount = unt.Element(1).Value;
            var actualCount = (transaction.Segments.Length + 2).ToString(); // UNH + body + UNT
            
            if (declaredCount != actualCount)
            {
                yield return new EdiValidationError(
                    EdiValidationLayer.Syntactic,
                    EdiValidationSeverity.Error,
                    "EDI-UNT-001",
                    $"UNT01 declares {declaredCount} segments but found {actualCount}",
                    new EdiPosition { SegmentIndex = unt.Position },
                    segmentId: "UNT",
                    elementPosition: 1);
            }
        }
    }
}
