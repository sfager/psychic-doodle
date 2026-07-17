namespace EdiX.Schema;

/// <summary>
/// Uniquely identifies a transaction schema by dialect and version information.
/// </summary>
public readonly record struct EdiSchemaKey
{
    /// <summary>
    /// Gets the EDI dialect (X12 or EDIFACT).
    /// </summary>
    public EdiDialect Dialect { get; init; }
    
    /// <summary>
    /// Gets the transaction type code (e.g., "850" for X12, "ORDERS" for EDIFACT).
    /// </summary>
    public string TransactionType { get; init; }
    
    /// <summary>
    /// Gets the version (e.g., "004" for X12 4010, "D" for EDIFACT).
    /// </summary>
    public string Version { get; init; }
    
    /// <summary>
    /// Gets the release (e.g., "010" for X12 4010, "96A" for EDIFACT D.96A).
    /// </summary>
    public string Release { get; init; }
    
    /// <summary>
    /// Gets the controlling agency code (EDIFACT only, e.g., "UN"). Null for X12.
    /// </summary>
    public string? ControllingAgency { get; init; }
    
    /// <summary>
    /// Gets the association code (EDIFACT only, e.g., "EAN008"). Null for X12.
    /// </summary>
    public string? AssociationCode { get; init; }

    /// <summary>
    /// Creates a schema key for an X12 transaction set.
    /// </summary>
    /// <param name="transactionType">The transaction set code (e.g., "850").</param>
    /// <param name="version">The version (e.g., "004").</param>
    /// <param name="release">The release (e.g., "010").</param>
    /// <returns>An X12 schema key.</returns>
    public static EdiSchemaKey ForX12(string transactionType, string version, string release)
    {
        return new EdiSchemaKey
        {
            Dialect = EdiDialect.X12,
            TransactionType = transactionType,
            Version = version,
            Release = release,
            ControllingAgency = null,
            AssociationCode = null
        };
    }

    /// <summary>
    /// Creates a schema key for an EDIFACT message.
    /// </summary>
    /// <param name="messageType">The message type (e.g., "ORDERS").</param>
    /// <param name="version">The version (e.g., "D").</param>
    /// <param name="release">The release (e.g., "96A").</param>
    /// <param name="controllingAgency">The controlling agency (default: "UN").</param>
    /// <param name="associationCode">Optional association code (e.g., "EAN008").</param>
    /// <returns>An EDIFACT schema key.</returns>
    public static EdiSchemaKey ForEdifact(string messageType, string version, string release,
        string controllingAgency = "UN", string? associationCode = null)
    {
        return new EdiSchemaKey
        {
            Dialect = EdiDialect.Edifact,
            TransactionType = messageType,
            Version = version,
            Release = release,
            ControllingAgency = controllingAgency,
            AssociationCode = associationCode
        };
    }
}