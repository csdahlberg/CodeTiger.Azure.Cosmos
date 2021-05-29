using System;
using System.Threading;
using System.Threading.Tasks;

namespace CodeTiger.Azure.Cosmos
{
    /// <summary>
    /// Provides functionality for querying aggregates of documents in Cosmos DB.
    /// </summary>
    /// <typeparam name="T">The type of the aggregate results of the query.</typeparam>
    public interface IAggregateDocumentQuery<T>
    {
        /// <summary>
        /// Gets whether there are potentially additional results that can be returned from the query by subsequent
        /// calls to <see cref="ExecuteNextAsync(CancellationToken)"/>.
        /// </summary>
        bool HasMoreResults { get; }

        /// <summary>
        /// Executes the query and retrieves the next page of aggregate results.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel execution.
        /// </param>
        /// <returns>An <see cref="AggregateQueryResponse{T}"/> object containing the next page of aggregate
        /// results and additional information about the current execution.</returns>
        Task<AggregateQueryResponse<T>> ExecuteNextAsync(
            CancellationToken cancellationToken = default(CancellationToken));
    }
}
