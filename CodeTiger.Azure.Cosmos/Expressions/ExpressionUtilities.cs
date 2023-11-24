using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace CodeTiger.Azure.Cosmos.Expressions;

/// <summary>
/// Provides helper methods used when translating expression trees to stored procedures for Cosmos DB.
/// </summary>
internal static class ExpressionUtilities
{
    public static string GetJsonPropertyName(MemberInfo memberInfo)
    {
        var jsonPropertyAttribute = memberInfo.GetCustomAttribute<JsonPropertyAttribute>(true);
        if (jsonPropertyAttribute != null && !string.IsNullOrEmpty(jsonPropertyAttribute.PropertyName))
        {
            return jsonPropertyAttribute.PropertyName;
        }

        return memberInfo.Name;
    }

    public static string GetQuotedJsonPropertyName(MemberInfo memberInfo)
    {
        return "\"" + GetJsonPropertyName(memberInfo) + "\"";
    }

    public static bool ShouldSerialize(MemberInfo memberInfo)
    {
        if (memberInfo.MemberType != MemberTypes.Property && memberInfo.MemberType != MemberTypes.Field)
        {
            return false;
        }

        var jsonPropertyAttribute = memberInfo.GetCustomAttribute<JsonIgnoreAttribute>(true);
        if (jsonPropertyAttribute != null)
        {
            return false;
        }

        return true;
    }

    public static object? GetEvaluatedValue(ConstantExpression expression)
    {
        if (expression.Type.IsCompilerGenerated())
        {
            return expression.Type.GetFields().Single().GetValue(expression.Value);
        }

        return expression.Value;
    }
}
