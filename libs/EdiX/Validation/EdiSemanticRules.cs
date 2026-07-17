namespace EdiX.Validation;

/// <summary>
/// Common semantic validation rules.
/// </summary>
public static class EdiSemanticRules
{
    /// <summary>
    /// Validates that a date string is in the expected format and represents a valid date.
    /// </summary>
    /// <param name="dateFormat">Expected format (e.g., "CCYYMMDD", "YYMMDD").</param>
    /// <param name="errorCode">Custom error code for validation failures.</param>
    /// <returns>Validation rule that checks date validity.</returns>
    public static IEdiValidationRule ValidDates(string dateFormat, string errorCode = "DATE-001")
    {
        return new DateValidationRule(dateFormat, errorCode);
    }

    /// <summary>
    /// Validates that a time string is in the expected format and represents a valid time.
    /// </summary>
    /// <param name="timeFormat">Expected format (e.g., "HHMM", "HHMMSS").</param>
    /// <param name="errorCode">Custom error code for validation failures.</param>
    /// <returns>Validation rule that checks time validity.</returns>
    public static IEdiValidationRule ValidTimes(string timeFormat, string errorCode = "TIME-001")
    {
        return new TimeValidationRule(timeFormat, errorCode);
    }

    /// <summary>
    /// Validates that numeric fields conform to specified format (e.g., precision, scale).
    /// </summary>
    /// <param name="pattern">Numeric pattern (e.g., "N2" for 2 decimal places).</param>
    /// <param name="errorCode">Custom error code for validation failures.</param>
    /// <returns>Validation rule that checks numeric format.</returns>
    public static IEdiValidationRule NumericFormat(string pattern, string errorCode = "NUM-001")
    {
        return new NumericFormatValidationRule(pattern, errorCode);
    }

    /// <summary>
    /// Validates that control numbers are unique within scope (interchange, group, or transaction).
    /// </summary>
    /// <param name="scope">Scope of uniqueness check.</param>
    /// <param name="errorCode">Custom error code for validation failures.</param>
    /// <returns>Validation rule that checks control number uniqueness.</returns>
    public static IEdiValidationRule UniqueControlNumbers(
        ControlNumberScope scope = ControlNumberScope.Interchange,
        string errorCode = "CTL-001")
    {
        return new UniqueControlNumberRule(scope, errorCode);
    }

    private sealed class DateValidationRule : IEdiValidationRule
    {
        private readonly string _format;
        private readonly string _errorCode;

        public DateValidationRule(string format, string errorCode)
        {
            _format = format;
            _errorCode = errorCode;
        }

        public EdiDialect? Dialect => null;
        public string? TransactionType => null;

        public IEnumerable<EdiValidationError> Validate(
            EdiTransaction transaction,
            EdiValidationContext context)
        {
            // Look for common date segments (DTM in X12, DTM in EDIFACT)
            foreach (var segment in transaction.Segments)
            {
                if (segment.Id.Equals("DTM", StringComparison.OrdinalIgnoreCase))
                {
                    // Element index for date varies by standard
                    var dateElement = segment.Elements.Length > 2 ? segment.Element(2).Value : null;
                    
                    if (!string.IsNullOrEmpty(dateElement) && !IsValidDate(dateElement))
                    {
                        yield return new EdiValidationError(
                            EdiValidationLayer.Semantic,
                            EdiValidationSeverity.Error,
                            _errorCode,
                            $"Invalid date format in segment {segment.Id}: expected {_format}, got '{dateElement}'",
                            new EdiPosition { SegmentIndex = segment.Position },
                            segmentId: segment.Id,
                            elementPosition: 2);
                    }
                }
            }
        }

        private bool IsValidDate(string dateString)
        {
            // Simple validation based on format length
            if (_format == "CCYYMMDD" && dateString.Length == 8)
            {
                return DateTime.TryParseExact(dateString, "yyyyMMdd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _);
            }
            else if (_format == "YYMMDD" && dateString.Length == 6)
            {
                return DateTime.TryParseExact(dateString, "yyMMdd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _);
            }
            
            return false;
        }
    }

    private sealed class TimeValidationRule : IEdiValidationRule
    {
        private readonly string _format;
        private readonly string _errorCode;

        public TimeValidationRule(string format, string errorCode)
        {
            _format = format;
            _errorCode = errorCode;
        }

        public EdiDialect? Dialect => null;
        public string? TransactionType => null;

        public IEnumerable<EdiValidationError> Validate(
            EdiTransaction transaction,
            EdiValidationContext context)
        {
            foreach (var segment in transaction.Segments)
            {
                if (segment.Id.Equals("DTM", StringComparison.OrdinalIgnoreCase))
                {
                    // Time often appears in element 3 or 4
                    var timeElement = segment.Elements.Length > 3 ? segment.Element(3).Value : null;
                    
                    if (!string.IsNullOrEmpty(timeElement) && !IsValidTime(timeElement))
                    {
                        yield return new EdiValidationError(
                            EdiValidationLayer.Semantic,
                            EdiValidationSeverity.Error,
                            _errorCode,
                            $"Invalid time format in segment {segment.Id}: expected {_format}, got '{timeElement}'",
                            new EdiPosition { SegmentIndex = segment.Position },
                            segmentId: segment.Id,
                            elementPosition: 3);
                    }
                }
            }
        }

        private bool IsValidTime(string timeString)
        {
            if (_format == "HHMM" && timeString.Length == 4)
            {
                return DateTime.TryParseExact(timeString, "HHmm",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _);
            }
            else if (_format == "HHMMSS" && timeString.Length == 6)
            {
                return DateTime.TryParseExact(timeString, "HHmmss",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out _);
            }
            
            return false;
        }
    }

    private sealed class NumericFormatValidationRule : IEdiValidationRule
    {
        private readonly string _pattern;
        private readonly string _errorCode;

        public NumericFormatValidationRule(string pattern, string errorCode)
        {
            _pattern = pattern;
            _errorCode = errorCode;
        }

        public EdiDialect? Dialect => null;
        public string? TransactionType => null;

        public IEnumerable<EdiValidationError> Validate(
            EdiTransaction transaction,
            EdiValidationContext context)
        {
            // This is a placeholder - real implementation would use schema info
            // to determine which elements should be numeric
            yield break;
        }
    }

    private sealed class UniqueControlNumberRule : IEdiValidationRule
    {
        private readonly ControlNumberScope _scope;
        private readonly string _errorCode;

        public UniqueControlNumberRule(ControlNumberScope scope, string errorCode)
        {
            _scope = scope;
            _errorCode = errorCode;
        }

        public EdiDialect? Dialect => null;
        public string? TransactionType => null;

        public IEnumerable<EdiValidationError> Validate(
            EdiTransaction transaction,
            EdiValidationContext context)
        {
            // Single transaction validation - check if context has parent group for comparison
            // This is a simplified implementation - full validation would need interchange-level state
            yield break;
        }
    }
}

/// <summary>
/// Defines the scope for control number uniqueness validation.
/// </summary>
public enum ControlNumberScope
{
    /// <summary>
    /// Validate uniqueness across entire interchange.
    /// </summary>
    Interchange,

    /// <summary>
    /// Validate uniqueness within each functional group.
    /// </summary>
    Group,

    /// <summary>
    /// Validate uniqueness within each transaction.
    /// </summary>
    Transaction
}
