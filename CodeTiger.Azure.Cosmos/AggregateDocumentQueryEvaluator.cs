using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using CodeTiger.Azure.Cosmos.Expressions;

namespace CodeTiger.Azure.Cosmos
{
    /// <summary>
    /// Provides functionality for evaluating expressions representing aggregate document queries.
    /// </summary>
    internal static partial class AggregateDocumentQueryEvaluator
    {
        /// <summary>
        /// Evaluates an expression representing an aggregate document query and returns an
        /// <see cref="AggregateStoredProcedure"/> object containing all information needed to execute that query
        /// in Cosmos.
        /// </summary>
        /// <param name="expression">An expression representing an aggregate document query.</param>
        /// <returns>An <see cref="AggregateStoredProcedure"/> object containing all information needed to execute
        /// the query in Cosmos.</returns>
        public static AggregateStoredProcedure Evaluate(Expression expression)
        {
            Guard.ArgumentIsNotNull(nameof(expression), expression);

            var expressions = GetQueryExpressions(expression);

            var queryParameters = new Dictionary<string, object>();

            string storedProcedureBody = new StringBuilder(StoredProcedureTemplate)
                .Replace(QueryPlaceholder, GetQuery(expressions, queryParameters))
                .Replace(CreateAggregateSeedFunctionPlaceholder,
                    GetCreateAggregateSeedFunction(expressions))
                .Replace(CreateAggregateFunctionPlaceholder, GetCreateAggregateFunction(expressions))
                .Replace(CreateResultFunctionPlaceholder, GetCreateResultFunction(expressions))
                .Replace(ShouldStartNewAggregateFunctionPlaceholder,
                    GetShouldStartNewAggregateFunction(expressions))
                .ToString();

            return new AggregateStoredProcedure(storedProcedureBody, queryParameters);
        }

        /// <summary>
        /// Gets sub-expressions from the overall expression based on the extension method they were passed in to.
        /// </summary>
        /// <param name="expression">The overall expression created by all calls to extension methods on an
        /// <see cref="IAggregateQueryable{T}"/> object.</param>
        /// <returns>A <see cref="QueryExpressions"/> object containing expressions identified by which extension
        /// method they were passed in to.</returns>
        private static QueryExpressions GetQueryExpressions(Expression expression)
        {
            var expressions = new QueryExpressions();

            while (expression.NodeType == ExpressionType.Call)
            {
                var callExpression = (MethodCallExpression)expression;

                if (callExpression.Method.DeclaringType != typeof(AggregateQueryableExtensions))
                {
                    throw new NotImplementedException();
                }

                switch (callExpression.Method.Name)
                {
                    case nameof(AggregateQueryableExtensions.Where):
                        expressions.AddWhereExpression(Unquote(callExpression.Arguments[1]));
                        break;
                    case nameof(AggregateQueryableExtensions.GroupBy):
                        expressions.AddGroupByExpression(Unquote(callExpression.Arguments[1]));
                        break;
                    case nameof(AggregateQueryableExtensions.Aggregate) when (callExpression.Arguments.Count == 2):
                        expressions.AddCreateAggregateExpression(Unquote(callExpression.Arguments[1]));
                        break;
                    case nameof(AggregateQueryableExtensions.Aggregate) when (callExpression.Arguments.Count == 3):
                        expressions.AddCreateAggregateSeedExpression(Unquote(callExpression.Arguments[1]));
                        expressions.AddCreateAggregateExpression(Unquote(callExpression.Arguments[2]));
                        break;
                    case nameof(AggregateQueryableExtensions.Select):
                        expressions.AddCreateResultExpression(Unquote(callExpression.Arguments[1]));
                        break;
                    default:
                        throw new NotImplementedException();
                }

                expression = callExpression.Arguments[0];
            }

            Debug.Assert(expression.NodeType == ExpressionType.Constant,
                "The initial expression should be a constant containing an IAggregateDocumentQuery<T> instance.");
            Debug.Assert(expression.Type.IsGenericType,
                "The initial expression should be a constant containing an IAggregateDocumentQuery<T> instance.");
            Debug.Assert(expression.Type.GetInterfaces()
                .Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IAggregateDocumentQuery<>)),
                "The initial expression should be a constant containing an IAggregateDocumentQuery<T> instance.");

