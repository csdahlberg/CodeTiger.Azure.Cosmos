using System;
using Microsoft.Azure.Cosmos;

namespace CodeTiger.Azure.Cosmos
{
    /// <summary>
    /// Provides extension methods for creating aggregate document queries from <see cref="Container"/> objects.
    /// </summary>
    public static class CosmosContainerExtensions
    {
        /// <summary>
        /// Creates a query for aggregating data from documents in the Azure Cosmos DB service.
        /// </summary>
        /// <typeparam name="T">The type of the source documents.</typeparam>
        /// <param name="container">The document client to use to aggregate documents.</param>
        /// <param name="queryOptions">Specifies the options for the aggregate query.</param>
        /// <returns>A query for aggregating data from documents.</returns>
        public static IAggregateQueryable<T> CreateAggregateDocumentQuery<T>(
            this Container container, AggregateQueryOptions queryOptions)
        {
            Guard.ArgumentIsNotNull(nameof(container), container);

            return new AggregateDocumentQuery<T>(container, queryOptions);
        }
    }
}
