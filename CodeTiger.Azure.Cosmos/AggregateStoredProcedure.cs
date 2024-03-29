﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CodeTiger.Azure.Cosmos;

/// <summary>
/// Represents a stored procedure used to aggregate data from documents in Cosmos DB.
/// </summary>
internal class AggregateStoredProcedure
{
    private readonly Lazy<string> _id;

    /// <summary>
    /// Gets the ID of the stored procedure.
    /// </summary>
    public string Id => _id.Value;

    /// <summary>
    /// Gets the source code of the stored procedure.
    /// </summary>
    public string Body { get; }

    /// <summary>
    /// Gets the parameters to be passed in to the <c>SELECT</c> query used to retrieve the documents to be
    /// aggregated together.
    /// </summary>
    public IReadOnlyDictionary<string, object?> Parameters { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AggregateStoredProcedure"/> class.
    /// </summary>
    /// <param name="body">The source code of the stored procedure.</param>
    /// <param name="parameters">The parameters to be passed in to the <c>SELECT</c> query used to retrieve the
    /// documents to be aggregated together.</param>
    public AggregateStoredProcedure(string body, IReadOnlyDictionary<string, object?> parameters)
    {
        Body = body;
        Parameters = parameters;

        _id = new Lazy<string>(() => CalculateId(Body));
    }

    private static string CalculateId(string body)
    {
#if NET6_0_OR_GREATER
        return "codeTigerAggregate_" + ConvertToIdSafeString(SHA256.HashData(Encoding.UTF8.GetBytes(body)));
#else
        using (var hash = SHA256.Create())
        {
            return "codeTigerAggregate_" + ConvertToIdSafeString(hash.ComputeHash(Encoding.UTF8.GetBytes(body)));
        }
#endif
    }

    private static string ConvertToIdSafeString(byte[] input)
    {
        return string.Concat(input.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
    }
}