            return expressions;
        }

        private static string GetQuery(QueryExpressions expressions, Dictionary<string, object> queryParameters)
        {
            var queryBuilder = new StringBuilder("SELECT * FROM root r");

            if (expressions.WhereExpressions?.Any() == true)
            {
                queryBuilder.Append(" WHERE ");

                queryBuilder.Append(
                    QueryWhereClauseVisitor.Visit(expressions.WhereExpressions.First(), queryParameters));

                foreach (var whereExpression in expressions.WhereExpressions.Skip(1))
                {
                    queryBuilder.Append(" AND ")
                        .Append(QueryWhereClauseVisitor.Visit(whereExpression, queryParameters));
                }
            }

            if (expressions.GroupByExpression != null)
            {
                queryBuilder.Append(" ORDER BY ");

                queryBuilder.Append(
                    QueryOrderByClauseVisitor.Visit(expressions.GroupByExpression));
            }

            return queryBuilder.ToString();
        }

        private static string GetCreateAggregateSeedFunction(QueryExpressions expressions)
        {
            if (expressions.CreateAggregateSeedExpression == null)
            {
                return "return first;";
            }

            return "return " + CreateAggregateSeedVisitor.Visit(expressions.CreateAggregateSeedExpression) + ";";
        }

        private static string GetCreateAggregateFunction(QueryExpressions expressions)
        {
            return "return " + CreateAggregateVisitor.Visit(expressions.CreateAggregateExpression) + ";";
        }

        private static string GetCreateResultFunction(QueryExpressions expressions)
        {
            if (expressions.CreateResultExpression == null)
            {
                return "return aggregate;";
            }

            return "return " + CreateResultVisitor.Visit(expressions.CreateResultExpression) + ";";
        }

        private static string GetShouldStartNewAggregateFunction(QueryExpressions expressions)
        {
            if (expressions.GroupByExpression == null)
            {
                return "return false;";
            }

            return "return " + ShouldStartNewAggregateVisitor.Visit(expressions.GroupByExpression) + ";";
        }

        private static Expression Unquote(Expression expression)
        {
            while (expression.NodeType == ExpressionType.Quote)
            {
                expression = ((UnaryExpression)expression).Operand;
            }

            return expression;
        }

        private class QueryExpressions
        {
            public List<Expression> WhereExpressions { get; private set; }
            public Expression GroupByExpression { get; private set; }
            public Expression CreateAggregateSeedExpression { get; private set; }
            public Expression CreateAggregateExpression { get; private set; }
            public Expression CreateResultExpression { get; private set; }

            public void AddWhereExpression(Expression expression)
            {
                if (WhereExpressions == null)
                {
                    WhereExpressions = new List<Expression> { expression };
                }
                else
                {
                    WhereExpressions.Add(expression);
                }
            }

            public void AddGroupByExpression(Expression expression)
            {
                if (GroupByExpression != null || WhereExpressions?.Any() == true)
                {
                    throw new NotImplementedException();
                }

                GroupByExpression = expression;
            }

            public void AddCreateAggregateSeedExpression(Expression expression)
            {
                if (CreateAggregateSeedExpression != null || GroupByExpression != null
                    || WhereExpressions?.Any() == true)
                {
                    throw new NotImplementedException();
                }

                CreateAggregateSeedExpression = expression;
            }

            public void AddCreateAggregateExpression(Expression expression)
            {
                if (CreateAggregateExpression != null || GroupByExpression != null
                    || WhereExpressions?.Any() == true)
                {
                    throw new NotImplementedException();
                }

                CreateAggregateExpression = expression;
            }

            public void AddCreateResultExpression(Expression expression)
            {
                if (CreateResultExpression != null || CreateAggregateExpression != null
                    || CreateAggregateSeedExpression != null || GroupByExpression != null
                    || WhereExpressions?.Any() == true)
                {
                    throw new NotImplementedException();
                }

                CreateResultExpression = expression;
            }
        }
    }
}
