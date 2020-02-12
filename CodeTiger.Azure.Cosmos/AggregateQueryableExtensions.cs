using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodeTiger.Azure.Cosmos
{
    /// <summary>
    /// Provides extension methods for the <see cref="IAggregateQueryable{T}"/> interface.
    /// </summary>
    public static class AggregateQueryableExtensions
    {
        /// <summary>
        /// Filters a sequence of values based on a predicate.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IAggregateQueryable{T}"/> to filter.</param>
        /// <param name="predicate">A function to test each element for a condition.</param>
        /// <returns>An <see cref="IAggregateQueryable{T}"/> that contains elements from the input sequence that
        /// satisfy the condition specified by <paramref name="predicate"/>.</returns>
        public static IAggregateQueryable<T> Where<T>(this IAggregateQueryable<T> source,
            Expression<Func<T, bool>> predicate)
        {
            Guard.ArgumentIsNotNull(nameof(source), source);
            Guard.ArgumentIsNotNull(nameof(predicate), predicate);
            
            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    GetMethodInfo(Where, source, predicate),
                    source.Expression,
                    Expression.Quote(predicate)));
        }

        /// <summary>
        /// Groups the elements of a sequence according to a specified key selector function.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TKey">The type of the key returned by <paramref name="keySelector"/>.</typeparam>
        /// <param name="source">An <see cref="IAggregateQueryable{T}"/> whose elements to group.</param>
        /// <param name="keySelector">A function to extract the key for each element.</param>
        /// <returns>An <see cref="IAggregateQueryable{T}"/> where each element is an
        /// <see cref="IGrouping{TKey, TElement}"/> object containing a sequence of objects and a key.</returns>
        public static IAggregateQueryable<IGrouping<TKey, T>> GroupBy<T, TKey>(this IAggregateQueryable<T> source,
            Expression<Func<T, TKey>> keySelector)
        {
            Guard.ArgumentIsNotNull(nameof(source), source);
            Guard.ArgumentIsNotNull(nameof(keySelector), keySelector);

            return source.Provider.CreateQuery<IGrouping<TKey, T>>(
                Expression.Call(
                    null,
                    GetMethodInfo(GroupBy, source, keySelector),
                    source.Expression,
                    Expression.Quote(keySelector)));
        }

        /// <summary>
        /// Combines elements from a query into a single aggregate element.
        /// </summary>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IAggregateQueryable{T}"/> to aggregate elements from.</param>
        /// <param name="aggregateFunc">A function to be used to combine two source elements into an aggregate
        /// element.</param>
        /// <returns>An <see cref="IAggregateQueryable{T}"/> containing an aggregate of the elements in
        /// <paramref name="source"/>.</returns>
        public static IAggregateQueryable<T> Aggregate<T>(this IAggregateQueryable<T> source,
            Expression<Func<T, T, T>> aggregateFunc)
        {
            Guard.ArgumentIsNotNull(nameof(source), source);
            Guard.ArgumentIsNotNull(nameof(aggregateFunc), aggregateFunc);

            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    GetMethodInfo(Aggregate, source, aggregateFunc),
                    source.Expression,
                    Expression.Quote(aggregateFunc)));
        }

        /// <summary>
        /// Combines elements from a query into a single aggregate element.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TAggregate">The type of the resulting aggregate element.</typeparam>
        /// <param name="source">An <see cref="IAggregateQueryable{T}"/> containing elements to aggregate.</param>
        /// <param name="aggregateSeedFunc">A function for creating an initial aggregate element from a source
        /// element.</param>
        /// <param name="aggregateFunc">A function for creating aggregate elements by combining the existing
        /// aggregate element with another source element.</param>
        /// <returns>An <see cref="IAggregateQueryable{T}"/> containing an aggregate of the elements in
        /// <paramref name="source"/>.</returns>
        public static IAggregateQueryable<TAggregate> Aggregate<TSource, TAggregate>(
            this IAggregateQueryable<TSource> source,
            Expression<Func<TSource, TAggregate>> aggregateSeedFunc,
            Expression<Func<TAggregate, TSource, TAggregate>> aggregateFunc)
        {
            Guard.ArgumentIsNotNull(nameof(source), source);
            Guard.ArgumentIsNotNull(nameof(aggregateSeedFunc), aggregateSeedFunc);
            Guard.ArgumentIsNotNull(nameof(aggregateFunc), aggregateFunc);

            return source.Provider.CreateQuery<TAggregate>(
                Expression.Call(
                    null,
                    GetMethodInfo(Aggregate, source, aggregateSeedFunc, aggregateFunc),
                    source.Expression,
                    Expression.Quote(aggregateSeedFunc),
                    Expression.Quote(aggregateFunc)));
        }

        /// <summary>
        /// Combines elements from a query into aggregate elements.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in <paramref name="source"/>.</typeparam>
        /// <typeparam name="T">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <param name="source">An <see cref="IAggregateQueryable{T}"/> containing elements to aggregate.</param>
        /// <param name="aggregateFunc">A function for creating aggregate elements by combining the existing
        /// aggregate element with another source element.</param>
        /// <returns>An <see cref="IAggregateQueryable{T}"/> containing aggregates of the elements in
        /// <paramref name="source"/>.</returns>
        public static IAggregateQueryable<T> Aggregate<TKey, T>(
            this IAggregateQueryable<IGrouping<TKey, T>> source, Expression<Func<T, T, T>> aggregateFunc)
        {
            Guard.ArgumentIsNotNull(nameof(source), source);
            Guard.ArgumentIsNotNull(nameof(aggregateFunc), aggregateFunc);

            return source.Provider.CreateQuery<T>(
                Expression.Call(
                    null,
                    GetMethodInfo(Aggregate, source, aggregateFunc),
                    source.Expression,
                    Expression.Quote(aggregateFunc)));
        }

        /// <summary>
        /// Combines elements from a query into aggregate elements.
        /// </summary>
        /// <typeparam name="TKey">The type of the keys in <paramref name="source"/>.</typeparam>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TAggregate">The type of the resulting aggregate element.</typeparam>
        /// <param name="source">An <see cref="IAggregateQueryable{T}"/> containing elements to aggregate.</param>
        /// <param name="aggregateSeedFunc">A function for creating an initial aggregate element from a source
        /// element.</param>
        /// <param name="aggregateFunc">A function for creating aggregate elements by combining the existing
        /// aggregate element with another source element.</param>
        /// <returns>An <see cref="IAggregateQueryable{T}"/> containing aggregates of the elements in
        /// <paramref name="source"/>.</returns>
        public static IAggregateQueryable<TAggregate> Aggregate<TKey, TSource, TAggregate>(
            this IAggregateQueryable<IGrouping<TKey, TSource>> source,
            Expression<Func<TSource, TAggregate>> aggregateSeedFunc,
            Expression<Func<TAggregate, TSource, TAggregate>> aggregateFunc)
        {
            Guard.ArgumentIsNotNull(nameof(source), source);
            Guard.ArgumentIsNotNull(nameof(aggregateSeedFunc), aggregateSeedFunc);
            Guard.ArgumentIsNotNull(nameof(aggregateFunc), aggregateFunc);

            return source.Provider.CreateQuery<TAggregate>(
                Expression.Call(
                    null,
                    GetMethodInfo(Aggregate, source, aggregateSeedFunc, aggregateFunc),
                    source.Expression,
                    Expression.Quote(aggregateSeedFunc),
                    Expression.Quote(aggregateFunc)));
        }

        /// <summary>
        /// Projects each element of a sequence into a new form.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="source"/>.</typeparam>
        /// <typeparam name="TResult">The type of the value returned by the function represented by
        /// <paramref name="selector"/>.</typeparam>
        /// <param name="source">A sequence of values to project.</param>
        /// <param name="selector">A projection function to apply to each element.</param>
        /// <returns>An <see cref="IAggregateQueryable{T}"/> whose elements are the result of invoking a projection
        /// function on each element of <paramref name="source"/>.</returns>
        public static IAggregateQueryable<TResult> Select<TSource, TResult>(
            this IAggregateQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            Guard.ArgumentIsNotNull(nameof(source), source);
            Guard.ArgumentIsNotNull(nameof(selector), selector);

            return source.Provider.CreateQuery<TResult>(
                Expression.Call(
                    null,
                    GetMethodInfo(Select, source, selector),
                    source.Expression,
                    Expression.Quote(selector)));
        }

        /// <summary>
        /// Converts an <see cref="IAggregateQueryable{T}"/> to <see cref="IAggregateDocumentQuery{T}"/>, which
        /// supports pagination and asynchronous execution in the Azure Cosmos DB service.
        /// </summary>
        /// <typeparam name="T">The type of object to query.</typeparam>
        /// <param name="query">The <see cref="IAggregateQueryable{T}"/> to be converted.</param>
        /// <returns>An <see cref="IAggregateDocumentQuery{T}"/> that can evaluate the query.</returns>
        public static IAggregateDocumentQuery<T> AsDocumentQuery<T>(this IAggregateQueryable<T> query)
        {
            return (IAggregateDocumentQuery<T>)query;
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "These parameters are used via reflection.")]
        [SuppressMessage("Usage", "CA1801:Review unused parameters",
            Justification = "These parameters are used via reflection.")]
        private static MethodInfo GetMethodInfo<T1, T2, TResult>(Func<T1, T2, TResult> func, T1 unused, T2 unused2)
        {
            return func.Method;
        }

        [SuppressMessage("Style", "IDE0060:Remove unused parameter",
            Justification = "These parameters are used via reflection.")]
        [SuppressMessage("Usage", "CA1801:Review unused parameters",
            Justification = "These parameters are used via reflection.")]
        private static MethodInfo GetMethodInfo<T1, T2, T3, TResult>(Func<T1, T2, T3, TResult> func, T1 unused1,
            T2 unused2, T3 unused3)
        {
            return func.Method;
        }
    }
}
