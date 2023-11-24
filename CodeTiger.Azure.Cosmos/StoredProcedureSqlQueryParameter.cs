using Newtonsoft.Json;

namespace CodeTiger.Azure.Cosmos;

/// <summary>
/// Represents a parameter that a stored procedure can include in a SQL query.
/// </summary>
internal class StoredProcedureSqlQueryParameter
{
    /// <summary>
    /// Gets or sets the name of the parameter, which should typically include an <c>@</c> prefix.
    /// </summary>
    [JsonProperty("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the parameter.
    /// </summary>
    [JsonProperty("value")]
    public object? Value { get; set; }
}
