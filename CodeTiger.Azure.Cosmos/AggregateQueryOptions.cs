using Microsoft.Azure.Cosmos;

namespace CodeTiger.Azure.Cosmos
{
    /// <summary>
    /// Specifies the options for the aggregate query.
    /// </summary>
    public sealed class AggregateQueryOptions
    {
        /// <summary>
        /// Gets or sets the partition key for the aggregate query.
        /// </summary>
        public PartitionKey PartitionKey { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of aggregate results to be returned.
        /// </summary>
        public int? MaxItemCount { get; set; }

        /// <summary>
        /// Gets or sets the token used to continue the aggregate query from a previous request.
        /// </summary>
        public string? RequestContinuation { get; set; }
    }
}
