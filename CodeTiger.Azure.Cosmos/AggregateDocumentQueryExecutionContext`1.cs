using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Newtonsoft.Json;

namespace CodeTiger.Azure.Cosmos
{
    /// <summary>
    /// Provides functionality for executing an <see cref="IAggregateDocumentQuery{T}"/> instance.
    /// </summary>
    /// <typeparam name="T">The type of the results returned from the aggregate query.</typeparam>
    internal class AggregateDocumentQueryExecutionContext<T> : IAggregateDocumentQueryExecutionContext<T>
    {
        private readonly Container _container;
        private readonly Expression _expression;
        private readonly AggregateQueryOptions _queryOptions;

        private AggregateStoredProcedure? _storedProcedure;
        private AggregateDocumentQueryState? _queryState;

        /// <summary>
        /// Gets whether all results of the <see cref="IAggregateDocumentQuery{T}"/> instance have been returned.
        /// </summary>
        public bool IsDone => _queryState != null && string.IsNullOrEmpty(_queryState.ContinuationToken);

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateDocumentQueryExecutionContext{T}"/> class.
        /// </summary>
        /// <param name="container">The <see cref="Container"/> object to use to interact with the Cosmos database
        /// when executing aggregate document queries.</param>
        /// <param name="queryOptions">Specifies the options for the aggregate query.</param>
        /// <param name="expression">The expression representing the query for aggregate data.</param>
        public AggregateDocumentQueryExecutionContext(Container container,
            AggregateQueryOptions queryOptions, Expression expression)
        {
            _container = container;
            _expression = expression;
            _queryOptions = queryOptions;
        }

        /// <summary>
        /// Executes the query and retrieves the next page of aggregate results.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel execution.
        /// </param>
        /// <returns>An <see cref="AggregateQueryResponse{T}"/> object containing the next page of aggregate
        /// results and additional information about the current execution.</returns>
        public async Task<AggregateQueryResponse<T>> ExecuteNextAsync(CancellationToken cancellationToken)
        {
            if (_storedProcedure == null)
            {
                _storedProcedure = AggregateDocumentQueryEvaluator.Evaluate(_expression);

                if (!string.IsNullOrWhiteSpace(_queryOptions.RequestContinuation))
                {
                    _queryState = JsonConvert.DeserializeObject<AggregateDocumentQueryState>(
                        _queryOptions.RequestContinuation);
                }
                else
                {
                    _queryState = new AggregateDocumentQueryState
                    {
                        Parameters = _storedProcedure.Parameters
                            .Select(x => new StoredProcedureSqlQueryParameter { Name = x.Key, Value = x.Value })
                            .ToList(),
                        MaxResultCount = _queryOptions.MaxItemCount,
                    };
                }
            }
            
            var response = await _container.Scripts
                .ExecuteStoredProcedureAsync<AggregateDocumentQueryState>(_storedProcedure.Id,
                    _queryOptions.PartitionKey, new[] { _queryState }, null, cancellationToken)
                .ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                // The stored procedure does not exist. Create it, then attempt to execute it again.
                try
                {
                    await _container.Scripts
                        .CreateStoredProcedureAsync(
                            new StoredProcedureProperties(_storedProcedure.Id, _storedProcedure.Body),
                            cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                }
                catch (CosmosException ex2)
                {
                    if (ex2.StatusCode != HttpStatusCode.Conflict)
                    {
                        throw;
                    }

                    // The stored procedure was likely just now created by something else
                }

                response = await _container.Scripts
                    .ExecuteStoredProcedureAsync<AggregateDocumentQueryState>(_storedProcedure.Id,
                        _queryOptions.PartitionKey, new[] { _queryState }, null, cancellationToken)
                    .ConfigureAwait(false);
            }

            _queryState = response.Resource;

            return new AggregateQueryResponse<T>(response);
        }
    }
}
