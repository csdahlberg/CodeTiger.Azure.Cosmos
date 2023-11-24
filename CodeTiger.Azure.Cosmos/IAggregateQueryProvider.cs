using System.Linq.Expressions;

namespace CodeTiger.Azure.Cosmos;

/// <summary>
/// Provides functionality to create and execute queries that are described by an
/// <see cref="IAggregateQueryable{T}"/> object.
/// </summary>
public interface IAggregateQueryProvider
{
    /// <summary>
    /// Constructs an <see cref="IAggregateQueryable{T}"/> object that can evaluate the query represented by a
    /// specified expression tree.
    /// </summary>
    /// <typeparam name="T">The type of the elements of the <see cref="IAggregateQueryable{T}"/> that is returned.
    /// </typeparam>
    /// <param name="expression">An expression tree that represents a LINQ query.</param>
    /// <returns>An <see cref="IAggregateQueryable{T}"/> that can evaluate the query represented by the specified
    /// expression tree.</returns>
    IAggregateQueryable<T> CreateQuery<T>(Expression expression);
}
