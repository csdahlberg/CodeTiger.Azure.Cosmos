using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Newtonsoft.Json;

namespace CodeTiger.Azure.Cosmos
{
    /// <summary>
    /// Represents a response from executing a stored procedure used to aggregate data from documents in Cosmos DB.
    /// </summary>
    /// <typeparam name="T">The type of the aggregates returned by the aggregate query.</typeparam>
    public class AggregateQueryResponse<T> : IEnumerable<T>
    {
        private readonly StoredProcedureExecuteResponse<AggregateDocumentQueryState> _storedProcedureResponse;
        private readonly string _responseContinuation;

        /// <summary>
        /// Gets the headers in the response from executing the stored procedure.
        /// </summary>
        public Headers ResponseHeaders => _storedProcedureResponse.Headers;

        /// <summary>
        /// Gets the status code from executing the stored procedure.
        /// </summary>
        public HttpStatusCode StatusCode => _storedProcedureResponse.StatusCode;

        /// <summary>
        /// Gets the Activity ID of the request.
        /// </summary>
        public string ActivityId => _storedProcedureResponse.ActivityId;

        /// <summary>
        /// Gets the number of normalized request units charged from executing the stored procedure.
        /// </summary>
        public double RequestCharge => _storedProcedureResponse.RequestCharge;

        /// <summary>
        /// Gets a continuation token that can be used to continue the aggregate query in a subsequent request,
        /// or <c>null</c> if the query has completed.
        /// </summary>
        public string ResponseContinuation => _storedProcedureResponse.Resource.ContinuationToken;
        
        internal AggregateQueryResponse(StoredProcedureExecuteResponse<AggregateDocumentQueryState> response)
        {
            _storedProcedureResponse = response;
            _responseContinuation = response.Headers.ContinuationToken;
        }

        /// <summary>
        /// Returns an enumerator that iterates through the aggregate results.
        /// </summary>
        /// <returns>An enumerator that iterates through the aggregate results.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (_storedProcedureResponse?.Resource?.Results != null)
            {
                return _storedProcedureResponse.Resource.Results
                    .Select(x => x.ToObject<T>())
                    .GetEnumerator();
            }

            return Enumerable.Empty<T>().GetEnumerator();
        }

        private string CreateResponseContinuation()
        {
            var queryStateForResponseContinuation = new AggregateDocumentQueryState
            {
                Parameters = _storedProcedureResponse.Resource.Parameters,
                MaxResultCount = _storedProcedureResponse.Resource.MaxResultCount,
                ContinuationToken = _storedProcedureResponse.Resource.ContinuationToken,
                PartialAggregate = _storedProcedureResponse.Resource.PartialAggregate,
                ReturnedResultCount = _storedProcedureResponse.Resource.ReturnedResultCount,
            };

            return JsonConvert.SerializeObject(queryStateForResponseContinuation);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
