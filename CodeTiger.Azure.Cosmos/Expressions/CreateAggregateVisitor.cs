﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Newtonsoft.Json;

namespace CodeTiger.Azure.Cosmos.Expressions;

/// <summary>
/// A visitor that builds the body of a JavaScript function for a stored procedure to create aggregate elements by
/// combining the existing aggregate element with another source element.
/// </summary>
internal static class CreateAggregateVisitor
{
    /// <summary>
    /// Visits an <see cref="Expression"/> object to build the body of a JavaScript function for a stored procedure
    /// to create an aggregate element from an existing aggregate object and an additional source element.
    /// </summary>
    /// <param name="expression">The <see cref="Expression"/> to visit.</param>
    /// <returns>The body of a JavaScript function for a stored procedure to create an aggregate element from an
    /// existing aggregate object and an additional source element.</returns>
    /// <remarks>The function accepts two parameters, <c>aggregate</c> and <c>current</c>. It returns an object
    /// that contains an aggregation of those two objects.</remarks>
    public static string Visit(Expression expression)
    {
        Guard.ArgumentIsNotNull(nameof(expression), expression);
        Guard.ArgumentIsValid(nameof(expression), expression.NodeType == ExpressionType.Lambda);

        var lambdaExpression = (LambdaExpression)expression;
        Guard.ArgumentIsValid(nameof(expression), lambdaExpression.Parameters.Count == 2);
        Guard.ArgumentIsValid(nameof(expression),
            lambdaExpression.Parameters[0].Type == lambdaExpression.ReturnType);
        Guard.ArgumentIsValid(nameof(expression),
            lambdaExpression.Body.NodeType == ExpressionType.MemberInit
            || lambdaExpression.Body.NodeType == ExpressionType.New);

        return VisitSubExpression(lambdaExpression.Body, lambdaExpression.Parameters[0],
            lambdaExpression.Parameters[1]);
    }

