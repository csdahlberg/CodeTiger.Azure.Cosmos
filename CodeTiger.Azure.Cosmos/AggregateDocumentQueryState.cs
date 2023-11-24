using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CodeTiger.Azure.Cosmos;

/// <summary>
/// Contains data about the state of an aggregate document query.
/// </summary>
internal class AggregateDocumentQueryState
{
    /// <summary>
    /// Gets or sets the parameters that the stored procedure passes in to the SQL query used to retrieve the
    /// source documents from Cosmos DB.
    /// </summary>
    [JsonProperty("parameters")]
    public List<StoredProcedureSqlQueryParameter>? Parameters { get; set; }

    /// <summary>
    /// Gets or sets the maximum number of aggregate results to be returned.
    /// </summary>
    [JsonProperty("maxResultCount")]
    public int? MaxResultCount { get; set; }

    /// <summary>
    /// Gets or sets the token used to continue the aggregate query from a previous request.
    /// </summary>
    [JsonProperty("continuationToken")]
    public string? ContinuationToken { get; set; }

    /// <summary>
    /// Gets or sets the partial aggregate result that is currently being aggregated.
    /// </summary>
    [JsonProperty("partialAggregate")]
    public JObject? PartialAggregate { get; set; }

    /// <summary>
    /// Gets or sets the most recent source document processed while executing the query, or <c>null</c> if no such
    /// documents have been processed.
    /// </summary>
    [JsonProperty("previousSourceDocument")]
    public JObject? PreviousSourceDocument { get; set; }

    /// <summary>
    /// Gets or sets the aggregate results returned by the current execution of the aggregate query.
    /// </summary>
    [JsonProperty("results")]
    public List<JObject>? Results { get; set; }

    /// <summary>
    /// Gets or sets the total number of aggregate documents that have been returned by this aggregate query.
    /// </summary>
    [JsonProperty("returnedResultCount")]
    public int ReturnedResultCount { get; set; }
}
