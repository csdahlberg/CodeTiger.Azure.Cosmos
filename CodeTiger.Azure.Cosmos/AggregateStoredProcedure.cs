using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CodeTiger.Azure.Cosmos
{
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
        public IReadOnlyDictionary<string, object> Parameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AggregateStoredProcedure"/> class.
        /// </summary>
        /// <param name="body">The source code of the stored procedure.</param>
        /// <param name="parameters">The parameters to be passed in to the <c>SELECT</c> query used to retrieve the
        /// documents to be aggregated together.</param>
        public AggregateStoredProcedure(string body, IReadOnlyDictionary<string, object> parameters)
        {
            Body = body;
            Parameters = parameters;

            _id = new Lazy<string>(() => CalculateId(Body));
        }

        private string CalculateId(string body)
        {
            using (var hash = SHA256.Create())
            {
                return "codeTigerAggregate_"
                    + ConvertToIdSafeString(hash.ComputeHash(Encoding.UTF8.GetBytes(body)));
            }
        }

        private static string ConvertToIdSafeString(byte[] input)
        {
            return string.Concat(input.Select(x => x.ToString("x2", CultureInfo.InvariantCulture)));
        }
    }
}
