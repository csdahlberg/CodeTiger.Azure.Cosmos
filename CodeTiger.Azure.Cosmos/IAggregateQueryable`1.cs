using System.Linq.Expressions;

namespace CodeTiger.Azure.Cosmos;

/// <summary>
/// Provides functionality to evaluate aggregate queries against a specific data source.
/// </summary>
/// <typeparam name="T">The type of the results of the query.</typeparam>
public interface IAggregateQueryable<out T>
{
    /// <summary>
    /// Gets the expression tree that is associated with the instance of <see cref="IAggregateQueryable{T}"/>.
    /// </summary>
    Expression Expression { get; }

    /// <summary>
    /// Gets the aggregate query provider that is associated with this data source.
    /// </summary>
    IAggregateQueryProvider Provider { get; }
}