    private static string VisitSubExpression(Expression expression, ParameterExpression aggregateParameter,
        ParameterExpression currentParameter)
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
                return VisitSubExpression((BinaryExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.Conditional:
                return VisitSubExpression(
                    (ConditionalExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.Constant:
                return VisitSubExpression((ConstantExpression)expression);
            case ExpressionType.Default:
                return VisitSubExpression((DefaultExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.ArrayIndex:
            case ExpressionType.Index:
                return VisitSubExpression((IndexExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.Invoke:
                return VisitSubExpression(
                    (InvocationExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.Lambda:
                return VisitSubExpression((LambdaExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.ListInit:
                return VisitSubExpression(
                    (ListInitExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.MemberAccess:
                return VisitSubExpression((MemberExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.MemberInit:
                return VisitSubExpression(
                    (MemberInitExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.Call:
                return VisitSubExpression(
                    (MethodCallExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.NewArrayBounds:
            case ExpressionType.NewArrayInit:
                return VisitSubExpression(
                    (NewArrayExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.New:
                return VisitSubExpression((NewExpression)expression);
            case ExpressionType.Parameter:
                return VisitSubExpression(
                    (ParameterExpression)expression, aggregateParameter, currentParameter);
            case ExpressionType.ArrayLength:
            case ExpressionType.Convert:
            case ExpressionType.IsFalse:
            case ExpressionType.IsTrue:
            case ExpressionType.Negate:
            case ExpressionType.Not:
            case ExpressionType.OnesComplement:
            case ExpressionType.Quote:
            case ExpressionType.UnaryPlus:
                return VisitSubExpression((UnaryExpression)expression, aggregateParameter, currentParameter);
            default:
                throw new NotImplementedException();
        }
    }

    private static string VisitSubExpression(BinaryExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        switch (expression.NodeType)
        {
            case ExpressionType.Add:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " + "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.And:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " & "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.AndAlso:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " && "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.Coalesce:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " || "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.Divide:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " / "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.Equal:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " == "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.ExclusiveOr:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " ^ "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.GreaterThan:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " > "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.GreaterThanOrEqual:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " >= "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.LeftShift:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " << "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.LessThan:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " < "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.LessThanOrEqual:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " <= "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.Modulo:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " % "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.Multiply:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " * "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.NotEqual:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " != "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.Or:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " | "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.OrElse:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " || "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.Power:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " ** "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.RightShift:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " >> "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            case ExpressionType.Subtract:
                return "(" + VisitSubExpression(expression.Left, aggregateParameter, currentParameter) + " - "
                    + VisitSubExpression(expression.Right, aggregateParameter, currentParameter) + ")";
            default:
                throw new NotImplementedException();
        }
    }

    private static string VisitSubExpression(ConditionalExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        return "(" + VisitSubExpression(expression.Test, aggregateParameter, currentParameter)
            + " ? " + VisitSubExpression(expression.IfTrue, aggregateParameter, currentParameter)
            + " : " + VisitSubExpression(expression.IfFalse, aggregateParameter, currentParameter) + ")";
    }

    private static string VisitSubExpression(ConstantExpression expression)
    {
        if (expression.Value == null)
        {
            return "null";
        }

        object? evaluatedValue = ExpressionUtilities.GetEvaluatedValue(expression);

        if (evaluatedValue == null)
        {
            return "null";
        }

        var evaluatedValueType = evaluatedValue.GetType();

        if (evaluatedValueType.IsPrimitive || evaluatedValueType == typeof(string)
            || evaluatedValueType == typeof(decimal))
        {
            return JsonConvert.ToString(evaluatedValue);
        }

        if (evaluatedValueType == typeof(DateTime))
        {
            return "\"" + ((DateTime)evaluatedValue).ToString("O", CultureInfo.InvariantCulture) + "\"";
        }

        throw new NotImplementedException();
    }

    private static string VisitSubExpression(DefaultExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitSubExpression(IndexExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitSubExpression(InvocationExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitSubExpression(LambdaExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitSubExpression(ListInitExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitSubExpression(MemberExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        switch (expression.Expression?.NodeType)
        {
            case ExpressionType.Parameter:
                return VisitSubExpression((ParameterExpression)expression.Expression, aggregateParameter,
                    currentParameter) + "." + ExpressionUtilities.GetJsonPropertyName(expression.Member);
            case ExpressionType.MemberAccess:
                return VisitSubExpression((MemberExpression)expression.Expression, aggregateParameter,
                    currentParameter) + "." + ExpressionUtilities.GetJsonPropertyName(expression.Member);
            case ExpressionType.Constant:
                return VisitSubExpression((ConstantExpression)expression.Expression);
            default:
                throw new NotImplementedException();
        }
    }

    private static string VisitSubExpression(MemberInitExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        if (expression.NewExpression.Arguments?.Any(x => x.NodeType != ExpressionType.Constant) == true)
        {
            throw new NotImplementedException();
        }

        object? defaultValue = Activator.CreateInstance(expression.NewExpression.Type,
            expression.NewExpression.Arguments?.Cast<ConstantExpression>().Select(x => x.Value).ToArray());

        var sb = new StringBuilder("{ ");

        bool haveAnyMembersBeenWritten = false;

        foreach (var propertyOrField in expression.NewExpression.Type
            .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            .Where(ExpressionUtilities.ShouldSerialize))
        {
            if (haveAnyMembersBeenWritten)
            {
                sb.Append(", ");
            }

            var memberBinding = expression.Bindings?.SingleOrDefault(x => x.Member == propertyOrField);
            if (memberBinding != null)
            {
                sb.Append(VisitBinding(memberBinding, aggregateParameter, currentParameter));
            }
            else
            {
                if (propertyOrField.MemberType == MemberTypes.Property)
                {
                    sb.Append(ExpressionUtilities.GetQuotedJsonPropertyName(propertyOrField)).Append(": ")
                        .Append(JsonConvert.SerializeObject(
                            ((PropertyInfo)propertyOrField).GetValue(defaultValue)));
                }
                else if (propertyOrField.MemberType == MemberTypes.Field)
                {
                    sb.Append(ExpressionUtilities.GetQuotedJsonPropertyName(propertyOrField)).Append(": ")
                        .Append(JsonConvert.SerializeObject(
                            ((FieldInfo)propertyOrField).GetValue(defaultValue)));
                }
                else
                {
                    Debug.Fail($"{propertyOrField.MemberType} members should not be encountered here.");
                }
            }

            haveAnyMembersBeenWritten = true;
        }

        sb.Append(" }");

        return sb.ToString();
    }

    private static string VisitBinding(MemberBinding binding, ParameterExpression aggregateParameter,
        ParameterExpression currentParameter)
    {
        switch (binding.BindingType)
        {
            case MemberBindingType.Assignment:
                return VisitBinding((MemberAssignment)binding, aggregateParameter, currentParameter);
            case MemberBindingType.ListBinding:
                return VisitBinding((MemberListBinding)binding, aggregateParameter, currentParameter);
            case MemberBindingType.MemberBinding:
                return VisitBinding((MemberMemberBinding)binding, aggregateParameter, currentParameter);
            default:
                throw new NotImplementedException();
        }
    }

    private static string VisitBinding(MemberAssignment binding, ParameterExpression aggregateParameter,
        ParameterExpression currentParameter)
    {
        return ExpressionUtilities.GetQuotedJsonPropertyName(binding.Member) + ": "
            + VisitSubExpression(binding.Expression, aggregateParameter, currentParameter);
    }

    private static string VisitBinding(MemberListBinding binding, ParameterExpression aggregateParameter,
        ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitBinding(MemberMemberBinding binding, ParameterExpression aggregateParameter,
        ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitSubExpression(MethodCallExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitSubExpression(NewArrayExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        throw new NotImplementedException();
    }

    private static string VisitSubExpression(NewExpression expression)
    {
        if (expression.Arguments?.Any(x => x.NodeType != ExpressionType.Constant) == true)
        {
            throw new NotImplementedException();
        }

        object? defaultValue = Activator.CreateInstance(expression.Type,
            expression.Arguments?.Cast<ConstantExpression>().Select(x => x.Value).ToArray());

        var sb = new StringBuilder("{ ");

        bool haveAnyMembersBeenWritten = false;

        foreach (var propertyOrField in expression.Type
            .GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)
            .Where(ExpressionUtilities.ShouldSerialize))
        {
            if (haveAnyMembersBeenWritten)
            {
                sb.Append(", ");
            }

            if (propertyOrField.MemberType == MemberTypes.Property)
            {
                sb.Append(ExpressionUtilities.GetQuotedJsonPropertyName(propertyOrField))
                    .Append(": ")
                    .Append(JsonConvert.SerializeObject(((PropertyInfo)propertyOrField).GetValue(defaultValue)));
            }
            else if (propertyOrField.MemberType == MemberTypes.Field)
            {
                sb.Append(ExpressionUtilities.GetQuotedJsonPropertyName(propertyOrField))
                    .Append(": ")
                    .Append(JsonConvert.SerializeObject(((FieldInfo)propertyOrField).GetValue(defaultValue)));
            }
            else
            {
                Debug.Fail($"{propertyOrField.MemberType} members should not be encountered here.");
            }

            haveAnyMembersBeenWritten = true;
        }

        sb.Append(" }");
        return sb.ToString();
    }

    private static string VisitSubExpression(ParameterExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        if (expression == aggregateParameter)
        {
            return "aggregate";
        }

        if (expression == currentParameter)
        {
            return "current";
        }

        throw new NotImplementedException();
    }

    private static string VisitSubExpression(UnaryExpression expression,
        ParameterExpression aggregateParameter, ParameterExpression currentParameter)
    {
        switch (expression.NodeType)
        {
            case ExpressionType.Quote:
            case ExpressionType.UnaryPlus:
            case ExpressionType.Convert:
                return VisitSubExpression(expression.Operand, aggregateParameter, currentParameter);
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
}
