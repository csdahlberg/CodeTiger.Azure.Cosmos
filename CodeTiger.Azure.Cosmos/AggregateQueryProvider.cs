using System;
using System.Linq.Expressions;
using Microsoft.Azure.Cosmos;

namespace CodeTiger.Azure.Cosmos
{
    /// <summary>
    /// Provides functionality to create and execute queries that are described by an
    /// <see cref="IAggregateQueryable{T}"/> object.
    /// </summary>
    public class AggregateQueryProvider : IAggregateQueryProvider
    {
        private readonly CosmosContainer _container;
        private readonly AggregateQueryOptions _queryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateQueryProvider"/> class.
        /// </summary>
        /// <param name="container">The <see cref="CosmosContainer"/> object to use to interact with the Cosmos
        /// database when executing aggregate document queries.</param>
        /// <param name="queryOptions">Specifies the options for the aggregate query.</param>
        public AggregateQueryProvider(CosmosContainer container, AggregateQueryOptions queryOptions)
        {
            _container = Guard.ArgumentIsNotNull(nameof(container), container);
            _queryOptions = Guard.ArgumentIsNotNull(nameof(queryOptions), queryOptions);
        }

        /// <summary>
        /// Constructs an <see cref="IAggregateQueryable{T}"/> object that can evaluate the query represented by a
        /// specified expression tree.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the <see cref="IAggregateQueryable{T}"/> that is
        /// returned.</typeparam>
        /// <param name="expression">An expression tree that represents a LINQ query.</param>
        /// <returns>An <see cref="IAggregateQueryable{T}"/> that can evaluate the query represented by the
        /// specified expression tree.</returns>
        public IAggregateQueryable<T> CreateQuery<T>(Expression expression)
        {
            Guard.ArgumentIsNotNull(nameof(expression), expression);

            return new AggregateDocumentQuery<T>(_container, _queryOptions, expression);
        }
    }
}
