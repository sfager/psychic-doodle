using System.Text;

namespace EdiX.Parsing.Internal;

/// <summary>
/// Detects delimiters from EDI interchange headers.
/// </summary>
internal static class DelimiterDetector
{
    /// <summary>
    /// Detects X12 delimiters from an ISA segment (exactly 106 bytes).
    /// </summary>
    /// <param name="isaBytes">The ISA segment bytes (must be exactly 106 bytes).</param>
    /// <returns>The detected delimiters.</returns>
    public static EdiDelimiters DetectX12(byte[] isaBytes)
    {
        if (isaBytes.Length != 106)
        {
            throw new ArgumentException("X12 ISA segment must be exactly 106 bytes", nameof(isaBytes));
        }

        // X12 ISA structure (fixed positions):
        // Position 3: Element separator
        // Position 82: Repetition separator (ISA11)
        // Position 104: Component separator (ISA16)
        // Position 105: Segment terminator
        
        char element = (char)isaBytes[3];
        char repetition = (char)isaBytes[82];
        char component = (char)isaBytes[104];
        char segment = (char)isaBytes[105];

        return new EdiDelimiters
        {
            Segment = segment,
            Element = element,
            Component = component,
            Repetition = repetition
        };
    }

    /// <summary>
    /// Detects EDIFACT delimiters from a UNA service string or uses defaults.
    /// </summary>
    /// <param name="stream">The input stream positioned at the start.</param>
    /// <param name="encoding">The text encoding.</param>
    /// <returns>The detected delimiters.</returns>
    public static EdiDelimiters DetectEdifact(Stream stream, Encoding encoding)
    {
        // Peek first 3 bytes to check for UNA
        var buffer = new byte[9];
        var bytesRead = stream.Read(buffer, 0, 9);
        stream.Position = 0;  // Reset to start

        if (bytesRead >= 9)
        {
            var header = encoding.GetString(buffer, 0, 3);
            if (header == "UNA")
            {
                // UNA service string advisory (9 bytes total):
                // UNA:+.? '
                // Position 3: Component separator
                // Position 4: Element separator
                // Position 5: Decimal notation
                // Position 6: Release character
                // Position 7: Reserved (space)
                // Position 8: Segment terminator
                
                return new EdiDelimiters
                {
                    Segment = (char)buffer[8],
                    Element = (char)buffer[4],
                    Component = (char)buffer[3],
                    Repetition = (char)buffer[7]  // EDIFACT doesn't use repetition, use reserved space
                };
            }
        }

        // Use EDIFACT defaults
        return EdiDelimiters.EdifactDefaults;
    }
}