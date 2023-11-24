using System.Threading;
using System.Threading.Tasks;

namespace CodeTiger.Azure.Cosmos;

/// <summary>
/// Provides functionality for executing an <see cref="IAggregateDocumentQuery{T}"/> instance.
/// </summary>
/// <typeparam name="T">The type of the results returned from the aggregate query.</typeparam>
internal interface IAggregateDocumentQueryExecutionContext<T>
{
    /// <summary>
    /// Gets whether all results of the <see cref="IAggregateDocumentQuery{T}"/> instance have been returned.
    /// </summary>
    bool IsDone { get; }

    /// <summary>
    /// Executes the query and retrieves the next page of aggregate results.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel execution.
    /// </param>
    /// <returns>An <see cref="AggregateQueryResponse{T}"/> object containing the next page of aggregate results
    /// and additional information about the current execution.</returns>
    Task<AggregateQueryResponse<T>> ExecuteNextAsync(CancellationToken cancellationToken);
}
