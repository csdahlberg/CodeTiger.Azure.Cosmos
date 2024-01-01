using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace CodeTiger.Azure.Cosmos;

/// <summary>
/// Provides functionality for querying aggregates of documents in Cosmos DB.
/// </summary>
/// <typeparam name="T">The type of the results of the query.</typeparam>
internal class AggregateDocumentQuery<T> : IAggregateDocumentQuery<T>, IAggregateQueryable<T>
{
    private readonly Container _container;
    private readonly AggregateQueryOptions _queryOptions;

    private AggregateDocumentQueryExecutionContext<T>? _executionContext;

    /// <summary>
    /// Gets the expression tree that is associated with this instance of <see cref="AggregateDocumentQuery{T}"/>.
    /// </summary>
    public Expression Expression { get; }

    /// <summary>
    /// Gets the aggregate query provider that is associated with this data source.
    /// </summary>
    public IAggregateQueryProvider Provider { get; }

    /// <summary>
    /// Gets whether there are potentially additional results that can be returned from the query by subsequent
    /// calls to <see cref="ExecuteNextAsync(CancellationToken)"/>.
    /// </summary>
    public bool HasMoreResults => _executionContext?.IsDone != true;

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateDocumentQuery{T}"/> class.
    /// </summary>
    /// <param name="container">The <see cref="Container"/> object to use to interact with the Cosmos database when
    /// executing aggregate document queries.</param>
    /// <param name="queryOptions">Specifies the options for the aggregate query.</param>
    public AggregateDocumentQuery(Container container, AggregateQueryOptions queryOptions)
        : this(container, queryOptions, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateDocumentQuery{T}"/> class.
    /// </summary>
    /// <param name="container">The <see cref="Container"/> object to use to interact with the Cosmos database when
    /// executing aggregate document queries.</param>
    /// <param name="queryOptions">Specifies the options for the aggregate query.</param>
    /// <param name="expression">The expression representing the query for aggregate data, or <c>null</c> if this
    /// is the initial <see cref="AggregateDocumentQuery{T}"/> being created for a query.</param>
    public AggregateDocumentQuery(Container container, AggregateQueryOptions queryOptions,
        Expression? expression)
    {
        _container = container;
        _queryOptions = queryOptions;

        Expression = expression ?? Expression.Constant(this);
        Provider = new AggregateQueryProvider(container, queryOptions);
    }

    /// <summary>
    /// Executes the query and retrieves the next page of aggregate results.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel execution.
    /// </param>
    /// <returns>An <see cref="AggregateQueryResponse{T}"/> object containing the next page of aggregate results
    /// and additional information about the current execution.</returns>
    public async Task<AggregateQueryResponse<T>> ExecuteNextAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        if (_executionContext == null)
        {
            _executionContext = new AggregateDocumentQueryExecutionContext<T>(_container,
                _queryOptions, Expression);
        }
        else if (_executionContext.IsDone)
        {
            throw new InvalidOperationException();
        }

        return await _executionContext.ExecuteNextAsync(cancellationToken).ConfigureAwait(false);
    }
}
