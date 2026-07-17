using System.Collections.Immutable;

namespace EdiX.Schema;

/// <summary>
/// Registry for looking up transaction schemas by key.
/// </summary>
public sealed class EdiSchemaRegistry
{
    private static readonly object _defaultLock = new();
    private static EdiSchemaRegistry? _defaultInstance;
    private static readonly List<EdiTransactionSchema> _builtInSchemas = new();
    
    private readonly ImmutableDictionary<EdiSchemaKey, EdiTransactionSchema> _schemas;

    /// <summary>
    /// Gets the default global schema registry.
    /// </summary>
    public static EdiSchemaRegistry Default
    {
        get
        {
            if (_defaultInstance == null)
            {
                lock (_defaultLock)
                {
                    if (_defaultInstance == null)
                    {
                        // Build default instance with all registered built-in schemas
                        var registry = new EdiSchemaRegistry();
                        foreach (var schema in _builtInSchemas)
                        {
                            registry = registry.WithSchema(schema);
                        }
                        _defaultInstance = registry;
                    }
                }
            }
            return _defaultInstance;
        }
    }

    /// <summary>
    /// Gets the strategy used to resolve schemas when exact matches are not found.
    /// </summary>
    public SchemaResolutionStrategy ResolutionStrategy { get; init; }

    /// <summary>
    /// Gets the collection of registered schema keys.
    /// </summary>
    public IReadOnlyCollection<EdiSchemaKey> RegisteredKeys => _schemas.Keys.ToList();

    /// <summary>
    /// Initializes a new empty registry.
    /// </summary>
    public EdiSchemaRegistry()
        : this(ImmutableDictionary<EdiSchemaKey, EdiTransactionSchema>.Empty, SchemaResolutionStrategy.ExactOnly)
    {
    }

    private EdiSchemaRegistry(
        ImmutableDictionary<EdiSchemaKey, EdiTransactionSchema> schemas,
        SchemaResolutionStrategy resolutionStrategy)
    {
        _schemas = schemas;
        ResolutionStrategy = resolutionStrategy;
    }

    /// <summary>
    /// Gets a schema by key, applying the configured resolution strategy.
    /// </summary>
    /// <param name="key">The schema key.</param>
    /// <returns>The schema if found; otherwise null.</returns>
    public EdiTransactionSchema? GetSchema(EdiSchemaKey key)
    {
        // Try exact match first
        if (_schemas.TryGetValue(key, out var schema))
        {
            return schema;
        }

        // Apply resolution strategy
        return ResolutionStrategy switch
        {
            SchemaResolutionStrategy.ExactOnly => null,
            SchemaResolutionStrategy.LatestMatchingVersion => GetLatestMatchingVersion(key),
            SchemaResolutionStrategy.AnyVersion => GetAnyVersion(key),
            _ => null
        };
    }

    /// <summary>
    /// Gets a schema by individual components.
    /// </summary>
    /// <param name="transactionType">The transaction type.</param>
    /// <param name="dialect">The EDI dialect.</param>
    /// <param name="version">The version.</param>
    /// <param name="release">The release.</param>
    /// <param name="associationCode">Optional association code (EDIFACT only).</param>
    /// <returns>The schema if found; otherwise null.</returns>
    public EdiTransactionSchema? GetSchema(
        string transactionType,
        EdiDialect dialect,
        string version,
        string release,
        string? associationCode = null)
    {
        var key = dialect == EdiDialect.X12
            ? EdiSchemaKey.ForX12(transactionType, version, release)
            : EdiSchemaKey.ForEdifact(transactionType, version, release, "UN", associationCode);

        return GetSchema(key);
    }

    /// <summary>
    /// Returns a new registry with the specified schema added.
    /// </summary>
    /// <param name="schema">The schema to add.</param>
    /// <returns>A new registry instance.</returns>
    public EdiSchemaRegistry WithSchema(EdiTransactionSchema schema)
    {
        return new EdiSchemaRegistry(
            _schemas.SetItem(schema.Key, schema),
            ResolutionStrategy);
    }

    /// <summary>
    /// Returns a new registry with the specified schemas added.
    /// </summary>
    /// <param name="schemas">The schemas to add.</param>
    /// <returns>A new registry instance.</returns>
    public EdiSchemaRegistry WithSchemas(IEnumerable<EdiTransactionSchema> schemas)
    {
        var builder = _schemas.ToBuilder();
        foreach (var schema in schemas)
        {
            builder[schema.Key] = schema;
        }
        return new EdiSchemaRegistry(builder.ToImmutable(), ResolutionStrategy);
    }

    /// <summary>
    /// Returns a new registry with the specified resolution strategy.
    /// </summary>
    /// <param name="strategy">The resolution strategy.</param>
    /// <returns>A new registry instance.</returns>
    public EdiSchemaRegistry WithResolutionStrategy(SchemaResolutionStrategy strategy)
    {
        return new EdiSchemaRegistry(_schemas, strategy);
    }

    private EdiTransactionSchema? GetLatestMatchingVersion(EdiSchemaKey key)
    {
        // Find all keys with matching TransactionType, Dialect, and Version
        // Return the one with lexicographically largest Release
        var matches = _schemas
            .Where(kvp =>
                kvp.Key.TransactionType == key.TransactionType &&
                kvp.Key.Dialect == key.Dialect &&
                kvp.Key.Version == key.Version)
            .OrderByDescending(kvp => kvp.Key.Release, StringComparer.Ordinal)
            .ToList();

        return matches.FirstOrDefault().Value;
    }

    private EdiTransactionSchema? GetAnyVersion(EdiSchemaKey key)
    {
        // Find any schema with matching TransactionType and Dialect
        var match = _schemas
            .FirstOrDefault(kvp =>
                kvp.Key.TransactionType == key.TransactionType &&
                kvp.Key.Dialect == key.Dialect);

        return match.Value;
    }

    /// <summary>
    /// Registers a built-in schema with the default registry.
    /// Called by module initializers in schema packages.
    /// Must be called before the default registry is first accessed.
    /// </summary>
    /// <param name="schema">The schema to register.</param>
    internal static void RegisterBuiltIn(EdiTransactionSchema schema)
    {
        lock (_defaultLock)
        {
            if (_defaultInstance != null)
            {
                throw new InvalidOperationException(
                    "Built-in schemas must be registered via [ModuleInitializer] before first access to EdiSchemaRegistry.Default.");
            }
            _builtInSchemas.Add(schema);
        }
    }
}