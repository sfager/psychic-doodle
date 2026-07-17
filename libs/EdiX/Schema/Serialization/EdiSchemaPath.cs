namespace EdiX.Schema.Serialization;

/// <summary>
/// Computes the canonical file path for an EDI schema based on its key.
/// </summary>
/// <remarks>
/// <para>Path conventions:</para>
/// <list type="bullet">
///   <item>X12: <c>edi-schemas/X12/{version}{release}/{transactionType}.json</c></item>
///   <item>EDIFACT: <c>edi-schemas/EDIFACT/{version}{release}/{messageType}.json</c></item>
/// </list>
/// <para>Examples:</para>
/// <list type="bullet">
///   <item>X12 850 v004010 → <c>edi-schemas/X12/004010/850.json</c></item>
///   <item>EDIFACT ORDERS D96A → <c>edi-schemas/EDIFACT/D96A/ORDERS.json</c></item>
/// </list>
/// </remarks>
public static class EdiSchemaPath
{
    private const string RootFolder = "edi-schemas";

    /// <summary>
    /// Returns the relative path (using forward slashes) for the given schema key.
    /// </summary>
    /// <param name="key">The schema key.</param>
    /// <returns>A relative path such as <c>edi-schemas/X12/004010/850.json</c>.</returns>
    public static string ForKey(EdiSchemaKey key)
    {
        string dialectFolder = key.Dialect == EdiDialect.X12 ? "X12" : "EDIFACT";
        string releaseVersion = key.Version + key.Release;
        string fileName = $"{key.TransactionType}.json";
        return $"{RootFolder}/{dialectFolder}/{releaseVersion}/{fileName}";
    }

    /// <summary>
    /// Returns the release-version segment of the path for the given schema key
    /// (e.g., <c>004010</c> for X12 v004010 or <c>D96A</c> for EDIFACT D.96A).
    /// </summary>
    /// <param name="key">The schema key.</param>
    /// <returns>The release-version string.</returns>
    public static string ReleaseVersion(EdiSchemaKey key) => key.Version + key.Release;
}
