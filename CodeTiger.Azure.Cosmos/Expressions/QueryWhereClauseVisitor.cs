using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace CodeTiger.Azure.Cosmos.Expressions
{
    /// <summary>
    /// A visitor that builds a filter for the <c>WHERE</c> clause of a SQL query used by a stored procedure to
    /// retrieve source documents.
    /// </summary>
    internal static class QueryWhereClauseVisitor
    {
        /// <summary>
        /// Visits an <see cref="Expression"/> object to build a filter for the <c>WHERE</c> clause of a SQL query
        /// used by a stored procedure to retrieve source documents.
        /// </summary>
        /// <param name="expression">The <see cref="Expression"/> to visit.</param>
        /// <param name="queryParameters">A dictionary containing all parameters to be passed in to the SQL query
        /// executed by the stored procedure.</param>
        /// <returns>A filter for the <c>WHERE</c> clause of a SQL query used to retrieve source documents. For
        /// example, <c>r.customerId = 18502 AND c.date > '2019-03-18T00:00:00.0000000Z'</c>.</returns>
        /// <remarks>The identifier for the root document in the query is <c>r</c>.</remarks>
        public static string Visit(Expression expression, IDictionary<string, object?> queryParameters)
        {
            Guard.ArgumentIsNotNull(nameof(expression), expression);
            Guard.ArgumentIsValid(nameof(expression), expression.NodeType == ExpressionType.Lambda);
            Guard.ArgumentIsNotNull(nameof(queryParameters), queryParameters);

            var lambdaExpression = (LambdaExpression)expression;
            Guard.ArgumentIsValid(nameof(expression), lambdaExpression.ReturnType == typeof(bool));
            Guard.ArgumentIsValid(nameof(expression), lambdaExpression.Parameters.Count == 1);

            return VisitSubExpression(lambdaExpression.Body, lambdaExpression.Parameters.Single(),
                queryParameters);
        }

        private static string VisitSubExpression(Expression expression, ParameterExpression sourceParameter,
            IDictionary<string, object?> queryParameters)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                case ExpressionType.Coalesce:
                case ExpressionType.Divide:
                case ExpressionType.Equal:
                case ExpressionType.ExclusiveOr:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.LeftShift:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.Modulo:
                case ExpressionType.Multiply:
                case ExpressionType.NotEqual:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                case ExpressionType.Power:
                case ExpressionType.RightShift:
                case ExpressionType.Subtract:
                    return VisitSubExpression((BinaryExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.Conditional:
                    return VisitSubExpression((ConditionalExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.Constant:
                    return VisitSubExpression((ConstantExpression)expression, queryParameters);
                case ExpressionType.Default:
                    return VisitSubExpression((DefaultExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.ArrayIndex:
                case ExpressionType.Index:
                    return VisitSubExpression((IndexExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.Invoke:
                    return VisitSubExpression((InvocationExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.Lambda:
                    return VisitSubExpression((LambdaExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.ListInit:
                    return VisitSubExpression((ListInitExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.MemberAccess:
                    return VisitSubExpression((MemberExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.MemberInit:
                    return VisitSubExpression((MemberInitExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.Call:
                    return VisitSubExpression((MethodCallExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.NewArrayBounds:
                case ExpressionType.NewArrayInit:
                    return VisitSubExpression((NewArrayExpression)expression, sourceParameter, queryParameters);
                case ExpressionType.New:
                    return VisitSubExpression((NewExpression)expression, queryParameters);
                case ExpressionType.Parameter:
                    return VisitSubExpression((ParameterExpression)expression, sourceParameter);
                case ExpressionType.ArrayLength:
                case ExpressionType.IsFalse:
                case ExpressionType.IsTrue:
                case ExpressionType.Negate:
                case ExpressionType.Not:
                case ExpressionType.OnesComplement:
                case ExpressionType.Quote:
                case ExpressionType.UnaryPlus:
                    return VisitSubExpression((UnaryExpression)expression, sourceParameter, queryParameters);
                default:
                    throw new NotImplementedException();

            }
        }

        private static string VisitSubExpression(BinaryExpression expression, ParameterExpression sourceParameter,
            IDictionary<string, object?> queryParameters)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Add:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " + "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.And:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " & "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.AndAlso:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " && "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.Coalesce:
                    return "(" + VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " || "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters) + ")";
                case ExpressionType.Divide:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " / "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.Equal:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " == "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.ExclusiveOr:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " ^ "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.GreaterThan:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " > "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.GreaterThanOrEqual:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " >= "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.LeftShift:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " << "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.LessThan:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " < "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.LessThanOrEqual:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " <= "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.Modulo:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " % "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.Multiply:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " * "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.NotEqual:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " <> "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.Or:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " | "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.OrElse:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " || "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.Power:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " ** "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.RightShift:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " >> "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                case ExpressionType.Subtract:
                    return VisitSubExpression(expression.Left, sourceParameter, queryParameters) + " - "
                        + VisitSubExpression(expression.Right, sourceParameter, queryParameters);
                default:
                    throw new NotImplementedException();
            }
        }

        private static string VisitSubExpression(ConditionalExpression expression,
            ParameterExpression sourceParameter, IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(ConstantExpression expression,
            IDictionary<string, object?> queryParameters)
        {
            if (expression.Type == typeof(string) || expression.Type == typeof(decimal)
                || expression.Type.IsPrimitive)
            {
                string newParameterName = GetNewParameterName(queryParameters);
                queryParameters[newParameterName] = expression.Value;

                return newParameterName;
            }

            throw new NotImplementedException();
        }

        private static string VisitSubExpression(DefaultExpression expression, ParameterExpression sourceParameter,
            IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(IndexExpression expression, ParameterExpression sourceParameter,
            IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(InvocationExpression expression,
             ParameterExpression sourceParameter, IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(LambdaExpression expression, ParameterExpression sourceParameter,
            IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(ListInitExpression expression,
            ParameterExpression sourceParameter, IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(MemberExpression expression, ParameterExpression sourceParameter,
            IDictionary<string, object?> queryParameters)
        {
            switch (expression.Expression?.NodeType)
            {
                case ExpressionType.Parameter:
                    return VisitSubExpression((ParameterExpression)expression.Expression, sourceParameter)
                        + "." + ExpressionUtilities.GetJsonPropertyName(expression.Member);
                case ExpressionType.MemberAccess:
                    return VisitSubExpression((MemberExpression)expression.Expression, sourceParameter,
                        queryParameters) + "." + ExpressionUtilities.GetJsonPropertyName(expression.Member);
                default:
                    throw new NotImplementedException();
            }
        }

        private static string VisitSubExpression(MemberInitExpression expression,
             ParameterExpression sourceParameter, IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(MethodCallExpression expression,
            ParameterExpression sourceParameter, IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(NewArrayExpression expression,
            ParameterExpression sourceParameter, IDictionary<string, object?> queryParameters)
        {
            throw new NotImplementedException();
        }

        private static string VisitSubExpression(NewExpression expression,
            IDictionary<string, object?> queryParameters)
        {
            if (expression.Type == typeof(DateTime))
            {
                var constructorParameters = expression.Constructor?.GetParameters();
                if (constructorParameters?.Length == 3
                    && constructorParameters[0].ParameterType == typeof(int)
                    && constructorParameters[1].ParameterType == typeof(int)
                    && constructorParameters[2].ParameterType == typeof(int))
                {
                    if (expression.Arguments.All(x => x.NodeType == ExpressionType.Constant))
                    {
                        string parameterName = GetNewParameterName(queryParameters);

                        queryParameters[parameterName] = string.Join("-",
                            expression.Arguments.Cast<ConstantExpression>()
                                .Select(x => ((int)x.Value!).ToString("00", CultureInfo.InvariantCulture)));

                        return parameterName;
                    }
                }
            }

            throw new NotImplementedException();
        }

        private static string VisitSubExpression(ParameterExpression expression,
            ParameterExpression sourceParameter)
        {
            if (expression != sourceParameter)
            {
                throw new Exception();
            }

            return "r";
        }

        private static string VisitSubExpression(UnaryExpression expression, ParameterExpression sourceParameter,
            IDictionary<string, object?> queryParameters)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.Quote:
                case ExpressionType.UnaryPlus:
                    return VisitSubExpression(expression.Operand, sourceParameter, queryParameters);
                case ExpressionType.ArrayLength:
                case ExpressionType.IsFalse:
                case ExpressionType.IsTrue:
                case ExpressionType.Negate:
                case ExpressionType.Not:
                case ExpressionType.OnesComplement:
                default:
                    throw new NotImplementedException();
            }
        }

        private static string GetNewParameterName(IDictionary<string, object?> queryParameters)
        {
            return $"@p{queryParameters.Count + 1}";
        }
    }
}
