using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CodeTiger.Azure.Cosmos.Expressions;

/// <summary>
/// A visitor that builds the content for the <c>ORDER BY</c> clause of a SQL query used by a stored procedure
/// to retrieve source documents.
/// </summary>
internal static class QueryOrderByClauseVisitor
{
    /// <summary>
    /// Visits an <see cref="Expression"/> object to build the content for the <c>ORDER BY</c> clause of a SQL
    /// query used by a stored procedure to retrieve source documents.
    /// </summary>
    /// <param name="expression">The <see cref="Expression"/> to visit.</param>
    /// <returns>The content for the <c>ORDER BY</c> clause of a SQL query used to retrieve source documents. For
    /// example, <c>r.customerId, r.date</c>.</returns>
    /// <remarks>The identifier for the root document in the query is <c>r</c>.</remarks>
    public static string Visit(Expression expression)
    {
        Guard.ArgumentIsNotNull(nameof(expression), expression);
        Guard.ArgumentIsValid(nameof(expression), expression.NodeType == ExpressionType.Lambda);

        var lambdaExpression = (LambdaExpression)expression;
        Guard.ArgumentIsValid(nameof(expression), lambdaExpression.Parameters.Count == 1);
        Guard.ArgumentIsValid(nameof(expression),
            lambdaExpression.Body.NodeType == ExpressionType.MemberInit
            || lambdaExpression.Body.NodeType == ExpressionType.New
            || lambdaExpression.Body.NodeType == ExpressionType.MemberAccess);

        var orderByProperties = new List<string>();

        VisitSubExpression(lambdaExpression.Body, lambdaExpression.Parameters[0], orderByProperties, "");

        return string.Join(", ", orderByProperties);
    }

    private static void VisitSubExpression(Expression expression, ParameterExpression currentParameter,
        List<string> orderByProperties, string currentMemberPath)
    {
        switch (expression.NodeType)
        {
            case ExpressionType.MemberAccess:
                VisitSubExpression((MemberExpression)expression, currentParameter, orderByProperties,
                    currentMemberPath);
                break;
            case ExpressionType.MemberInit:
                VisitSubExpression((MemberInitExpression)expression, currentParameter, orderByProperties);
                break;
            case ExpressionType.Parameter:
                VisitSubExpression((ParameterExpression)expression, currentParameter, orderByProperties,
                    currentMemberPath);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private static void VisitSubExpression(MemberExpression expression,
        ParameterExpression currentParameter, List<string> orderByProperties, string currentMemberPath)
    {
        Type memberType;

        switch (expression.Member.MemberType)
        {
            case MemberTypes.Field:
                memberType = ((FieldInfo)expression.Member).FieldType;
                break;
            case MemberTypes.Property:
                memberType = ((PropertyInfo)expression.Member).PropertyType;
                break;
            default:
                throw new NotImplementedException();
        }

        if (!memberType.IsPrimitive && memberType != typeof(string) && memberType != typeof(decimal))
        {
            throw new NotImplementedException();
        }

        string newMemberPath = ExpressionUtilities.GetJsonPropertyName(expression.Member);
        if (!string.IsNullOrEmpty(currentMemberPath))
        {
            newMemberPath = newMemberPath + "." + currentMemberPath;
        }

        switch (expression.Expression?.NodeType)
        {
            case ExpressionType.Parameter:
                VisitSubExpression((ParameterExpression)expression.Expression, currentParameter,
                    orderByProperties, newMemberPath);
                break;
            case ExpressionType.MemberAccess:
                VisitSubExpression((MemberExpression)expression.Expression, currentParameter,
                    orderByProperties, newMemberPath);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private static void VisitSubExpression(MemberInitExpression expression,
        ParameterExpression currentParameter, List<string> orderByProperties)
    {
        if (expression.NewExpression.Arguments?.Any() == true)
        {
            throw new NotImplementedException();
        }

        foreach (var memberBinding in expression.Bindings)
        {
            VisitBinding(memberBinding, currentParameter, orderByProperties);
        }
    }

    private static void VisitBinding(MemberBinding binding, ParameterExpression currentParameter,
        List<string> orderByProperties)
    {
        switch (binding.BindingType)
        {
            case MemberBindingType.Assignment:
                VisitBinding((MemberAssignment)binding, currentParameter, orderByProperties);
                break;
            case MemberBindingType.MemberBinding:
                VisitBinding((MemberMemberBinding)binding, currentParameter, orderByProperties);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    private static void VisitBinding(MemberAssignment binding, ParameterExpression currentParameter,
        List<string> orderByProperties)
    {
        VisitSubExpression(binding.Expression, currentParameter, orderByProperties, "");
    }

    private static void VisitBinding(MemberMemberBinding binding, ParameterExpression currentParameter,
        List<string> orderByProperties)
    {
        throw new NotImplementedException();
    }

    private static void VisitSubExpression(ParameterExpression expression,
        ParameterExpression currentParameter, List<string> orderByProperties, string currentMemberPath)
    {
        if (expression != currentParameter)
        {
            throw new Exception();
        }

        string fullMemberPath = "r." + currentMemberPath;
        if (!orderByProperties.Contains(fullMemberPath))
        {
            orderByProperties.Add(fullMemberPath);
        }
    }
}
